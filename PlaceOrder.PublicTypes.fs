namespace OrderTaking.PlaceOrder

open OrderTaking.Common

// types for public API

// -----------------------------------------------------
// Inputs to the workflow
type UnvalidatedCustomerInfo =
    { FirstName: string
      LastName: string
      EmailAddress: string }

type UnvalidatedAddress =
    { AddressLine1: string
      AddressLine2: string
      AddressLine3: string
      AddressLine4: string
      City: string
      ZipCode: string }

type UnvalidatedOrderLine =
    { OrderLineId: string
      ProductCode: string
      Quantity: decimal }

type UnvalidatedOrder =
    { OrderId: string
      CustomerInfo: UnvalidatedCustomerInfo
      ShippingAddress: UnvalidatedAddress
      BillingAddress: UnvalidatedAddress
      Lines: UnvalidatedOrderLine list }

// command
type Command<'data> =
    { Data: 'data
      Timestamp: DateTime
      UserId: string }

and DateTime = Undefined

type PlaceOrderCommand = Command<UnvalidatedOrder>

// -----------------------------------------------------
// Outputs from the workflow(success case)

// Event will be created if the acknowledgement was sucessfully posted
type OrderAcknowledgementSent =
    { OrderId: OrderId
      EmailAddress: EmailAddress }

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

// Event to send to shipping context
type OrderPlaced = PricedOrder

// Event to send to billing context
// This will only be created if the AmountToBill is not zero.
type BillableOrderPlaced =
    { OrderId: OrderId
      BillingAddress: Address
      AmmountToBill: BillingAmount }

// Possible events resulting from the PlaceOrder workflow
// Not all events will occur, depending on the logic of the workflow
type PlaceOrderEvent =
    | OrderPlaced of OrderPlaced
    | AcknowledgmentSent of OrderAcknowledgementSent
    | BillableOrderPlaced of BillableOrderPlaced


// -----------------------------------------------------
// The workflow itself
// Failure output of PlaceOrder workflow
type PlaceOrderError = ValidationError of ValidationError list

// type PlaceOrderWorkflow = PlaceOrderCommand -> AsyncResult<PlaceOrderEvent list, PlaceOrderError>
type PlaceOrderWorkflow = UnvalidatedOrder -> PlaceOrderEvent list // without effects
