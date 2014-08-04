namespace axmlprinter

module Option =
    let fill defaultValue = function
                            | Some v -> v
                            | None -> defaultValue

module String =
    let inline replace (oldValue: string) (newValue: string) (str: string) =
        str.Replace(oldValue, newValue)
