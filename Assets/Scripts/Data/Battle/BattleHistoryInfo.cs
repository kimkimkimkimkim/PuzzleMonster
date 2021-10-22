using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PM.Enum.Battle;

/// <summary>
/// バトル履歴情報
/// </summary>
public class BattleHistoryInfo
{
    /// <summary>
    /// バトルログタイプ
    /// </summary>
    public BattleLogType type { get; set; }
    
    /// <summary>
    /// 処理前プレイヤーバトルモンスターリスト
    /// </summary>
    public List<BattleMonsterInfo> beforePlayerBattleMonsterList { get; set; }
    
    /// <summary>
    /// 処理前敵バトルモンスターリスト
    /// </summary>
    public List<BattleMonsterInfo> beforeEnemyBattleMonsterList { get; set; }
    
    /// <summary>
    /// 処理後プレイヤーバトルモンスターリスト
    /// </summary>
    public List<BattleMonsterInfo> afterPlayerBattleMonsterList { get; set; }
    
    /// <summary>
    /// 処理後プレイヤーバトルモンスターリスト
    /// </summary>
    public List<BattleMonsterInfo> afterEnemyBattleMonsterList { get; set; }
    
    /// <summary>
    /// する側のモンスターのインデックス
    /// </summary>
    public int doMonsterIndex { get; set; }
    
    /// <summary>
    /// される側のモンスターのインデックス
    /// </summary>
    public int beDoneMonsterIndex { get; set; }
    
    /// <summary>
    /// 処理前ウェーブ数
    /// </summary>
    public int beforeWaveCount { get; set; }
    
    /// <summary>
    /// 処理後ウェーブ数
    /// </summary>
    public int afterWaveCount { get; set; }
    
    /// <summary>
    /// 勝敗
    /// </summary>
    public WinOrLose winOrLose { get; set; }
}
