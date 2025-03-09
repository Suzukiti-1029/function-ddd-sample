namespace OrderTaking.Domain

// 共通の型
open FSharpx.Collections

// TODO 未知の型
type Undefined = exn

type DateTime = Undefined
type Command<'data> = {
  Data: 'data
  Timestamp: DateTime
  UserID: string
  // TODO ...
}

// 型の定義

// 製品コード関連
type WidgetCode = WidgetCode of string
  // 制約: 先頭が"W"＋数字4桁
type GizmoCode = GizmoCode of string
  // 制約: 先頭が"G"＋数字3桁
type ProductCode =
  | Widget of WidgetCode
  | Gizmo of GizmoCode

// 注文数量関係
type UnitQuantity = private UnitQuantity of int
module UnitQuantity =
  // ユニット数の「スマートコンストラクタ」を定義
  let create qty =
    if qty < 1 then
      // 失敗
      Error "UnitQuantity can not be negative"
    else if qty > 1000 then
      // 失敗
      Error "UnitQuantity can not be more than 1000"
    else
      // 成功 -- 戻り値を構築
      Ok (UnitQuantity qty)
  let value(UnitQuantity qty) = qty

type KilogramQuantity = KilogramQuantity of decimal<
  Microsoft.FSharp.Data.UnitSystems.SI.UnitSymbols.kg
>
type OrderQuantity =
    | Unit of UnitQuantity
    | Kilos of KilogramQuantity

// 識別子
type OrderID = Undefined
type OrderLineID = Undefined
type CustomerID = Undefined

// 注文とその構成要素
type CustomerInfo = Undefined
type BillingAddress = Undefined

type UnValidatedAddress = UnValidateAddress of string
type ValidatedAddress = private ValidateAddress of string
module ValidateAddress =
  let value(ValidateAddress address) = address

type Price = Undefined
type BillingAmount = Undefined

type Order = {
    ID: OrderID // エンティティのID
    CustomerID: CustomerID // 顧客の参照
    ShippingAddress: ValidatedAddress
    BillingAddress: BillingAddress
    OrderLines: NonEmptyList<OrderLine>
    AmountToBill: BillingAmount
}
and OrderLine = {
  ID: OrderLineID // エンティティのID
  OrderID: OrderID
  ProductCode: ProductCode
  OrderQuantity: OrderQuantity
  Price: Price
}

// ワークフローの入力（コマンド）
type UnValidatedCustomerInfo = Undefined

type UnValidatedOrder = {
  OrderID: string
  CustomerInfo: UnValidatedCustomerInfo
  ShippingAddress: UnValidatedAddress
  // TODO ...
}

type PlaceOrder = Command<UnValidatedOrder>

type OrderTakingCommand =
  | Place of PlaceOrder
  // * 他のコマンドをひとまとめにする（チャネルなどで1つのデータ構造で受け取る時）
  // | Change of ChangeOrder
  // | Cancel of CancelOrder

// ワークフロー成功時の出力（イベント型）
type AcknowledgementSent = Undefined
type OrderPlaced = Undefined
type BillableOrderPlaced = Undefined

type PlaceOrderEvents = {
    AcknowledgementSent: AcknowledgementSent
    OrderPlaced: OrderPlaced
    BillableOrderPlaced: BillableOrderPlaced
}

// ワークフロー失敗時の出力（エラー型）
type ValidationError = {
    FieldName: string
    ErrorDescription: string
}

type PlaceOrderError =
  | ValidationError of ValidationError list
// TODO  | ... その他のエラー

// サブステップ：住所検証サービス
type AddressValidationService =
  UnValidatedAddress -> ValidatedAddress option // 失敗するかも

// 注文確定のワークフロー：「注文確定」プロセス
type PlacingOrder =
  UnValidatedOrder -> Result<PlaceOrderEvents, PlaceOrderError>
