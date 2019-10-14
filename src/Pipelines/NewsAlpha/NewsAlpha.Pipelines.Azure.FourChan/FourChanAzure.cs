using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using MoreLinq;

using Canaan;

namespace NewsAlpha
{
    public class FourChanAzurePipeline : Api
    {
        #region Constructors
        public FourChanAzurePipeline() : base()
        {
            Info("Initializing 4chan Azure pipeline.");
            Aggregator = new FourChan();
            Db = new CosmosDB("socialnews");
            NLU = new AzureLUIS();
            Initialized = Aggregator.Initialized && Db.Initialized && NLU.Initialized;
        }
        #endregion

        #region Properties
        FourChan Aggregator { get; } 

        CosmosDB Db { get; }

        AzureLUIS NLU { get; }
        #endregion

        #region Methods
        public async Task Update(string board)
        {
            ThrowIfNotInitialized();
            var threads = await Aggregator.GetThreads(board);
            var dbThreads = await Db.GetAsync<ItemId>("threads", "4ch." + board, "SELECT t.id FROM t", null);
            var newThreads = new List<NewsThread>();
            var updateThreads = new List<NewsThread>();
            foreach (var thread in threads)
            {
                var _dbt = dbThreads.SingleOrDefault(t => t.Id == thread.Id);
                if (_dbt == null)
                {
                    newThreads.Add(thread);
                }
                else
                {
                    updateThreads.Add(thread);   
                }
            }
            Info("Got {0} new {1} threads from 4Chan API.", newThreads.Count, board);
            Info("Got {0} {1} threads to update from 4Chan API.", updateThreads.Count, board);
            var newThreadPosts = await Aggregator.GetPosts(board, newThreads);
            using (var op = Begin("Create {0} new thread(s) with {1} new post(s) for board {2} in database {3}",
                    newThreadPosts.Keys.Count, newThreadPosts.Values.Flatten().Count(), board, "socialnews"))
            {
                try
                {
                    foreach (var tp in newThreadPosts)
                    {
                        var thread = tp.Key;
                        var posts = tp.Value;
                        await Db.CreateAsync("threads", thread.Source, thread);
                        if (!string.IsNullOrEmpty(Config("CognitiveServices:EnableNLU")))
                        {
                            using (var op2 = Begin("Get intents for {0} posts from Azure LUIS", posts.Count()))
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
                                op2.Complete();
                            }
                        }
                        foreach (var b in posts.Batch(2))
                        {
                            await Task.WhenAll(b.Select(p => Db.CreateAsync("posts", p.Source, p)));
                        }
                    }
                    op.Complete();
                }
                catch (Exception e)
                {
                    Error(e, "Update operation failed. Resetting thread reply count.");
                    await UpdateThreadReplyCount(board);
                    return;
                }
            }

            var updateThreadPosts = await Aggregator.GetPosts(board, updateThreads);
            using (var op = Begin("Update {0} thread(s) with {1} post(s) for board {2} in database {3}",
                    updateThreadPosts.Keys.Count, updateThreadPosts.Values.Flatten().Count(), board, "socialnews"))
            {
                try
                {
                    foreach (var tp in updateThreadPosts)
                    {
                        var thread = tp.Key;
                        var posts = tp.Value;
                        var dbPostIds = await Db.GetAsync<ItemId>("posts", "4ch." + board, "SELECT p.id from p WHERE p.tid = @tid",
                            new Dictionary<string, object> { { "@tid", thread.Id } });
                        var ids = dbPostIds.Select(i => i.Id).ToArray();
                        posts.RemoveAll(p => ids.Contains(p.Id));
                        if (!string.IsNullOrEmpty(Config("CognitiveServices:EnableNLU")))
                        {
                            using (var op2 = Begin("Get intents for {0} posts from Azure LUIS", posts.Count()))
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
                                op2.Complete();
                            }
                        }
                        using (var op2 = Begin("Insert {0} new post(s) for thread {1} into database {2}", posts.Count(), thread.Id, "socialnews"))
                        {
                            if (string.IsNullOrEmpty(Config("NLU:Enable")))
                            {
                                foreach (var post in posts)
                                {
                                    await NLU.GetPredictionForPost(post);
                                }
                            }
                            foreach (var b in posts.Batch(2))
                            {
                                await Task.WhenAll(b.Select(p => Db.CreateAsync("posts", "4ch." + board, p)));
                            }
                            thread.ReplyCount = dbPostIds.Count() + posts.Count();
                            await Db.UpsertAsync("threads", thread.Source, thread);
                            op2.Complete();
                        }
                    }
                    op.Complete();
                }
                catch (Exception e)
                {
                    Error(e, "Update operation failed. Resetting thread reply count.");
                    await UpdateThreadReplyCount(board);
                    return;
                }
            }
        }

        public async Task UpdateThreadReplyCount(string board)
        {
            ThrowIfNotInitialized();
            var dbThreads = await Db.GetAsync<NewsThread>("threads", "4ch." + board, "SELECT * FROM t", null);
            Info("Got {0} from container threads for board {1}.", dbThreads.Count(), board);
            foreach (var thread in dbThreads)
            {
                var count = await Db.GetScalarAsync<int>("posts", "4ch." + board,
                    "SELECT VALUE COUNT(1) FROM p where p.tid = @tid", new Dictionary<string, object>()
                    { {"@tid", thread.Id } });
                thread.ReplyCount = count - 1;
            }
            foreach (var t in dbThreads)
            {
                await Db.UpsertAsync("threads", t.Source, t);
            }
        }

        public async Task UpdateThreadReplyCount(string board, NewsThread dbThread)
        {
            ThrowIfNotInitialized();
            var count = await Db.GetScalarAsync<int>("posts", "4ch." + board,
                "SELECT VALUE COUNT(1) FROM p where p.tid = @tid", new Dictionary<string, object>()
                { {"@tid", dbThread.Id } });
            dbThread.ReplyCount = count - 1;
            await Db.UpsertAsync("threads", "4ch." + "pol", dbThread);
        }

        #endregion
    }
}
