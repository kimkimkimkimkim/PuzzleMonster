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
    /// phase1のクエストモンスターIDリスト
    /// </summary>
    public List<long> phase1QuestMonsterIdList { get; set; }

    /// <summary>
    /// phase2のクエストモンスターIDリスト
    /// </summary>
    public List<long> phase2QuestMonsterIdList { get; set; }

    /// <summary>
    /// phase3のクエストモンスターIDリスト
    /// </summary>
    public List<long> phase3QuestMonsterIdList { get; set; }
}
