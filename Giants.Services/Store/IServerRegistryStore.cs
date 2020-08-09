namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IServerRegistryStore
    {
        Task Initialize();

        Task<IEnumerable<ServerInfo>> GetServerInfos(Expression<Func<ServerInfo, bool>> whereExpression = null, string partitionKey = null);

        Task<IEnumerable<TSelect>> GetServerInfos<TSelect>(
            Expression<Func<ServerInfo, TSelect>> selectExpression,
            Expression<Func<ServerInfo, bool>> whereExpression = null,
            string partitionKey = null);

        Task<ServerInfo> GetServerInfo(string ipAddress);

        Task UpsertServerInfo(ServerInfo serverInfo);

        Task DeleteServers(IEnumerable<string> ids, string partitionKey = null);
    }
}
