namespace Giants.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Giants.Services.Core;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ServerRegistryService : IServerRegistryService
    {
        private static readonly string[] SupportedGameNames = new[] { "Giants" };
        private readonly ILogger<ServerRegistryService> logger;
        private readonly IServerRegistryStore registryStore;
        private readonly IConfiguration configuration;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly IMemoryCache memoryCache;
        private readonly TimeSpan timeoutPeriod;
        private readonly int maxServerCount;

        public ServerRegistryService(
            ILogger<ServerRegistryService> logger,
            IServerRegistryStore registryStore,
            IConfiguration configuration,
            IDateTimeProvider dateTimeProvider,
            IMemoryCache memoryCache)
        {
            this.logger = logger;
            this.registryStore = registryStore;
            this.configuration = configuration;
            this.dateTimeProvider = dateTimeProvider;
            this.memoryCache = memoryCache;
            this.timeoutPeriod = TimeSpan.FromMinutes(Convert.ToDouble(this.configuration["ServerTimeoutPeriodInMinutes"]));
            this.maxServerCount = Convert.ToInt32(this.configuration["MaxServerCount"]);
        }

        public async Task AddServer(
            ServerInfo serverInfo)
        {
            ArgumentUtility.CheckForNull(serverInfo, nameof(serverInfo));
            ArgumentUtility.CheckStringForNullOrEmpty(serverInfo.HostIpAddress, nameof(serverInfo.HostIpAddress));

            if (!SupportedGameNames.Contains(serverInfo.GameName, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Unsupported game name {serverInfo.GameName}", nameof(serverInfo));
            }

            // Check cache before we write to store
            ConcurrentDictionary<string, ServerInfo> allServerInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, PopulateCache);

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
            await this.registryStore.UpsertServerInfo(serverInfo);

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

        public async Task<IEnumerable<ServerInfo>> GetAllServers()
        {
            ConcurrentDictionary<string, ServerInfo> serverInfo = await this.memoryCache.GetOrCreateAsync(CacheKeys.ServerInfo, PopulateCache);

            return serverInfo.Values;
        }

        public async Task DeleteServer(string ipAddress)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(ipAddress, nameof(ipAddress));

            ServerInfo serverInfo = await this.registryStore.GetServerInfo(ipAddress);

            if (serverInfo != null)
            {
                await this.registryStore.DeleteServer(serverInfo.id);
            }
        }

        private async Task<ConcurrentDictionary<string, ServerInfo>> PopulateCache(ICacheEntry entry)
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);

            IDictionary<string, ServerInfo> serverInfo = (await this.registryStore
                .GetServerInfos(whereExpression: c => c.LastHeartbeat > this.dateTimeProvider.UtcNow - this.timeoutPeriod))
                .Take(this.maxServerCount)
                .ToDictionary(x => x.HostIpAddress, y => y);

            return new ConcurrentDictionary<string, ServerInfo>(serverInfo);
        }
    }
}
