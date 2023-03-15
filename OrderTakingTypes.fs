namespace OrderTaking.Domain

type Undefined = exn

type NonEmptyList<'a> = { First: 'a; Rest: 'a list }

// units of measure
[<Measure>]
type kg

[<Measure>]
type m

// Product code related
type WidgetCode = WidgetCode of string
// constraint: starting with "W" and 4 digits

type GizmoCode = GizmoCode of string
// constraint: starting with "G" and 3 digits

type ProductCode =
    | Widget of WidgetCode
    | Gizmo of GizmoCode


// Order Quantity related
type UnitQuantity = private UnitQuantity of int // between 1 and 1000
type KilogramQuantity = private KilogramQuantity of decimal<kg> // between 0.05 and 100.00

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
      OrderLines: NonEmptyList<OrderLine>
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
type VerifiedEmailAddress = private VerifiedEmailAddress of string
// only verification service create VerifiedEmailAddress

type CustomerEmail =
    | Unverified of EmailAddress
    | Verified of VerifiedEmailAddress

type EmailContactInfo = Undefined
type PostalContactInfo = Undefined

type ContactInfo =
    | EmailOnly of EmailContactInfo
    | AddrOnly of PostalContactInfo
    | EmailAndAddr of BothContactMethods

and BothContactMethods =
    { Email: EmailContactInfo
      Address: PostalContactInfo }

type Name = Undefined

[<NoEquality; NoComparison>]
type Contact =
    { Name: Name; ContactInfo: ContactInfo }


module UnitQuantity =
    let create qty =
        if qty < 1 then
            Error "UnitQuantity can not be negative"
        else if qty > 1000 then
            Error "UnitQuantity can not be more than 1000"
        else
            Ok(UnitQuantity qty)

    let value (UnitQuantity qty) = qty

module KilogramQuantity =
    let create qty =
        if qty < 0.05m<kg> then
            Error "KilogramQuantity can not be less than 0.05kg"
        else if qty > 100.00m<kg> then
            Error "KilogramQuantity can not be more than 100.00kg"
        else
            Ok(KilogramQuantity qty)

    let value (KilogramQuantity qty) = qty
