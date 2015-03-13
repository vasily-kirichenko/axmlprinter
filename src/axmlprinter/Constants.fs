module internal axmlprinter.Constants

[<Literal>]
let ATTRIBUTE_LENGHT            = 5u

[<Literal>]
let ATTRIBUTE_IX_NAMESPACE_URI  = 0u
[<Literal>]
let ATTRIBUTE_IX_NAME           = 1u
[<Literal>]
let ATTRIBUTE_IX_VALUE_STRING   = 2u
[<Literal>]
let ATTRIBUTE_IX_VALUE_TYPE     = 3u
[<Literal>]
let ATTRIBUTE_IX_VALUE_DATA     = 4u

[<Literal>]
let CHUNK_AXML_FILE             = 0x00080003u
[<Literal>]
let CHUNK_RESOURCEIDS           = 0x00080180u
[<Literal>]
let CHUNK_XML_FIRST             = 0x00100100u
[<Literal>]
let CHUNK_XML_START_NAMESPACE   = 0x00100100u
[<Literal>]
let CHUNK_XML_END_NAMESPACE     = 0x00100101u
[<Literal>]
let CHUNK_XML_START_TAG         = 0x00100102u
[<Literal>]
let CHUNK_XML_END_TAG           = 0x00100103u
[<Literal>]
let CHUNK_XML_TEXT              = 0x00100104u
[<Literal>]
let CHUNK_XML_LAST              = 0x00100104u

[<Literal>]
let TYPE_ATTRIBUTE          = 2u
[<Literal>]
let TYPE_DIMENSION          = 5u
[<Literal>]
let TYPE_FIRST_COLOR_INT    = 28u
[<Literal>]
let TYPE_FIRST_INT          = 16u
[<Literal>]
let TYPE_FLOAT              = 4u
[<Literal>]
let TYPE_FRACTION           = 6u
[<Literal>]
let TYPE_INT_BOOLEAN        = 18u
[<Literal>]
let TYPE_INT_COLOR_ARGB4    = 30u
[<Literal>]
let TYPE_INT_COLOR_ARGB8    = 28u
[<Literal>]
let TYPE_INT_COLOR_RGB4     = 31u
[<Literal>]
let TYPE_INT_COLOR_RGB8     = 29u
[<Literal>]
let TYPE_INT_DEC            = 16u
[<Literal>]
let TYPE_INT_HEX            = 17u
[<Literal>]
let TYPE_LAST_COLOR_INT     = 31u
[<Literal>]
let TYPE_LAST_INT           = 31u
[<Literal>]
let TYPE_NULL               = 0u
[<Literal>]
let TYPE_REFERENCE          = 1u
[<Literal>]
let TYPE_STRING             = 3u

[<Literal>]
let COMPLEX_UNIT_MASK        =   15u

let RADIX_MULTS             =   [ 0.00390625; 3.051758E-005; 1.192093E-007; 4.656613E-010 ]
let DIMENSION_UNITS         =   [ "px"; "dip"; "sp"; "pt"; "in"; "mm"; ""; "" ]
let FRACTION_UNITS          =   [ "%";  "%p";  "";   "";   "";   "";   ""; "" ]
