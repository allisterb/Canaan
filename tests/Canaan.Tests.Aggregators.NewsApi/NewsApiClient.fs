namespace Canaan.Tests

open System
open Xunit

open Canaan
open Canaan.Aggregators.NewsApi

module NewsApiClient =
    
    do Api.SetDefaultLoggerIfNone()

    let apiKey =  Environment.GetEnvironmentVariable "NEWSAPI_KEY"

    [<Fact>]
    let ``Can construct client``() =     
        let client = NewsApiClient apiKey
        Assert.NotNull client
        Assert.True client.Initialized

    [<Fact>]
    let ``Can get top headlines``() =    
        let client = NewsApiClient apiKey
        let r = client.GetTopHeadlines()
        test r |> Assert.False  


    



    