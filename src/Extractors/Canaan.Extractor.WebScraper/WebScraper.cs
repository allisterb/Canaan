using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;

namespace Canaan.Extractors
{
    public class WebScraper : Api
    {
        static WebScraper()
        {
            ParseMercuryContentSelectors();
        }

        public static Dictionary<string, string[]> ContentSelectors = new Dictionary<string, string[]>();

        public static List<Article> GetArticlesFullTextFromUrl(List<Article> articles)
        {
            HttpClient client = new HttpClient();
            Parallel.For(0, articles.Count, (i) =>
            {
                Uri u = articles[i].Uri;
                if (!ContentSelectors.ContainsKey(u.Host))
                {
                    Error("No content selectors present for article with url {0}.", u.ToString());
                    return;
                }
                string c = client.GetStringAsync(u).Result;
                var html = new HtmlDocument();
                html.LoadHtml(c);
                var selectors = ContentSelectors[u.Host];
                var content = html.DocumentNode.QuerySelector(selectors.Last());
                if (content == null)
                {
                    Error("Content selectors returned null for article with url {0}.", u.ToString());
                    return;
                }
                articles[i].FullText = content.InnerText;
            });
            return articles;
        }
        private static void ParseMercuryContentSelectors()
        {
            var o = JObject.Parse(File.ReadAllText("mercury-content-selectors.json"))
                .Properties()
                .Select(p => p.Value as JObject);
            foreach (dynamic p in o)
            {
                JArray selectors = p.content.selectors;
                List<string> _selectors = new List<string>();
                foreach (var s in selectors)
                {
                    if (s.Type == JTokenType.String)
                    {
                        _selectors.Add((string)s);
                    }
                    else if (s.Type == JTokenType.Array)
                    {
                        foreach (var _s in (JArray)s)
                        {
                            _selectors.Add((string)_s);
                        }
                    }
                }
                string domain = p.domain;
                ContentSelectors.Add(domain, _selectors.ToArray());
                foreach (string d in p.supportedDomains)
                {
                    if (domain != d)
                    {
                        ContentSelectors.Add(d, _selectors.ToArray());
                    }
                }
            }
        }

         
    }
}
