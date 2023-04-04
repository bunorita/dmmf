namespace OrderTaking.Common

open System

type Undefined = exn

type NonEmptyList<'a> = { First: 'a; Rest: 'a list }

type String50 = private String50 of string

// units of measure
[<Measure>]
type kg

[<Measure>]
type m

// Product code related
type WidgetCode = private WidgetCode of string
// constraint: starting with "W" and 4 digits

type GizmoCode = private GizmoCode of string
// constraint: starting with "G" and 3 digits

type ProductCode =
    | Widget of WidgetCode
    | Gizmo of GizmoCode


// Order Quantity related
type UnitQuantity = private UnitQuantity of int // between 1 and 1000
type KilogramQuantity = private KilogramQuantity of decimal<kg> // between 0.05 and 100.00

type OrderQuantity =
    | Unit of UnitQuantity
    | Kilogram of KilogramQuantity


// Order related
type Address =
    { AddressLine1: String50
      AddressLine2: String50
      AddressLine3: String50
      AddressLine4: String50
      City: String50
      ZipCode: ZipCode }

and City = City of string
and ZipCode = ZipCode of string

type OrderId = private OrderId of string
type OrderLineId = private OrderLineId of string
type CustomerId = Undefined
type ShippingAddress = Undefined
type BillingAddress = Undefined
type Price = private Price of decimal
type BillingAmount = private BillingAmount of decimal

and OrderLine =
    { Id: OrderLineId
      OrderId: OrderId
      ProductCode: ProductCode
      OrderQuantity: OrderQuantity
      Price: Price }

// functions




type AsyncResult<'success, 'failure> = Async<Result<'success, 'failure>>





type PersonalName =
    { FirstName: String50
      //   MiddleInitial: string option
      LastName: String50 }

type ContactId = ContactId of int
type PhoneNumber = PhoneNumber of string
type EmailAddress = EmailAddress of string

type CustomerInfo =
    { Name: PersonalName
      EmailAddress: EmailAddress }

type VerifiedEmailAddress = private VerifiedEmailAddress of string

type ValidationError =
    { FieldName: string
      ErrorDescription: string }

type QuoteForm = Undefined
type OrderForm = Undefined

type CategolizedMail =
    | Quote of QuoteForm
    | Order of OrderForm

type EnvelopeContents = Undefined
type CategolizeInboundMail = EnvelopeContents -> CategolizedMail

type ProductCatalog = Undefined
// type CalculatePrices = OrderForm -> ProductCatalog -> PricedOrder


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

// Command
// type OrderTakingCommand =
//     | Place of PlaceOrder
//     | Change of ChangeOrder
//     | Cancel of CancelOrder

and ChangeOrder = Undefined
and CancelOrder = Undefined

module String50 =

    let create str =
        // TODO: validation
        String50 str

    let createOption str =
        // TODO: Implementation
        String50 str

module EmailAddress =
    // TODO
    let create str = EmailAddress str

module ZipCode =
    let create str = ZipCode str

module OrderId =
    // Smart constructor
    let create str =
        if String.IsNullOrEmpty(str) then
            failwith "OrderId must not be null or empty"
        elif str.Length > 50 then
            failwith "OrderId must not be more than 50 chars"
        else
            OrderId str

    // Extract the inner value
    let value (OrderId str) = str

module OrderLineId =
    let create str = OrderLineId str

module WidgetCode =
    let create str = WidgetCode str

module GizmoCode =
    let create str = GizmoCode str

module ProductCode =
    let create code : ProductCode =
        // TODO: more validation
        if String.IsNullOrEmpty(code) then
            failwith "product code must not be null or empty"
        else if code.StartsWith("W") then
            WidgetCode.create code |> Widget
        else if code.StartsWith("G") then
            GizmoCode.create code |> Gizmo
        else
            failwith "invalid product code"

module UnitQuantity =
    let create qty =
        // TODO: return Result
        if qty < 1 then
            // Error "UnitQuantity can not be negative"
            UnitQuantity 0
        else if qty > 1000 then
            // Error "UnitQuantity can not be more than 1000"
            UnitQuantity 1000
        else
            // Ok(UnitQuantity qty)
            UnitQuantity qty

    let value (UnitQuantity qty) = qty

module KilogramQuantity =
    let create qty =
        // TODO: return Result
        if qty < 0.05m<kg> then
            // Error "KilogramQuantity can not be less than 0.05kg"
            KilogramQuantity 0.00m<kg>
        else if qty > 100.00m<kg> then
            // Error "KilogramQuantity can not be more than 100.00kg"
            KilogramQuantity 100.00m<kg>
        else
            // Ok(KilogramQuantity qty)
            KilogramQuantity qty

    let value (KilogramQuantity qty) = qty

module Price =
    let create v = Price v
    let value (Price v) = v

    let multipy qty (Price p) = qty * p |> create

module BillingAmount =
    let create amount = BillingAmount amount
    let value (BillingAmount v) = v

    let sumPrices prices =
        let total = prices |> List.map Price.value |> List.sum
        create total

module OrderQuantity =
    let value qty =
        match qty with
        | Unit uq -> uq |> UnitQuantity.value |> decimal
        | Kilogram kgq -> kgq |> KilogramQuantity.value |> decimal
