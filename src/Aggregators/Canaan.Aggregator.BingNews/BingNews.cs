using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Search.NewsSearch;

using Canaan.Extractors;
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

        public BingNews() : this(Config("BingNews")) { }

        public string ApiKey { get; }

        public NewsSearchClient Client { get; protected set; }

        public async Task<List<Article>> SearchAsync(string query, int count = 100)
        { 
            var results = await Client.News.SearchAsync(query: query, count: count, cancellationToken: CancellationToken);
            var articles = results.Value.Select(r => new Article()
            {
                Aggregator = "BingNews",
                Id = r.Id,
                Category = r.Category,
                Title = r.Name,
                Uri = new Uri(r.Url),
                DatePublished = DateTime.Parse(r.DatePublished),
                Description = r.Description,
                Source = r.Provider.First().Name

            }).ToList();
            return WebScraper.GetArticlesFullTextFromUrl(articles);
        }
    }
}
