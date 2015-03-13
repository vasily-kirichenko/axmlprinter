#load "load-project.fsx"
open axmlprinter
open System

let axmlPath = IO.Path.GetFullPath(__SOURCE_DIRECTORY__ + "/../../../tests/axmlprinter.tests/samples/axml/air.geardice.PicrossMadness.xml.packed")

using (IO.File.OpenRead(axmlPath))
      (fun fs -> let xml = AXMLPrinter.getXmlFromStream fs
                 printfn "%s" xml)
