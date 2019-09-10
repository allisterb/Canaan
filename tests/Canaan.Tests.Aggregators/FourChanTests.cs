using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;

using Xunit;

namespace Canaan.Tests
{
    public class FourChanTests : BaseTests
    {
        protected FourChan aggregator;

        public FourChanTests()
        {
            aggregator = new FourChan();
        }

        [Fact]
        public void CanGetThreads()
        {
            var threads = aggregator.GetThreads("pol").Result;
            Assert.NotEmpty(threads);
            var posts = aggregator.GetPosts("pol", threads).Result;
            Assert.NotEmpty(posts);
        }

        [Fact]
        public void CanGetAdditionalProperties()
        {
            var t = aggregator.GetThread("pol", "224988448").Result;
            Assert.NotNull(t.Item1);
        }

        [Fact]
        public void CanGetIntents()
        {

            var t = aggregator.GetThread("pol", "226020907").Result;
            var post = t.Item2.First();
        }

    }
}
