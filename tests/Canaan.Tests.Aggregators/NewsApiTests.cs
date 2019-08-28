using System;
using System.Collections.Generic;

using Xunit;

using Canaan.Aggregators;

namespace Canaan.Tests
{
    public class NewsApiTests : BaseTests
    {
        public NewsApiTests() : base() { }
        [Fact]
        public void CanConstructClient()
        {
            var c = new Canaan.Aggregators.NewsApi();
            Assert.NotNull(c);
            c.GetTopHeadlines("Donald Trump", "Business").Wait();
        }

        [Fact]
        public void CanSearchAPI()
        {
            NewsApi agg = new NewsApi();
            Assert.NotNull(agg);
            var r = agg.SearchAsync("Donald Trump", null, null).Result;
            Assert.NotEmpty(r);
            var now = DateTime.Now;
            r = agg.SearchAsync("China", now - TimeSpan.FromMinutes(5), now).Result;
            Assert.NotEmpty(r);
        }

        [Fact]
        public void CanPageThroughResults()
        {
            NewsApi agg = new NewsApi();
            Assert.NotNull(agg);
            var now = DateTime.Now;
            var r = agg.SearchAsync("Donald Trump", now -TimeSpan.FromDays(30), now, 1000).Result;
        }
    }
}
