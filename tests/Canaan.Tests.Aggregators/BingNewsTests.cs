using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Canaan.Aggregators;

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
    }
}
