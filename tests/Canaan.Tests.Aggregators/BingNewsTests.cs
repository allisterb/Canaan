using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Canaan;

namespace Canaan.Tests
{
    public class BingNewsTests : BaseTests
    {
        public BingNewsTests() : base()
        {
            Api.SetLogger(new SerilogLogger());
        }

        [Fact]
        public void CanConstructClient()
        {
            BingNews b = new BingNews();
            Assert.NotNull(b);
            Assert.True(b.Initialized);
            var q = b.SearchAsync("Donald Trump").Result;
            Assert.NotEmpty(q);
        }

        [Fact]
        public void CanPageThroughResults()
        {
            var agg = new BingNews();
            var q = agg.SearchAsync("China", count: 361).Result;
            Assert.NotEmpty(q);
        }
    }
}
