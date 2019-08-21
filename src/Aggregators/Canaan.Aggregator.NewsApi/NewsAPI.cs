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
    public class NewsAPI : Api
    {
        public NewsAPI(string apiKey, CancellationToken ct) : base(ct)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException("apiKey");
            Client = new global::NewsAPI.NewsApiClient(apiKey);
            Initialized = true;
        }

        public NewsAPI(string apiKey) : this(apiKey, Api.Cts.Token) { }

        public NewsAPI() : this(Api.Config("NewsAPI")) {}

        public string ApiKey { get; }

        public global::NewsAPI.NewsApiClient Client { get; }

        public async Task GetTopHeadlines(string category, string keyword, string language = "EN")
        {
            var c = (Categories) Enum.Parse(typeof(Categories), category);
            
            var l = (Languages)Enum.Parse(typeof(Languages), language);
            var result = await Client.GetTopHeadlinesAsync(new TopHeadlinesRequest() { Category = c,  Q = keyword, Language = l})  ;
            CancellationToken.ThrowIfCancellationRequested();
        }       
    }
}
