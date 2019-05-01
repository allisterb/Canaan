namespace Canaan.Aggregators.NewsApi

open System
open System.Collections.Generic
open System.Threading

open NewsAPI
open NewsAPI.Models
open NewsAPI.Constants

open Canaan
open NewsAPI.Constants

exception NewsApiException of int * string
 
type NewsApiClient(apiKey: string, ?ct: CancellationToken) = 
    inherit Aggregator(?ct = ct)

    let client = if String.IsNullOrEmpty(apiKey) then None else NewsAPI.NewsApiClient(apiKey) |> Some

    override x.Initialized = (apiKey  <> "") && (client <> None)

    member x.GetTopHeadlines() = 
        let c = client |> x.EnsureInit |> Option.get 
        let r =  !> c.GetTopHeadlines <| TopHeadlinesRequest() 
        match r with
        | Success result -> 
            if result.Status = Statuses.Ok then 
                result.Articles |> Seq.map(fun a -> NewsApiArticle(a, x.CancellationToken)) |> Success
            else
                NewsApiException((int)result.Error.Code, result.Error.Message) |> Failure
        | Failure e -> e |> Failure         

        
        
 