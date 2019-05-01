namespace Canaan.Tests

open System
open Xunit

open Canaan
open Canaan.Aggregators.NewsApi

module NewsApiClient =
    
    let apiKey =  Environment.GetEnvironmentVariable "NEWSAPI_KEY"

    [<Fact>]
    let ``Can get top headlines``() = 
        do Api.SetDefaultLoggerIfNone()
        let client = NewsApiClient apiKey
        Assert.NotNull client
        Assert.True client.Initialized
        

    