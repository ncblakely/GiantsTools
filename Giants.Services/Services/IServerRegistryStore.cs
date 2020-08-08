namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading.Tasks;

    public interface IServerRegistryStore
    {
        public Task<IEnumerable<ServerInfo>> GetAllServerInfos();

        public Task<ServerInfo> GetServerInfo(IPAddress ipAddress);

        public Task UpsertServerInfo(IPAddress ipAddress, ServerInfo serverInfo);
    }
}
