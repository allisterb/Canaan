namespace Canaan.Tests.Loggers

open System

open Xunit

open Canaan

module Serilog = 

    [<Fact>]
    let ``Can create SerilogLogger ``() =
        printfn "%i log file(s) deleted." <| IO.DeleteFiles true "." "*.log"
        let s = SerilogLogger()
        do s.Info "Hello"
        IO.GetDirFiles "*.log" "." |> fst > 0 |> Assert.True
    
