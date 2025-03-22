namespace OrderTaking.DomainApi

open OrderTaking.Domain

// * Undefined
// TODO 未知の型
type Undefined = exn

// * Domain.Shared
type AsyncResult<'success, 'failure> = Async<Result<'success, 'failure>>

// * Domain.ValueObject
type UserID = UserID of Undefined
type Address = Address of string
type BillingAmount = Undefined
type EmailAddress = Undefined

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
  Name: string
  Email: string
}
type UnValidatedAddress = UnValidateAddress of string

// * Domain.Entity
type UnValidatedOrder = {
  OrderID: string
  CustomerInfo: UnValidatedCustomer
  ShippingAddress: UnValidatedAddress
  // TODO etc...
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
