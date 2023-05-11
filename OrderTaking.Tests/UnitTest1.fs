module MathService.Tests

open NUnit.Framework

open OrderTaking.Common
open OrderTaking.PlaceOrder
open OrderTaking.PlaceOrder.Implementation

[<Test>]
let ``If product exists, validation succeeds`` () =
    // arrange: set up stub versions of service dependencies
    let checkAddressExists address = CheckedAddress address // succeed
    let checkProductCodeExists productCode = true // succeed

    // arrange: set up input
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

    // act: call validateOrder
    let result =
        // validateOrder checkProductCodeExists checkAddressExists unvalidatedOrder
        validateOrder unvalidatedOrder

    // assert: check that result is a validatedOrder, not an error
    printfn "result: ---\n%A" result

[<Test>]
let ``If product doesn't exist, validation fails`` () =
    // arrange: set up stub versions of service dependencies
    let checkAddressExists address = CheckedAddress address // succeed
    let checkProductCodeExists productCode = false // fail

    // arrange: set up input
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

    // act: call validateOrder
    // and assert
    let f =
        fun () ->
            // validateOrder checkProductCodeExists checkAddressExists unvalidatedOrder
            validateOrder unvalidatedOrder |> ignore

    Assert.Throws(f) |> ignore
