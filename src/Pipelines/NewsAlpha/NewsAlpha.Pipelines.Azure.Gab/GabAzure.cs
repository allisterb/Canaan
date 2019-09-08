using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MoreLinq;
using Canaan;

namespace NewsAlpha
{
    public class GabAzurePipeline : Api
    {
        #region Constructors
        public GabAzurePipeline() : base()
        {
            Aggregator = new Gab();
            Db = new CosmosDB("socialnews");
            Initialized = Aggregator.Initialized && Db.Initialized;
        }
        #endregion

        #region Properties
        Gab Aggregator { get; }

        CosmosDB Db { get; }
        #endregion

        #region Methods
        public async Task Update()
        {
            ThrowIfNotInitialized();
            Info("Listening to Gab live stream for 100 seconds.");
            var posts = await Aggregator.GetUpdates(100);
        
            using (var op = Begin("Insert {0} posts into container {1} in database {2}", posts.Count(), "posts", "socialnews"))
            {
                foreach (var b in posts.Batch(4))
                {
                    await Task.WhenAll(b.Select(p => Db.CreateAsync("posts", "gab", p)));
                }
                op.Complete();
            }
        }
        #endregion
    }
}
