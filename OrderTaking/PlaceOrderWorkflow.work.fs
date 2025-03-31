namespace OrderTaking.PlaceOrderWorkflowWork

// ! PlaceOrderWorkflowの型定義のエフェクトを排除したものを実装している

open OrderTaking.Domain
// ドメインAPIモジュールから型を持ってくる
open OrderTaking.DomainApi
open OrderTaking.PlaceOrderWorkflow

// TODO InComplete 未整理モジュール
type ServiceInfo = {
  Name: string
  Endpoint: System.Uri
}
type RemoteServiceError = {
  Service: ServiceInfo
  Exception: System.Exception
}

// * Domain.Errors
type PlaceOrderError =
  | Validation of ValidationError
  | Pricing of PricingError
  | RemoteServiceError of RemoteServiceError

// ワークフローそれ自体
// * Usecases.Workflows
type PlaceOrderWorkflow =
  PlaceOrderCommand // 入力
    -> PlaceOrderEvent list // 出力

// TODO InComplete 未整理モジュール
type CheckAddressExistsR =
  UnValidatedAddress -> Result<CheckedAddress, PlaceOrderError>

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

// ==============================
// パート1: 設計
// ==============================

// ワークフローのパブリックな部分（API）は、
// `PlaceOrderWorkflow`関数やその入力である
// `UnValidatedOrder`のように別の場所で定義されています。
// 以下の型はワークフロー実装に対し、プライベートです。

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
    -> Result<ValidatedOrder, ValidationError>

// ----- 注文の価格計算 -----

// 注文の価格計算が使用するサービス
// // * Domain.Interface.Provider <|.. Infrastructure.Provider
// type GetProductPrice = ProductCode -> Price

// // * Domain.Errors
// type PricingError = PricingError of string

// * Usecases.Workflows
// サブステップ自体にエラーが発生する可能性がある（結果が馬鹿でかい値やマイナス値など）
type PriceOrder =
  GetProductPrice // 依存関係
    -> ValidatedOrder // 入力
    -> Result<PricedOrder, PricingError> // 出力

// ----- 注文の確認 -----

// // * Domain.ValueObject
// type HTMLString = HTMLString of string

// // * Domain.Service
// // ローカル関数
// type CreateOrderAcknowledgmentLetter =
//   PricedOrder -> HTMLString

// // * Domain.ValueObject
// type OrderAcknowledgment = {
//   EmailAddress: EmailAddress
//   Letter: HTMLString
// }
// type SendResult = Sent | NotSent

// * Domain.Interface.Remote <|.. Infrastructure.Remote
// I/O処理をするが、エラーは気にしない
type SendOrderAcknowledgment =
  OrderAcknowledgment -> SendResult

// * Usecases.Workflows
type AcknowledgeOrder =
  CreateOrderAcknowledgmentLetter // 依存関係
    -> SendOrderAcknowledgment // 依存関係
    -> PricedOrder // 入力
    // 注文書が送信されていない可能性
    -> OrderAcknowledgmentSent option // 出力

// ----- イベントの作成 -----

// // * Usecases.Workflows
// type CreateEvents =
//   PricedOrder // 入力
//     -> OrderAcknowledgmentSent option // 入力（前のステップのイベント）
//     -> PlaceOrderEvent list // 出力

// --------------------
// 実装
// --------------------
// TODO 未整理モジュール
module InComplete =
  /// 例外を投げるサービスをResultを返すサービスに
  /// 変換するアダプターブロック
  let serviceExceptionAdapter serviceInfo serviceFn x =
    try
      // サービスを呼び出して成功を返す
      Ok (serviceFn x)
    with
      // ドメインに関連するエラーだけをキャッチする（他はトップレベルで補足する）
      | :? System.TimeoutException as ex ->
        Error { Service=serviceInfo; Exception=ex }
      | :? System.UnauthorizedAccessException as ex ->
        Error { Service=serviceInfo; Exception=ex }

  let checkAddressExistsR
    checkAddressExists
    : CheckAddressExistsR =
      fun address ->
        let serviceInfo = {
          Name = "AddressCheckingService"
          Endpoint = System.Uri "http://localhost:8080" // 適当
        }
        // サービスを適合させる
        let adaptedService =
          serviceExceptionAdapter serviceInfo checkAddressExists
        // サービスを呼び出す
        address
        |> adaptedService
        // PlaceOrderError型に持ち上げる
        |> Result.mapError PlaceOrderError.RemoteServiceError

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

  let predicateToPassthru errorMsg f x =
    if f x then
      x
    else
      failwith errorMsg

  let toProductCode (checkProductCodeExists: CheckProductCodeExists) productCode =
    // パイプラインで使用するのに適した
    // ProductCode -> ProductCode 型のローカル関数を作る
    let checkProduct productCode =
      let errorMsg = sprintf "ProductCode（%A）は正しい値を指定してください" productCode
      predicateToPassthru errorMsg checkProductCodeExists productCode

    // パイプラインを組み立てる
    productCode
    |> ProductCode.create
    |> checkProduct

  let toOrderQuantity productCode (quantity: decimal<Data.UnitSystems.SI.UnitSymbols.kg>) =
    match productCode with
      | Widget _ ->
        quantity
        |> (fun x -> x / 1.0M<Data.UnitSystems.SI.UnitSymbols.kg>) // 単位排除
        |> int // decimalをintに変換
        |> UnitQuantity.create // ユニット数に変換
        |> Unit // OrderQuantity型に持ち上げる
      | Gizmo _ ->
        quantity
        |> KilogramQuantity.create // キログラム量に変換
        |> Kilogram // OrderQuantity型に持ち上げる

  let toValidatedOrderLine checkProductCodeExists
   (unValidatedOrderLine: UnValidatedOrderLine) =
    let orderLineID =
      unValidatedOrderLine.OrderLineID
      |> ValueObject.OrderLineID.create
    let productCode =
      unValidatedOrderLine.ProductCode
      |> toProductCode checkProductCodeExists // ヘルパー関数
    let quantity =
      unValidatedOrderLine.Quantity
      |> toOrderQuantity productCode // ヘルパー関数

    let validatedOrderLine = {
      OrderLineID = orderLineID
      ProductCode = productCode
      Quantity = quantity
    }
    validatedOrderLine

  /// 検証済みの注文明細行を価格計算済みの注文明細行に変換する
  let toPricedOrderLine getProductPrice (line: ValidatedOrderLine): PricedOrderLine =
    let qty = line.Quantity |> OrderQuantity.value
    let price = line.ProductCode |> getProductPrice
    let linePrice = price |> Price.multiply qty
    {
      OrderLineID = line.OrderLineID
      ProductCode = line.ProductCode
      Quantity = line.Quantity
      LinePrice = linePrice
    }

  let createBillingEvent(placedOrder: PricedOrder): BillableOrderPlaced option =
    let billingAmount = placedOrder.AmountToBill |> BillingAmount.value
    if billingAmount > 0m then
      let order = {
        OrderID = placedOrder.OrderID
        BillingAddress = placedOrder.BillingAddress
        AmountToBill = placedOrder.AmountToBill
      }
      Some order
    else
      None

  /// Option型をList型に変換する
  let listOfOption opt =
    match opt with
    | Some x -> [x]
    | None -> []

  // TODO 一旦validateOrderを引数としている（依存関係があるため）
  // PlaceOrderErrorを返すように変換した
  let validateOrderAdapted validateOrder input =
    input
    |> validateOrder // 元の関数
    |> Result.mapError PlaceOrderError.Validation

  // TODO 一旦priceOrderを引数としている（依存関係があるため）
  // PlaceOrderErrorを返すように変換した
  let priceOrderAdapted priceOrder input =
    input
    |> priceOrder // 元の関数
    |> Result.mapError PlaceOrderError.Pricing

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

    let billingAddress =
      unValidatedOrder.BillingAddress
      |> InComplete.toAddress checkAddressExists // ヘルパー関数

    let orderLines =
      unValidatedOrder.OrderLines
      // `toValidatedOrderLine` を用いて各行を変換する
      |> List.map (InComplete.toValidatedOrderLine checkProductCodeExists)

    // すべてのフィールドの準備ができたら、それらを使って
    // 新しい「検証済みの注文」レコードを作成し、返す
    Ok {
      OrderID = orderID
      CustomerInfo = customerInfo
      ShippingAddress = shippingAddress
      BillingAddress = billingAddress
      OrderLines = orderLines
    }

  let priceOrder: PriceOrder =
    fun getProductPrice validatedOrder ->
      let lines =
        validatedOrder.OrderLines
        |> List.map(InComplete.toPricedOrderLine getProductPrice)
      let amountToBill =
        lines
        // 各行の価格を取得する
        |> List.map(fun line -> line.LinePrice)
        // 合計して請求総額にする
        |> BillingAmount.sumPrices
      let pricedOrder: PricedOrder = {
        OrderID = validatedOrder.OrderID
        CustomerInfo = validatedOrder.CustomerInfo
        ShippingAddress = validatedOrder.ShippingAddress
        BillingAddress = validatedOrder.BillingAddress
        OrderLines = lines
        AmountToBill = amountToBill
      }
      Ok pricedOrder

  let acknowledgeOrder: AcknowledgeOrder =
    fun createOrderAcknowledgmentLetter sendOrderAcknowledgment pricedOrder ->
      let letter = createOrderAcknowledgmentLetter pricedOrder
      let acknowledgment = {
        EmailAddress = pricedOrder.CustomerInfo.EmailAddress
        Letter = letter
      }
      // 確認が正常に送信された場合、対応するイベントを返す
      // そうでなければNoneを返す
      match sendOrderAcknowledgment acknowledgment with
      | Sent ->
        let event = {
          OrderID = pricedOrder.OrderID
          EmailAddress = pricedOrder.CustomerInfo.EmailAddress
        }
        Some event
      | NotSent ->
        None

  let createEvents: CreateEvents =
    fun pricedOrder acknowledgmentEventOpt ->
      let events1 =
        PricedOrder
        // 共通の選択型に変換する
        |> PlaceOrderEvent.OrderPlaced
        // リストに変換する
        |> List.singleton
      let events2 =
        acknowledgmentEventOpt
        // 共通の選択型に変換する
        |> Option.map PlaceOrderEvent.AcknowledgementSent
        // リストに変換する
        |> InComplete.listOfOption
      let events3 =
        pricedOrder
        |> InComplete.createBillingEvent
        // 共通の選択型に変換する
        |> Option.map PlaceOrderEvent.BillableOrderPlaced
        // リストに変換する
        |> InComplete.listOfOption

      // すべてのイベントを返す
      [
        yield! events1
        yield! events2
        yield! events3
      ]

  let placeOrder
    checkProductCodeExists // 依存関係
    checkAddressExists // 依存関係
    getProductPrice // 依存関係
    createOrderAcknowledgmentLetter // 依存関係
    sendOrderAcknowledgment // 依存関係
    : PlaceOrderWorkflow = // 関数の定義
    fun placeOrderCommand ->
      // TODO パイプラインで連鎖させるために部分適用している
        placeOrderCommand.Data
        |> InComplete.validateOrderAdapted (validateOrder checkProductCodeExists checkAddressExists)
        |> Result.bind (InComplete.priceOrderAdapted (priceOrder getProductPrice))
        |> Result.map (acknowledgeOrder createOrderAcknowledgmentLetter sendOrderAcknowledgment)
        |> Result.map createEvents // TODO PriceOrderの結果がないのでエラー！
