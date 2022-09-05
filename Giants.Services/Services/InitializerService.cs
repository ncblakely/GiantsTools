namespace Giants.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;

    public class InitializerService : IHostedService
    {
        private readonly IVersioningStore updaterStore;
        private readonly IServerRegistryStore serverRegistryStore;

        public InitializerService(
            IVersioningStore updaterStore,
            IServerRegistryStore serverRegistryStore)
        {
            // TODO: Pick these up from reflection and auto initialize
            this.updaterStore = updaterStore;
            this.serverRegistryStore = serverRegistryStore;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await this.serverRegistryStore.Initialize();
            await this.updaterStore.Initialize();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
