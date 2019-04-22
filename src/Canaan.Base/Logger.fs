namespace Canaan

open System

/// Logging interface
[<AbstractClass>]
type Logger() =
    static member val IsConfigured = false with get,set
    
    abstract member Info : messageTemplate:string * [<ParamArray>]args:obj[] -> unit
    abstract member Debug : messageTemplate:string * [<ParamArray>]args:obj[] -> unit
    abstract member Error : messageTemplate:string * [<ParamArray>]args:obj[] -> unit
    abstract member Error : ex:Exception * messageTemplate:string * [<ParamArray>]args:obj[] -> unit

/// Basic console logger available anywhere
type ConsoleLogger() =
    inherit Logger()

    override this.Debug (messageTemplate:string, [<ParamArray>] args: obj[]) = 
        String.Format(messageTemplate, args) |> printfn "%s"
    override this.Info (messageTemplate:string, [<ParamArray>] args: obj[])  = 
        String.Format(messageTemplate, args) |> printfn "%s"
    override this.Error (messageTemplate:string, [<ParamArray>] args: obj[])  = 
        String.Format(messageTemplate, args) |> printfn "%s"
    override this.Error (ex: Exception, messageTemplate:string, [<ParamArray>] args: obj[])  = 
        String.Format(messageTemplate, args) |> printfn "%s"