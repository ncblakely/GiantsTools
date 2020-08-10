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

        public Task<IEnumerable<ServerInfo>> GetServerInfos(Expression<Func<ServerInfo, bool>> whereExpression = null, string partitionKey = null)
        {
            IQueryable<ServerInfo> serverInfoQuery = this.servers.Values.AsQueryable();

            if (whereExpression != null)
            {
                serverInfoQuery = serverInfoQuery.Where(whereExpression);
            }

            return Task.FromResult(serverInfoQuery.AsEnumerable());
        }

        public Task<IEnumerable<TSelect>> GetServerInfos<TSelect>(Expression<Func<ServerInfo, TSelect>> selectExpression, Expression<Func<ServerInfo, bool>> whereExpression = null, string partitionKey = null)
        {
            IQueryable<ServerInfo> serverInfoQuery = this.servers.Values.AsQueryable();

            if (whereExpression != null)
            {
                serverInfoQuery = serverInfoQuery.Where(whereExpression);
            }

            return Task.FromResult(serverInfoQuery.Select(selectExpression).AsEnumerable());
        }

        public Task DeleteServers(IEnumerable<string> ids, string partitionKey = null)
        {
            foreach (string id in ids)
            {
                this.servers.TryRemove(id, out _);
            }

            return Task.CompletedTask;
        }

        public Task DeleteServer(string id, string partitionKey = null)
        {
            this.servers.TryRemove(id, out ServerInfo _);

            return Task.CompletedTask;
        }
    }
}
