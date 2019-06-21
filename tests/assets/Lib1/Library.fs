namespace Lib1

type Duck () =
    member val Position : int * int = (0,0) with get, set
    member __.Quack () = "quack"
    member this.Walk () = this.Position <- (fst this.Position + 1), snd this.Position
#if LIB1_API_CHANGED1
    member this.WalkBack () = this.Position <- (fst this.Position - 1), snd this.Position
#endif

module Say =
    let hello name =
        printfn "Hello %s" name

    module InItalian =
        let ciao name =
            printfn "Ciao %s" name
