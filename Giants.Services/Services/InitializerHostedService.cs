namespace Giants.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public class InitializerHostedService : IHostedService
    {
        private readonly IServerRegistryStore serverRegistryStore;

        public InitializerHostedService(IServerRegistryStore serverRegistryStore)
        {
            this.serverRegistryStore = serverRegistryStore;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.serverRegistryStore.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
