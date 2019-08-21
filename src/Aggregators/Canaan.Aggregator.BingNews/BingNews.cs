using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Search.NewsSearch;
using Microsoft.Azure.CognitiveServices.Search.NewsSearch.Models;

namespace Canaan.Aggregators
{
    public class BingNews : Api
    {
        public BingNews(string apiKey, CancellationToken ct) : base(ct)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException("apiKey");
            Client = new NewsSearchClient(new ApiKeyServiceClientCredentials(ApiKey));
            //Client.Endpoint = "https://newsalpha.cognitiveservices.azure.com/bing/v7.0";
            Initialized = true;
        }

        public BingNews(string apiKey) : this(apiKey, Api.Cts.Token) { }

        public BingNews() : this(Api.Config("BingNews")) { }

        public string ApiKey { get; }

        public NewsSearchClient Client { get; protected set; }

        public async Task<bool> SearchAsync(string query)
        {
            var results = await Client.News.SearchAsync(query: query, cancellationToken: CancellationToken);
            return true;
        }
    }
}
