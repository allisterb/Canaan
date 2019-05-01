namespace Canaan.Aggregators.NewsApi

open System
open System.Collections.Generic
open System.Threading

open NewsAPI
open NewsAPI.Models
open NewsAPI.Constants

open Canaan

type NewsApiClient(apiKey: string, ?ct: CancellationToken) = 
    inherit Aggregator(?ct = ct)

    let client = if String.IsNullOrEmpty(apiKey) then None else NewsAPI.NewsApiClient(apiKey) |> Some

    override x.Initialized = (apiKey  <> "") && (client <> None)

    member x.GetTopHeadlines() = 
        let c = client |> x.EnsureInit |> Option.get 
        c.GetTopHeadlines <| TopHeadlinesRequest() |> fun r -> r.Articles |> Seq.map (fun a -> NewsApiArticle(a, x.CancellationToken))
        
        
 