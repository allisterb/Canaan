using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Azure.CognitiveServices.Search.NewsSearch;
using Microsoft.Azure.CognitiveServices.Search.NewsSearch.Models;

namespace Canaan
{
    public class BingNews : Api
    {
        #region Constructors
        public BingNews(string apiKey, CancellationToken ct) : base(ct)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException("apiKey");
            Client = new NewsSearchClient(new ApiKeyServiceClientCredentials(ApiKey));
            Initialized = true;
        }

        public BingNews(string apiKey) : this(apiKey, Api.Cts.Token) { }

        public BingNews() : this(Config("BingNews")) { }
        #endregion

        #region Properties
        public string ApiKey { get; }

        public NewsSearchClient Client { get; protected set; }
        #endregion

        #region Methods
        public async Task<List<Article>> SearchAsync(string query, string freshness = null, int count = 100, bool sortByDate = false)
        {
            ThrowIfNotInitialized();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var result = await Client.News.SearchAsync(query: query, freshness: freshness, 
                sortBy: sortByDate ? "Date" : null, count: count, originalImage: true, cancellationToken: CancellationToken);
            sw.Stop();
            if (sw.ElapsedMilliseconds < 330)
            {
                Thread.Sleep(Convert.ToInt32(330 - sw.ElapsedMilliseconds));
            }
            var articles = new List<Article>();
            articles.AddRange(GetArticlesFromResult(result));
            if (count > 100 && result.TotalEstimatedMatches.HasValue && result.TotalEstimatedMatches.Value >= count)
            {
                int pages = checked ((count - 1) / 100 + 1);
                int page = 1;
                while (++page <= pages)
                {
                    sw.Restart();
                    var r = await Client.News.SearchAsync(query: query, freshness: freshness,
                        sortBy: sortByDate ? "Date" : null, count: 100, offset: (page - 1) * 100,  
                        cancellationToken: CancellationToken);
                    sw.Stop();
                    if (sw.ElapsedMilliseconds < 330)
                    {
                        Thread.Sleep(Convert.ToInt32(330 - sw.ElapsedMilliseconds));
                    }
                    articles.AddRange(GetArticlesFromResult(r));
                }
            }
            
            var unique_articles =
                (from a in articles
                 group a by a.Id into g
                 select g.First()).ToList();
                
            unique_articles.ForEach(a => a.Topics.Add(query));
            return unique_articles;
        }

        protected IEnumerable<Article> GetArticlesFromResult(News result)
        {
            ThrowIfNotInitialized();
            return result.Value.Select((r, index) => new Article()
            {
                Id = CalculateMD5Hash(r.Url),
                Position = index,
                Aggregator = "BingNews",
                Category = r.Category,
                Title = r.Name,
                Url = new Uri(r.Url),
                DatePublished = DateTime.Parse(r.DatePublished),
                Description = r.Description,
                Source = r.Provider.First().Name,
                ImageUrl = r.Image?.ContentUrl,
            });
        }
        #endregion
    }
}
