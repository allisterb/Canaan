namespace Canaan.Aggregators.NewsApi

open System;
open System.IO;
open System.Threading.Tasks;
open Microsoft.AspNetCore.Mvc;
open Microsoft.Azure.WebJobs;
open Microsoft.Azure.WebJobs.Extensions.Http;
open Microsoft.AspNetCore.Http;
open Microsoft.Extensions.Logging;
open Newtonsoft.Json;

open Canaan

module AzureFunctions =
    
    do Api.SetDefaultLoggerIfNone()

    [<FunctionName("NewsApi")>]
    let run([<HttpTrigger(AuthorizationLevel.Function, "get", Route = "hello")>] req: HttpRequest, log: ILogger) =
        async {    
            return ContentResult(Content = "oo", ContentType = "text/html")
        } |> Async.StartAsTask
        