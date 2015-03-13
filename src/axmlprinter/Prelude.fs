namespace axmlprinter

module internal Assembly =
    open System.Runtime.CompilerServices

    [<assembly: InternalsVisibleTo("axmlprinter.tests")>]
    do()

module internal Option =
    let fill defaultValue = function
                            | Some v -> v
                            | None -> defaultValue

module internal String =
    let inline replace (oldValue: string) (newValue: string) (str: string) =
        str.Replace(oldValue, newValue)
