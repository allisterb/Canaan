using System;
using Xunit;

namespace Canaan.Tests
{
    public class ApiTests : BaseTests
    {
        [Fact]
        public void CanConfigureApi()
        {
            Assert.NotNull(Api.Configuration);
        }

        [Fact]
        public void CanSetDefaultLogger()
        {
            Api.SetDefaultLoggerIfNone();
            Assert.NotNull(Api.Logger);
        }
    }
}
