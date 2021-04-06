using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// クエストマスタ
/// </summary>
[Description("QuestMB")]
public class QuestMB : MasterBookBase
{
    /// <summary>
    /// クエストカテゴリ名
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// クエストカテゴリID
    /// </summary>
    public long questCategoryId { get; set; }

    /// <summary>
    /// 報酬アイテムリスト
    /// </summary>
    public List<ItemMI> firstRewardItemList { get; set; }

    /// <summary>
    /// 報酬アイテムリスト
    /// </summary>
    public List<ItemMI> dropItemList { get; set; }
}
