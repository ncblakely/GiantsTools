namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ServerRegistryService : IServerRegistryService
    {
        private static readonly string[] SupportedGameNames = new[] { "Giants", "Giants Beta" };
        private readonly ILogger<ServerRegistryService> logger;
        private readonly IServerRegistryStore registryStore;
        private readonly IConfiguration configuration;
        private readonly int maxServerCount;
        private readonly int maxServersPerIpGame;

        public ServerRegistryService(
            ILogger<ServerRegistryService> logger,
            IServerRegistryStore registryStore,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.registryStore = registryStore;
            this.configuration = configuration;
            this.maxServerCount = Convert.ToInt32(this.configuration["MaxServerCount"]);
            this.maxServersPerIpGame = Convert.ToInt32(this.configuration["MaxServersPerIpGame"]);
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

            var existingServers = await this.registryStore.GetServerInfos(whereExpression: x => x.HostIpAddress == serverInfo.HostIpAddress);
            if (existingServers.GroupBy(g => g.GameName).Any(g => g.Count() > this.maxServersPerIpGame))
            {
                throw new InvalidOperationException("Exceeded maximum servers per IP.");
            }

            await this.registryStore.UpsertServerInfo(serverInfo);
        }

        public async Task<IEnumerable<ServerInfo>> GetAllServers()
        {
            return (await this.registryStore.GetServerInfos())
                .Take(this.maxServerCount);
        }

        // Old API, soon to be deprecated
        public async Task DeleteServer(string ipAddress)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(ipAddress, nameof(ipAddress));

            var serverInfos = await this.registryStore.GetServerInfos(whereExpression: x => x.HostIpAddress == ipAddress);

            foreach (var serverInfo in serverInfos)
            {
                await this.registryStore.DeleteServer(serverInfo.id);
            }
        }

        public async Task DeleteServer(string ipAddress, string gameName, int port)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(ipAddress, nameof(ipAddress));
            ArgumentUtility.CheckStringForNullOrEmpty(gameName, nameof(gameName));
            ArgumentUtility.CheckForNonnegativeInt(port, nameof(port));

            var existingServerInfo = (await this.registryStore.GetServerInfos(
                whereExpression: 
                    x => x.HostIpAddress == ipAddress &&
                    x.Port == port &&
                    x.GameName.Equals(gameName, StringComparison.OrdinalIgnoreCase)))
                    .FirstOrDefault();

            if (existingServerInfo != null)
            {
                await this.registryStore.DeleteServer(existingServerInfo.id);
            }
        }
    }
}
