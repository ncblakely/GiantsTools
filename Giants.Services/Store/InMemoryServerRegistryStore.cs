namespace Giants.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Threading.Tasks;

    public class InMemoryServerRegistryStore : IServerRegistryStore
    {
        private ConcurrentDictionary<string, ServerInfo> servers = new ConcurrentDictionary<string, ServerInfo>();

        public Task<IEnumerable<ServerInfo>> GetServerInfos(
            Expression<Func<ServerInfo, bool>> whereExpression = null)
        {
            return Task.FromResult(
                this.servers.Values.AsEnumerable());
        }

        public Task<ServerInfo> GetServerInfo(string ipAddress)
        {
            if (servers.ContainsKey(ipAddress))
            { 
                return Task.FromResult(servers[ipAddress]);
            }

            return Task.FromResult((ServerInfo)null);
        }

        public Task Initialize()
        {
            this.servers.Clear();

            return Task.CompletedTask;
        }

        public Task UpsertServerInfo(ServerInfo serverInfo)
        {
            this.servers.TryAdd(serverInfo.HostIpAddress, serverInfo);

            return Task.CompletedTask;
        }
    }
}
