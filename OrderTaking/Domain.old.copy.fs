namespace OrderTaking.DomainOldCopy

// ! 共通の型
// 未使用
// open FSharpx.Collections

// TODO 未知の型
type Undefined = exn

type DateTime = Undefined
type Command<'data> = {
  Data: 'data
  Timestamp: DateTime
  UserID: string
  // TODO etc...
}

type AsyncResult<'success, 'failure> = Async<Result<'success, 'failure>>

// ! 型の定義

// * 製品コード関連
type WidgetCode = WidgetCode of string
  // 制約: 先頭が"W"＋数字4桁
type GizmoCode = GizmoCode of string
  // 制約: 先頭が"G"＋数字3桁
type ProductCode =
  | Widget of WidgetCode
  | Gizmo of GizmoCode

// * 注文数量関係
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
  Data.UnitSystems.SI.UnitSymbols.kg
>
type OrderQuantity =
    | Unit of UnitQuantity
    | Kilos of KilogramQuantity

// * 識別子
type OrderID = Undefined
type OrderLineID = Undefined
type CustomerID = Undefined

// * 注文とその構成要素
type UnValidatedCustomerInfo = Undefined
type UnValidatedAddress = UnValidateAddress of string

type UnValidatedOrder = {
  OrderID: string
  CustomerInfo: UnValidatedCustomerInfo
  ShippingAddress: UnValidatedAddress
  // TODO etc...
}

type CustomerInfo = Undefined
type Address = Address of string
type ValidatedOrderLine = Undefined

type ValidatedOrder = {
  OrderID: OrderID // エンティティのID
  CustomerInfo: CustomerInfo
  ShippingAddress: Address
  BillingAddress: Address
  OrderLines: ValidatedOrderLine list
}

type PricedOrderLine = Undefined
type BillingAmount = Undefined
type PricedOrder = {
  OrderID: OrderID // エンティティのID
  CustomerInfo: CustomerInfo
  ShippingAddress: Address
  BillingAddress: Address
  // 検証済の注文明細行とは異なる
  OrderLines: PricedOrderLine list
  AmountToBill: BillingAmount
}

type Order =
  | UnValidated of UnValidatedOrder
  | Validated of ValidatedOrder
  | Priced of PricedOrder
  // etc...

// // type BillingAddress = Undefined
// // type ValidatedAddress = private ValidateAddress of string
// // module ValidateAddress =
// //   let value(ValidateAddress address) = address

// // type Order = {
// //     ID: OrderID // エンティティのID
// //     CustomerID: CustomerID // 顧客の参照
// //     ShippingAddress: ValidatedAddress
// //     BillingAddress: BillingAddress
// //     OrderLines: NonEmptyList<OrderLine>
// //     AmountToBill: BillingAmount
// // }
// // and OrderLine = {
// //   ID: OrderLineID // エンティティのID
// //   OrderID: OrderID
// //   ProductCode: ProductCode
// //   OrderQuantity: OrderQuantity
// //   Price: Price
// // }
// // // 住所検証サービス
// // type AddressValidationService =
// //   UnValidatedAddress -> ValidatedAddress option // 失敗するかも

// * エラー
type ValidationError = {
    FieldName: string
    ErrorDescription: string
}

// * ワークフローの入力（コマンド）
type PlaceOrder = Command<UnValidatedOrder>

type OrderTakingCommand =
  | Place of PlaceOrder
  // * 他のコマンドをひとまとめにする（チャネルなどで1つのデータ構造で受け取る時）
  // | Change of ChangeOrder
  // | Cancel of CancelOrder

// * サブステップ：検証
// ? 依存関係：製品コード存在確認サービス
// エラーを返す可能性がなく、リモート呼び出しでもない（自律性）
// （ローカルなキャッシュコピーが利用可能） としている
type CheckProductCodeExists = ProductCode -> bool

// ? 依存関係：住所存在確認サービス
// TODO 仮
type CheckedAddress = CheckedAddress of UnValidatedAddress

type AddressValidationError = AddressValidationError of string
// リモートのサービスを呼び出している
type CheckAddressExists =
  UnValidatedAddress -> AsyncResult<CheckedAddress, AddressValidationError>

type ValidateOrder =
  CheckProductCodeExists // 依存関係
    -> CheckAddressExists // 依存関係
    -> UnValidatedOrder // 入力
    -> AsyncResult<ValidatedOrder, ValidationError list> // 出力

// * サブステップ：価格計算
// ? 依存関係：価格計算サービス
type Price = Undefined
type GetProductPrice = ProductCode -> Price

type PricingError = PricingError of string
// サブステップ自体にエラーが発生する可能性がある（結果が馬鹿でかい値やマイナス値など）
type PriceOrder =
  GetProductPrice // 依存関係
    -> ValidateOrder // 入力
    -> Result<PricedOrder, PricingError> // 出力

// * サブステップ：注文確認
// ? 依存関係：注文確認の文書生成サービス
type HTMLString = HTMLString of string
type EmailAddress = Undefined
// ローカル関数
type OrderAcknowledgment = {
  EmailAddress: EmailAddress
  Letter: HTMLString
}
type CreateOrderAcknowledgementLetter =
  PricedOrder -> HTMLString

// ? 依存関係：注文確認送信サービス
type SendResult = Sent | NotSent
// I/O処理をするが、エラーは気にしない
type SendOrderAcknowledgment =
  OrderAcknowledgment -> Async<SendResult>

type OrderAcknowledgmentSent = {
  OrderID: OrderID
  EmailAddress: EmailAddress
}
type AcknowledgeOrder =
  CreateOrderAcknowledgementLetter // 依存関係
    -> SendOrderAcknowledgment // 依存関係
    -> PricedOrder // 入力
    // 注文書が送信されていない可能性
    -> Async<OrderAcknowledgmentSent option> // 出力

// * サブステップ：イベント作成・返却
// * （ワークフロー成功時の出力（イベント型））
type OrderPlaced = PricedOrder
type BillableOrderPlaced = {
  OrderID: OrderID
  BillingAddress: Address
  AmountToBill: BillingAmount
}
type PlaceOrderEvent =
  | OrderPlaced of OrderPlaced
  | BillableOrderPlaced of BillableOrderPlaced
  | AcknowledgementSent of OrderAcknowledgmentSent
type CreateEvents =
  PricedOrder -> PlaceOrderEvent list

// * ワークフロー失敗時の出力（エラー型）
type PlaceOrderError =
  | ValidationError of ValidationError list
// TODO  | etc... その他のエラー

// * 注文確定のワークフロー：「注文確定」プロセス
type PlaceOrderWorkflow =
  PlaceOrder // 入力
    -> AsyncResult<PlaceOrderEvent list, PlaceOrderError> // 出力
