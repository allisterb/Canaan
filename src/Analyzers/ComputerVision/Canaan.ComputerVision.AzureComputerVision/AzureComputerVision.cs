using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Rest;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;

namespace Canaan
{
    public class AzureComputerVision : Api
    {
        public AzureComputerVision(string endpointUrl, string authKey, CancellationToken ct) : base(ct)
        {

            EndpointUrl = endpointUrl ?? throw new ArgumentNullException("endpointUrl");
            AuthKey = authKey ?? throw new ArgumentNullException("authKey");
            Client = new ComputerVisionClient(new ApiKeyServiceClientCredentials(authKey));
            Client.Endpoint = EndpointUrl;
            Initialized = true;
        }

        public AzureComputerVision(CancellationToken ct): this(Config("CognitiveServices:EndpointUrl"), Config("CognitiveServices:AuthKey"), ct) {}

        public AzureComputerVision() : this(Cts.Token) { }

        public string EndpointUrl { get; set; }
        
        public string AuthKey { get; set; }

        public ComputerVisionClient Client { get; set; }

        public async Task Analyze(byte[] imageData)
        {
            ImageAnalysis analysis = null;

            var visualFeatures = new List<VisualFeatureTypes>()
            {
                VisualFeatureTypes.Categories,
                VisualFeatureTypes.Description,
                VisualFeatureTypes.Faces,
                VisualFeatureTypes.Tags
            };
        
            using (var op = Begin("Analyze image using MS Computer Vision API."))
            {
                try
                {
                    using (Stream stream = new MemoryStream(imageData))
                    {
                        analysis = await Client.AnalyzeImageInStreamAsync(stream,
                            visualFeatures, null, null, CancellationToken);
                       
                        
                        op.Complete();
                    }
                }
                catch (Exception e)
                {
                    Error(e, "An error occurred during image analysis using the Microsoft Computer Vision API.");
                    return;
                   
                }
            }

        }
    }
}
