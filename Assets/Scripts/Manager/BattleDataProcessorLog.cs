using GameBase;
using PM.Enum.Battle;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ログの出力を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
    /// <summary>
    /// デフォルトログ情報を取得します
    /// </summary>
    /// <returns></returns>
    private BattleLogInfo GetDefaultLog()
    {
        return new BattleLogInfo()
        {
            type = BattleLogType.None,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            doBattleMonsterIndex = null,
            beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>(),
            waveCount = currentWaveCount,
            turnCount = currentTurnCount,
            winOrLose = currentWinOrLose,
            log = "",
            skillFxId = 0,
            actionType = BattleActionType.None,
            skillEffectIndex = 0,
            enemyBattleMonsterListByWave = enemyBattleMonsterListByWave.Clone(),
        };
    }

    /// <summary>
    /// バトル開始ログの追加
    /// </summary>
    private void AddStartBattleLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.StartBattle;
        battleLog.log = "バトルを開始します";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ウェーブ進行ログの追加
    /// </summary>
    private void AddMoveWaveLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.MoveWave;
        battleLog.log = $"ウェーブ{currentWaveCount}を開始します";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ターン進行ログの追加
    /// </summary>
    private void AddMoveTurnLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.MoveTurn;
        battleLog.log = $"ターン{currentTurnCount}を開始します";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// アクション失敗ログの追加
    /// </summary>
    private void AddActionFailedLog(BattleMonsterIndex actionMonsterIndex)
    {
        var battleMonster = GetBattleMonster(actionMonsterIndex);
        var possess = actionMonsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.ActionFailed;
        battleLog.doBattleMonsterIndex = actionMonsterIndex;
        battleLog.log = $"{possess}{monster.name}は動けない";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常ターン進行ログの追加
    /// </summary>
    private void AddProgressBattleConditionTurnLog(List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.ProgressBattleConditionTurn;
        battleLog.beDoneBattleMonsterDataList = beDoneBattleMonsterDataList.Clone();
        battleLog.log = "状態異常のターンを進行しました";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常解除ログの追加
    /// </summary>
    private void AddTakeBattleConditionRemoveLog(List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeBattleConditionRemove;
        battleLog.beDoneBattleMonsterDataList = beDoneBattleMonsterDataList.Clone();
        battleLog.log = "状態異常を解除しました";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// アクション開始ログの追加
    /// </summary>
    private void AddStartActionLog(BattleMonsterIndex doMonsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(doMonsterIndex);
        var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        var skillName = GetSkillName(battleMonster, actionType);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.StartAction;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.actionType = actionType;
        battleLog.log = $"{possess}{monster.name}が{skillName}を発動";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// アクションアニメーション開始ログの追加
    /// </summary>
    private void AddStartActionAnimationLog(BattleMonsterIndex doMonsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(doMonsterIndex);
        var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        var skillName = GetSkillName(battleMonster, actionType);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.StartActionAnimation;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.actionType = actionType;
        battleLog.log = $"{possess}{monster.name}が{skillName}を発動";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 被ダメージログの追加
    /// </summary>
    private void AddTakeDamageLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, long skillFxId)
    {
        var logList = beDoneMonsterDataList.Select(d => {
            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

            return $"{possess}{monster.name}に{Math.Abs(d.hpChanges)}ダメージ";
        }).ToList();
        var log = string.Join("\n", logList);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeDamage;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillFxId;
        battleLog.log = log;

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 被回復ログの追加
    /// </summary>
    private void AddTakeHealLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, long skillFxId)
    {
        var logList = beDoneMonsterDataList.Select(d => {
            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

            return $"{possess}{monster.name}の体力を{Math.Abs(d.hpChanges)}回復";
        }).ToList();
        var log = string.Join("\n", logList);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeHeal;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillFxId;
        battleLog.log = log;

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常付与ログの追加
    /// </summary>
    private void AddTakeBattleConditionAddLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, SkillEffectMI skillEffect)
    {
        var battleConditionMB = MasterRecord.GetMasterOf<BattleConditionMB>().Get(skillEffect.battleConditionId);
        var logList = beDoneMonsterDataList.Select(d => {

            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

            return d.isMissed ?
                $"{possess}{monster.name}への{battleConditionMB.name}の付与が失敗" :
                $"{possess}{monster.name}に{battleConditionMB.name}を付与";
        }).ToList();
        var log = string.Join("\n", logList);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeBattleConditionAdd;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillEffect.skillFxId;
        battleLog.log = log;

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ステータス変化ログの追加
    /// </summary>
    private void AddTakeStatusChangeLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, SkillEffectMI skillEffect, int value)
    {
        var battleCondition = MasterRecord.GetMasterOf<BattleConditionMB>().Get(skillEffect.battleConditionId);
        var logList = beDoneMonsterDataList.Select(d => {

            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
            var changeText = battleCondition.buffType == BuffType.Buff ? "上昇" : "減少";

            return $"{possess}{monster.name}の{battleCondition.targetBattleMonsterStatusType}を{Math.Abs(value)}だけ{changeText}";
        }).ToList();
        var log = string.Join("\n", logList);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeStatusChange;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillEffect.skillFxId;
        battleLog.log = log;

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 死亡ログの追加
    /// </summary>
    private void AddDieLog(List<BeDoneBattleMonsterData> beDoneMonsterDataList)
    {
        var logList = beDoneMonsterDataList.Select(d => {
            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            return $"{possess}{monster.name}が倒れた";
        }).ToList();
        var log = string.Join("\n", logList);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.Die;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.log = log;

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// アクション終了ログの追加
    /// </summary>
    private void AddEndActionLog(BattleMonsterIndex doMonsterIndex)
    {
        var battleMonster = GetBattleMonster(doMonsterIndex);
        var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EndAction;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.log = $"{possess}{monster.name}のアクションが終了しました";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ターン終了ログの追加
    /// </summary>
    private void AddEndTurnLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EndTurn;
        battleLog.log = $"ターン{currentTurnCount}が終了しました";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ウェーブ終了ログの追加
    /// </summary>
    private void AddEndWaveLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EndWave;
        battleLog.log = $"ウェーブ{currentWaveCount}が終了しました";

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// バトル終了ログの追加
    /// </summary>
    private void AddEndBattleLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.Result;
        battleLog.log = currentWinOrLose == WinOrLose.Win ? "バトルに勝利しました" : "バトルに敗北しました";

        battleLogList.Add(battleLog);
    }
}
