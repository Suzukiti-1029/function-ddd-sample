namespace OrderTaking.PlaceOrderWorkflow

open OrderTaking.Domain
// ドメインAPIモジュールから型を持ってくる
open OrderTaking.DomainApi

// * Domain.ValueObject
type PersonalName = {
  FirstName: String50
  LastName: String50
}
// 製品コード関連
type WidgetCode = WidgetCode of string
  // 制約: 先頭が"W"＋数字4桁
type GizmoCode = GizmoCode of string
  // 制約: 先頭が"G"＋数字3桁
type ProductCode =
  | Widget of WidgetCode
  | Gizmo of GizmoCode
module ProductCode =
  let create (code: string) : ProductCode =
    if code.StartsWith "W" then
      if code.Length = 5 then
        let digits = code.Substring 1
        match System.Int32.TryParse digits with
        | true, _ -> Widget (WidgetCode code)
        | _ -> invalidArg "code" "WidgetCodeの数字部分は正しい数字4桁にしてください"
      else
        invalidArg "code" "WidgetCodeは「'W'+数字4桁」にしてください"
    elif code.StartsWith "G" then
      if code.Length = 4 then
        let digits = code.Substring 1
        match System.Int32.TryParse digits with
        | true, _ -> Gizmo (GizmoCode code)
        | _ -> invalidArg "code" "GizmoCodeの数字部分は正しい数字3桁にしてください。"
      else
        invalidArg "code" "GizmoCodeは「'G'+数字3桁」にしてください"
    else
        invalidArg "code" "ProductCodeは、「'W'+数字4桁」または「'G'+数字3桁」にしてください"

// 注文数量関係
type UnitQuantity = private UnitQuantity of int
module UnitQuantity =
  /// ユニット数の「スマートコンストラクタ」を定義
  let create qty =
    if qty < 1 then
      // TODO
      invalidArg "qty" "UnitQuantityは、0以下にしないでください"
    else if qty > 1000 then
      // TODO
      invalidArg "qty" "UnitQuantityは、1000超過にしないでください"
    else
      // 成功 -- 戻り値を構築
      UnitQuantity qty
  // let value(UnitQuantity qty) = qty

type KilogramQuantity = private KilogramQuantity of decimal<
  Data.UnitSystems.SI.UnitSymbols.kg
>
module KilogramQuantity =
  let create qty =
    if qty < 0.05M<Data.UnitSystems.SI.UnitSymbols.kg> then
      invalidArg "qty" "KilogramQuantityは、0.05未満にできません"
    elif qty > 100.00M<Data.UnitSystems.SI.UnitSymbols.kg> then
      invalidArg "qty" "KilogramQuantityは、100.00を超えることはできません"
    else
      KilogramQuantity qty

type OrderQuantity =
    | Unit of UnitQuantity
    | Kilogram of KilogramQuantity
module OrderQuantity =
  let value(orderQuantity) =
    match orderQuantity with
    | Unit (UnitQuantity qty) -> decimal qty
    | Kilogram (KilogramQuantity qty) ->
      qty
      |> (fun x -> x / 1.0M<Data.UnitSystems.SI.UnitSymbols.kg>) // 単位排除

// --------------------
// 注文のライフサイクル
// --------------------

// 検証済みの状態
// * Domain.ValueObject
type CustomerInfo = {
  Name: PersonalName
  EmailAddress: EmailAddress
}

// * Domain.Entity
type ValidatedOrderLine = {
  OrderLineID: ValueObject.OrderLineID
  ProductCode: ProductCode
  Quantity: OrderQuantity
}

type ValidatedOrder = {
  OrderID: ValueObject.OrderID
  CustomerInfo: CustomerInfo
  ShippingAddress: Address
  BillingAddress: Address
  OrderLines: ValidatedOrderLine list
}

// 価格計算済みの状態
// * Domain.Entity
type PricedOrderLine = {
  OrderLineID: ValueObject.OrderLineID
  ProductCode: ProductCode
  Quantity: OrderQuantity
  LinePrice: Price
}

type PricedOrder = {
  OrderID: ValueObject.OrderID // エンティティのID
  CustomerInfo: CustomerInfo
  ShippingAddress: Address
  BillingAddress: Address
  // 検証済の注文明細行とは異なる
  OrderLines: PricedOrderLine list
  AmountToBill: BillingAmount
}

// 全状態の結合
// * Domain.Entity(Aggregate)
type Order =
  | UnValidated of UnValidatedOrder
  | Validated of ValidatedOrder
  | Priced of PricedOrder
  // etc...

// --------------------
// 内部ステップの定義
// --------------------

// ----- 注文の検証 -----

// 注文の検証が使用するサービス
// * Domain.Interface.Provider <|.. Infrastructure.Provider
// エラーを返す可能性がなく、リモート呼び出しでもない（自律性）
// （ローカルなキャッシュコピーが利用可能） としている
type CheckProductCodeExists = ProductCode -> bool

// * Domain.Errors
type AddressValidationError = AddressValidationError of string

// * Domain.ValueObject
type CheckedAddress = CheckAddress of CheckedAddressData
and CheckedAddressData = {
    AddressLine1: string
    AddressLine2: string
    AddressLine3: string
    AddressLine4: string
    City: string
    ZipCode: string
}

// * Domain.Interface.Remote <|.. Infrastructure.Remote
// リモートのサービスを呼び出している
type CheckAddressExists =
  UnValidatedAddress -> AsyncResult<CheckedAddress, AddressValidationError>

// * Usecases.Workflows
type ValidateOrder =
  CheckProductCodeExists // 依存関係
    -> CheckAddressExists // 依存関係
    -> UnValidatedOrder // 入力
    -> AsyncResult<ValidatedOrder, ValidationError list> // 出力

// ----- 注文の価格計算 -----

// 注文の価格計算が使用するサービス
// * Domain.Interface.Provider <|.. Infrastructure.Provider
type GetProductPrice = ProductCode -> Price

// * Domain.Errors
type PricingError = PricingError of string

// * Usecases.Workflows
// サブステップ自体にエラーが発生する可能性がある（結果が馬鹿でかい値やマイナス値など）
type PriceOrder =
  GetProductPrice // 依存関係
    -> ValidatedOrder // 入力
    -> Result<PricedOrder, PricingError> // 出力
