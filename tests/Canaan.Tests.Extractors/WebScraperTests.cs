using System;
using System.Linq;
using System.Net;

using Xunit;

using HtmlAgilityPack;
using Fizzler.Systems.HtmlAgilityPack;
using Canaan.Extractors;

namespace Canaan.Tests
{
    public class WebScraperTests
    {
        [Fact]
        public void CanParseMercurySelectors()
        {
            Assert.NotEmpty(WebScraper.ContentSelectors);
        }

        [Fact]
        public void CanScrapeWebPage()
        {
            Assert.NotEmpty(WebScraper.ContentSelectors);
            WebClient client = new WebClient();
            Uri u = new Uri("https://www.cnn.com/2019/08/24/politics/trump-china-trade-war-emergency-economic-powers-act/index.html");
            string text = client.DownloadString("https://www.cnn.com/2019/08/24/politics/trump-china-trade-war-emergency-economic-powers-act/index.html");
            var html = new HtmlDocument();
            html.LoadHtml(text);

            Assert.True(WebScraper.ContentSelectors.ContainsKey(u.Host));
            var selectors = WebScraper.ContentSelectors[u.Host];
            var content = html.DocumentNode.QuerySelector(selectors.Last());
            Assert.NotNull(content);
        }
    }
}
