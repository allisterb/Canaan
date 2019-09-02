using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using TThread = System.Threading.Thread;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Search.NewsSearch;
using Microsoft.Azure.CognitiveServices.Search.NewsSearch.Models;

namespace Canaan
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
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = await Client.News.SearchAsync(query: query, count: count, originalImage: true, cancellationToken: CancellationToken);
            sw.Stop();
            if (sw.ElapsedMilliseconds < 330)
            {
                TThread.Sleep(Convert.ToInt32(330 - sw.ElapsedMilliseconds));
            }
            var articles = new List<Article>();
            if (result.Value.Count == 0)
            {
                Error("Did not find any articles for query {0}.", query);
                return articles;
            }
            articles.AddRange(GetArticlesFromResult(result));
            if (count > 100 && result.TotalEstimatedMatches.HasValue && result.TotalEstimatedMatches.Value >= count)
            {
                int pages = checked ((count - 1) / 100 + 1);
                int page = 1;
                while (++page <= pages)
                {
                    sw.Restart();
                    var r = await Client.News.SearchAsync(query: query, count: count, offset: (page - 1) * 100,  
                        cancellationToken: CancellationToken);
                    sw.Stop();
                    if (sw.ElapsedMilliseconds < 330)
                    {
                        TThread.Sleep(Convert.ToInt32(330 - sw.ElapsedMilliseconds));
                    }
                    articles.AddRange(GetArticlesFromResult(r));
                }
            }
            return WebScraper.GetArticlesFullTextFromUrl(articles);
        }

        protected IEnumerable<Article> GetArticlesFromResult(News result)
        {
            return result.Value.Select((r, index) => new Article()
            {
                Position = index,
                Aggregator = "BingNews",
                Id = r.Id,
                Category = r.Category,
                Title = r.Name,
                Uri = new Uri(r.Url),
                DatePublished = DateTime.Parse(r.DatePublished),
                Description = r.Description,
                Source = r.Provider.First().Name

            });
        }

    }
}
