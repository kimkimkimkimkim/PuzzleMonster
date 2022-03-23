using PM.Enum.Battle;
using System.Collections.Generic;

public class BattleResultDialogRequest
{
    /// <summary>
    /// 勝敗
    /// </summary>
    public WinOrLose winOrLose { get; set; }

    /// <summary>
    /// スコア用の味方バトルモンスターリスト
    /// </summary>
    public List<BattleMonsterInfo> playerBattleMonsterList { get; set; }

    /// <summary>
    /// スコア用のWave毎敵バトルモンスターリスト
    /// </summary>
    public List<List<BattleMonsterInfo>> enemyBattleMonsterListByWave { get; set; }
}

public class BattleResultDialogResponse
{
}