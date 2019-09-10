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

        [JsonProperty("iid")]
        public string IId => Source + "-" + Id;

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
        public List<string> Replies { get; set; } = new List<string>();

        [JsonProperty("replyto")]
        public List<string> ReplyTo { get; set; } = new List<string>();

        [JsonProperty("links")]
        public Link[] Links { get; set; }

        [JsonProperty("additional")]
        public Dictionary<string, object> Additional { get; set; }

        [JsonProperty("entities")]
        public List<string> Entities { get; set; }

        [JsonProperty("threat_intent")]
        public double ThreatIntent { get; set; }

        [JsonIgnore]
        public DateTime Date => DatePublished;

        [JsonProperty("identity_hate")]
        public bool HasIdentityHate { get; set; }
    }
}
