// module internal OrderTaking.PlaceOrder.Implementation
module OrderTaking.PlaceOrder.Implementation

open OrderTaking.Common


// -----------------------------------------------------------------
// Type definition of each steps
//   * Validation
//   * Pricing
//   * Sending OrderAcknowledgement
//   * Creating events
// -----------------------------------------------------------------

// ---------------------------------
// Validation step
// ---------------------------------
type CheckProductCodeExists = ProductCode -> bool
type AddressValidationError = AddressValidationError of string
type CheckedAddress = CheckedAddress of UnvalidatedAddress
// type CheckAddressExists = UnvalidatedAddress -> AsyncResult<CheckedAddress, AddressValidationError>
type CheckAddressExists = UnvalidatedAddress -> CheckedAddress // without effects for now

type ValidatedOrderLine =
    { OrderLineId: OrderLineId
      ProductCode: ProductCode
      Quantity: OrderQuantity }

type ValidatedOrder =
    { OrderId: OrderId
      CustomerInfo: CustomerInfo
      ShippingAddress: Address
      BillingAddress: Address
      OrderLines: ValidatedOrderLine list }

type ValidateOrder =
    // ignoring dependencies for now
    // CheckProductCodeExists // dependency
    //     -> CheckAddressExists // dependency
    //     -> UnvalidatedOrder // input
    UnvalidatedOrder // input
        // -> AsyncResult<ValidatedOrder, ValidationError list> // output
        // -> ValidatedOrder // output, without effects
        -> Result<ValidatedOrder, ValidationError>

// ---------------------------------
//  Pricing step
// ---------------------------------
type GetProductPrice = ProductCode -> Price

type PriceOrder =
    // GetProductPrice // dependency
    //     -> ValidatedOrder // input
    ValidatedOrder
        //  -> PricedOrder // output: without effects for now
        -> Result<PricedOrder, PricingError> // output

// ---------------------------------
//  Send acknowledgement
// ---------------------------------
type HtmlString = HtmlString of string

type OrderAcknowledgement =
    { EmailAddress: EmailAddress
      Letter: HtmlString }

type CreateOrderAcknowledgementLetter = PricedOrder -> HtmlString

type SendResult =
    | Sent
    | NotSent
// type SendOrderAknowledgement = OrderAcknowledgement -> Async<SendResult>
type SendOrderAknowledgement = OrderAcknowledgement -> SendResult // without effects for now

type AcknowledgeOrder =
    // CreateOrderAcknowledgementLetter // dependency
    //     -> SendOrderAknowledgement // dependency
    //     -> PricedOrder // input
    PricedOrder -> OrderAcknowledgementSent option // output

// ---------------------------------
//  Create events
// ---------------------------------
type CreateEvents =
    // PricedOrder // input
    //     -> OrderAcknowledgementSent option // input (event from previous step)
    OrderAcknowledgementSent option // input (event from previous step)
        -> PlaceOrderEvent list //outputs


// -----------------------------------------------------------------
// Implementation
// -----------------------------------------------------------------

// ---------------------------------
// Validation step
// ---------------------------------
let toCustomerInfo (customer: UnvalidatedCustomerInfo) =
    { Name =
        { FirstName = customer.FirstName |> String50.create
          LastName = customer.LastName |> String50.create }
      EmailAddress = customer.EmailAddress |> EmailAddress.create }

let toAddress (checkAddressExists: CheckAddressExists) unvalidatedAddress =
    let checkedAddress = checkAddressExists unvalidatedAddress
    let (CheckedAddress checkedAddress) = checkedAddress
    let addressLine1 = checkedAddress.AddressLine1 |> String50.create
    let addressLine2 = checkedAddress.AddressLine2 |> String50.createOption
    let addressLine3 = checkedAddress.AddressLine3 |> String50.createOption
    let addressLine4 = checkedAddress.AddressLine4 |> String50.createOption
    let city = checkedAddress.City |> String50.create
    let zipCode = checkedAddress.ZipCode |> ZipCode.create

    { AddressLine1 = addressLine1
      AddressLine2 = addressLine2
      AddressLine3 = addressLine3
      AddressLine4 = addressLine4
      City = city
      ZipCode = zipCode }

let predicateToPassthru errorMsg f x = if f x then x else failwith errorMsg

let toProductCode (checkProductCodeExists: CheckProductCodeExists) productCode =
    let checkProductCode productCode =
        let errMsg = sprintf "Invalid Product Code: %A" productCode
        predicateToPassthru errMsg checkProductCodeExists productCode

    productCode |> ProductCode.create |> checkProductCode


let toOrderQuantity (productCode: ProductCode) (qty: decimal) : OrderQuantity =
    match productCode with
    | Widget _ -> qty |> int |> UnitQuantity.create |> OrderQuantity.Unit
    | Gizmo _ -> qty * 1.0m<kg> |> KilogramQuantity.create |> OrderQuantity.Kilogram

let toValidatedOrderLine
    checkProductCodeExists // dependency for toProductCode
    (unvalidatedOrderLine: UnvalidatedOrderLine)
    =
    let orderLineId = unvalidatedOrderLine.OrderLineId |> OrderLineId.create

    let productCode =
        unvalidatedOrderLine.ProductCode |> toProductCode checkProductCodeExists // use service

    let quantity = unvalidatedOrderLine.Quantity |> toOrderQuantity productCode

    { OrderLineId = orderLineId
      ProductCode = productCode
      Quantity = quantity }


let validateOrder: ValidateOrder =
    fun
        // checkProductCodeExists // dependency for toValidatedOrderLine
        // checkAddressExists // dependency for toAddress
        unvalidatedOrder ->

        // dummy dependencies
        let checkProductCodeExists: CheckProductCodeExists = fun productCode -> true

        let checkAddressExists: CheckAddressExists =
            fun unvalidatedAddress -> CheckedAddress unvalidatedAddress


        let orderId = unvalidatedOrder.OrderId |> OrderId.create
        let customerInfo = unvalidatedOrder.CustomerInfo |> toCustomerInfo
        let shippingAddr = unvalidatedOrder.ShippingAddress |> toAddress checkAddressExists
        let billingAddr = unvalidatedOrder.BillingAddress |> toAddress checkAddressExists

        let orderLines =
            unvalidatedOrder.Lines |> List.map (toValidatedOrderLine checkProductCodeExists)


        Ok
            { OrderId = orderId
              CustomerInfo = customerInfo
              ShippingAddress = shippingAddr
              BillingAddress = billingAddr
              OrderLines = orderLines }



// ---------------------------------
//  Pricing step
// ---------------------------------
let toPricedOrderLine getProductPrice (line: ValidatedOrderLine) : PricedOrderLine =
    let qty = line.Quantity |> OrderQuantity.value
    let price = line.ProductCode |> getProductPrice
    let linePrice = price |> Price.multipy qty

    { OrderLineId = line.OrderLineId
      ProductCode = line.ProductCode
      Quantity = line.Quantity
      LinePrice = linePrice }


let priceOrder: PriceOrder =
    fun
        // getProductPrice
        validatedOrder ->

        let getProductPrice: GetProductPrice = fun productCode -> Price.create 1M // dummy dependency

        let lines =
            validatedOrder.OrderLines |> List.map (toPricedOrderLine getProductPrice)

        let amountToBill =
            lines |> List.map (fun line -> line.LinePrice) |> BillingAmount.sumPrices


        Ok
            { OrderId = validatedOrder.OrderId
              CustomerInfo = validatedOrder.CustomerInfo
              ShippingAddress = validatedOrder.ShippingAddress
              BillingAddress = validatedOrder.BillingAddress
              OrderLines = lines
              AmmountToBill = amountToBill }


// ---------------------------------
//  Send acknowledgement
// ---------------------------------
let acknowledgeOrder: AcknowledgeOrder =
    fun
        // createAcknowledgementLetter
        // sendAcknowledgement
        pricedOrder ->

        // dummy dependencies
        let createAcknowledgementLetter: CreateOrderAcknowledgementLetter =
            fun pricedOrder -> HtmlString "Some text"

        let sendAcknowledgement: SendOrderAknowledgement = fun acknowledgement -> Sent

        let letter = createAcknowledgementLetter pricedOrder

        let acknowledge =
            { EmailAddress = pricedOrder.CustomerInfo.EmailAddress
              Letter = letter }

        match sendAcknowledgement acknowledge with
        | Sent ->
            let event =
                { OrderId = pricedOrder.OrderId
                  EmailAddress = pricedOrder.CustomerInfo.EmailAddress }

            Some event
        | NotSent -> None

// ---------------------------------
//  Create events
// ---------------------------------
let createBillingEvent (placedOrder: PricedOrder) : BillableOrderPlaced option =
    let billingAmount = placedOrder.AmmountToBill |> BillingAmount.value

    if billingAmount > 0M then
        let order =
            { OrderId = placedOrder.OrderId
              BillingAddress = placedOrder.BillingAddress
              AmmountToBill = placedOrder.AmmountToBill }

        Some order
    else
        None


// convert an Option into a List
let listOfOption opt =
    match opt with
    | Some x -> [ x ]
    | None -> []

let createEvents: CreateEvents =
    // fun pricedOrder acknowledgmentEventOpt ->
    fun acknowledgmentEventOpt ->
        // let events1 = pricedOrder |> PlaceOrderEvent.OrderPlaced |> List.singleton

        let events2 =
            acknowledgmentEventOpt
            |> Option.map PlaceOrderEvent.AcknowledgmentSent
            |> listOfOption

        // let events3 =
        //     pricedOrder
        //     |> createBillingEvent
        //     |> Option.map PlaceOrderEvent.BillableOrderPlaced
        //     |> listOfOption

        // [ yield! events1; yield! events2; yield! events3 ]
        [ yield! events2 ]



// -----------------------------------------------------------------
// Composing the pipeline steps together
// -----------------------------------------------------------------
let placeOrder unvalidatedOrder =

    let validateOrderAdapted input =
        input |> validateOrder |> Result.mapError PlaceOrderError.Validation

    let priceOrderAdapted input =
        input |> priceOrder |> Result.mapError PlaceOrderError.Pricing

    unvalidatedOrder
    |> validateOrderAdapted
    |> Result.bind priceOrderAdapted
    |> Result.map acknowledgeOrder
    |> Result.map createEvents


// let placeOrder
//     checkProductCodeExists // dependency
//     checkAddressExists // dependency
//     getProductPrice // dependency
//     createAcknowledgementLetter // dependency
//     sendAcknowledgement // dependency
//     : PlaceOrderWorkflow = // function definition

//     let validateOrder = validateOrder checkProductCodeExists checkAddressExists
//     let priceOrder = priceOrder getProductPrice

//     let acknowledgeOrder =
//         acknowledgeOrder createAcknowledgementLetter sendAcknowledgement

//     fun unvalidatedOrder ->
//         let pricedOrder = unvalidatedOrder |> validateOrder |> priceOrder
//         pricedOrder |> acknowledgeOrder |> createEvents pricedOrder

// WANT: complete pipeline like this
// unvalidatedOrder
// |> validateOrder
// |> priceOrder
// |> acknowledgeOrder
// |> createEvents
