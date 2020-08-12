namespace Giants.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Giants.Services.Core;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class CosmosDbServerRegistryStore : IServerRegistryStore
    {
        private readonly ILogger<CosmosDbServerRegistryStore> logger;
        private readonly IConfiguration configuration;
        private readonly IMemoryCache memoryCache;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly TimeSpan timeoutPeriod;
        private CosmosDbClient client;
        
        public CosmosDbServerRegistryStore(
            ILogger<CosmosDbServerRegistryStore> logger,
            IConfiguration configuration,
            IMemoryCache memoryCache,
            IDateTimeProvider dateTimeProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.memoryCache = memoryCache;
            this.dateTimeProvider = dateTimeProvider;
            this.timeoutPeriod = TimeSpan.FromMinutes(Convert.ToDouble(this.configuration["ServerTimeoutPeriodInMinutes"]));
        }

        public async Task<IEnumerable<ServerInfo>> GetServerInfos(
            Expression<Func<ServerInfo, bool>> whereExpression = null,
            string partitionKey = null)
        {
            ConcurrentDictionary<string, ServerInfo> serverInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);

            IQueryable<ServerInfo> serverInfoQuery = serverInfo
                .Values
                .AsQueryable();

            if (whereExpression != null)
            {
                serverInfoQuery = serverInfoQuery.Where(whereExpression);
            }

            return serverInfoQuery.Where(c => c.LastHeartbeat > this.dateTimeProvider.UtcNow - this.timeoutPeriod)
                .ToList();
        }

        public async Task<IEnumerable<TSelect>> GetServerInfos<TSelect>(
            Expression<Func<ServerInfo, TSelect>> selectExpression,
            Expression<Func<ServerInfo, bool>> whereExpression = null,
            string partitionKey = null)
        {
            ConcurrentDictionary<string, ServerInfo> serverInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);

            IQueryable<ServerInfo> serverInfoQuery = serverInfo
                .Values
                .AsQueryable();

            if (serverInfoQuery != null)
            {
                serverInfoQuery = serverInfoQuery.Where(whereExpression);
            }

            return serverInfoQuery.Where(c => c.LastHeartbeat > this.dateTimeProvider.UtcNow - this.timeoutPeriod)
                .Select(selectExpression)
                .ToList();
        }

        public async Task<ServerInfo> GetServerInfo(string ipAddress)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(ipAddress, nameof(ipAddress));

            ConcurrentDictionary<string, ServerInfo> serverInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);
            if (serverInfo.ContainsKey(ipAddress))
            {
                try
                { 
                    return serverInfo[ipAddress];
                }
                catch (Exception e)
                {
                    this.logger.LogInformation("Cached server for {IPAddress} no longer found: {Exception}", ipAddress, e.ToString());
                    // May have been removed from the cache by another thread. Ignore and query DB instead.
                }
            }

            return await this.client.GetItemById<ServerInfo>(ipAddress);
        }

        public async Task UpsertServerInfo(ServerInfo serverInfo)
        {
            ArgumentUtility.CheckForNull(serverInfo, nameof(serverInfo));

            // Check cache before we write to store
            ConcurrentDictionary<string, ServerInfo> allServerInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);

            if (allServerInfo.ContainsKey(serverInfo.HostIpAddress))
            {
                if (allServerInfo[serverInfo.HostIpAddress].Equals(serverInfo))
                {
                    this.logger.LogInformation("State for {IPAddress} is unchanged. Skipping write to store.", serverInfo.HostIpAddress);
                    return;
                }
                else
                {
                    this.logger.LogInformation("State for {IPAddress} has changed. Committing update to store.", serverInfo.HostIpAddress);
                }
            }

            await this.client.UpsertItem(
                item: serverInfo,
                partitionKey: new PartitionKey(serverInfo.DocumentType));

            this.logger.LogInformation("Updating cache for request from {IPAddress}.", serverInfo.HostIpAddress);

            if (allServerInfo.ContainsKey(serverInfo.HostIpAddress))
            {
                allServerInfo[serverInfo.HostIpAddress] = serverInfo;
            }
            else
            {
                allServerInfo.TryAdd(serverInfo.HostIpAddress, serverInfo);
            }
        }

        public async Task DeleteServer(string id, string partitionKey = null)
        {
            await this.client.DeleteItem<ServerInfo>(id, partitionKey);

            // Remove from cache
            ConcurrentDictionary<string, ServerInfo> allServerInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);
            allServerInfo.TryRemove(id, out ServerInfo _);
        }

        public async Task DeleteServers(IEnumerable<string> ids, string partitionKey = null)
        {
            ArgumentUtility.CheckEnumerableForNullOrEmpty(ids, nameof(ids));

            foreach (string id in ids)
            { 
                this.logger.LogInformation("Deleting server for host IP {IPAddress}", id);

                await this.DeleteServer(id, partitionKey);
            }
        }

        public async Task Initialize()
        {
            this.client = new CosmosDbClient(
                connectionString: this.configuration["CosmosDbEndpoint"],
                authKeyOrResourceToken: this.configuration["CosmosDbKey"],
                databaseId: this.configuration["DatabaseId"],
                containerId: this.configuration["ContainerId"]);

            await this.client.Initialize();
        }

        private async Task<ConcurrentDictionary<string, ServerInfo>> PopulateCache(ICacheEntry entry)
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

            IDictionary<string, ServerInfo> serverInfo = 
                (await this.client.GetItems<ServerInfo>())
                .ToDictionary(x => x.HostIpAddress, y => y);

            return new ConcurrentDictionary<string, ServerInfo>(serverInfo);
        }
    }
}