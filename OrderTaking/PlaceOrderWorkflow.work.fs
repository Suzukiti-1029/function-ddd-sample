namespace OrderTaking.PlaceOrderWorkflowWork

// ! PlaceOrderWorkflowの型定義のエフェクトを排除したものを実装している

open OrderTaking.Domain
// ドメインAPIモジュールから型を持ってくる
open OrderTaking.DomainApi
open OrderTaking.PlaceOrderWorkflow

// // * Domain.ValueObject
// // 製品コード関連
// type WidgetCode = WidgetCode of string
//   // 制約: 先頭が"W"＋数字4桁
// type GizmoCode = GizmoCode of string
//   // 制約: 先頭が"G"＋数字3桁
// type ProductCode =
//   | Widget of WidgetCode
//   | Gizmo of GizmoCode
// type Price = Undefined

// --------------------
// 注文のライフサイクル
// --------------------

// 検証済みの状態
// // * Domain.ValueObject
// type ValidatedOrderLine = Undefined

// type CustomerInfo = Undefined

// // * Domain.Entity
// type ValidatedOrder = {
//   OrderID: ValueObject.OrderID
//   CustomerInfo: CustomerInfo
//   ShippingAddress: Address
//   BillingAddress: Address
//   OrderLines: ValidatedOrderLine list
// }

// 価格計算済みの状態
// // * Domain.ValueObject
// type PricedOrderLine = Undefined

// // * Domain.Entity
// type PricedOrder = {
//   OrderID: ValueObject.OrderID // エンティティのID
//   CustomerInfo: CustomerInfo
//   ShippingAddress: Address
//   BillingAddress: Address
//   // 検証済の注文明細行とは異なる
//   OrderLines: PricedOrderLine list
//   AmountToBill: BillingAmount
// }

// 全状態の結合
// // * Domain.Entity(Aggregate)
// type Order =
//   | UnValidated of UnValidatedOrder
//   | Validated of ValidatedOrder
//   | Priced of PricedOrder
//   // etc...

// --------------------
// 内部ステップの定義
// --------------------

// ----- 注文の検証 -----

// 注文の検証が使用するサービス
// // * Domain.Interface.Provider <|.. Infrastructure.Provider
// // エラーを返す可能性がなく、リモート呼び出しでもない（自律性）
// // （ローカルなキャッシュコピーが利用可能） としている
// type CheckProductCodeExists = ProductCode -> bool

// // * Domain.Errors
// type AddressValidationError = AddressValidationError of string

// // * Domain.ValueObject
// type CheckedAddress = CheckedAddress of Undefined

// * Domain.Interface.Remote <|.. Infrastructure.Remote
// リモートのサービスを呼び出している
type CheckAddressExists =
  UnValidatedAddress -> CheckedAddress

// * Usecases.Workflows
type ValidateOrder =
  CheckProductCodeExists // 依存関係
    -> CheckAddressExists // 依存関係
    -> UnValidatedOrder // 入力
    -> ValidatedOrder

// ----- 注文の価格計算 -----

// 注文の価格計算が使用するサービス
// // * Domain.Interface.Provider <|.. Infrastructure.Provider
// type GetProductPrice = ProductCode -> Price

// // * Domain.Errors
// type PricingError = PricingError of string

// // * Usecases.Workflows
// // サブステップ自体にエラーが発生する可能性がある（結果が馬鹿でかい値やマイナス値など）
// type PriceOrder =
//   GetProductPrice // 依存関係
//     -> ValidateOrder // 入力
//     -> Result<PricedOrder, PricingError> // 出力

// --------------------
// 実装
// --------------------
// TODO 未整理モジュール
module InComplete =
  let toCustomerInfo (customer: UnValidatedCustomer) : CustomerInfo =
    // 顧客情報の各種プロパティを作成する
    // 無効な場合は例外を投げる
    let firstName = customer.FirstName |> String50.create
    let lastName = customer.LastName |> String50.create
    let emailAddress = customer.EmailAddress |> EmailAddress.create

    // 個人名を作成する
    let name: PersonalName = {
      FirstName = firstName
      LastName = lastName
    }

    // 顧客情報を作成する
    let customerInfo: CustomerInfo = {
      Name = name
      EmailAddress = emailAddress
    }
    // そしてそれを返す
    customerInfo

  let toAddress (checkAddressExists: CheckAddressExists) unValidatedAddress =
    // リモートサービスを呼び出す
    let checkedAddress = checkAddressExists unValidatedAddress
    // パターンマッチを使用して内部値を抽出する
    let (CheckAddress checkedAddress) = checkedAddress

    let addressLine1 =
      checkedAddress.AddressLine1 |> String50.create
    let addressLine2 =
      checkedAddress.AddressLine2 |> String50.createOption
    let addressLine3 =
      checkedAddress.AddressLine3 |> String50.createOption
    let addressLine4 =
      checkedAddress.AddressLine4 |> String50.createOption
    let city =
      checkedAddress.City |> String50.create
    let zipCode =
      checkedAddress.ZipCode |> ZipCode.create
    // 住所を作成する
    let address: Address = {
      AddressLine1 = addressLine1
      AddressLine2 = addressLine2
      AddressLine3 = addressLine3
      AddressLine4 = addressLine4
      City = city
      ZipCode = zipCode
    }
    // 住所を返す
    address

module Workflows =
  let validateOrder: ValidateOrder =
    fun checkProductCodeExists checkAddressExists unValidatedOrder ->

    let orderID =
      unValidatedOrder.OrderID
      |> ValueObject.OrderID.create

    let customerInfo =
      unValidatedOrder.CustomerInfo
      |> InComplete.toCustomerInfo // ヘルパー関数

    let shippingAddress =
      unValidatedOrder.ShippingAddress
      |> InComplete.toAddress checkAddressExists // ヘルパー関数

    // TODO unValidatedOrder の各プロパティに対して同様に行う

    // すべてのフィールドの準備ができたら、それらを使って
    // 新しい「検証済みの注文」レコードを作成し、返す
    {
      OrderID = orderID
      CustomerInfo = customerInfo
      ShippingAddress = shippingAddress
      // BillingAddress = // TODO ...
      // Lines = // TODO ...
    }
