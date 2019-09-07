using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using Microsoft.Azure.Cosmos;

namespace Canaan
{
    public class CosmosDB : Api
    {
        #region Constructors
        public CosmosDB(string endpointUrl, string authKey, string databaseId, CancellationToken ct) : base(ct)
        {
            EndpointUrl = endpointUrl;
            AuthKey = authKey;
            Client = new CosmosClient(EndpointUrl, AuthKey,
                new CosmosClientOptions()
                {
                    ApplicationName = "Canaan",
                    ConnectionMode = ConnectionMode.Direct,
                    MaxRetryAttemptsOnRateLimitedRequests = 30,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30),
                    ConsistencyLevel = ConsistencyLevel.Session});
            DatabaseId = databaseId;
            Initialized = true;
        }
        
        public CosmosDB(string databaseId, CancellationToken ct) : this(Config("CosmosDB:EndpointUrl"), Config("CosmosDB:AuthKey"), databaseId, ct) {}
        
        public CosmosDB(string databaseId) : this(databaseId, Cts.Token) {}
        #endregion

        #region Properties
        public string EndpointUrl { get; }

        public string AuthKey { get; }

        public string DatabaseId { get; }

        public CosmosClient Client { get; protected set; }
        #endregion

        #region Methods
        public async Task<T> GetAsync<T>(string containerId, string partitionKey, string itemId) 
        {
            ThrowIfNotInitialized();
            var container = Client.GetContainer(DatabaseId, containerId);
            var r =  await container.ReadItemAsync<T>(itemId, new PartitionKey(partitionKey), cancellationToken: CancellationToken);
            return r.Resource;            
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string containerId, string partitionKey, string query, Dictionary<string, object> parameters)
        {
            ThrowIfNotInitialized();
            var container = Client.GetContainer(DatabaseId, containerId);
            QueryDefinition q = new QueryDefinition(query);
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    q = q.WithParameter(parameters.Keys.ElementAt(i), parameters.Values.ElementAt(i));
                }
            }
            var r = container.GetItemQueryIterator<T>(q,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });
            var items = new List<T>();
            while (r.HasMoreResults)
            {
                var itemr = await r.ReadNextAsync(CancellationToken);
                items.AddRange(itemr.Resource);
            }
            return items;
        }

        public async Task<IEnumerable<T>> GetAsync<T>(string containerId, string partitionKey, IEnumerable<string> itemIds)
        {
            ThrowIfNotInitialized();
            string ids = itemIds.Select(s => "'" + s + "'").Aggregate((i1, i2) => $"{i1},{i2}");
            string query = $"select * from c where c.id in ({ids})";
            return await GetAsync<T>(containerId, partitionKey, query, null);
        }

        public async Task<T> GetScalarAsync<T>(string containerId, string partitionKey, string query, Dictionary<string, object> parameters)
        {
            ThrowIfNotInitialized();
            var container = Client.GetContainer(DatabaseId, containerId);
            QueryDefinition q = new QueryDefinition(query);
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    q = q.WithParameter(parameters.Keys.ElementAt(i), parameters.Values.ElementAt(i));
                }
            }
            var r = container.GetItemQueryIterator<T>(q,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(partitionKey)
                });
            /*
            var resultr = await r.ReadNextAsync();
            using (var s = new StreamReader(resultr.Content))
            {
                return await s.ReadToEndAsync();
            }
            */
            var result = await r.ReadNextAsync();
            return result.Resource.Single();
        }
        public async Task CreateAsync<T>(string containerId, string partitionKey, T item) where T : IItem
        {
            ThrowIfNotInitialized();            
            var container = Client.GetContainer(DatabaseId, containerId);
            await container.CreateItemAsync(item, partitionKey: new PartitionKey(partitionKey), cancellationToken: CancellationToken);
        }

        public async Task UpsertAsync<T>(string containerId, string partitionKey, T item) where T : IItem
        {
            ThrowIfNotInitialized();
            var container = Client.GetContainer(DatabaseId, containerId);
            await container.UpsertItemAsync(item, partitionKey: new PartitionKey(partitionKey), cancellationToken: CancellationToken);
        }
        #endregion
    }
}
