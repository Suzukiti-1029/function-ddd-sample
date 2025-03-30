namespace OrderTaking.TestPlaceOrderWorkflowWork

open NUnit.Framework

open OrderTaking.Domain
// ドメインAPIモジュールから型を持ってくる
open OrderTaking.DomainApi
open OrderTaking.PlaceOrderWorkflow
open OrderTaking.PlaceOrderWorkflowWork

[<TestFixture>]
type DomainApiTests () =

  [<Test>]
  member _.``製品が存在する場合は、検証に成功する``() =
    // 準備: サービス依存関係のスタブを設定
    let checkAddressExists: CheckAddressExists =
      fun _ ->
        CheckAddress {
          AddressLine1 = "千代田区"
          AddressLine2 = "千代田"
          AddressLine3 = "1-1"
          AddressLine4 = "皇城"
          City = "東京都"
          ZipCode = "100-8111"
        } // 成功

    let checkProductCodeExists: CheckProductCodeExists =
      fun _ ->
        true // 成功

    // 準備: 入力の設定
    let unValidateOrder: UnValidatedOrder = {
      OrderID = "固定OrderID"
      CustomerInfo = {
        FirstName = "固定FirstName"
        LastName = "固定LastName"
        EmailAddress = "sample@test.com"
      }
      ShippingAddress = UnValidateAddress "東京都千代田区千代田1-1 皇城"
      BillingAddress = UnValidateAddress "東京都千代田区千代田1-1 皇城"
      OrderLines = [
        {
          OrderLineID = "OI12345678"
          ProductCode = "W1234"
          Quantity = 1.0m<Data.UnitSystems.SI.UnitSymbols.kg>
        }
        {
          OrderLineID = "OI23456789"
          ProductCode = "G123"
          Quantity = 2.0m<Data.UnitSystems.SI.UnitSymbols.kg>
        }
      ]
    }

    // 実行: validateOrderを呼び出す
    let result =
      Workflows.validateOrder checkProductCodeExists checkAddressExists unValidateOrder

    // 検証: 結果がエラーではなく、検証済みの注文であることを確認する
    Assert.That(result, Is.Not.Null)

    let resultOrderID= ValueObject.OrderID.value result.OrderID
    Assert.That(resultOrderID, Is.EqualTo "固定OrderID")

  [<Test>]
  member _.``製品が存在しない場合は、検証に失敗する``() =
    // 準備: サービス依存関係のスタブを設定
    let checkAddressExists: CheckAddressExists =
      fun _ ->
        CheckAddress {
          AddressLine1 = "千代田区"
          AddressLine2 = "千代田"
          AddressLine3 = "1-1"
          AddressLine4 = "皇城"
          City = "東京都"
          ZipCode = "100-8111"
        } // 成功

    let checkProductCodeExists: CheckProductCodeExists =
      fun _ ->
        false // 失敗

    // 準備: 入力の設定
    let unValidateOrder: UnValidatedOrder = {
      OrderID = "固定OrderID"
      CustomerInfo = {
        FirstName = "固定FirstName"
        LastName = "固定LastName"
        EmailAddress = "sample@test.com"
      }
      ShippingAddress = UnValidateAddress "東京都千代田区千代田1-1 皇城"
      BillingAddress = UnValidateAddress "東京都千代田区千代田1-1 皇城"
      OrderLines = [
        {
          OrderLineID = "OI12345678"
          ProductCode = "W1234"
          Quantity = 1.0m<Data.UnitSystems.SI.UnitSymbols.kg>
        }
        {
          OrderLineID = "OI23456789"
          ProductCode = "G123"
          Quantity = 2.0m<Data.UnitSystems.SI.UnitSymbols.kg>
        }
      ]
    }

    // TODO 現状、例外が発生するので、例外をキャッチして、Assertする
    // 実行: validateOrderを呼び出す
    // 検証: 結果が失敗であることを確認する
    Assert.Throws<System.Exception>(fun () ->
      Workflows.validateOrder
        checkProductCodeExists
        checkAddressExists
        unValidateOrder
        |> ignore
    ) |> ignore
