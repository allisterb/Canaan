using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Azure.Cosmos;

namespace Canaan
{
    public class CosmosDB : Api
    {
        public CosmosDB(string endpointUrl, string authKey, string databaseId, CancellationToken ct) : base(ct)
        {
            EndpointUrl = endpointUrl;
            AuthKey = authKey;
            Client = new CosmosClient(EndpointUrl, AuthKey,
                new CosmosClientOptions() { ApplicationName = "Canaan", ConsistencyLevel = ConsistencyLevel.Session});
            DatabaseId = databaseId;
            Initialized = true;
        }

        public CosmosDB(string databaseId, CancellationToken ct) : this(Config("CosmosDB:EndpointUrl"), Config("CosmosDB:AuthKey"), databaseId, ct) {}
        
        public CosmosDB(string databaseId) : this(databaseId, Cts.Token) {}

        public async Task<T> GetAsync<T>(string containerId, string partitionKey, string itemId) where T : IItem
        {
            ThrowIfNotInitialized();
            try
            {
                var container = Client.GetContainer(DatabaseId, containerId);
                var r =  await container.ReadItemAsync<T>(itemId, new PartitionKey(partitionKey), cancellationToken: CancellationToken);
                return r.Resource;
            }
            catch (Exception e)
            {
                Error(e, $"Exception thrown retrieving item with id {itemId} from container {containerId} in CosmosDB {DatabaseId}.");
                return default;
            }
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string containerId, string partitionKey, IEnumerable<string> itemIds) where T : IItem
        {
            ThrowIfNotInitialized();
            string ids = itemIds.Select(s => "'" + s + "'").Aggregate((i1, i2) => $"{i1},{i2}");
            QueryDefinition query = new QueryDefinition($"select * from c where c.id in ({ids})");
            try
            {
                var container = Client.GetContainer(DatabaseId, containerId);
                var r = container.GetItemQueryIterator<T>(query,
                    requestOptions: new QueryRequestOptions()
                    {
                        PartitionKey = new PartitionKey(partitionKey),
                        MaxItemCount = 100
                    });
                var items = new List<T>();
                while (r.HasMoreResults)
                {
                    var itemr = await r.ReadNextAsync(CancellationToken);
                    items.AddRange(itemr.Resource);
                }
                return items;
            }
            catch (Exception e)
            {
                Error(e, $"Exception thrown retrieving items with id {ids} from container {containerId} in CosmosDB {DatabaseId}.");
                return default;
            }
        }

        public async Task<bool> CreateAsync<T>(string containerId, string partitionKey, T item) where T : IItem
        {
            ThrowIfNotInitialized();
            try
            {
                var container = Client.GetContainer(DatabaseId, containerId);
                await container.CreateItemAsync(item, partitionKey: new PartitionKey(partitionKey), cancellationToken: CancellationToken);
                return true;
            }
            catch (Exception e)
            {
                var _item = (IItem) item;
                Error(e, $"Exception thrown creating item with id {_item.Id} and source {_item.Source} and date {_item.Date} in container {containerId} in CosmosDB {DatabaseId} .");
                return false;
            }
        }

        public string EndpointUrl { get; }

        public string AuthKey { get; }

        public string DatabaseId { get; }

        public static CosmosClient Client { get; protected set; }
    }
}
