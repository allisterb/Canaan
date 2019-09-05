using System;
using System.Threading;

using NewsThread = Canaan.NewsThread;

using Xunit;

namespace Canaan.Tests
{
    public class CosmosDbStorageTests : BaseTests
    {
        public CosmosDbStorageTests() : base()
        {
            db = new CosmosDB("socialnews");
        }
        protected CosmosDB db; 

        [Fact]
        public void CanConnectToCosmosDb()
        {
            string id = DateTime.UtcNow.Ticks.ToString();
            NewsThread t = new NewsThread()
            {
                Id = id,
                DatePublished = DateTime.Now,
                Position = 0,
                Source = "unittest",
                LastModified = DateTime.Now,
                User = "nobody",
                Subject = "CanConnectToCosmosDb"
            };
            Assert.True(db.CreateAsync("threads", "unittest", t).Result);

            var item = db.GetAsync<NewsThread>("threads", "unittest", id).Result;
            Assert.NotNull(item);
        }

        [Fact]
        public void CanGetNewsThreads()
        {
            string id = DateTime.UtcNow.Ticks.ToString();
            for (int i = 0; i < 5; i++)
            {
                NewsThread t = new NewsThread()
                {
                    Id = id + i.ToString(),
                    DatePublished = DateTime.Now,
                    Position = 0,
                    Source = "unittest",
                    LastModified = DateTime.Now,
                    User = "nobody",
                    Subject = "CanGetNewsThreads" + i.ToString()
                };
                Assert.True(db.CreateAsync("threads", "unittest", t).Result);
            }
            var items = db.GetAsync<NewsThread>("threads", "unittest", new[] {id + "0", id + "1", id + "2" }).Result;
            Assert.NotEmpty(items);
        }
    }
}
