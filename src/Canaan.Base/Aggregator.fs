namespace Canaan

open System
open System.Threading

[<AbstractClass>]
type Aggregator(?ct: CancellationToken) = inherit Api(?ct = ct)
