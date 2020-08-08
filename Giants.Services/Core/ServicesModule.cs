namespace Giants.Services
{
    using Giants.Services.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServicesModule
    {
        public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IServerRegistryService, ServerRegistryService>();
            services.AddSingleton<IServerRegistryStore, CosmosDbServerRegistryStore>();
            services.AddSingleton<IDateTimeProvider, DefaultDateTimeProvider>();
        }
    }
}
