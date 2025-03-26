namespace OrderTaking.DomainApi

open OrderTaking.Domain

// * Undefined
// TODO 未知の型
type Undefined = exn

// * Domain.Shared
type AsyncResult<'success, 'failure> = Async<Result<'success, 'failure>>

// * Domain.ValueObject
type UserID = UserID of Undefined

type Address = {
  AddressLine1: String50
  AddressLine2: String50 option
  AddressLine3: String50 option
  AddressLine4: String50 option
  City: String50
  ZipCode: ZipCode
}

type Price = private Price of decimal
module Price =
  let create price =
    if price < 0m then
      invalidArg "price" "Priceは、負の値にしないでください。"
    else
      Price price
  let value(Price p) =
    p
  let multiply qty (Price p) =
    create (qty * p)

type BillingAmount = private BillingAmount of decimal
module BillingAmount =
  let create amount =
    if amount < 0m then
      invalidArg "amount" "BillingAmountは負の値にしないでください"
    // とりあえず１億以上はアウト
    elif amount >= 100000000m then
      invalidArg "amount" "BillingAmountは1億以上にしないでください"
    else
      BillingAmount amount
  let sumPrices prices =
    let total = prices |> List.map Price.value |> List.sum
    create total

// * Domain.Errors
type ValidationError = {
    FieldName: string
    ErrorDescription: string
}

// --------------------
// 入力
// --------------------
// * Domain.ValueObject
type UnValidatedCustomer = {
  FirstName: string
  LastName: string
  EmailAddress: string
}
type UnValidatedAddress = UnValidateAddress of string

// * Domain.Entity
type UnValidatedOrderLine = {
  OrderLineID: string
  ProductCode: string
  Quantity: decimal<Data.UnitSystems.SI.UnitSymbols.kg>
}

type UnValidatedOrder = {
  OrderID: string
  CustomerInfo: UnValidatedCustomer
  ShippingAddress: UnValidatedAddress
  BillingAddress: UnValidatedAddress
  OrderLines: UnValidatedOrderLine list
}

// --------------------
// 入力コマンド
// --------------------
// * Domain.Shared
type Command<'data> = {
  Data: 'data
  Timestamp: System.DateTime
  UserID: UserID
  // TODO etc...
}

// * Usecases.Command
type PlaceOrderCommand = Command<UnValidatedOrder>

// --------------------
// パブリックAPI
// --------------------

// 受注確定ワークフローの成功出力
// * Domain.Events
type OrderPlaced = PricedOrder
type BillableOrderPlaced = {
  OrderID: ValueObject.OrderID
  BillingAddress: Address
  AmountToBill: BillingAmount
}
type OrderAcknowledgmentSent = {
  OrderID: ValueObject.OrderID
  EmailAddress: EmailAddress
}
type PlaceOrderEvent =
  | OrderPlaced of OrderPlaced
  | BillableOrderPlaced of BillableOrderPlaced
  | AcknowledgementSent of OrderAcknowledgmentSent

// 受注確定ワークフローの失敗出力
// * Domain.Errors
type PlaceOrderError =
  | ValidationError of ValidationError list
  // TODO  | etc... その他のエラー

// * Usecases.Workflows
type PlaceOrderWorkflow =
  PlaceOrderCommand // 入力
    -> AsyncResult<PlaceOrderEvent list, PlaceOrderError> // 出力
