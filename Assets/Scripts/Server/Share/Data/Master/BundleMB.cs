using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// バンドルマスタ
/// </summary>
[Description("BundleMB")]
public class BundleMB : MasterBookBase
{
    /// <summary>
    /// クエストカテゴリ名
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// アイテムリスト
    /// </summary>
    public List<ItemMI> itemList { get; set; }
}
