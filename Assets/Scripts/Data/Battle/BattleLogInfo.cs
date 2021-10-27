using System.Collections.Generic;
using PM.Enum.Battle;

/// <summary>
/// バトルログ情報
/// </summary>
public class BattleLogInfo
{
    /// <summary>
    /// バトルログタイプ
    /// </summary>
    public BattleLogType type { get; set; }
    
    /// <summary>
    /// プレイヤーバトルモンスターリスト
    /// </summary>
    public List<BattleMonsterInfo> playerBattleMonsterList { get; set; }
    
    /// <summary>
    /// 敵バトルモンスターリスト
    /// </summary>
    public List<BattleMonsterInfo> enemyBattleMonsterList { get; set; }
    
    /// <summary>
    /// する側のモンスターのインデックス
    /// </summary>
    public BattleMonsterIndex doBattleMonsterIndex { get; set; }
    
    /// <summary>
    /// される側のモンスターのインデックス
    /// </summary>
    public BattleMonsterIndex beDoneBattleMonsterIndex { get; set; }
    
    /// <summary>
    /// ウェーブ数
    /// </summary>
    public int waveCount { get; set; }

    /// <summary>
    /// ターン数
    /// </summary>
    public int turnCount { get; set; }

    /// <summary>
    /// 勝敗
    /// </summary>
    public WinOrLose winOrLose { get; set; }

    /// <summary>
    /// ログ文字列
    /// </summary>
    public string log { get; set; }

    /// <summary>
    /// 発生したダメージ
    /// </summary>
    public int damage { get; set; }
}
