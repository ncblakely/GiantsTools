namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Giants.Services.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ServerRegistryCleanupService : IServerRegistryCleanupService, IDisposable
    {
        private readonly ILogger<ServerRegistryCleanupService> logger;
        private readonly IConfiguration configuration;
        private readonly IServerRegistryStore serverRegistryStore;
        private readonly IDateTimeProvider dateTimeProvider;
        private readonly TimeSpan timeoutPeriod;
        private readonly TimeSpan cleanupInterval;
        private Timer timer;

        public ServerRegistryCleanupService(
            ILogger<ServerRegistryCleanupService> logger,
            IConfiguration configuration,
            IServerRegistryStore serverRegistryStore,
            IDateTimeProvider dateTimeProvider)
        {
            this.logger = logger;
            this.configuration = configuration;
            this.serverRegistryStore = serverRegistryStore;
            this.dateTimeProvider = dateTimeProvider;
            this.timeoutPeriod = TimeSpan.FromMinutes(Convert.ToDouble(this.configuration["ServerTimeoutPeriodInMinutes"]));
            this.cleanupInterval = TimeSpan.FromMinutes(Convert.ToDouble(this.configuration["ServerCleanupIntervalInMinutes"]));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.timer = new Timer(this.TimerCallback, null, TimeSpan.Zero, this.cleanupInterval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this.timer?.Dispose();
        }

        private void TimerCallback(object state)
        {
            this.CleanupServers().GetAwaiter().GetResult();
        }

        private async Task CleanupServers()
        {
            List<string> expiredServers = (await this
                .serverRegistryStore
                .GetServerInfos(whereExpression: s => s.LastHeartbeat < (this.dateTimeProvider.UtcNow - this.timeoutPeriod)))
                .Select(s => s.id)
                .ToList();

            if (expiredServers.Any())
            {
                this.logger.LogInformation("Cleaning up {Count} servers.", expiredServers.Count);

                await this.serverRegistryStore.DeleteServers(expiredServers);
            }
        }
    }
}
