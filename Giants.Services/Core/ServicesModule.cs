namespace Giants.Services
{
    using Giants.Services.Core;
    using Giants.Services.Services;
    using Giants.Services.Store;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServicesModule
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IServerRegistryService, ServerRegistryService>();
            services.AddSingleton<IServerRegistryStore, CosmosDbServerRegistryStore>();
            services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddSingleton<IUpdaterStore, CosmosDbUpdaterStore>();
            services.AddSingleton<IUpdaterService, UpdaterService>();
            services.AddSingleton<ICommunityService, CommunityService>();
            services.AddSingleton<ICrashReportService, CrashReportService>();

            services.AddHostedService<InitializerService>();
            services.AddHostedService<ServerRegistryCleanupService>();
        }
    }
}
