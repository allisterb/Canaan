using System;
using System.Collections.Generic;
using System.Text;

using Xunit;

namespace Canaan.Tests
{
    public class FourChanTests
    {
        [Fact]
        public void CanGetBoardThreads()
        {
            var threads = FourChan.GetThreads("pol").Result;
            Assert.NotEmpty(threads);
            var posts = FourChan.GetPosts("pol", threads).Result;
            Assert.NotEmpty(posts);
        }

    }
}
