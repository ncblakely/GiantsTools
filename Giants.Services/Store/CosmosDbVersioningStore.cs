namespace Giants.Services.Store
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class CosmosDbVersioningStore : IVersioningStore
    {
        private readonly ILogger<CosmosDbServerRegistryStore> logger;
        private readonly IMemoryCache memoryCache;
        private readonly IConfiguration configuration;
        private CosmosDbClient client;

        public CosmosDbVersioningStore(
            ILogger<CosmosDbServerRegistryStore> logger,
            IMemoryCache memoryCache,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.memoryCache = memoryCache;
            this.configuration = configuration;
        }

        public async Task<VersionInfo> GetVersionInfo(string appName)
        {
            VersionInfo versionInfo = await this.memoryCache.GetOrCreateAsync<VersionInfo>(
                key: GetCacheKey(appName),
                factory: async (entry) => 
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                    return await this.client.GetItemById<VersionInfo>(
                        VersionInfo.GenerateId(appName),
                        nameof(VersionInfo));
                });

            return versionInfo;
        }

        public async Task UpdateVersionInfo(VersionInfo versionInfo)
        {
            await this.client.UpsertItem(versionInfo);
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

        private static string GetCacheKey(string gameName)
        {
            return $"{CacheKeys.VersionInfo}-{gameName}";
        }
    }
}
