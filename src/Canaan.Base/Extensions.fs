namespace Canaan

open System
open System.Collections.Generic

module Extensions =
    type Dictionary<'TKey, 'TValue> with
            member this.TryFind key =
                let value = ref (Unchecked.defaultof<'TValue>)
                if this.TryGetValue (key, value) then Some !value
                else None

    type String with
        member this.AsUrl (url:string) =
            let (p, r) = Uri.TryCreate(url, UriKind.Absolute)
            if p then Some r else None
