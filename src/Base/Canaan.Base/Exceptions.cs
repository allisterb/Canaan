using System;
using System.Collections.Generic;
using System.Text;

namespace Canaan
{
    public class AggregatorException : Exception
    {
        public string AggregatorName { get; }
        public AggregatorException(string aggregatorName, string message) : base(message)
        {
            AggregatorName = aggregatorName;
        }
    }
}
