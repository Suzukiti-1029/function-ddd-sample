namespace OrderTaking.Domain

open System.Text.RegularExpressions

// * Domain.Shared
type String50 = private String50 of string
module String50 =
  let create str =
    if System.String.IsNullOrWhiteSpace str then
      // TODO
      invalidArg "str" "String50は、nullまたは空にしないでください"
    elif str.Length > 50 then
      // TODO
      invalidArg "str" "String50は、50文字超過にしないでください"
    else
      String50 str

  let createOption str =
    if System.String.IsNullOrWhiteSpace str then
      None
    else
      Some(create str)

// * Domain.ValueObject
type EmailAddress = private EmailAddress of string
module EmailAddress =
  let create str =
    if System.String.IsNullOrEmpty(str) then
      // TODO
      invalidArg "str" "EmailAddressは、nullまたは空にしないでください"
    elif not (Regex.IsMatch(str, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)) then
      // TODO
      invalidArg "str" "EmailAddressは、正しい形式で指定してください"
    else
      EmailAddress str

type ZipCode = private ZipCode of string
module ZipCode =
  let create (str: string) =
    if System.String.IsNullOrWhiteSpace str then
      // TODO
      invalidArg "str" "ZipCodeは、nullまたは空にしないでください"
    elif not (Regex.IsMatch(str, @"^\d{5}(-\d{4})?$")) &&
     not (Regex.IsMatch(str, @"^\d{3}-\d{4}$")) then
      // TODO
      invalidArg "str" "ZipCodeは、正しい形式で指定してください"
    else
      ZipCode str

module ValueObject =
  type OrderID = private OrderID of string

  module OrderID =
    /// 注文ID の「スマートコンストラクタ」を定義
    let create str =
      if System.String.IsNullOrEmpty(str) then
        // TODO ひとまず、Resultではなく、例外を使う
        invalidArg "str" "OrderIDは、nullまたは空にしないでください"
      elif str.Length > 50 then
        // TODO
        invalidArg "str" "OrderIDは、50文字超過にしないでください"
      else
        OrderID str

    /// 注文ID の内部値を取り出す
    let value(OrderID str) = // パラメーターのところですでにアンラップしている
      str // 内部値を返す

  type OrderLineID = private OrderLineID of string
  module OrderLineID =
    let create str =
      if System.String.IsNullOrEmpty(str) then
        // TODO
        invalidArg "str" "OrderLineIDは、nullまたは空にしないでください"
      elif str.Length > 50 then
        // TODO
        invalidArg "str" "OrderLineIDは、50文字超過にしないでください"
      else
        OrderLineID str
