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

    let client = if String.IsNullOrEmpty(apiKey) then new NewsAPI.NewsApiClient(apiKey) |> Some else None

    override x.Initialized = (apiKey  <> "") && (client <> None)

    member x.GetTopHeadlines() = 
        let c = client |> x.EnsureInit |> Option.get 
        c.GetTopHeadlines <| TopHeadlinesRequest() |> fun r -> r.Articles |> Seq.map (fun a -> NewsApiArticle(a, x.CancellationToken))
        //let r =Seq.map (fun a -> NewsApiArticle(a, x.CancellationToken)) <<@=  c.GetTopHeadlines <| TopHeadlinesRequest() >>@= fun r -> r.Articles
        //!> r >>@= Seq.map (fun a -> NewsApiArticle(a, x.CancellationToken))
        
        
 