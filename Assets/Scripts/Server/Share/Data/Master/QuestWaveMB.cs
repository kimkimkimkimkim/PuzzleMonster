using System.Collections.Generic;
using System.ComponentModel;

/// <summary>
/// クエストウェーブマスタ
/// </summary>
[Description("QuestWaveMB")]
public class QuestWaveMB : MasterBookBase
{
    /// <summary>
    /// クエストモンスターIDリスト
    /// </summary>
    public List<long> questMonsterIdList { get; set; }
}
