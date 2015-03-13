namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("axmlprinter")>]
[<assembly: AssemblyProductAttribute("axmlprinter")>]
[<assembly: AssemblyDescriptionAttribute("Unpacker for Android binary XML (AXML) format")>]
[<assembly: AssemblyVersionAttribute("1.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0"
