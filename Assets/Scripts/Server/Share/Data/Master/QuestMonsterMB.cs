using System.ComponentModel;

/// <summary>
/// クエストモンスターマスタ
/// </summary>
[Description("QuestMonsterMB")]
public class QuestMonsterMB : MasterBookBase
{
    /// <summary>
    /// モンスターID
    /// </summary>
    public long monsterId { get; set; }

    /// <summary>
    /// HP
    /// </summary>
    public int hp { get; set; }
}
