open OrderTaking.PlaceOrder
open OrderTaking.PlaceOrder.Implementation

[<EntryPoint>]
let main argv =
    let addr =
        { AddressLine1 = "1190 Benedum Drive"
          AddressLine2 = "Narrowsburg"
          AddressLine3 = ""
          AddressLine4 = ""
          City = "NY"
          ZipCode = "12764" }

    let customerInfo =
        { FirstName = "John"
          LastName = "Lennon"
          EmailAddress = "john@beatles.com" }

    let unvalidatedOrder: UnvalidatedOrder =
        { OrderId = "foo"
          CustomerInfo = customerInfo
          ShippingAddress = addr
          BillingAddress = addr
          Lines =
            [ { OrderLineId = "foo"
                ProductCode = "W1234"
                Quantity = 1.0M } ] }

    let eventList = placeOrder unvalidatedOrder
    eventList |> List.iter (fun event -> printfn "event: %A" event)
    printfn "done."
    0
// let fiveKilos = 5.0<kg>
// let fiveMeters = 5.0<m>
// let listOfWeights = [ fiveKilos; fiveMeters ]
// printfn "%b" (fiveKilos = fiveMeters)


// let result = UnitQuantity.create 100

// match result with
// | Error msg -> printfn "Failuer, message is %s" msg
// | Ok uq ->
//     printfn "Success value is %A" uq
//     let innerValue = UnitQuantity.value uq
//     printfn "innverValue is %i" innerValue

// 0
