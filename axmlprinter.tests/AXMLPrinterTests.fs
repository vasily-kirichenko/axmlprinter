module axmlprinter.tests.AXMLPrinterTests

open System
open NUnit.Framework
open axmlprinter

[<Test>]
let decode () =
    let strings = [ 0; 0; 0; 8; 0
                    109; 0  // m
                    97; 0   // a
                    110; 0  // n
                    105; 0  // i
                    102; 0  // f
                    101; 0  // e
                    115; 0  // s
                    116; 0  // t
                    0; 0; 23 ] |> List.map byte

    Assert.That(AXMLPrinter.decode strings 5 8, Is.EqualTo("manifest"))

[<Test>]
let acceptance () =
    let packedFileNames = IO.Directory.GetFiles("../../samples/", "*.packed") |> Array.toList
    let samples =
        packedFileNames
        |> List.map (fun fn -> fn.Substring(0, fn.LastIndexOf(".packed")))
        |> List.zip packedFileNames

    let check (packedFile: string, expectedFile: string) =
        use packedFs = IO.File.OpenRead(packedFile)
        let actualXml = AXMLPrinter.getXmlFromStream packedFs
        printfn "%s" actualXml
        let expectedXml = IO.File.ReadAllText(expectedFile)
        Assert.That(actualXml, Is.EqualTo(expectedXml), sprintf "packed file: %s, expected file: %s" packedFile expectedFile)

    samples |> List.iter check
