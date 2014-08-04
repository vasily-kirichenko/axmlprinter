module internal axmlprinter.AssemblyInfo

open System
open System.Reflection
open System.Resources
open System.Runtime.CompilerServices
open System.Runtime.InteropServices
open System.Security
open System.Security.Permissions

// Version information
[<assembly: AssemblyVersion("1.0.0")>]
[<assembly: AssemblyFileVersion("1.0.0")>]
[<assembly: AssemblyInformationalVersion("1.0.0")>]

// Assembly information
[<assembly: AssemblyTitle("axmlprinter")>]
[<assembly: AssemblyDescription("axmlprinter")>] // todo
[<assembly: NeutralResourcesLanguage("en-US")>]
[<assembly: Guid("A6ED5261-0B13-4D17-B67B-DD7176F8E2D3")>]
[<assembly: AssemblyCopyright("Copyright © Pavel Martynov 2014")>]
[<assembly: ComVisible(false)>]

(*  Makes internal modules, types, and functions visible
    to the test project so they can be unit-tested. *)
[<assembly: InternalsVisibleTo("axmlprinter.tests")>]

(*  F# considers modules which only contain attributes to be empty;
    so, we appease the compiler by adding an empty function. *)
do ()
