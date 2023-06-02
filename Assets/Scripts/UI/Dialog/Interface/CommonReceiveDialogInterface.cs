using System.Collections.Generic;

public class CommonReceiveDialogRequest {
    /// <summary>
    /// タイトル
    /// </summary>
    public string title { get; set; } = "アイテム受け取り";

    /// <summary>
    /// 本文
    /// </summary>
    public string content { get; set; } = "以下のアイテムを受け取りました";

    /// <summary>
    /// アイテムリスト
    /// </summary>
    public List<ItemMI> itemList { get; set; }
}

public class CommonReceiveDialogResponse {
}
