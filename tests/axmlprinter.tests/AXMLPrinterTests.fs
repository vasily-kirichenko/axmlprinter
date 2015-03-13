module axmlprinter.tests.AXMLPrinterTests

open System
open NUnit.Framework
open axmlprinter

let private getPath relPath =
    let codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase
    let uri = UriBuilder(codeBase)
    let path = Uri.UnescapeDataString(uri.Path)
    let currAssembyDirPath = IO.FileInfo(path).Directory.FullName
    let targetDir = IO.DirectoryInfo(IO.Path.Combine(currAssembyDirPath, relPath))
    targetDir.FullName

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
    let dirPath = getPath("../../samples/axml/")
    let packedFileNames = IO.Directory.GetFiles(dirPath, "*.packed") |> Array.toList
    Assert.That(packedFileNames.Length, Is.GreaterThan(0), "no test samples")

    let samples =
        packedFileNames
        |> List.map (fun fn -> fn.Substring(0, fn.LastIndexOf(".packed")))
        |> List.zip packedFileNames

    let check (packedFile: string, expectedFile: string) =
        use packedFs = IO.File.OpenRead(packedFile)
        let actualXml = AXMLPrinter.getXmlFromStream packedFs
        let expectedXml = IO.File.ReadAllText(expectedFile)
        Assert.That(actualXml, Is.EqualTo(expectedXml), sprintf "packed file: %s, expected file: %s" packedFile expectedFile)

    samples
    |> List.iter (fun (packed, unpacked) ->
        printfn "%s - %s" packed unpacked
        check (packed,unpacked))

[<Test>]
let notaxml () =
    let checkNotAxml fpath =
        use fs = IO.File.OpenRead(fpath)
        let fn () = AXMLPrinter.getXmlFromStream fs |> ignore
        let exc = Assert.Throws<Exception>(TestDelegate fn)
        Assert.That(exc.Message, Is.StringContaining("it seems this is not android xml"))

    let dirPath = getPath("../../samples/notaxml/")
    let notAxmls = IO.Directory.GetFiles("../../samples/notaxml/", "*") |> Array.toList
    Assert.That(notAxmls.Length, Is.GreaterThan(0), "no test samples")
    notAxmls |> List.iter checkNotAxml
