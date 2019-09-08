using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
namespace Canaan.Tests
{
    public class GabTests : BaseTests
    {
        public Gab Aggregator;
        public GabTests() : base()
        {
            Api.SetDefaultLoggerIfNone();
            Aggregator = new Gab();
        }

        [Fact]
        public void CanGetStream()
        {
            Aggregator.GetUpdates(30).Wait();
            Assert.True(true);
        }
    }
}
