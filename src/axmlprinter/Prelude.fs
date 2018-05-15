namespace axmlprinter

module internal Assembly =
    open System.Runtime.CompilerServices

    [<assembly: InternalsVisibleTo("axmlprinter.tests")>]
    do()

module internal String =
    let inline replace (oldValue: string) (newValue: string) (str: string) =
        str.Replace(oldValue, newValue)
