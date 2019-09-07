using System;
using System.Threading;
using System.Threading.Tasks;

using Canaan;

namespace NewsAlpha
{
    class Program
    {
        static FourChanAzurePipeline p1 = new FourChanAzurePipeline();
        static async Task Main(string[] args)
        {
            Api.SetLogger(new SerilogLogger("NewsAlpha-FourChanAzure.log"));
            //await p1.InitThreads("pol");
            //await p1.InitPosts("pol");
            //await p1.UpdateThreadReplyCount("pol");

           await p1.Update("pol");
        }


    }
}
