namespace Giants.Services.Store
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class CosmosDbVersioningStore : IVersioningStore
    {
        private readonly CosmosDbClient client;

        public CosmosDbVersioningStore(
            CosmosDbClient cosmosDbClient)
        {
            this.client = cosmosDbClient;
        }

        public Task<IEnumerable<VersionInfo>> GetVersions()
        {
            return this.client.GetItems<VersionInfo>();
        }

        public async Task UpdateVersionInfo(VersionInfo versionInfo)
        {
            await this.client.UpsertItem(versionInfo);
        }
    }
}
