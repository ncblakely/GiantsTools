namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ServerRegistryService : IServerRegistryService
    {
        private static readonly string[] SupportedGameNames = new[] { "Giants" };
        private readonly ILogger<ServerRegistryService> logger;
        private readonly IServerRegistryStore registryStore;
        private readonly IConfiguration configuration;
        private readonly int maxServerCount;

        public ServerRegistryService(
            ILogger<ServerRegistryService> logger,
            IServerRegistryStore registryStore,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.registryStore = registryStore;
            this.configuration = configuration;
            this.maxServerCount = Convert.ToInt32(this.configuration["MaxServerCount"]);
        }

        public async Task AddServer(
            ServerInfo serverInfo)
        {
            ArgumentUtility.CheckForNull(serverInfo, nameof(serverInfo));
            ArgumentUtility.CheckStringForNullOrEmpty(serverInfo.HostIpAddress, nameof(serverInfo.HostIpAddress));

            if (!SupportedGameNames.Contains(serverInfo.GameName, StringComparer.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Unsupported game name {serverInfo.GameName}", nameof(serverInfo));
            }

            await this.registryStore.UpsertServerInfo(serverInfo);
        }

        public async Task<IEnumerable<ServerInfo>> GetAllServers()
        {
            return (await this.registryStore.GetServerInfos())
                .Take(this.maxServerCount);
        }

        public async Task DeleteServer(string ipAddress)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(ipAddress, nameof(ipAddress));

            ServerInfo serverInfo = await this.registryStore.GetServerInfo(ipAddress);

            if (serverInfo != null)
            {
                await this.registryStore.DeleteServer(serverInfo.id);
            }
        }
    }
}
