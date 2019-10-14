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
            NLU = new AzureLUIS();
            Initialized = Aggregator.Initialized && Db.Initialized;
        }
        #endregion

        #region Properties
        Gab Aggregator { get; }

        CosmosDB Db { get; }

        AzureLUIS NLU { get; }
        #endregion

        #region Methods
        public async Task Update()
        {
            ThrowIfNotInitialized();
            Info("Listening to Gab live stream for 100 seconds.");
            var posts = await Aggregator.GetUpdates(100);
            if (string.IsNullOrEmpty(Config("CognitiveServices:EnableNLU")))
            {
                using (var op = Begin("Get intents for {0} posts from Azure LUIS", posts.Count()))
                {
                    foreach (var post in posts)
                    {
                        await NLU.GetPredictionForPost(post);
                        if (post.Entities.Count > 0)
                        {
                            Info("Detected {0} entities in post {1}.", post.Entities.Count, post.Id);
                        }
                        if (post.ThreatIntent > 0.0)
                        {
                            Info("Detected threat intent {0:0.00} in post {1}.", post.ThreatIntent, post.Id);
                        }
                    }
                    op.Complete();
                }
            }

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
