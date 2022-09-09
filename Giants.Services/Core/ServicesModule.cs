namespace Giants.Services
{
    using Autofac;
    using Giants.Services.Core;
    using Giants.Services.Services;
    using Giants.Services.Store;
    using Giants.Services.Utility;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using System;

    public class ServicesModule : Module
    {
        private readonly IConfiguration configuration;

        public ServicesModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ServerRegistryService>()
                .As<IServerRegistryService>()
                .SingleInstance();
            builder.RegisterType<CosmosDbServerRegistryStore>()
                .As<IServerRegistryStore>()
                .SingleInstance();
            builder.RegisterType<DefaultDateTimeProvider>()
                .As<IDateTimeProvider>()
                .SingleInstance();
            builder.RegisterType<MemoryCache>()
                .As<IMemoryCache>()
                .SingleInstance();
            builder.RegisterType<CosmosDbVersioningStore>()
                .As<IVersioningStore>()
                .SingleInstance();
            builder.RegisterType<VersioningService>()
                .As<IVersioningService>()
                .SingleInstance();
            builder.RegisterType<CommunityService>()
                .As<ICommunityService>()
                .SingleInstance();
            builder.RegisterType<CrashReportService>()
                .As<ICrashReportService>()
                .SingleInstance();

            var cosmosClient = new CosmosDbClient(
                connectionString: this.configuration["CosmosDbEndpoint"],
                authKeyOrResourceToken: this.configuration["CosmosDbKey"],
                databaseId: this.configuration["DatabaseId"],
                containerId: this.configuration["ContainerId"]);

            cosmosClient.Initialize().GetAwaiter().GetResult();

            builder.RegisterInstance(cosmosClient).SingleInstance();

            builder.Register<ISimpleMemoryCache<VersionInfo>>(icc =>
            {
                var versionStore = icc.Resolve<IVersioningStore>();

                return new SimpleMemoryCache<VersionInfo>(
                    expirationPeriod: TimeSpan.FromMinutes(5),
                    memoryCache: icc.Resolve<IMemoryCache>(),
                    cacheKey: CacheKeys.VersionInfo,
                    getAllItems: async (cacheEntry) =>
                    {
                        return await versionStore.GetVersions();
                    });
            })
            .AsSelf()
            .SingleInstance();
        }
    }
}
