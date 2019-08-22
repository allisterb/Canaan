using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Search.NewsSearch;

namespace Canaan.Aggregators
{
    public class BingNews : Api
    {
        public BingNews(string apiKey, CancellationToken ct) : base(ct)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException("apiKey");
            Client = new NewsSearchClient(new ApiKeyServiceClientCredentials(ApiKey));
            Initialized = true;
        }

        public BingNews(string apiKey) : this(apiKey, Api.Cts.Token) { }

        public BingNews() : this(Api.Config("BingNews")) { }

        public string ApiKey { get; }

        public NewsSearchClient Client { get; protected set; }

        public async Task<List<Article>> SearchAsync(string query, int count = 100)
        { 
            var results = await Client.News.SearchAsync(query: query, cancellationToken: CancellationToken);
            return results.Value.Select(r => new Article()
            {
                Id = r.Id,
                Category = r.Category,

            }).ToList();
        }
    }
}
