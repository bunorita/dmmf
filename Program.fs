type Undefined = exn

type CustomerId = CustomerId of int
type OrderId = OrderId of int
type ProductId = ProductId of string

type CustomerInfo = Undefined
type ShippingAddress = Undefined
type BillingAddress = Undefined

[<NoEquality; NoComparison>]
type OrderLine =
    { OrderId: OrderId
      ProductId: ProductId
      Qty: int }

    member this.Key = (this.OrderId, this.ProductId)


type BillingAmount = Undefined

type Order =
    { CustomerInfo: CustomerInfo
      ShippingAddress: ShippingAddress
      BillingAddress: BillingAddress
      OrderLines: OrderLine list
      AmountToBill: BillingAmount }

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



type PlaceOrderEvents =
    { AcknowledgmentSent: AcknowledgmentSent
      OrderPlaced: OrderPlaced
      BillableOrderPlaced: BillableOrderPlaced }

type PlaceOrder = UnvalidatedOrder -> PlaceOrderEvents

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


let line1 =
    { OrderId = OrderId 1
      ProductId = ProductId "abc"
      Qty = 100 }

printfn $"{line1.Key}"
