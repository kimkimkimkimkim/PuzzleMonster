using System.ComponentModel;

/// <summary>
/// ユーザーランク経験値テーブルマスタ
/// </summary>
[Description("UserRankUpTableMB")]
public class UserRankUpTableMB : MasterBookBase
{
    /// <summary>
    /// ランク
    /// </summary>
    public int rank { get; set; }

    /// <summary>
    /// １つ前のランクから目標ランクになるために必要な経験値量
    /// </summary>
    public int requiredExp { get; set; }

    /// <summary>
    /// 目標ランクになるために必要な総経験値量
    /// </summary>
    public int totalRequiredExp { get; set; }
}
