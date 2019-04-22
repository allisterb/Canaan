namespace Canaan

open System
open Microsoft.FSharp.Quotations

[<AutoOpen>]
module Code = 
    type SourceLocation = {Line : string; Dir: string; File: string}

    let inline here() = {
        Line = __LINE__;
        Dir = __SOURCE_DIRECTORY__
        File = __SOURCE_FILE__
    }
        

    let rec funName = function
    | Patterns.Call(None, methodInfo, _) -> methodInfo.Name
    | Patterns.Lambda(_, expr) -> funName expr
    | _ -> failwith "Unexpected input"
