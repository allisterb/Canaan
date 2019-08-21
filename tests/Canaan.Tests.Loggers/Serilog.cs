using System;
using Xunit;

namespace Canaan.Tests
{
    public class SerilogTests
    {
        public SerilogTests()
        {
            Api.SetDefaultLoggerIfNone();
        }

        [Fact]
        public void CanCreateSerilogLogger()
        {
            IO.DeleteFiles("*.log");
            Assert.Equal(0, IO.GetFiles("*.log").Item1);
            Api.SetLogger(new SerilogLogger("Canaan.Tests.Loggers.log"));
            Assert.NotNull(Api.Logger);
            Api.Info("Hello World");
            Assert.NotEqual(0, IO.GetFiles("*.log").Item1);

        }
    }
}
