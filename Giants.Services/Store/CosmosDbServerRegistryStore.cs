namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;

    public class CosmosDbServerRegistryStore : IServerRegistryStore
    {
        private readonly IConfiguration configuration;
        private CosmosDbClient client;
        
        public CosmosDbServerRegistryStore(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task<IEnumerable<ServerInfo>> GetServerInfos(
            Expression<Func<ServerInfo, bool>> whereExpression = null)
        {
            return await this.client.GetItems<ServerInfo>(whereExpression);
        }

        public async Task<ServerInfo> GetServerInfo(string ipAddress)
        {
            return await this.client.GetItemById<ServerInfo>(ipAddress);
        }

        public async Task UpsertServerInfo(ServerInfo serverInfo)
        {
            await this.client.UpsertItem(serverInfo, new PartitionKey(serverInfo.DocumentType));
        }

        public async Task Initialize()
        {
            this.client = new CosmosDbClient(
                connectionString: this.configuration["CosmosDbEndpoint"],
                authKeyOrResourceToken: this.configuration["CosmosDbKey"],
                databaseId: this.configuration["DatabaseId"],
                containerId: this.configuration["ContainerId"]);

            await this.client.Initialize();
        }
    }
}
