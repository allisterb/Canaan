using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Canaan
{
    public class Gab : Api
    {
        public Gab() : base()
        {
            Initialized = true;
        }

        public AzureLUIS intentService = new AzureLUIS();
        public async Task<IEnumerable<Post>> GetUpdates(int listenTimeout)
        {
            if (HttpClient.Timeout != TimeSpan.FromSeconds(listenTimeout))
            {
                HttpClient.Timeout = TimeSpan.FromSeconds(listenTimeout);
            }
            List<string> updates = new List<string>();
            List<Post> posts = new List<Post>();
            var requestUri = "https://gab.com/api/v1/streaming/public";
            try
            {
                using (var op = Begin("Listen to Gab live stream for {0} seconds", listenTimeout))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    var stream = await HttpClient.GetStreamAsync(requestUri);
                    using (var reader = new StreamReader(stream))
                    {
                        StringBuilder eventBuilder = new StringBuilder();

                        bool reading = false;
                        while (!reader.EndOfStream)
                        {
                            var l = reader.ReadLine();
                            if (l.StartsWith("event: update"))
                            {
                                reading = true;
                                continue;
                            }
                            else if (l == "" && reading)
                            {
                                updates.Add(eventBuilder.ToString());
                                eventBuilder.Clear();
                                reading = false;
                                continue;
                            }
                            else if (reading)
                            {
                                eventBuilder.Append(l);
                                continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                    op.Complete();
                }
            }
            catch (Exception e)
            {
                Error(e, "Error occurred listening to Gab live stream.");
            }

            if (updates.Count == 0)
            {
                Info("0 updates received.");
                return posts;
            }

            Info("{0} updates received from Gab live stream.", updates.Count);

            foreach (var s in updates)
            {
                var props = JObject.Parse(s.Replace("data: ", "")).Properties();
                var account = (JObject)props.First(p => p.Name == "account").Value;
            
                var post = new Post()
                {
                    No = (long) props.First(p => p.Name == "id").Value,
                    Id = (string) props.First(p => p.Name == "id").Value + "-" + YY,
                    DatePublished = (DateTime) props.First(p => p.Name == "created_at").Value,
                    Text = (string) props.First(p => p.Name == "content").Value,
                    User = (string) account.Properties().First(p => p.Name == "username").Value,
                    Source = "gab"

                };
                posts.Add(post);
            }            
            foreach (var post in posts)
            {
                var html = post.Text;
                post.Text = WebScraper.ExtractTextFromHtmlFrag(html);
                post.Links = WebScraper.ExtractLinksFromHtmlFrag(html);
                post.Text = WebScraper.RemoveUrlsFromText(post.Text);
                post.HasIdentityHate = HateWords.IdentityHateWords.Any(w => post.Text.Contains(w));
                await intentService.GetPredictionForPost(post);
                if (post.Entities.Count > 0)
                {
                    Info("Detected {0} entities in post {1}.", post.Entities.Count, post.Id);
                }
                if (post.ThreatIntent > 0.0)
                {
                    Info("Detected threat intent {0:0.00} in post {1}.", post.ThreatIntent, post.Id);
                }
            }   
            return posts;
        }
    }
}
