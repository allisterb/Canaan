using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Canaan
{
    public class NewsThread : INewsItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("iid")]
        public string IId => Source + "-" + Id;

        [JsonProperty("no")]
        public long No { get; set; }

        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("subject")]
        public string Subject { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("links")]
        public Link[] Links { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("date_published")]
        public DateTime DatePublished { get; set; }

        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }

        [JsonProperty("reply_count")]
        public int ReplyCount { get; set; }

        [JsonIgnore]
        public DateTime Date => DatePublished;

        [JsonIgnore]
        public string AuthorOrUser => User;
    }
}
