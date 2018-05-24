namespace axmlprinter

module internal XmlBeautifizer =
    open System.IO
    open System.Xml.Linq

    type private Utf8StringWriter() =
        inherit StringWriter()
        override x.Encoding = System.Text.Encoding.UTF8

    let beautifize (xml: string) =
        let xdoc = XDocument.Parse(xml)
        
        for xel in xdoc.DescendantNodes() do
            match xel with
            | :? XElement as xel when not xel.HasElements ->
                xel.RemoveNodes()
            | _ -> ()

        use sw = new Utf8StringWriter()
        xdoc.Save(sw)
        sw.GetStringBuilder().ToString()

/// Documentation for AXMLPrinter
///
/// ## Example
///
///     use fs = File.OpenRead("/path/to/AndroidManifest.xml")
///     let xml: string = AXMLPrinter.getXmlFromStream fs
///     printfn "%s" xml
///
module AXMLPrinter =
    open System
    open System.IO
    open axmlprinter.Constants

    type internal Context = { Reader: BinaryReader
                              IsUtf8: bool
                              StringOffsets: int[]
                              Strings: byte[]
                              ResourceIds: uint32 list
                              PrefixUri: Map<uint32, uint32>
                              UriPrefix: Map<uint32, uint32>
                              XmlnsEmitted: bool
                              OutputXml: string }

    let internal getShort2 (lst: byte[]) offset =
        let x = (lst.[offset + 1] &&& 0xFFuy) <<< 8
        let y = lst.[offset] &&& 0xFFuy
        let result = x ||| y
        result

    let internal getVarint (lst: byte[]) offset =
        let value = lst.[offset] |> int
        let more = (value &&& 0x80) <> 0
        let value = value &&& 0x7F
        if not more
        then (value, 1)
        else (value <<< 8 ||| (int (lst.[offset + 1]) &&& 0xFF), 2)

    let internal decode (strings: byte[]) offset len =
        let len = len * 2
        let len = len + len % 2

        let rec loop ind length result =
            if ind < length
            then
                let t_data = strings.[offset + ind]
                let newResult = t_data :: result
                match newResult with
                | h1 :: h2 :: tail when h1 = 0uy && h2 = 0uy -> List.rev tail
                | _ -> loop (ind + 1) length newResult
            else List.rev result
        let res = loop 0 len [] |> List.toArray
        System.Text.Encoding.Unicode.GetString(res)

    let internal decode2 (strings: byte[]) offset len =
        let bytes = [offset .. offset+len-1] |> List.map (fun x -> strings.[x]) |> List.toArray
        System.Text.Encoding.UTF8.GetString(bytes)

    let internal getRaw ctx (idx: uint32) =
        let offset = ctx.StringOffsets.[int idx]

        if ctx.IsUtf8
        then
            let offset = offset + snd (getVarint ctx.Strings offset)
            let varint = getVarint ctx.Strings offset
            let offset = offset + snd varint
            let length = fst varint
            decode2 ctx.Strings offset length
        else
            let length = getShort2 ctx.Strings offset |> int
            decode ctx.Strings (offset + 2) length

    let internal getPrefix ctx nameSpaceUri =
        ctx.UriPrefix
        |> Map.tryFind nameSpaceUri
        |> Option.map (getRaw ctx)
        |> Option.map (sprintf "%s:")
        |> Option.defaultValue ""

    let private readLineNumber ctx =
        do ctx.Reader.ReadBytes(4) |> ignore // chunkSize
        let lineNumber = ctx.Reader.ReadUInt32()
        do ctx.Reader.ReadBytes(4) |> ignore // 0xFFFFFFFF
        lineNumber

    let private getAttributeOffset ind = ind * ATTRIBUTE_LENGHT

    let private getAttributePrefix ctx ind (attributes: uint32[]) =
        let offset = getAttributeOffset ind
        let uri = attributes.[int (offset + ATTRIBUTE_IX_NAMESPACE_URI)]

        let result =
            ctx.UriPrefix
            |> Map.tryFind uri
            |> Option.map (getRaw ctx)
            |> Option.map (sprintf "%s:")
            |> Option.defaultValue ""
        result

    let private getAttributeName ctx ind (attributes: uint32[]) =
        let offset = getAttributeOffset ind
        let name = attributes.[int (offset + ATTRIBUTE_IX_NAME)]
        let result = getRaw ctx name
        result

    let private getAttributeValueType ind (attributes: uint32[]) =
        let offset = getAttributeOffset ind
        attributes.[int (offset + ATTRIBUTE_IX_VALUE_TYPE)]

    let private getAttributeValueData ind (attributes: uint32[]) =
        let offset = getAttributeOffset ind
        attributes.[int (offset + ATTRIBUTE_IX_VALUE_DATA)]

    let private doGetAttributeValue ctx ind (attributes: uint32[]) =
        let offset = getAttributeOffset ind
        let valueType = attributes.[int (offset + ATTRIBUTE_IX_VALUE_TYPE)]
        if valueType = TYPE_STRING
        then let valueString = attributes.[int (offset + ATTRIBUTE_IX_VALUE_STRING)]
             getRaw ctx (uint32 valueString)
        else ""

    let private getPackage data = if data >>> 24 = 1u then "android:" else ""

    let private complexToFloat data = (float)(data &&& 0xFFFFFF00u) * (RADIX_MULTS.[((int data) >>> 4) &&& 3])
    let private demensionUnit data = DIMENSION_UNITS.[int (data &&& COMPLEX_UNIT_MASK)]
    let private fractionUnit data = FRACTION_UNITS.[int (data &&& COMPLEX_UNIT_MASK)]

    let private escape =
        String.replace "&" "&amp;"
        >> String.replace "\"" "&quot;"
        >> String.replace "'" "&apos;"
        >> String.replace "<" "&lt;"
        >> String.replace ">" "&gt;"

    let private getAttributeValue ctx ind (attributes: uint32[]) =
        let ``type`` = getAttributeValueType ind attributes
        let data = getAttributeValueData ind attributes
        match ``type`` with
        | TYPE_STRING -> doGetAttributeValue ctx ind attributes
        | TYPE_ATTRIBUTE -> sprintf "?%s%08X" (getPackage data) data
        | TYPE_REFERENCE -> sprintf "@%s%08X" (getPackage data) data
        | TYPE_FLOAT -> sprintf "%f" (data |> BitConverter.GetBytes |> (fun bs -> BitConverter.ToSingle(bs, 0)))
        | TYPE_INT_HEX -> sprintf "0x%08X" data
        | TYPE_INT_BOOLEAN -> match data with | 0u -> "false" | _ -> "true"
        | TYPE_DIMENSION -> sprintf "%f%s" (complexToFloat data) (demensionUnit data)
        | TYPE_FRACTION -> sprintf "%f%s" (complexToFloat data) (fractionUnit data)
        | ``type`` when ``type`` >= TYPE_FIRST_COLOR_INT && ``type`` <= TYPE_LAST_COLOR_INT -> sprintf "#%08X" data
        | ``type`` when ``type`` >= TYPE_FIRST_INT && ``type`` <= TYPE_LAST_INT -> sprintf "%d" data
        | ``type`` -> sprintf "<0x%X, type 0x%02X>" data ``type``

    let rec internal parse ctx =
        if ctx.Reader.BaseStream.Position = ctx.Reader.BaseStream.Length
        then ctx.OutputXml
        else
            let chunkType = ctx.Reader.ReadUInt32()
            match chunkType with
            | CHUNK_RESOURCEIDS ->
                let chunkSize = ctx.Reader.ReadUInt32()
                let resourceIds = [1u .. chunkSize / 4u - 2u] |> List.map (fun _ -> ctx.Reader.ReadUInt32())
                parse { ctx with ResourceIds = resourceIds }
            | CHUNK_XML_START_NAMESPACE ->
                do readLineNumber ctx |> ignore
                let prefix = ctx.Reader.ReadUInt32()
                let uri = ctx.Reader.ReadUInt32()
                parse { ctx with PrefixUri = ctx.PrefixUri.Add(prefix, uri)
                                 UriPrefix = ctx.UriPrefix.Add(uri, prefix) }
            | CHUNK_XML_END_NAMESPACE ->
                do readLineNumber ctx |> ignore
                do ctx.Reader.ReadBytes(8) |> ignore
                parse ctx
            | CHUNK_XML_START_TAG ->
                do readLineNumber ctx |> ignore
                let nameSpaceUri = ctx.Reader.ReadUInt32()
                let nameIdx = ctx.Reader.ReadUInt32()
                do ctx.Reader.ReadBytes(4) |> ignore // flags
    
                let attributeCount = ctx.Reader.ReadUInt32() &&& 0xFFFFu
                do ctx.Reader.ReadBytes(4) |> ignore // class attribute
    
                let attributes =
                    [|1u .. attributeCount * ATTRIBUTE_LENGHT|]
                    |> Array.mapi (fun ind _ ->
                        let v = ctx.Reader.ReadUInt32()
                        match ind with
                        | ind when (ind - int(ATTRIBUTE_IX_VALUE_TYPE)) % int(ATTRIBUTE_LENGHT) = 0 -> v >>> 24
                        | _ -> v)
    
                let name = getRaw ctx nameIdx
                let xmlns =
                    if ctx.XmlnsEmitted = true
                    then ""
                    else ctx.UriPrefix
                         |> Seq.map (fun p -> sprintf "xmlns:%s=\"%s\"" (getRaw ctx p.Value) (getRaw ctx (ctx.PrefixUri.[p.Value])))
                         |> String.concat "\n"
    
                let attrs =
                    Seq.init (int attributeCount) uint32
                    |> Seq.map (fun ind -> sprintf "%s%s=\"%s\"" (getAttributePrefix ctx ind attributes)
                                                                 (getAttributeName ctx ind attributes)
                                                                 (escape (getAttributeValue ctx ind attributes)))
                    |> String.concat "\n"
    
                let prefix = getPrefix ctx nameSpaceUri
                let xml = sprintf "<%s%s\n%s\n%s>\n" prefix name xmlns attrs
                parse { ctx with XmlnsEmitted = true
                                 OutputXml = ctx.OutputXml + xml }
            | CHUNK_XML_END_TAG ->
                do readLineNumber ctx |> ignore
                let namespaceUri = ctx.Reader.ReadUInt32()
                let nameIdx = ctx.Reader.ReadUInt32()
    
                let prefix = getPrefix ctx namespaceUri
                let name = getRaw ctx nameIdx
    
                let xml = sprintf "</%s%s>" prefix name
                parse { ctx with OutputXml = sprintf "%s%s" ctx.OutputXml xml }
            | CHUNK_XML_TEXT ->
                do readLineNumber ctx |> ignore
                let nameIdx = ctx.Reader.ReadUInt32()
                do ctx.Reader.ReadBytes(8) |> ignore
                let xml = sprintf "%s" (getRaw ctx nameIdx)
                parse { ctx with OutputXml = sprintf "%s%s" ctx.OutputXml xml }
            | _ -> failwithf "Unknown chunk type = %A" chunkType

    let private validate name value =
        // just heuristic to prevent out of memory
        // maybe threshold should be increased
        if value > 100 * 1000
        then failwithf "it seems this is not android xml, %s = %d too large" name value

    /// Try to unpack axml
    ///
    /// ## Parameters
    ///  - `axmlStream` - stream of AXML bytes
    let getXmlFromStream (axmlStream: Stream) =
        use br = new BinaryReader(axmlStream)
        do br.ReadBytes(8) |> ignore // ?
        do br.ReadBytes(2) |> ignore // unused header
        do br.ReadBytes(2) |> ignore // unused header size

        let chunkSize = br.ReadInt32()
        let stringCount = br.ReadInt32()
        let styleOffsetCount = br.ReadInt32()
        let flags = br.ReadInt32()
        let isUtf8 = (flags &&& 0x00000100) <> 0

        let stringsOffset = br.ReadInt32()
        let stylesOffset = br.ReadInt32()

        do validate "chunkSize" chunkSize
        do validate "stringCount" stringCount
        do validate "styleOffsetCount" styleOffsetCount
        do validate "stringsOffset" stringsOffset
        do validate "stylesOffset" stylesOffset

        let stringOffsets = [|1 .. stringCount|] |> Array.map (fun _ -> br.ReadInt32())
        [1 .. styleOffsetCount] |> List.iter (fun _ -> br.ReadInt32() |> ignore) // unused style offsets

        let size =
            if stylesOffset <> 0
            then stylesOffset - stringsOffset
            else chunkSize - stringsOffset

        let strings = [|1 .. size|] |> Array.map (fun _ -> br.ReadByte())

        if stylesOffset <> 0
        then let size = chunkSize - stylesOffset
             [1 .. size / 4] |> List.iter (fun _ -> br.ReadInt32() |> ignore) // unused styles

        let context = { Reader = br
                        IsUtf8 = isUtf8
                        StringOffsets = stringOffsets
                        Strings = strings
                        ResourceIds = []
                        PrefixUri = Map.empty
                        UriPrefix = Map.empty
                        XmlnsEmitted = false
                        OutputXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" }
        let uglyXml = parse context
        try XmlBeautifizer.beautifize uglyXml
        with _ -> uglyXml

    /// Try to unpack axml
    ///
    /// ## Parameters
    ///  - `axmlBytes` - bytes readed from AXML file
    let getXmlFromBytes (axmlBytes: byte array) =
        use ms = new MemoryStream(axmlBytes)
        getXmlFromStream ms
