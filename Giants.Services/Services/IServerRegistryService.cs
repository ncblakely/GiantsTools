namespace Giants.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServerRegistryService
    {
        Task DeleteServer(string ipAddress);

        Task DeleteServer(string ipAddress, string gameName, int port);

        Task<IEnumerable<ServerInfo>> GetAllServers();

        Task AddServer(ServerInfo server);
    }
}
