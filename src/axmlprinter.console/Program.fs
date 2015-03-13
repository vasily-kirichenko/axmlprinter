open System

[<EntryPoint>]
let main argv =
    match argv with
    | [| axmlpath |] ->
        try
            let bytes = IO.File.ReadAllBytes(axmlpath)
            let xml = axmlprinter.AXMLPrinter.getXmlFromBytes bytes
            do printfn "%s" xml
        with
        | ex -> do eprintfn "error was occured: %s" ex.Message
    | _ -> do printfn "usage: axmlprinter.console <android-xml-file>"
    0
