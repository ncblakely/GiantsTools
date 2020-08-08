namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public interface IServerRegistryService
    {
        Task<IEnumerable<ServerInfo>> GetAllServers();

        Task AddServer(ServerInfo server);
    }
}
