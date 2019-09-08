using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;

using Xunit;

namespace Canaan.Tests
{
    public class RedditTests : BaseTests
    {
        protected Reddit aggregator;

        public RedditTests()
        {
            aggregator = new Reddit();
        }

        [Fact]
        public void CanGetThreads()
        {
            var threads = aggregator.GetThreads("The_Donald").Result;
            Assert.NotEmpty(threads);
    
        }

        

    }
}
