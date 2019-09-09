using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Canaan
{
    public class Article : IItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("iid")]
        public string IId => Id + "-" + Year;

        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("topics")]
        public List<string> Topics { get; set; } = new List<string>();

        [JsonProperty("desc")]
        public string Description { get; set; }

        [JsonProperty("date_published")]
        public DateTime DatePublished { get; set; }

        [JsonProperty("year")]
        public string Year => DateTime.Now.Year.ToString();

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("agg")]
        public string Aggregator { get; set; }

        [JsonProperty("fulltext")]
        public string FullText { get; set; }

        [JsonProperty("lede")]
        public string Lede { get; set; }

        [JsonProperty("wordcount")]
        public int? WordCount { get; set; }

        [JsonProperty("image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty("title_sentiment")]
        public double? TitleSentiment { get; set; }

        [JsonIgnore]
        public DateTime Date => DatePublished;
    }
}
