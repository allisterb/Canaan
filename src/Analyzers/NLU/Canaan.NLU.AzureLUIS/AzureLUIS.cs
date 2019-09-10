using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime;

using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;


namespace Canaan
{
    public class AzureLUIS : Api
    {
        #region Constructors
        public AzureLUIS(string endpointUrl, string authKey, string appId, CancellationToken ct) : base(ct)
        {
            EndpointUrl = endpointUrl ?? throw new ArgumentNullException("endpointUrl");
            AuthKey = authKey ?? throw new ArgumentNullException("authKey");
            AppId = appId ?? throw new ArgumentNullException("appId");
            var credentials = new ApiKeyServiceClientCredentials(authKey);
            Client = new LUISRuntimeClient(credentials, new System.Net.Http.DelegatingHandler[] { });
            Client.Endpoint = EndpointUrl;
            AppId = appId;
            Initialized = true;
        }
        #endregion
        public AzureLUIS(CancellationToken ct) : this(Config("CognitiveServices:LUISEndpointUrl"), Config("CognitiveServices:LUISAuthKey"), Config("CognitiveServices:LUISAppId"), ct) { }

        #region Properties
        public AzureLUIS() : this(Cts.Token) { }

        public string EndpointUrl { get; }

        public string AuthKey { get; }

        public string AppId { get; }
        public LUISRuntimeClient Client { get; protected set; }
        #endregion

        #region Methods
        public async Task<Post> GetPredictionForPost(Post post)
        {
            var prediction = new Prediction(Client);
            var query = post.Text.Length <= 500 ? post.Text : post.Text.Substring(0, 500);
            var result = await prediction.ResolveAsync(AppId, query);
            var results = new Dictionary<string, object>();
            List<string> entities = new List<string>();
            foreach(var e in result.Entities)
            {
                entities.Add(e.Entity);
            }
            post.Entities = entities;
            if (result.TopScoringIntent != null && result.TopScoringIntent.Intent.ToLower() == "threat")
            {
                post.ThreatIntent = result.TopScoringIntent.Score.Value;
            }
            return post;
        }
        #endregion
    }
}
