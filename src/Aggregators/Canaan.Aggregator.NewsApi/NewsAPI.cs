using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using NewsAPI.Constants;
using NewsAPI.Models;

namespace Canaan
{
    public class NewsApi : Api
    {
        #region Constructors
        public NewsApi(string apiKey, CancellationToken ct) : base(ct)
        {
            ApiKey = apiKey ?? throw new ArgumentNullException("apiKey");
            Client = new global::NewsAPI.NewsApiClient(apiKey);
            Initialized = true;
        }

        public NewsApi(string apiKey) : this(apiKey, Api.Cts.Token) { }

        public NewsApi() : this(Api.Config("NewsAPI")) { }
        #endregion

        #region Properties
        public string ApiKey { get; }

        public global::NewsAPI.NewsApiClient Client { get; }
        #endregion

        public async Task GetTopHeadlines(string keyword, string category = "", string language = "EN")
        {
            var c = (Categories)Enum.Parse(typeof(Categories), category);

            var l = (Languages)Enum.Parse(typeof(Languages), language);
            var result = await Client.GetTopHeadlinesAsync(new TopHeadlinesRequest() { Category = c, Q = keyword, Language = l, PageSize = 100 });
            CancellationToken.ThrowIfCancellationRequested();
        }

        public async Task<List<Article>> SearchAsync(string query, DateTime? from, DateTime? to, string lang = "EN", params string[] sources)
        {
            int count = 100; //limited for for free API
            EverythingRequest req = new EverythingRequest()
            {
                Q = query,
                From = from,
                To = to,
                Language = (Languages)Enum.Parse(typeof(Languages), lang),
                PageSize = 100,
                Sources = sources.ToList()
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
                DatePublished = article.PublishedAt.GetValueOrDefault(),
                Description = article.Description,
                Author = article.Author,
                Source = article.Source.Name,
                Url = new Uri(article.Url),
            });
        }

        public partial class NewsSources
        {
            [JsonProperty("sources")]
            public Source[] Sources { get; set; }
        }

        public partial class Source
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("url")]
            public Uri Url { get; set; }

            [JsonProperty("category")]
            public Category Category { get; set; }

            [JsonProperty("language")]
            public Language Language { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }
        }

        public enum Category { General };

        public enum Language { En };

        public partial class NewsSources
        {
            public static NewsSources FromJson(string json) => JsonConvert.DeserializeObject<NewsSources>(json, Converter.Settings);
        }

        /*
        public static class Serialize
        {
            public static string ToJson(this NewsSources self) => JsonConvert.SerializeObject(self, Converter.Settings);
        }
        */

        internal static class Converter
        {
            public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                DateParseHandling = DateParseHandling.None,
                Converters =
            {
                CategoryConverter.Singleton,
                LanguageConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
            };
        }

        internal class CategoryConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(Category) || t == typeof(Category?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "general")
                {
                    return Category.General;
                }
                throw new Exception("Cannot unmarshal type Category");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (Category)untypedValue;
                if (value == Category.General)
                {
                    serializer.Serialize(writer, "general");
                    return;
                }
                throw new Exception("Cannot marshal type Category");
            }

            public static readonly CategoryConverter Singleton = new CategoryConverter();
        }

        internal class LanguageConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(Language) || t == typeof(Language?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Null) return null;
                var value = serializer.Deserialize<string>(reader);
                if (value == "en")
                {
                    return Language.En;
                }
                throw new Exception("Cannot unmarshal type Language");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                if (untypedValue == null)
                {
                    serializer.Serialize(writer, null);
                    return;
                }
                var value = (Language)untypedValue;
                if (value == Language.En)
                {
                    serializer.Serialize(writer, "en");
                    return;
                }
                throw new Exception("Cannot marshal type Language");
            }

            public static readonly LanguageConverter Singleton = new LanguageConverter();
        }

    }
}
