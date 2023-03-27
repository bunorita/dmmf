namespace OrderTaking.Domain

open OrderTaking.Base


// types for public API

// Input data
type UnvalidatedOrder =
    { OrderId: string
      CustomerInfo: UnvalidatedCustomerInfo
      ShippingAddress: UnvalidatedAddress
      BillingAddress: UnvalidatedAddress
      Lines: UnvalidatedOrderLine list }

and UnvalidatedCustomerInfo =
    { FirstName: string
      LastName: string
      EmailAddress: string }

and UnvalidatedAddress =
    { AddressLine1: string
      AddressLine2: string
      AddressLine3: string
      AddressLine4: string
      City: string
      ZipCode: string }

and UnvalidatedOrderLine =
    { OrderLineId: string
      ProductCode: string
      Quantity: decimal }

// Input command
type Command<'data> =
    { Data: 'data
      Timestamp: DateTime
      UserId: string }

and DateTime = Undefined

type PlaceOrderCommand = Command<UnvalidatedOrder>

// Public API
// ----
// Success output of PlaceOrder workflow
type OrderPlaced = PricedOrder // for shipping

type BillableOrderPlaced =
    { OrderId: OrderId
      BillingAddress: Address
      AmmountToBill: BillingAmount } // for billing

type OrderAcknowledgementSent =
    { OrderId: OrderId
      EmailAddress: EmailAddress }

type PlaceOrderEvent =
    | OrderPlaced of OrderPlaced
    | BillableOrderPlaced of BillableOrderPlaced
    | AcknowledgmentSent of OrderAcknowledgementSent
// Failure output of PlaceOrder workflow
type PlaceOrderError = ValidationError of ValidationError list

type PlaceOrderWorkflow = PlaceOrderCommand -> AsyncResult<PlaceOrderEvent list, PlaceOrderError>
