using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

using Canaan;
namespace NewsAlpha
{
    public static class FourChanAzureFunctions
    {
        [FunctionName("Update"), ]
        public static async Task Update([TimerTrigger("0 */10 * * * *")] TimerInfo myTimer, ILogger log)
        {
            Api.SetLogger(new SerilogLogger());
            Api.IsAzureFunction = true;
            FourChanAzurePipeline pipeline = new FourChanAzurePipeline();
            await pipeline.Update("pol");
        }
    }
}
