using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan.Tests
{
    public abstract class BaseTest
    {
        public BaseTest()
        {
            Api.SetDefaultLoggerIfNone();
        }
    }
}
