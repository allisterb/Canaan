using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Rest;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;

using MoreLinq;

namespace Canaan
{
    public class AzureTextAnalytics : Api
    {
        public AzureTextAnalytics(string endpointUrl, string authKey, CancellationToken ct) : base(ct)
        {

            EndpointUrl = endpointUrl ?? throw new ArgumentNullException("endpointUrl");
            AuthKey = authKey ?? throw new ArgumentNullException("authKey");
            Client = new TextAnalyticsClient(new ApiKeyServiceClientCredentials(AuthKey));
            Client.Endpoint = EndpointUrl;
            Initialized = true;
        }

        public AzureTextAnalytics(CancellationToken ct) : this(Config("CognitiveServices:EndpointUrl"), Config("CognitiveServices:AuthKey"), ct) { }

        public AzureTextAnalytics() : this(Cts.Token) { }

        public string EndpointUrl { get; set; }

        public string AuthKey { get; set; }

        public TextAnalyticsClient Client { get; set; }

        public async Task<Dictionary<string, double?>> AnalyzeSentiment(Dictionary<string, string> text)
        {
            using (var op = Begin("Analyze {0} text records using MS Text Analytics API.", text.Count()))
            {
                List<MultiLanguageInput> mlinput = text.Select(t => new MultiLanguageInput(t.Key, t.Key, "en")).ToList();
                try
                {
                    var r = await Client.SentimentBatchAsync(new MultiLanguageBatchInput(mlinput), cancellationToken: CancellationToken);
                    var results = r.Documents.ToDictionary(d => d.Id, d => d.Score);
                    op.Complete();
                    return results;
                    
                }
                catch (Exception e)
                {
                    Error(e, "An error occurred during sentiment analysis using the Microsoft Text Analytics API.");
                    return null;

                }
            }

        }
    }

    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        public ApiKeyServiceClientCredentials(string s)
        {
            subscriptionKey = s;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }

        protected string subscriptionKey;
    }
}
