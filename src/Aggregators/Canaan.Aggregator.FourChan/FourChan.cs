using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Canaan
{
    public class FourChan : Api
    {
        #region  Constructors
        public FourChan(CancellationToken ct) : base(ct) { Initialized = true; }
        
        public FourChan() : base() { Initialized = true; }
        #endregion

        #region Properties
        static Regex repliesRegex = new Regex(">>\\d+", RegexOptions.Compiled);
        #endregion

        #region Methods
        public async Task<IEnumerable<NewsThread>> GetThreads(string board)
        {
            using (var op = Begin("Get threads for board {0}", board))
            {
                var r = await HttpClient.GetAsync($"http://a.4cdn.org/{board}/threads.json", CancellationToken);
                r.EnsureSuccessStatusCode();
                var json = await r.Content.ReadAsStringAsync();
                var pages = JArray.Parse(json);
                var threads = new List<NewsThread>();
                foreach (dynamic page in pages)
                {
                    int p = 1;
                    foreach (dynamic thread in page.threads)
                    {
                        var t = new NewsThread()
                        {
                            Source = "4ch.pol",
                            No = thread.no,
                            Id = ((string)thread.no) + "-" + YY,
                            Position = p++,
                            LastModified = DateTimeOffset.FromUnixTimeSeconds((long)thread.last_modified).UtcDateTime,
                            ReplyCount = thread.replies
                        };
                        threads.Add(t);
                    }
                }
                op.Complete();
                return threads;
            }
        }

        public async Task<IDictionary<NewsThread, List<Post>>> GetPosts(string board, IEnumerable<NewsThread> threads, int delay = 500)
        {
            using (var op = Begin("Get posts for {0} threads for board {1}", threads.Count(), board))
            {
                Task<HttpResponseMessage>[] threadTasks =
                    threads.Select(t => HttpClient.GetAsync($"http://a.4cdn.org/{board}/thread/{t.No}.json", CancellationToken)).ToArray();
                try
                {
                    foreach (var task in threadTasks)
                    {
                        await task;
                        await Task.Delay(delay);
                    }
                }
                catch { }

                foreach (var task in threadTasks.Where(t => t.IsFaulted || t.IsCanceled))
                {
                    if (task.Exception != null)
                    {
                        Error(task.Exception, "Could not fetch thread.");
                    }
                    else
                    {
                        Error("Could not fetch thread. Thread task did not complete.");
                    }
                }
                var threadsResponseTask = threadTasks
                    .Where(t => t.IsCompleted)
                    .Select(t => t.Result)
                    .Where(r => r.StatusCode == HttpStatusCode.OK)
                    .Select(r => r.Content.ReadAsStringAsync());
                await Task.WhenAll(threadsResponseTask);
                var threadsJson = threadsResponseTask.Select(r => r.Result);

                var posts = new ConcurrentDictionary<NewsThread, List<Post>>();
                Parallel.ForEach(threadsJson, (j) =>
                {
                    dynamic o = JObject.Parse(j);
                    JArray threadPosts = o.posts;
                    dynamic subjectPost = threadPosts[0];
                    NewsThread thread = threads.Single(t => t.No == (long)subjectPost.no);
                    thread.Subject = subjectPost.sub;
                    thread.Text = WebScraper.ExtractTextFromHtmlFrag((string)subjectPost.com);
                    thread.User = subjectPost.name;
                    thread.ReplyCount = subjectPost.replies;
                    if (!posts.TryAdd(thread, new List<Post>()))
                    {
                        Error("Could not add thread {0} to posts collection.", thread.Id);
                    }
                    int pos = 1;
                    foreach (dynamic post in threadPosts)
                    {
                        posts[thread].Add(
                            new Post()
                            {
                                Source = $"4ch.{board}",
                                ThreadId = thread.Id,
                                No = post.no,
                                Id = post.no.ToString() + "-" + YY,
                                Position = pos++,
                                User = post.name,
                                DatePublished = DateTimeOffset.FromUnixTimeSeconds((long)post.time).UtcDateTime,
                                Text = WebScraper.ExtractTextFromHtmlFrag((string)post.com),
                                Links = WebScraper.ExtractLinksFromHtmlFrag((string)post.com),
                                Additional = GetAdditionalPropsForPost((JObject)post)
                            });
                    }
                });

                foreach (var tp in posts)
                {
                    foreach (var p in tp.Value)
                    {
                        var rnos = ExtractPostRepliesFromText(p);
                        foreach (var no in rnos)
                        {
                            p.ReplyTo.Add(no.ToString() + "-" + YY);
                            var replyto = tp.Value.SingleOrDefault(v => v.No == no);
                            if (replyto != null)
                            {
                                replyto.Replies.Add(p.Id);
                            }
                        }
                        p.Text = repliesRegex.Replace(p.Text, string.Empty).Replace(">", string.Empty);
                    }

                }
                op.Complete();
                return posts;
            }
        }

        public List<Post> ParsePostsFromThreadJson(string board, string json)
        {
            dynamic o = JObject.Parse(json);
            JArray threadPosts = o.posts;
            dynamic subjectPost = threadPosts[0];
            int pos = 1;
            List<Post> posts = new List<Post>();
            foreach (dynamic post in threadPosts)
            {
                posts.Add(
                    new Post()
                    {
                        Source = $"4ch.{board}",
                        No = post.no,
                        Id = post.no.ToString() + "-" + YY,
                        Position = pos++,
                        User = post.name,
                        DatePublished = DateTimeOffset.FromUnixTimeSeconds((long)post.time).UtcDateTime,
                        Text = WebScraper.ExtractTextFromHtmlFrag((string)post.com),
                        Links = WebScraper.ExtractLinksFromHtmlFrag((string)post.com),
                        Additional = GetAdditionalPropsForPost(post)
                    });
            }
            return posts;
        }

        public async Task<ValueTuple<NewsThread, List<Post>>> GetThread(string board, string threadno)
        {
            using (var op = Begin("Get thread no {0} for board {1}", board, threadno))
            {
                var r = await HttpClient.GetAsync($"http://a.4cdn.org/{board}/thread/{threadno}.json", CancellationToken);
                r.EnsureSuccessStatusCode();
                var json = await r.Content.ReadAsStringAsync();
                dynamic o = JObject.Parse(json);
                JArray threadPosts = o.posts;
                dynamic subjectPost = threadPosts[0];
                NewsThread thread = new NewsThread()
                {
                    Source = $"4ch.{board}",
                    No = subjectPost.no,
                    Id = subjectPost.no.ToString() + "-" + YY,
                    DatePublished = DateTimeOffset.FromUnixTimeSeconds((long)subjectPost.time).UtcDateTime,
                    Subject = subjectPost.sub,
                    Text = WebScraper.ExtractTextFromHtmlFrag((string)subjectPost.com),
                    User = subjectPost.name,
                    ReplyCount = subjectPost.replies
                };
                var posts = ParsePostsFromThreadJson(board, json);
                foreach (var p in posts)
                {
                    p.ThreadId = thread.Id;
                }
                foreach (var p in posts)
                {
                    var rnos = ExtractPostRepliesFromText(p);
                    foreach (var no in rnos)
                    {
                        p.ReplyTo.Add(no.ToString() + "-" + YY);
                        var replyto = posts.SingleOrDefault(v => v.No == no);
                        if (replyto != null)
                        {
                            replyto.Replies.Add(p.Id);
                        }
                    }

                    p.Text = repliesRegex.Replace(p.Text, string.Empty).Replace(">", string.Empty);
                }
                op.Complete();
                return (thread, posts);
            }
        }

        private Dictionary<string, object> GetAdditionalPropsForPost(JObject post)
        {
            Dictionary<string, object> props = new Dictionary<string, object>();
            if (post.TryGetValue("filename", out JToken filename))
            {
                props.Add("filename", (string) filename);
            }
            if (post.TryGetValue("ext", out JToken ext))
            {
                props.Add("ext", (string) ext);
            }
            if (post.TryGetValue("w", out JToken w))
            {
                props.Add("w", (int) w);
            }
            if (post.TryGetValue("h", out JToken h))
            {
                props.Add("h", (int) h);
            }
            if (post.TryGetValue("fsize", out JToken fsize))
            {
                props.Add("fsize", (int) fsize);
            }
            if (post.TryGetValue("country_name", out JToken country_name))
            {
                props.Add("country_name", (string) country_name);
            }
            return props;
        }

        private IEnumerable<long> ExtractPostRepliesFromText(Post post)
        {
            var match = repliesRegex.Match(post.Text);
            if (!match.Success)
            {
                return Array.Empty<long>();
            }
            else
            {
                List<long> rnos = new List<long>();
                while (match.Success)
                {
                    string no = match.Value.Replace(">>", string.Empty);
                    if (Int64.TryParse(no, out long _no))
                    {
                        rnos.Add(_no);
                    }
                    match = match.NextMatch();
                }
                return rnos;
            }
        }
        #endregion
    }
}
 