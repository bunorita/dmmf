open OrderTaking.Common
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

    // setup (dummy) services used by the workflow
    let checkProductCodeExists: CheckProductCodeExists = fun productCode -> true

    let checkAddressExists: CheckAddressExists =
        fun unvalidatedAddress -> CheckedAddress unvalidatedAddress

    let getProductPrice: GetProductPrice = fun productCode -> Price.create 1M

    let createAcknowledgementLetter: CreateOrderAcknowledgementLetter =
        fun pricedOrder -> HtmlString "Some text"

    let sendAcknowledgement: SendOrderAknowledgement = fun acknowledgement -> Sent

    // partially apply the services to the workflow
    let placeOrder =
        placeOrder
            checkProductCodeExists
            checkAddressExists
            getProductPrice
            createAcknowledgementLetter
            sendAcknowledgement

    //  run the workflow
    let eventList = placeOrder unvalidatedOrder

    eventList |> List.iter (fun event -> printfn "event: %A" event)
    printfn "done."

    0
