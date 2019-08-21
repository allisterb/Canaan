using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan.Tests
{
    public abstract class BaseTests
    {
        static BaseTests()
        {
            Api.SetDefaultLoggerIfNone();
        }
    }
}
