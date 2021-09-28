using System.Collections.Generic;
using System.ComponentModel;
using PM.Enum.Quest;

/// <summary>
/// クエストカテゴリマスタ
/// </summary>
[Description("QuestCategoryMB")]
public class QuestCategoryMB : MasterBookBase
{
    /// <summary>
    /// クエストカテゴリ名
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// クエストタイプ
    /// </summary>
    public QuestType questType { get; set; }

    /// <summary>
    /// 報酬アイテムリスト
    /// </summary>
    public List<ItemMI> rewardItemList { get; set; }
}
