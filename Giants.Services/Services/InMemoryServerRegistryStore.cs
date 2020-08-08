namespace Giants.Services
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;

    public class InMemoryServerRegistryStore : IServerRegistryStore
    {
        private ConcurrentDictionary<IPAddress, ServerInfo> servers = new ConcurrentDictionary<IPAddress, ServerInfo>();

        public Task<IEnumerable<ServerInfo>> GetAllServerInfos()
        {
            return Task.FromResult(
                this.servers.Values.AsEnumerable());
        }

        public Task<ServerInfo> GetServerInfo(IPAddress ipAddress)
        {
            if (servers.ContainsKey(ipAddress))
            { 
                return Task.FromResult(servers[ipAddress]);
            }

            return Task.FromResult((ServerInfo)null);
        }

        public Task UpsertServerInfo(IPAddress ipAddress, ServerInfo serverInfo)
        {
            servers.TryAdd(ipAddress, serverInfo);

            return Task.CompletedTask;
        }
    }
}
