namespace Canaan.Aggregator.NewsApi

open System
open System.Threading

open NewsAPI
open NewsAPI.Models
open NewsAPI.Constants

open Canaan

[<AbstractClass>]
type NewsApiClient(apiKey: string, ?ct: CancellationToken) = 
    inherit Aggregator(?ct = ct)

    

    override x.Initialized = apiKey  <> "" 

    member x.GetLatest() = 
        let c = NewsAPI.NewsApiClient(apiKey)
        //let e = c.GetTopHeadlines()
        ()


