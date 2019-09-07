using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using CsQuery;

namespace Canaan
{
    public class WebScraper : Api
    {
        static WebScraper() {}

        public static Dictionary<string, string[]> ContentSelectors { get; } = new Dictionary<string, string[]>();

        public static Dictionary<string, Dictionary<string, Func<Article, Article>>> ContentTransforms { get; } =
            new Dictionary<string, Dictionary<string, Func<Article, Article>>>();

        public static List<Article> GetArticlesFullTextFromUrl(List<Article> articles)
        {
      
            Parallel.For(0, articles.Count, (i) =>
            {
                Uri u = articles[i].Url;
                if (!ContentSelectors.ContainsKey(u.Host))
                {
                    Error("No content selectors present for article with url {0}.", u.ToString());
                    return;
                }
                string c = HttpClient.GetStringAsync(u).Result;
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

        public static Link[] ExtractLinksFromHtmlFrag(string html)
        {
            CQ dom = html;
            var links = dom["a"];
            if (links != null && links.Count() > 0)
            {
                return links.Select(l => new Link()
                {
                    Uri = l.HasAttribute("href") && Uri.TryCreate(l.GetAttribute("href"), UriKind.RelativeOrAbsolute, out Uri u) ? u : null,
                    HtmlAttributes = l.Attributes.ToDictionary(a => a.Key, a => a.Value),
                    InnerHtml = l.InnerHTML,
                    InnerText = l.InnerText
                }).ToArray();

            }
            else
            {
                return Array.Empty<Link>();
            }
        }

        public static string ExtractTextFromHtmlFrag(string html)
        {
            CQ dom = html;
            return dom.Text();
        }

        public static void LoadMercuryContentSelectors(string path)
        {
            var o = JObject.Parse(File.ReadAllText(path))
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

        
        private static void CreateContentTransforms()
        {
            //ContentTransforms.Add()
        }

         
    }
}
