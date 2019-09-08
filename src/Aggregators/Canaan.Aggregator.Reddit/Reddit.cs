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
using RedditSharp;
using RedditSharp.Things;
namespace Canaan
{
    public class Reddit : Api
    {
        #region  Constructors
        public Reddit(CancellationToken ct) : base(ct) { Initialized = true; }

        public Reddit() : base() { Initialized = true; }
        #endregion

        #region Methods
        public async Task<IEnumerable<NewsThread>> GetThreads(string board)
        {
            using (var op = Begin("Get threads for board {0}", board))
            {
                var source = $"r.{board}";
                var webAgent = new BotWebAgent(Config("Reddit:User"), Config("Reddit:Pass"), Config("Reddit:ClientId"), Config("Reddit:ClientSecret"), "https://github.com/allisterb/Canaan");
                webAgent.UserAgent = "Canaan/0.1";
                var reddit = new RedditSharp.Reddit(webAgent);
                var threads = new List<NewsThread>();
                var r = await reddit.GetSubredditAsync(board);
                await r.GetPosts(Subreddit.Sort.Top, 400).ForEachAsync((post, p) =>
                {
                    var text = post.IsSelfPost ? post.SelfText : string.Empty;
                    var html = post.IsSelfPost ? post.SelfTextHtml : null;
                    NewsThread thread = new NewsThread()
                    {
                        Id = post.Id + "-" + YY,
                        Source = source,
                        Position = p + 1,
                        Subject = post.Title,
                        DatePublished = post.CreatedUTC,
                        User = post.AuthorName,
                        Text = text,
                        Links = post.IsSelfPost ? WebScraper.ExtractLinksFromHtmlFrag(html) : new Link[] { new Link() { Uri = post.Url } }
                    };
                    threads.Add(thread);
                });
                return threads;
            }
        }

        public async Task<IDictionary<NewsThread, List<Post>>> GetPosts(string board, IEnumerable<NewsThread> threads)
        {
            using (var op = Begin("Get posts for {0} threads for board {1}", threads.Count(), board))
            {
                var source = $"r.{board}";
                var webAgent = new BotWebAgent(Config("Reddit:User"), Config("Reddit:Pass"), Config("Reddit:ClientId"), Config("Reddit:ClientSecret"), "https://github.com/allisterb/Canaan");
                webAgent.UserAgent = "Canaan/0.1";
                var reddit = new RedditSharp.Reddit(webAgent);
                var threadPosts = new Dictionary<NewsThread, List<Post>>();
                var redditPosts = new Dictionary<RedditSharp.Things.Post, List<Post>>();
                var r = await reddit.GetSubredditAsync(board);
                await r.GetPosts(Subreddit.Sort.Top, 400).ForEachAsync(async (post, p) =>
                {
                    var thread = threads.SingleOrDefault(t => t.Id == post.Id + "-" + YY);
                    if (thread != null)
                    {
                        var comments = await post.GetCommentsAsync();
                        var posts = comments.Select((c,cp) => GetPostsFromComment(board,cp, thread.Id, c, null)).SelectMany(x => x).ToList();
                        threadPosts.Add(thread, posts);
                    }
                });
                return threadPosts;
            }
            
        }

        protected IEnumerable<Post> GetPostsFromComment(string board, int pos, string tid, Comment c, Comment parent = null)
        {
            var source = $"r.{board}";
            List<Post> posts = new List<Post>();
            var post = new Post
            {
                Id = c.Id + "-" + YY,
                ThreadId = tid,
                Source = source,
                Position = pos,
                DatePublished = c.CreatedUTC,
                User = c.AuthorName,
                Text = c.Body,
                ReplyTo = new List<string>(1) { c.ParentId + "-" + YY },
                Links = WebScraper.ExtractLinksFromHtmlFrag(c.BodyHtml),
            };
            posts.Add(post);
            if (c.Comments.Count > 0)
            {
                posts.AddRange(c.Comments.Select((cn, p) => GetPostsFromComment(board, p, tid, cn)).SelectMany(x => x));
            }
            return posts;
                        
        }
        #endregion
    }
}
