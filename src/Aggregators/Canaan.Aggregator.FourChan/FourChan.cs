using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Canaan
{
    public class FourChan : Api
    {
        public static async Task<IEnumerable<NewsThread>> GetThreads(string board)
        {
            var json = await HttpClient.GetStringAsync($"http://a.4cdn.org/{board}/threads.json");
            var pages = JArray.Parse(json);
            var threads = new List<NewsThread>();
            foreach (dynamic page in pages)
            {
                int p = 1;
                foreach (dynamic thread in page.threads)
                {
                    var t = new NewsThread()
                    {
                        No = thread.no,
                        Id = (string)thread.no,
                        Position = p++,
                        LastModified = DateTimeOffset.FromUnixTimeSeconds((long) thread.last_modified).UtcDateTime,
                        ReplyCount = thread.replies
                    };
                    threads.Add(t);
                }
            }
            return threads;
        }

        public static async Task<IDictionary<NewsThread, List<Post>>> GetPosts(string board, IEnumerable<NewsThread> threads)
        {
            Task<string>[] threadTasks = threads.Select(t => HttpClient.GetStringAsync($"http://a.4cdn.org/{board}/thread/{t.No}.json")).ToArray();
            try
            {
                await Task.WhenAll(threadTasks);
            }
            catch {}

            foreach (var task in threadTasks.Where(t => t.IsFaulted || t.IsCanceled))
            {
                if (task.Exception != null)
                {
                    Error(task.Exception, "Could not fetch thread.");
                }
                else
                {
                    Error("Could not fetch thread");
                }
            }

            var threadsJson = threadTasks.Where(t => t.IsCompleted).Select(t => t.Result);
            var posts = new ConcurrentDictionary<NewsThread, List<Post>>();
            Parallel.ForEach(threadsJson, (j) =>
            {
                dynamic o = JObject.Parse(j);
                JArray threadPosts = o.posts;
                dynamic subjectPost = threadPosts[0];
                NewsThread thread = threads.Single(t => t.No == (long) subjectPost.no);
                thread.Title = subjectPost.sub;
                thread.Text = subjectPost.name;
                posts.TryAdd(thread, new List<Post>());
                int pos = 1;
                foreach (dynamic post in threadPosts)
                {
                    posts[thread].Add(
                        new Post()
                        {
                            No = post.no,
                            Id = post.no.ToString(),
                            ThreadId = thread.Id,
                            Position = pos++,
                            Author = post.name,
                            DatePublished = DateTimeOffset.FromUnixTimeSeconds((long) post.time).UtcDateTime,
                            Text = post.com,
                            Links = WebScraper.ExtractLinksFromHtmlFrag((string)post.com)
                        });
                }
            });
            return posts;
        }
    }
}
 