namespace ApiDiff.Tests

open System
open System.IO
open Xunit
open Xunit.Abstractions
open ApiDiff

[<AutoOpen>]
module IOHelpers =
    let (/) path1 path2 = Path.Combine(path1, path2)

module TestAssets =
    let rootDir = (__SOURCE_DIRECTORY__ / ".." / "..") |> Path.GetFullPath
    let assetsDir = rootDir / "tests" / "assets"
    let lib1Dir = assetsDir / "Lib1"

type ApiDiffTest (output: ITestOutputHelper) =

    [<Fact>]
    member x.``Build and diff`` () =

        let conf =
#if RELEASE
            "Release"
#else
            "Debug"
#endif

        let assemblyPath = TestAssets.lib1Dir / "bin" / conf / "netstandard2.0" / "Lib1.dll"
        let outputPath = TestAssets.assetsDir / "apis" / "Lib1.netstandard20.txt"
        let diff = ApiDiff.apiDiff assemblyPath outputPath

        match diff with
        | Choice1Of2 () -> ()
        | Choice2Of2 diffLines ->
            diffLines
            |> Seq.iter (output.WriteLine)
            Assert.True(false, "api doesnt match")
