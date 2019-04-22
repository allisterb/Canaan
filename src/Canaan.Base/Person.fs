namespace Canaan

open System;

type Person = {Name: Name; Email: Email option; Uri: Uri option; Attrs: Map<string, obj>}
    
    and Email = {Address: string}        
    
    and Name = 
    | FullName of string
    | FirstLastName of string * string

    with 
        static member Create (name:string, ?email:string) = {
            Name = FullName name; 
            Email = if email.IsSome then Some {Address = email.Value} else None; 
            Uri = None;
            Attrs = Map.empty
        } 