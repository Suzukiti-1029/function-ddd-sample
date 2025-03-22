namespace OrderTaking.Domain

module ValueObject =
  type OrderID = private OrderID of string

  module OrderID =
    /// 注文ID の「スマートコンストラクタ」を定義
    let create str =
      if System.String.IsNullOrEmpty(str) then
        // TODO ひとまず、Resultではなく、例外を使う
        failwith "OrderIDは、nullまたは空にしないでください"
      elif str.Length > 50 then
        // TODO
        failwith "OrderIDは、50文字超過にしないでください"
      else
        OrderID str

    /// 注文ID の内部値を取り出す
    let value(OrderID str) = // パラメーターのところですでにアンラップしている
      str // 内部値を返す
