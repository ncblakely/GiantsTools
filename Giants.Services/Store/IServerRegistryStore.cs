namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IServerRegistryStore
    {
        public Task Initialize();

        public Task<IEnumerable<ServerInfo>> GetServerInfos(Expression<Func<ServerInfo, bool>> whereExpression = null);

        public Task<ServerInfo> GetServerInfo(string ipAddress);

        public Task UpsertServerInfo(ServerInfo serverInfo);
    }
}
