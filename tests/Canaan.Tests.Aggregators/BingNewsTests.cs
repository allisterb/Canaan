using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

using Canaan.Aggregators;

namespace Canaan.Tests
{
    public class BingNewsTests : BaseTests
    {
      [Fact]
      public void CanConstructClient()
        {
            BingNews b = new BingNews();
            Assert.NotNull(b);
            Assert.True(b.Initialized);
            var q = b.SearchAsync("Donald Trump").Result;
            Assert.True(q);
        }
    }
}
