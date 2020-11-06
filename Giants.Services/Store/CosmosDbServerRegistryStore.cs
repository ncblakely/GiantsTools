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

        private const int ServerRefreshIntervalInMinutes = 1;
        
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
            bool includeExpired = false,
            string partitionKey = null)
        {
            ConcurrentDictionary<string, IList<ServerInfo>> serverInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);

            IQueryable<ServerInfo> serverInfoQuery = serverInfo
                .Values
                .SelectMany(s => s)
                .AsQueryable();

            if (whereExpression != null)
            {
                serverInfoQuery = serverInfoQuery.Where(whereExpression);
            }

            if (!includeExpired)
            {
                serverInfoQuery = serverInfoQuery.Where(c => c.LastHeartbeat > this.dateTimeProvider.UtcNow - this.timeoutPeriod);
            }

            return serverInfoQuery
                .ToList();
        }

        public async Task<IEnumerable<TSelect>> GetServerInfos<TSelect>(
            Expression<Func<ServerInfo, TSelect>> selectExpression,
            bool includeExpired = false,
            Expression<Func<ServerInfo, bool>> whereExpression = null,
            string partitionKey = null)
        {
            ConcurrentDictionary<string, IList<ServerInfo>> serverInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);

            IQueryable<ServerInfo> serverInfoQuery = serverInfo
                .Values
                .SelectMany(s => s)
                .AsQueryable();

            if (serverInfoQuery != null)
            {
                serverInfoQuery = serverInfoQuery.Where(whereExpression);
            }

            if (!includeExpired)
            {
                serverInfoQuery = serverInfoQuery.Where(c => c.LastHeartbeat > this.dateTimeProvider.UtcNow - this.timeoutPeriod);
            }

            return serverInfoQuery
                .Select(selectExpression)
                .ToList();
        }

        public async Task UpsertServerInfo(ServerInfo serverInfo)
        {
            ArgumentUtility.CheckForNull(serverInfo, nameof(serverInfo));

            // Check cache before we write to store
            ConcurrentDictionary<string, IList<ServerInfo>> allServerInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);

            if (allServerInfo.ContainsKey(serverInfo.HostIpAddress))
            {
                ServerInfo existingServerInfo = FindExistingServerForHostIp(allServerInfo[serverInfo.HostIpAddress], serverInfo);

                // DDOS protection: skip write to Cosmos if parameters have not changed,
                // or it's not been long enough.
                if (existingServerInfo != null && Math.Abs((existingServerInfo.LastHeartbeat - serverInfo.LastHeartbeat).TotalMinutes) < ServerRefreshIntervalInMinutes)
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
                allServerInfo[serverInfo.HostIpAddress].Add(serverInfo);
            }
            else
            {
                allServerInfo.TryAdd(serverInfo.HostIpAddress, new List<ServerInfo>() { serverInfo });
            }
        }

        public async Task DeleteServer(string id, string partitionKey = null)
        {
            await this.client.DeleteItem<ServerInfo>(id, partitionKey);

            // Remove from cache
            ConcurrentDictionary<string, IList<ServerInfo>> allServerInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, this.PopulateCache);
            if (allServerInfo.ContainsKey(id))
            {
                var serverInfoCopy = allServerInfo[id].Where(s => s.id != id).ToList();
                if (!serverInfoCopy.Any())
                {
                    // No remaining servers, remove the key
                    allServerInfo.TryRemove(id, out IList<ServerInfo> _);
                }
                else
                {
                    // Shallow-copy and replace to keep thread safety guarantee
                    allServerInfo[id] = serverInfoCopy;
                }
            }
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

        private async Task<ConcurrentDictionary<string, IList<ServerInfo>>> PopulateCache(ICacheEntry entry)
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

            var allServerInfo = (await this.client.GetItems<ServerInfo>());
            var serverInfoDictionary = new ConcurrentDictionary<string, IList<ServerInfo>>();

            foreach (var serverInfo in allServerInfo)
            {
                if (!serverInfoDictionary.ContainsKey(serverInfo.HostIpAddress))
                {
                    serverInfoDictionary[serverInfo.HostIpAddress] = new List<ServerInfo>() { serverInfo };
                }
                else
                {
                    serverInfoDictionary[serverInfo.HostIpAddress].Add(serverInfo);
                }
            }

            return serverInfoDictionary;
        }

        private static ServerInfo FindExistingServerForHostIp(IList<ServerInfo> serverInfoForHostIp, ServerInfo candidateServerInfo)
        {
            foreach (var existingServerInfo in serverInfoForHostIp)
            {
                if (existingServerInfo.Equals(candidateServerInfo))
                {
                    return existingServerInfo;
                }
            }

            return null;
        }
    }
}