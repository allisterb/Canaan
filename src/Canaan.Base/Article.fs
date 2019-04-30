namespace Canaan

open System;
open System.Threading
open System.Threading.Tasks

[<AbstractClass>]
type Article(title: string, url:string, ?ct: CancellationToken) =
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

    abstract member Description: string

    abstract PublishedAt: DateTime option

and Content = { Text: string; Attrs: Map<string, obj> option}

and Category = {Name: string; Label:string option; Scheme: string option; Attrs: Map<string, Object> option}  




 