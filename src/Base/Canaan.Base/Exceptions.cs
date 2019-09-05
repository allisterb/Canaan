using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public class ApiNotInitializedException : Exception
    {
        public ApiNotInitializedException(Api api) : base($"The {api.GetType().Name} Api is not initialized.") {}
    }

    public class AggregatorException : Exception
    {
        public string AggregatorName { get; }
        public AggregatorException(string aggregatorName, string message) : base(message)
        {
            AggregatorName = aggregatorName;
        }
    }
}
