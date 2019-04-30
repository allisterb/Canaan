namespace Canaan.Tests

open System

open Xunit

open Canaan

module Api = 

    [<Fact>]
    let ``Can construct``() =
        do Api.SetDefaultLoggerIfNone()
        !! TestApi(true) |> Assert.NotNull
        Assert.Throws (fun _ -> !! TestApi(false) |> ignore) |> ignore