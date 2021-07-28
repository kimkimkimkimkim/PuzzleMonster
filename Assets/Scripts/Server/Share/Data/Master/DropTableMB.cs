using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// ドロップテーブルマスタ
/// </summary>
[Description("DropTableMB")]
public class DropTableMB : MasterBookBase
{
    /// <summary>
    /// クエストカテゴリ名
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// アイテムリスト
    /// </summary>
    public List<ProbabilityItemMI> itemList { get; set; }
}
