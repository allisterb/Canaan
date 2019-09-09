using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using MoreLinq;

using Canaan;

namespace NewsAlpha
{
    public class BingNewsAzure : Api
    {
        public BingNewsAzure(string topic)
        {
            Aggregator = new BingNews();
            Db = new CosmosDB("tradnews");
        }

        public BingNewsAzure(string topic, string dbEndpointUrl, string dbAuthKey, string searchApiKey)
        {
            Aggregator = new BingNews(searchApiKey);
            Db = new CosmosDB(dbEndpointUrl, dbAuthKey, "articles", CancellationToken);
        }

        public BingNews Aggregator { get; }

        public CosmosDB Db { get; }

        public async Task InsertArticlesForTopic(string topic, int year, int count)
        {
            var _year = year.ToString();
            Info("Searching Bing News aggregator for topic {0}...", topic);
            var articles = await Aggregator.SearchAsync(topic, count: count);
            Info("Got {0} articles from aggregator BingNews for topic {1}.", articles.Count, topic);
            
            using (var op = Begin("Insert {0} articles into container {1} in database {2}.", articles.Count, "articles", Db.DatabaseId))
            {
                var dbArticles = await Db.GetAsync<ItemId>("articles", _year, "SELECT c.id FROM c WHERE ARRAY_CONTAINS(c.topics, @topic)",
                        new Dictionary<string, object> { { "@topic", topic } });
                Info("{0} articles already exist in container.", articles.RemoveAll(a => dbArticles.Any(dba => dba.Id == a.Id)));

                var articlesToInsert = await AnalyzeArticleText(articles);
                foreach (var b in articlesToInsert.Batch(8))
                {
                    await Task.WhenAll(b.Select(a => Db.CreateAsync("articles", _year, a)));
                }
                await Db.UpsertAsync<Topic>("topics", _year, new Topic { Name = topic, Year = _year });
                op.Complete();
                
            }
        }

        public async Task AnalyzeArticleImages(IEnumerable<Article> articles)
        {
            AzureComputerVision cv = new AzureComputerVision();
            
                byte[] imageData = await WebScraper.GetImageFromUrlAsync("https://static.politico.com/36/0a/aff08ea841e5aa76edddb4bc50d7/190909-donald-trump-gty-773.jpg");
                //if (imageData == null) continue;
                await cv.Analyze(imageData);
            

            

        }

        public async Task<IEnumerable<Article>> AnalyzeArticleText(IEnumerable<Article> articles)
        {
            AzureTextAnalytics ta = new AzureTextAnalytics();
            foreach(var batch in articles.Batch(1000))
            {
                var text = batch.ToDictionary(b => b.Id, b => b.Title);
                var result = await ta.AnalyzeSentiment(text);
                foreach(var article in batch)
                {
                    article.TitleSentiment = result[article.Id];
                }
            }
            return articles;
        }
    }
}
