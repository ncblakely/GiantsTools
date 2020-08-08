using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Giants.Services;
using Microsoft.Extensions.Hosting;

namespace Giants.WebApi
{
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
