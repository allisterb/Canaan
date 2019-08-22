using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Canaan.Extractors
{
    public class WebScraper
    {
        public static void Parse()
        {
            var j = JObject.Parse(File.ReadAllText("mercury-content-selectors.json"));
        }

    }
}
