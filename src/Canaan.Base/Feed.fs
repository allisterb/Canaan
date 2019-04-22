namespace Canaan

open System;
open System.Threading
open System.Threading.Tasks

(*
type Feed = {Title: FeedTitle; Uri: Uri; Description: FeedDescription; Author: Person; Attrs: Map<string, Object>; Items: Content list}
    and FeedTitle = FeedTitle of string
    and FeedDescription = FeedDescription of string
*)
type Feed(title: string, url:string, ?ct: CancellationToken) =
    inherit Api(?ct = ct)

    let (urlParsed, urlResult) = Uri.TryCreate(url, UriKind.Absolute)

    let initialized = 
        if not urlParsed then
            err "The url {0} is not valid." [url]
            false
        else true

    override x.Initialized = initialized

    member x.Title = title

    member x.Url with get() = x.EnsureInit urlResult 

and Content = { Text: string; Uri : Uri option; Attrs: Map<string, obj> option}

and Category = {Name: string; Label:string option; Scheme: string option; Attrs: Map<string, Object> option}

and Urls = Urls of Uri list  




 