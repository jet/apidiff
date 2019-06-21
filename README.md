# Apidiff public api surface checker

the `Apidiff` library can be used where you want to add a check to the **public api surface** of your library.

It's a library who can be used in tests, to check all api changes are expected, to not have unwanted changes in the api surface for consumers.

It **make it easy for developers, with minimal maintenance, to be sure what consumer see and what api contract we may break with changes**.

Scenarios:

- A new method is added, but for a mistake is public instead of private. Removing that from next version mean a breaking change, who can be avoided.
- A library expose a function and after a refactoring, the api slightly changes (a generic type become non generic or vice versa) and that mean is not compatible anymore, so a breaking change.
- What's the impact of the PR on the public api?

**NOTE** It doesnt enforce any semver convention, that's out of scope.

## How it works

The public api surface of the library is written to a file committed in the repository, as baseline, who contains all the information about **public** types, methods, etc with full signature.
Private/internal types/methods are ignored.

The `ApiDiff.apiDiff` function is used in an test, where the new built assembly api is compared with the one committed in the repo.

If it's the same, the test pass.

Otherwise, the test fails showing the differences (like methods added or removed).
If that's expected, because the file is overridden when `ApiDiff.apiDiff` is called, you just need to commit the changes of the file, the test will pass next run.

The api surface file for a source file like [this](tests/assets/Lib1/Library.fs) is like

```
TYPE: Lib1.Duck
PROP: System.Tuple`2<System.Int32,System.Int32> Lib1.Duck::Position()
METHOD: System.Void Lib1.Duck::.ctor()
METHOD: System.String Lib1.Duck::Quack()
METHOD: System.Void Lib1.Duck::Walk()
TYPE: Lib1.Say
METHOD: System.Void Lib1.Say::hello(System.String)
TYPE: Lib1.Say/InItalian
METHOD: System.Void Lib1.Say/InItalian::ciao(System.String)
```

This format:

- It's concise, just the full signature
- All info is sorted by name (type, methods, etc), so order is deterministic and allow an easier diff
- Can be read by humans

**NOTE** the `ApiDiff.apiDiff` use `git diff` to show differences, in a style familiar for developers
**NOTE** `git` is required in PATH

## How to use

In the test project, create an helper functions like this

```fsharp
// assemblyPath is the full path to the assembly
// tfm is the target framework, because there is one baseline file for each target framework
let checkApi assemblyPath tfm =
    printfn "api check for tfm %s of assembly %s" tfm assemblyPath
    // the path for baseline file, in same lib, one file per target framework
    let outputPath = Path.Combine(__SOURCE_DIRECTORY__, sprintf "MyLib.%s.txt" tfm)
    // run the diff
    match ApiDiff.apiDiff assemblyPath outputPath with
    | Choice1Of2 () ->
        () // no diff, ok
    | Choice2Of2 diffLines -> //failure, diff lines contains the difference
        diffLines
        |> Seq.iter (printfn "%s")
        Assert.True(false, sprintf "the %s api of '%s' doesnt match, see git diff of '%s' " tfm assemblyPath outputPath)
```

and in a test, add one test for each target framework, like

```fsharp
#if NETCOREAPP2_0
    [<Fact>]
    member __.``netstandard2.0 api`` () =
        let assemblyPath = typeof<MyPublicType>.Assembly.Location
        checkApi assemblyPath "netstandard2.0"
#endif

#if NET461
    [<Fact>]
    member __.``net461 api`` () =
        let assemblyPath = typeof<MyPublicType>.Assembly.Location
        checkApi assemblyPath "net45"
#endif
```

That's because the test frameworks usually run multiple times, one for each target framework.

We reuse this behaviour, to get the path of the referenced assembly under test who contains the public `MyPublicType` type.

**NOTE** the correct test it's executed in the right target framework, because it's under a compiler define (the `#ifdef NET461`)

**NOTE** The helper function is not included in the library OOTB, because test frameworks may differ in the logging api (like `ITestOutputHelper` for XUnit vs `Console.Writeline`) and the assert library.


## Security
This repository is actively monitored by Jet Engineers and the Jet Security team. Please monitor this repo for security updates and advisories. For more information and contacts, please see [SECURITY](SECURITY.md)
