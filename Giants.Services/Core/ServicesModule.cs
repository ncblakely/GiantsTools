namespace Giants.Services
{
    using System;
    using System.Net.Http.Headers;
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
            services.AddSingleton<IVersioningStore, CosmosDbVersioningStore>();
            services.AddSingleton<IVersioningService, VersioningService>();
            services.AddSingleton<ICommunityService, CommunityService>();
            services.AddSingleton<ICrashReportService, CrashReportService>();

            services.AddHostedService<InitializerService>();
            services.AddHostedService<ServerRegistryCleanupService>();

            services.AddHttpClient("Sentry", c =>
            {
                c.BaseAddress = new Uri(configuration["SentryBaseUri"]);

                string sentryAuthenticationToken = configuration["SentryAuthenticationToken"];
                c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", sentryAuthenticationToken);
            });
        }
    }
}
