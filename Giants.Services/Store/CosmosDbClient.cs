namespace Giants.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Giants.Services.Core.Entities;
    using Microsoft.Azure.Cosmos;
    using Microsoft.Azure.Cosmos.Linq;

    public class CosmosDbClient
    {
        private readonly string connectionString;
        private readonly string authKeyOrResourceToken;
        private readonly string databaseId;
        private readonly string containerId;
        private CosmosClient client;
        private Container container;

        public CosmosDbClient(
            string connectionString,
            string authKeyOrResourceToken,
            string databaseId,
            string containerId)
        {
            this.connectionString = connectionString;
            this.authKeyOrResourceToken = authKeyOrResourceToken;
            this.databaseId = databaseId;
            this.containerId = containerId;
        }

        public async Task<IEnumerable<TSelect>> GetItems<T, TSelect>(
            Expression<Func<T, TSelect>> selectExpression,
            Expression<Func<T, bool>> whereExpression = null,
            string partitionKey = null)
            where T : IIdentifiable
        {
            if (partitionKey == null)
            {
                partitionKey = typeof(T).Name;
            }

            IQueryable<T> query = this.container
                .GetItemLinqQueryable<T>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                });

            if (whereExpression != null)
            {
                query = query.Where(whereExpression);
            }

            var feedIteratorQuery = query
                .Select(selectExpression)
                .ToFeedIterator();

            var items = new List<TSelect>();
            while (feedIteratorQuery.HasMoreResults)
            {
                var results = await feedIteratorQuery.ReadNextAsync();
                foreach (var result in results)
                {
                    items.Add(result);
                }
            }

            return items;
        }

        public async Task<IEnumerable<T>> GetItems<T>(
            Expression<Func<T, bool>> whereExpression = null,
            string partitionKey = null)
            where T : IIdentifiable
        {
            if (partitionKey == null)
            {
                partitionKey = typeof(T).Name;
            }

            IQueryable<T> query = this.container
                .GetItemLinqQueryable<T>(requestOptions: new QueryRequestOptions
                {
                    PartitionKey = new PartitionKey(partitionKey),
                });

            if (whereExpression != null)
            {
                query = query.Where(whereExpression);
            }

            var feedIteratorQuery = query
                .ToFeedIterator();

            var items = new List<T>();
            while (feedIteratorQuery.HasMoreResults)
            {
                var results = await feedIteratorQuery.ReadNextAsync();
                foreach (var result in results)
                {
                    items.Add(result);
                }
            }

            return items;
        }

        public async Task<T> GetItemById<T>(string id, string partitionKey = null)
            where T : IIdentifiable
        {
            return (await this.GetItems<T>(t => t.id == id, partitionKey)).FirstOrDefault();
        }

        public async Task UpsertItem<T>(
            T item,
            PartitionKey? partitionKey = null,
            ItemRequestOptions itemRequestOptions = null)
            where T : IIdentifiable
        {
            await this.container.UpsertItemAsync(item, partitionKey, itemRequestOptions);
        }

        public async Task Initialize(string partitionKeyPath = null)
        {
            this.client = new CosmosClient(
                this.connectionString,
                this.authKeyOrResourceToken);

            var databaseResponse = await this.client.CreateDatabaseIfNotExistsAsync(databaseId);
            var containerResponse = await databaseResponse.Database.CreateContainerIfNotExistsAsync(new ContainerProperties()
            {
                Id = containerId,
                PartitionKeyPath = partitionKeyPath ?? "/DocumentType"
            });

            this.container = containerResponse.Container;
        }

        public async Task DeleteItem<T>(
            string id,
            string partitionKey = null,
            ItemRequestOptions requestOptions = null)
        {
            if (partitionKey == null)
            {
                partitionKey = typeof(T).Name;
            }

            try
            { 
                await this.container.DeleteItemAsync<T>(id, new PartitionKey(partitionKey), requestOptions);
            }
            catch (CosmosException e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Ignore
            }
        }
    }
}
