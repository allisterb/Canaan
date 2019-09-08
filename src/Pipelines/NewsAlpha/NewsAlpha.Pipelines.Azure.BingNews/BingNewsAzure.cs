using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Canaan;

namespace NewsAlpha
{
    public class BingNewsAzure : Api
    {
        public BingNewsAzure()
        {
            Aggregator = new BingNews();
            Db = new CosmosDB("articles");
        }
        public BingNews Aggregator { get; }

        public CosmosDB Db { get; }
    }
}
