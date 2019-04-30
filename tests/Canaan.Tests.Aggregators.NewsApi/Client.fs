namespace Canaan.Tests.Aggregator.NewsApi

open System
open Xunit

open Canaan.Aggregators.NewsApi

module Client = 
    let apiKey =  Environment.GetEnvironmentVariable "NEWSAPI_KEY"

    [<Fact>]
    let ``Can get top headlines``() = 
        let client = NewsApiClient apiKey
        Assert.NotNull client
        

    