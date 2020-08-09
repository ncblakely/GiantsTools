namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class CosmosDbServerRegistryStore : IServerRegistryStore
    {
        private readonly ILogger<CosmosDbServerRegistryStore> logger;
        private readonly IConfiguration configuration;
        private CosmosDbClient client;
        
        public CosmosDbServerRegistryStore(
            ILogger<CosmosDbServerRegistryStore> logger,
            IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public async Task<IEnumerable<ServerInfo>> GetServerInfos(
            Expression<Func<ServerInfo, bool>> whereExpression = null,
            string partitionKey = null)
        {
            return await this.client.GetItems<ServerInfo>(whereExpression);
        }

        public async Task<IEnumerable<TSelect>> GetServerInfos<TSelect>(
            Expression<Func<ServerInfo, TSelect>> selectExpression,
            Expression<Func<ServerInfo, bool>> whereExpression = null,
            string partitionKey = null)
        {
            ArgumentUtility.CheckForNull(selectExpression, nameof(selectExpression));

            return await this.client.GetItems<ServerInfo, TSelect>(
                selectExpression: selectExpression,
                whereExpression: whereExpression,
                partitionKey: partitionKey);
        }

        public async Task<ServerInfo> GetServerInfo(string ipAddress)
        {
            ArgumentUtility.CheckStringForNullOrEmpty(ipAddress, nameof(ipAddress));

            return await this.client.GetItemById<ServerInfo>(ipAddress);
        }

        public async Task UpsertServerInfo(ServerInfo serverInfo)
        {
            ArgumentUtility.CheckForNull(serverInfo, nameof(serverInfo));

            await this.client.UpsertItem(
                item: serverInfo,
                partitionKey: new PartitionKey(serverInfo.DocumentType));
        }

        public async Task DeleteServers(IEnumerable<string> ids, string partitionKey = null)
        {
            ArgumentUtility.CheckEnumerableForNullOrEmpty(ids, nameof(ids));

            foreach (string id in ids)
            { 
                this.logger.LogInformation("Deleting server for host IP {IPAddress}", id);

                await this.client.DeleteItem<ServerInfo>(id, partitionKey);
            }
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