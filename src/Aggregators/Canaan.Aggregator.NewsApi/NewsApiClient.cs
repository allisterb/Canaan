using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NewsAPI.Constants;
using NewsAPI.Models;

namespace Canaan.Aggregators
{
    public class NewsApiClient : Api
    {
        public NewsApiClient(string apiKey, CancellationToken ct) : base(ct)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException("apiKey");
            Client = new NewsAPI.NewsApiClient(apiKey);
            Initialized = true;
        }

        public NewsApiClient(string apiKey) : this(apiKey, Api.Cts.Token) { }

        public NewsApiClient() : this(Api.Config("NewsAPI")) {}

        public string ApiKey { get; }

        public NewsAPI.NewsApiClient Client { get; }

        public async Task GetTopHeadlines(string category, string keyword)
        {
            var c = (Categories) Enum.Parse(typeof(Categories), category);
            var result = await Client.GetTopHeadlinesAsync(new TopHeadlinesRequest() { Category = c,  Q = keyword})  ;
            CancellationToken.ThrowIfCancellationRequested();
        }       
    }
}
