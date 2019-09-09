using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Canaan
{
    public class Topic
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("year")]
        public string Year { get; set; }

        [JsonProperty("id")]
        public string Id => Name;
    }
}
