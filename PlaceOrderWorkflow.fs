module internal OrderTaking.Domain.Implementation

open OrderTaking.Base


// ---------------------
// Order lifecycle
// ---------------------

// validated state
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


// priced state
type PricedOrderLine =
    { OrderLineId: OrderLineId
      ProductCode: ProductCode
      Quantity: OrderQuantity
      LinePrice: Price }

type PricedOrder =
    { OrderId: OrderId
      CustomerInfo: CustomerInfo
      ShippingAddress: Address
      BillingAddress: Address
      OrderLines: PricedOrderLine list
      AmmountToBill: BillingAmount }

// all states combined
type Order =
    | Unvalidated of UnvalidatedOrder
    | Validated of ValidatedOrder
    | Priced of PricedOrder


// -----------
// Internal steps
// -----------

// ----- Validate order
type CheckProductCodeExists = ProductCode -> bool
type AddressValidationError = AddressValidationError of string
type CheckedAddress = CheckedAddress of UnvalidatedAddress
// type CheckAddressExists = UnvalidatedAddress -> AsyncResult<CheckedAddress, AddressValidationError>
type CheckAddressExists = UnvalidatedAddress -> CheckedAddress // without effects for now

type ValidateOrder =
    CheckProductCodeExists // dependency
        -> CheckAddressExists // dependency
        -> UnvalidatedOrder // input
        // -> AsyncResult<ValidatedOrder, ValidationError list> // output
        -> ValidatedOrder // output, without effects

let toCustomerInfo (customer: UnvalidatedCustomerInfo) : CustomerInfo =
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

let toValidatedOrderLine checkProductCodeExists (unvalidatedOrderLine: UnvalidatedOrderLine) =
    let orderLineId = unvalidatedOrderLine.OrderLineId |> OrderLineId.create

    let productCode =
        unvalidatedOrderLine.ProductCode |> toProductCode checkProductCodeExists

    let quantity = unvalidatedOrderLine.Quantity |> toOrderQuantity productCode

    { OrderLineId = orderLineId
      ProductCode = productCode
      Quantity = quantity }


let validateOrder: ValidateOrder =
    fun checkProductCodeExists checkAddressExists unvalidatedOrder ->
        let orderId = unvalidatedOrder.OrderId |> OrderId.create
        let customerInfo = unvalidatedOrder.CustomerInfo |> toCustomerInfo
        let shippingAddr = unvalidatedOrder.ShippingAddress |> toAddress checkAddressExists
        let billingAddr = unvalidatedOrder.BillingAddress |> toAddress checkAddressExists

        let orderLines =
            unvalidatedOrder.Lines |> List.map (toValidatedOrderLine checkProductCodeExists)

        { OrderId = orderId
          CustomerInfo = customerInfo
          ShippingAddress = shippingAddr
          BillingAddress = billingAddr
          OrderLines = orderLines }

// ----- Price order
type GetProductPrice = ProductCode -> Price
type PricingError = PricingError of string

type PriceOrder =
    GetProductPrice // dependency
        -> ValidatedOrder // input
        -> PricedOrder // output: without effects for now
// -> Result<PricedOrder, PricingError> // output

let toPricedOrderLine getProductPrice (line: ValidatedOrderLine) : PricedOrderLine =
    let qty = line.Quantity |> OrderQuantity.value
    let price = line.ProductCode |> getProductPrice
    let linePrice = price |> Price.multipy qty

    { OrderLineId = line.OrderLineId
      ProductCode = line.ProductCode
      Quantity = line.Quantity
      LinePrice = linePrice }


let priceOrder: PriceOrder =
    fun getProductPrice validatedOrder ->
        let lines =
            validatedOrder.OrderLines |> List.map (toPricedOrderLine getProductPrice)

        let amountToBill =
            lines |> List.map (fun line -> line.LinePrice) |> BillingAmount.sumPrices

        { OrderId = validatedOrder.OrderId
          CustomerInfo = validatedOrder.CustomerInfo
          ShippingAddress = validatedOrder.ShippingAddress
          BillingAddress = validatedOrder.BillingAddress
          OrderLines = lines
          AmmountToBill = amountToBill }
