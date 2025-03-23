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
type Price = Undefined

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
type ValidatedOrderLine = Undefined
type ValidatedOrder = {
  OrderID: ValueObject.OrderID
  CustomerInfo: CustomerInfo
  ShippingAddress: Address
  BillingAddress: Address
  OrderLines: ValidatedOrderLine list
}

// 価格計算済みの状態
// * Domain.Entity
type PricedOrderLine = Undefined
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
    -> ValidateOrder // 入力
    -> Result<PricedOrder, PricingError> // 出力
