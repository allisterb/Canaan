namespace Canaan.Aggregators.NewsApi

open System;
open System.Threading
open System.Threading.Tasks

open NewsAPI
open Canaan

type NewsApiArticle(a: NewsAPI.Models.Article, ?ct: CancellationToken) =
    inherit Article(a.Title, a.Url, ?ct = ct)

    member x.SourceId = a.Source.Id
    member x.SourceName = a.Source.Name
    override x.Description = a.Description
    override x.PublishedAt = if a.PublishedAt.HasValue then Some a.PublishedAt.Value else None
    