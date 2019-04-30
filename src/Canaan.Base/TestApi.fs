namespace Canaan.Tests

open System

open Canaan

type TestApi(init) =
    inherit Api() 
    
    do Api.SetDefaultLoggerIfNone()

    override this.Initialized with get() = init
