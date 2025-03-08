namespace OrderTaking.Domain

// 型の定義

// TODO 未知の型
type Undefined = exn

// 製品コード関連
type WidgetCode = WidgetCode of string
  // 制約: 先頭が"W"＋数字4桁
type GizmoCode = GizmoCode of string
  // 制約: 先頭が"G"＋数字3桁
type ProductCode =
  | Widget of WidgetCode
  | Gizmo of GizmoCode

// 注文数量関係
type UnitQuantity = UnitQuantity of int
type KilogramQuantity = KilogramQuantity of decimal
type OrderQuantity =
    | Unit of UnitQuantity
    | Kilos of KilogramQuantity

// 識別子
type OrderID = Undefined
type OrderLineID = Undefined
type CustomerID = Undefined

// 注文とその構成要素
type CustomerInfo = Undefined
type ShippingAddress = Undefined
type BillingAddress = Undefined
type Price = Undefined
type BillingAmount = Undefined

type Order = {
    ID: OrderID // エンティティのID
    CustomerID: CustomerID // 顧客の参照
    ShippingAddress: ShippingAddress
    BillingAddress: BillingAddress
    OrderLines: OrderLine list
    AmountToBill: BillingAmount
}
and OrderLine = {
  ID: OrderLineID // エンティティのID
  OrderID: OrderID
  ProductCode: ProductCode
  OrderQuantity: OrderQuantity
  Price: Price
}

// ワークフローの入力
type UnValidatedOrder = {
  OrderID: string
  CustomerInfo: Undefined
  ShippingAddress: Undefined
  // TODO ...
}

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

// 注文確定のワークフロー：「注文確定」プロセス
type PlaceOrder =
  UnValidatedOrder -> Result<PlaceOrderEvents, PlaceOrderError>
