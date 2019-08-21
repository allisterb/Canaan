using System;
using Xunit;

using Canaan.Aggregators;

namespace Canaan.Tests
{
    public class NewsAPITests : BaseTests
    {
        public NewsAPITests() : base() {}
        [Fact]
        public void CanConstructClient()
        {
            NewsAPI c = new NewsAPI();
            Assert.NotNull(c);
            
        }
    }
}
