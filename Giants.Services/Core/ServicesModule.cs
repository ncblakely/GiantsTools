namespace Giants.Services
{
    using Microsoft.Extensions.DependencyInjection;

    public static class ServicesModule
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IServerRegistryService, ServerRegistryService>();
            services.AddSingleton<IServerRegistryStore, InMemoryServerRegistryStore>();
        }
    }
}
