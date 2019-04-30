namespace Canaan

open System
open Serilog
open Serilog.Core

type SerilogLogger(?logFileName: string) = 
    inherit Canaan.Logger()
    
    let file = defaultArg logFileName "Cana.log"

    let config = 
        LoggerConfiguration().WriteTo.RollingFile(pathFormat = file, outputTemplate=
            "{Timestamp:HH:mm:ss} [{Level:u3}] {Message}{NewLine}{Exception}") 
        
    let logger = 
        let l = config.CreateLogger()
        Logger.IsConfigured <- true
        l
        
    override this.Debug (messageTemplate:string, [<ParamArray>] args: obj[]) = logger.Debug(messageTemplate, args)
    override this.Info (messageTemplate:string, [<ParamArray>] args: obj[]) = logger.Information(messageTemplate, args)
    override this.Error (messageTemplate:string, [<ParamArray>] args: obj[]) = logger.Error(messageTemplate, args)
    override this.Error (ex: Exception, messageTemplate:string, [<ParamArray>] args: obj[]) = 
        logger.Error(ex, messageTemplate, args)
