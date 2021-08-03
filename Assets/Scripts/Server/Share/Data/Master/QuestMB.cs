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
    /// wave1のクエストモンスターIDリスト
    /// </summary>
    public List<long> wave1QuestMonsterIdList { get; set; }

    /// <summary>
    /// wave2のクエストモンスターIDリスト
    /// </summary>
    public List<long> wave2QuestMonsterIdList { get; set; }

    /// <summary>
    /// wave3のクエストモンスターIDリスト
    /// </summary>
    public List<long> wave3QuestMonsterIdList { get; set; }
}
