using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;

namespace Canaan
{
    public class ItemId
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
