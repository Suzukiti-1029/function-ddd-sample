namespace OrderTaking.DomainWork

open OrderTaking.Domain
open OrderTaking.DomainApi
open OrderTaking.PlaceOrderWorkflow

// ! 共通の型

// TODO 未知の型
type Undefined = exn

// ! 型の定義

// * 識別子

// * 注文とその構成要素

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

// * ワークフローの入力（コマンド）
type OrderTakingCommand =
  | Place of PlaceOrderCommand
  // * 他のコマンドをひとまとめにする（チャネルなどで1つのデータ構造で受け取る時）
  // | Change of ChangeOrder
  // | Cancel of CancelOrder

// * サブステップ：検証
// ? 依存関係：製品コード存在確認サービス

// ? 依存関係：住所存在確認サービス

// * サブステップ：価格計算
// ? 依存関係：価格計算サービス

// * サブステップ：注文確認
// ? 依存関係：注文確認の文書生成サービス
// ? 依存関係：注文確認送信サービス

// * サブステップ：イベント作成・返却
// * （ワークフロー成功時の出力（イベント型））

// * ワークフロー失敗時の出力（エラー型）

// * 注文確定のワークフロー：「注文確定」プロセス
