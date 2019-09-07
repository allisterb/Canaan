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
            Initialized = Aggregator.Initialized && Db.Initialized;
        }
        #endregion

        #region Properties
        FourChan Aggregator { get; } 

        CosmosDB Db { get; }
        #endregion

        #region Methods
        public async Task InitThreads(string board)
        {
            ThrowIfNotInitialized();
            var threads = await Aggregator.GetThreads(board);
            Info("Got {0} {1} threads from 4Chan API.", threads.Count(), board);
            using (var op = Begin("Create {0} new thread(s) in {1} container in database {2}", threads.Count(), "threads", "socialnews"))
            {
                foreach (var b in threads.Batch(8))
                {
                    await Task.WhenAll(b.Select(t => Db.CreateAsync("threads", t.Source, t)));
                }
                op.Complete();
            }
        }

        public async Task InitPosts(string board)
        {
            ThrowIfNotInitialized();
            var threads = await Db.GetAsync<NewsThread>("threads", "4ch." + board, "SELECT TOP 300 * FROM t", null);
            Info("Got {0} threads from {1} container in database {2}.", threads.Count(), "threads", "socialnews");
            var threadPosts = await Aggregator.GetPosts("pol", threads);
            Info("Got {0} {1} posts from {2} threads from 4Chan API.", threadPosts.Values.SelectMany(x => x).Count(), board, threads.Count());
            using (var op = Begin("Insert {0} {1} posts from {2} threads into container {3} in database {4}", 
                threadPosts.Values.SelectMany(x => x).Count(), board, threads.Count(), "posts", "socialnews"))
            {
                try
                {
                    foreach (var tp in threadPosts)
                    {
                        foreach (var b in tp.Value.Batch(8))
                        {
                            await Task.WhenAll(b.Select(p => Db.CreateAsync("posts", p.Source, p)));
                        }
                        await Db.UpsertAsync("threads", tp.Key.Source, tp.Key);
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

        public async Task Init(string board)
        {
            await InitPosts(board);
            await InitThreads(board);
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
                        using (var op2 = Begin("Insert {0} new post(s) for thread {1} into database {2}", posts.Count(), thread.Id, "socialnews"))
                        {
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
        public async Task Run()
        {
            var threads = await Aggregator.GetThreads("pol");
            //var dbThreads = await Db.GetAsync<NewsThread>("threads", "/pol/", threads.Se)
        }
        #endregion

    }
}
