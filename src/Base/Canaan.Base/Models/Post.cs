using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Canaan
{
    public class Post : IItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("pid")]
        public string Pid => Id;

        [JsonProperty("tid")]
        public string ThreadId { get; set; }

        [JsonProperty("pos")]
        public int Position { get; set; }

        [JsonProperty("no")]
        public long No { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("user")]
        public string User { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("date_published")]
        public DateTime DatePublished { get; set; }

        [JsonProperty("replies")]
        public string[] Replies { get; set;  }

        [JsonProperty("reply_to")]
        public string ReplyTo { get; set; }

        [JsonProperty("links")]
        public Link[] Links { get; set; }

        [JsonProperty("additional")]
        public Dictionary<string, object> Additional { get; set; }

        [JsonIgnore]
        public DateTime Date => DatePublished;
    }
}
