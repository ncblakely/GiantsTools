namespace Giants.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IServerRegistryService
    {
        Task DeleteServer(string ipAddress);

        Task<IEnumerable<ServerInfo>> GetAllServers();

        Task AddServer(ServerInfo server);
    }
}
