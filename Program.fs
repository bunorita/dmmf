open OrderTaking.Domain

[<EntryPoint>]
let main argv =
    let fiveKilos = 5.0<kg>
    let fiveMeters = 5.0<m>
    // let listOfWeights = [ fiveKilos; fiveMeters ]
    // printfn "%b" (fiveKilos = fiveMeters)


    let result = UnitQuantity.create 100

    match result with
    | Error msg -> printfn "Failuer, message is %s" msg
    | Ok uq ->
        printfn "Success value is %A" uq
        let innerValue = UnitQuantity.value uq
        printfn "innverValue is %i" innerValue

    0
