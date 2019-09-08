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
                await r.GetPosts(Subreddit.Sort.Top, 200).ForEachAsync((post, p) =>
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
                        Links = post.IsSelfPost ? WebScraper.ExtractLinksFromHtmlFrag(html) : new Link[] {new Link() {Uri = post.Url} }
                    };
                    threads.Add(thread);
                });
                return threads;
            }
        }
        #endregion
    }
}
