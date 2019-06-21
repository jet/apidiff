// Learn more about F# at http://fsharp.org

open System
open System.IO

[<EntryPoint>]
let main argv =

    match argv with
    | [| path |] ->
        ApiDiff.ApiDiff.print path
        |> Seq.iter (printfn "%s")
        0
    | _ ->
        printfn "invalid args."
        printfn "expecting the first argument as the path."
        1
