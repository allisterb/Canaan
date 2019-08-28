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
    public class NewsApi : Api
    {
        public NewsApi(string apiKey, CancellationToken ct) : base(ct)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException("apiKey");
            Client = new global::NewsAPI.NewsApiClient(apiKey);
            Initialized = true;
        }

        public NewsApi(string apiKey) : this(apiKey, Api.Cts.Token) { }

        public NewsApi() : this(Api.Config("NewsAPI")) { }

        public string ApiKey { get; }

        public global::NewsAPI.NewsApiClient Client { get; }

        public async Task GetTopHeadlines(string keyword, string category = "", string language = "EN")
        {
            var c = (Categories)Enum.Parse(typeof(Categories), category);

            var l = (Languages)Enum.Parse(typeof(Languages), language);
            var result = await Client.GetTopHeadlinesAsync(new TopHeadlinesRequest() { Category = c, Q = keyword, Language = l, PageSize = 100 });
            CancellationToken.ThrowIfCancellationRequested();
        }

        public async Task<List<Article>> SearchAsync(string query, DateTime? from, DateTime? to, int count = 100, string lang = "EN")
        {
            EverythingRequest req = new EverythingRequest()
            {
                Q = query,
                From = from,
                To = to,
                Language = (Languages)Enum.Parse(typeof(Languages), lang),
                PageSize = 100
            };
            var result = await Client.GetEverythingAsync(req);
            if (result.Status != Statuses.Ok)
            {
                throw new Exception();
            }
            List<Article> articles = new List<Article>();
            articles.AddRange(GetArticlesFromResult(result));
            if (count > 100 && result.TotalResults > 100)
            {
                int pages = result.TotalResults / 100 + 1;
                while (++req.Page <= pages)
                {
                    var r = await Client.GetEverythingAsync(req);
                    if (r.Status != Statuses.Ok)
                    {
                        Error("NewsAPI returned status {0} for page {1} of {2}.", r.Status, req.Page, pages);
                        continue;
                    }
                    articles.AddRange(GetArticlesFromResult(r));
                }
            }
            return articles;
        }

        protected IEnumerable<Article> GetArticlesFromResult(ArticlesResult result)
        {
            return result.Articles.Select((article, i) => new Article()
            {
                Position = i,
                Aggregator = "NewsAPI",
                Title = article.Title,
                DatePublished = article.PublishedAt,
                Description = article.Description,
                Author = article.Author,
                Source = article.Source.Name,
                Uri = new Uri(article.Url),
            });
        }
    }
}
