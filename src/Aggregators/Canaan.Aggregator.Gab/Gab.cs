using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Canaan
{
    public class Gab : Api
    {
        public async Task<IEnumerable<Post>> GetUpdates(int listenTimeout)
        {
            HttpClient.Timeout = TimeSpan.FromSeconds(listenTimeout);
            List<string> updates = new List<string>();
            List<Post> posts = new List<Post>();
            var requestUri = "https://gab.com/api/v1/streaming/public";
            try
            {
                using (var op = Begin("Listen to Gab live stream for {0} seconds", listenTimeout))
                {
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
                                break;
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
                dynamic o = JObject.Parse(s);
                dynamic data = o.data;
                var post = new Post()
                {
                    Id = data.id + "-" + YY,
                    DatePublished = DateTime.Parse(data.created_at)
                };

                posts.Add(post);
            }
            return posts;
        }
    }
}
