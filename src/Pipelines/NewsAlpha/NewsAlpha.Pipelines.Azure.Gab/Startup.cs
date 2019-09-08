using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

using System;

using Canaan;
namespace NewsAlpha.Functions
{
    public class Startup : IWebJobsStartup
    {
        public Startup()
        {
            Api.IsAzureFunction = true;
        }

        public void Configure(IWebJobsBuilder builder)
        {
            ConfigureServices(builder.Services).BuildServiceProvider(true);
        }

        private IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services
                .AddLogging(loggingBuilder =>
                    loggingBuilder.AddSerilog(dispose: true)
                );
            return services;
        }
    }
}