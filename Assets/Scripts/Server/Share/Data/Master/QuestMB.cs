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
    /// 初回報酬バンドルID
    /// </summary>
    public long firstRewardBundleId { get; set; }

    /// <summary>
    /// ドロップアイテムバンドルID
    /// </summary>
    public long dropBundleId { get; set; }

    /// <summary>
    /// クエストウェーブIDリスト
    /// </summary>
    public List<long> questWaveIdList { get; set; }
}
