namespace ApiDiff

module internal Core =

    open System
    open Mono.Cecil

    let publicTypes (types: TypeDefinition seq) =
        types
        |> Seq.filter (fun t -> t.IsPublic || t.IsNestedPublic)
        |> Seq.sortBy (fun t -> t.Name)

    let publicTypesOfModule (m: ModuleDefinition) =
        m.Types
        |> publicTypes

    let private isPublicMethod (m: MethodDefinition) =
        match m with
        | null -> false
        | method -> method.IsPublic

    // yield! t.GetRuntimeEvents()     |> Seq.filter (fun m -> m.AddMethod.IsPublic) |> Seq.map cast
    let publicEvents (t: TypeDefinition) =
        t.Events
        |> Seq.filter (fun e -> isPublicMethod e.AddMethod)
        |> Seq.sortBy (fun t -> t.Name)
        |> Seq.map (fun e -> e, e.FullName)

    // yield! t.GetRuntimeProperties() |> Seq.filter (fun m -> m.GetMethod.IsPublic) |> Seq.map cast
    let publicProperties (t: TypeDefinition) =
        t.Properties
        |> Seq.filter (fun p -> (isPublicMethod p.GetMethod) || (isPublicMethod p.SetMethod))
        |> Seq.sortBy (fun t -> t.Name)
        |> Seq.map (fun p -> p, p.FullName)

    // yield! t.GetRuntimeMethods()    |> Seq.filter (fun m -> m.IsPublic) |> Seq.map cast
    let publicMethods (t: TypeDefinition) =
        t.Methods
        |> Seq.filter isPublicMethod
        |> Seq.filter (fun m -> not(m.IsGetter) && not(m.IsSetter))
        |> Seq.sortBy (fun t -> t.Name)
        |> Seq.map (fun m -> m, m.FullName)

    // yield! t.GetRuntimeFields()     |> Seq.filter (fun m -> m.IsPublic) |> Seq.map cast
    let publicFields (t: TypeDefinition) =
        t.Fields
        |> Seq.filter (fun f -> f.IsPublic)
        |> Seq.sortBy (fun t -> t.Name)
        |> Seq.map (fun f -> f, f.FullName)

    // yield! ti.DeclaredNestedTypes   |> Seq.filter (fun ty -> ty.IsNestedPublic) |> Seq.map cast
    let nestedTypes (t: TypeDefinition) =
        t.NestedTypes
        |> publicTypes

    let print (filename: string) =
        let m = ModuleDefinition.ReadModule filename

        seq {
            let rec visitType (t: TypeDefinition) = seq {
                yield sprintf "TYPE: %s" t.FullName

                yield! t |> publicEvents |> Seq.map snd |> Seq.map (sprintf "EVENT: %s")
                yield! t |> publicProperties |> Seq.map snd |> Seq.map (sprintf "PROP: %s")
                yield! t |> publicMethods |> Seq.map snd |> Seq.map (sprintf "METHOD: %s")
                yield! t |> publicFields |> Seq.map snd |> Seq.map (sprintf "FIELD: %s")

                for n in nestedTypes t do
                    yield! visitType n 
            }

            for t in publicTypesOfModule m do
                yield! visitType t
        }

module ApiDiff =

    let print filename = Core.print filename

    open System.IO
    open Medallion.Shell

    let apiDiff assemblyFilePath outputFilePath =
        let lines = Core.print assemblyFilePath |> Seq.toArray

        File.WriteAllLines(outputFilePath, lines)

        let gitDir = Path.GetDirectoryName outputFilePath
        let gitDiffLines = ResizeArray<string>()
        let gitDiff = Command.Run("git", [| "-c"; "core.fileMode=false"; "diff"; "--exit-code"; outputFilePath|], options = (fun opt -> opt.WorkingDirectory(gitDir) |> ignore))
                             .RedirectTo(gitDiffLines)
        gitDiff.Wait()

        match gitDiff.Result.ExitCode with
        | 0 -> Choice1Of2 ()
        | _ -> Choice2Of2 gitDiffLines
