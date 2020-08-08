namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading.Tasks;

    public class ServerRegistryService : IServerRegistryService
    {
        private readonly IServerRegistryStore registryStore;

        public ServerRegistryService(IServerRegistryStore registryStore)
        {
            this.registryStore = registryStore;
        }

        public async Task AddServer(
            IPAddress ipAddress,
            ServerInfo server)
        {
            ServerInfo existingServer = await this.registryStore.GetServerInfo(ipAddress ?? throw new ArgumentNullException(nameof(ipAddress)));
            if (existingServer != null)
            {

            }

            await this.registryStore.UpsertServerInfo(ipAddress, server ?? throw new ArgumentNullException(nameof(server)));
        }

        public async Task<IEnumerable<ServerInfo>> GetAllServers()
        {
            return await this.registryStore.GetAllServerInfos();
        }
    }
}
