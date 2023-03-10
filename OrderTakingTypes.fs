namespace OrderTaking.Domain

type Undefined = exn

// Product code related
type WidgetCode = WidgetCode of string
// constraint: starting with "W" and 4 digits

type GizmoCode = GizmoCode of string
// constraint: starting with "G" and 3 digits

type ProductCode =
    | Widget of WidgetCode
    | Gizmo of GizmoCode


// Order Quantity related
type UnitQuantity = UnitQuantity of int
type KilogramQuantity = KilogramQuantity of decimal

type OrderQuantity =
    | Unit of UnitQuantity
    | Kilos of KilogramQuantity


// Order related
type OrderId = Undefined
type OrderLineId = Undefined
type CustomerId = Undefined
type CustomerInfo = Undefined
type ShippingAddress = Undefined
type BillingAddress = Undefined
type Price = Undefined
type BillingAmount = Undefined

type Order =
    { Id: OrderId
      CustomerId: CustomerId
      ShippingAddress: ShippingAddress
      BillingAddress: BillingAddress
      OrderLines: OrderLine list
      AmountToBill: BillingAmount }

and OrderLine =
    { Id: OrderLineId
      OrderId: OrderId
      ProductCode: ProductCode
      OrderQuantity: OrderQuantity
      Price: Price }


// events
type AcknowledgmentSent = Undefined
type OrderPlaced = Undefined // for shipping
type BillableOrderPlaced = Undefined // for billing

// functions
type UnvalidatedOrder = Undefined
type ValidatedOrder = Undefined
type ValidationResponse<'a> = Async<Result<'a, ValidationError list>>

and ValidationError =
    { FieldName: string
      ErrorDescription: string }

type ValidateOrder = UnvalidatedOrder -> ValidationResponse<ValidatedOrder>

type PlaceOrderError = ValidationError of ValidationError list
// | other errors

type PlaceOrderEvents =
    { AcknowledgmentSent: AcknowledgmentSent
      OrderPlaced: OrderPlaced
      BillableOrderPlaced: BillableOrderPlaced }

type PlaceOrder = UnvalidatedOrder -> Result<PlaceOrderEvents, PlaceOrderError>

type QuoteForm = Undefined
type OrderForm = Undefined

type CategolizedMail =
    | Quote of QuoteForm
    | Order of OrderForm

type EnvelopeContents = Undefined
type CategolizeInboundMail = EnvelopeContents -> CategolizedMail

type ProductCatalog = Undefined
type PricedOrder = Undefined
type CalculatePrices = OrderForm -> ProductCatalog -> PricedOrder

type PersonalName =
    { FirstName: string
      MiddleInitial: string option
      LastName: string }

type ContactId = ContactId of int
type PhoneNumber = PhoneNumber of string
type EmailAddress = EmailAddress of string

[<NoEquality; NoComparison>]
type Contact =
    { ContactId: ContactId
      PhoneNumber: PhoneNumber
      EmailAddress: EmailAddress }
