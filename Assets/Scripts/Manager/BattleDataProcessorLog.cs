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
            skillGuid = "",
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

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = "バトルを開始します";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ウェーブ進行ログの追加
    /// </summary>
    private void AddMoveWaveLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.MoveWave;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = $"ウェーブ{currentWaveCount}を開始します";
        }

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
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.ActionFailed;
        battleLog.doBattleMonsterIndex = actionMonsterIndex;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(actionMonsterIndex);
            var possess = actionMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            battleLog.log = $"{possess}{monster.name}は動けない";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 発動確率によるスキル効果未発動ログの追加
    /// </summary>
    private void AddSkillEffectFailedOfProbabilityMissLog(BattleMonsterIndex doBattleMonsterIndex, List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.SkillEffectFailedOfProbabilityMiss;
        battleLog.doBattleMonsterIndex = doBattleMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneBattleMonsterDataList.Clone();
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doBattleMonsterIndex);
            var possess = doBattleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            battleLog.log = $"{possess}{monster.name}の行動は発動しなかった";
        }

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

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = "状態異常のターンを進行しました";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常解除前ログの追加
    /// </summary>
    private void AddTakeBattleConditionRemoveBeforeLog(List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList, string skillGuid, BattleActionType actionType, int skillEffectIndex)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeBattleConditionRemoveBefore;
        battleLog.beDoneBattleMonsterDataList = beDoneBattleMonsterDataList.Clone();
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = "状態異常を解除します";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常解除後ログの追加
    /// </summary>
    private void AddTakeBattleConditionRemoveAfterLog(List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList, string skillGuid, BattleActionType actionType, int skillEffectIndex)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeBattleConditionRemoveAfter;
        battleLog.beDoneBattleMonsterDataList = beDoneBattleMonsterDataList.Clone();
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = "状態異常を解除しました";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ターンアクション開始ログの追加
    /// </summary>
    private void AddStartTurnActionLog(BattleMonsterIndex doMonsterIndex)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.StartTurnAction;
        battleLog.doBattleMonsterIndex = doMonsterIndex;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            battleLog.log = $"{possess}{monster.name}がターンアクションを開始";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ターンアクション終了ログの追加
    /// </summary>
    private void AddEndTurnActionLog(BattleMonsterIndex doMonsterIndex)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EndTurnAction;
        battleLog.doBattleMonsterIndex = doMonsterIndex;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            battleLog.log = $"{possess}{monster.name}がターンアクションを終了";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// アクション開始ログの追加
    /// </summary>
    private void AddStartActionLog(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.StartAction;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.actionType = actionType;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            var skillName = GetSkillName(battleMonster, actionType, battleCondition);
            battleLog.log = $"{possess}{monster.name}[{doMonsterIndex.index}]が{skillName}を発動";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// スキル対象決定ログの追加
    /// スキルの成功可否によってはスキル効果が適用されないこともある
    /// </summary>
    private void AddSetSkillTargetLog(BattleMonsterIndex doMonsterIndex, string skillGuid, BattleActionType actionType, int skillEffectIndex, List<BattleMonsterIndex> beDoneMonsterIndexList, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.SetSkillTarget;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.skillGuid = skillGuid;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.actionType = actionType;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterIndexList.Select(index => new BeDoneBattleMonsterData() { battleMonsterIndex = index }).ToList();
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            var skillName = GetSkillName(battleMonster, actionType, battleCondition);
            battleLog.log = $"{possess}{monster.name}[{doMonsterIndex.index}]が{skillName}のインデックス{skillEffectIndex}のスキル対象を決定";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// スキル効果開始ログの追加
    /// スキル効果が発動したかどうかの判定にも使用する
    /// </summary>
    private void AddStartSkillEffectLog(BattleMonsterIndex doMonsterIndex, string skillGuid, BattleActionType actionType, int skillEffectIndex, List<BattleMonsterIndex> beDoneMonsterIndexList, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.StartSkillEffect;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.skillGuid = skillGuid;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.actionType = actionType;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterIndexList.Select(index => new BeDoneBattleMonsterData() { battleMonsterIndex = index }).ToList();
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            var skillName = GetSkillName(battleMonster, actionType, battleCondition);
            battleLog.log = $"{possess}{monster.name}[{doMonsterIndex.index}]が{skillName}のインデックス{skillEffectIndex}の効果を発動";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// トリガースキルログの追加
    /// トリガースキル発動時の対象スキル情報
    /// </summary>
    private void AddTriggerSkillLog(BattleMonsterIndex doMonsterIndex, string skillGuid, BattleActionType actionType, int skillEffectIndex, List<BattleMonsterIndex> beDoneMonsterIndexList, BattleConditionInfo battleCondition, TriggerSkillData triggerSkillData)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TriggerSkill;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.skillGuid = skillGuid;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.actionType = actionType;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterIndexList.Select(index => new BeDoneBattleMonsterData() { battleMonsterIndex = index }).ToList();
        battleLog.battleCondition = battleCondition;
        battleLog.triggerSkillData = triggerSkillData;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            var skillName = GetSkillName(battleMonster, actionType, battleCondition);
            var targetBattleMonster = GetBattleMonster(triggerSkillData.battleMonsterIndex);
            var targetPossess = triggerSkillData.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var targetMonster = monsterList.First(m => m.id == targetBattleMonster.monsterId);
            var targetSkillName = GetSkillName(targetBattleMonster, triggerSkillData.battleActionType, new BattleConditionInfo() { battleConditionId = 1 });
            battleLog.log = $"{possess}{monster.name}[{doMonsterIndex.index}]が{targetPossess}{targetMonster.name}[{triggerSkillData.battleMonsterIndex.index}]の{targetSkillName}のインデックス{triggerSkillData.skillEffectIndex}に対して{skillName}のインデックス{skillEffectIndex}の効果を発動";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// アクションアニメーション開始ログの追加
    /// </summary>
    private void AddStartActionAnimationLog(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.StartActionAnimation;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.actionType = actionType;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            var skillName = GetSkillName(battleMonster, actionType, battleCondition);
            battleLog.log = $"{possess}{monster.name}[{doMonsterIndex.index}]が{skillName}を発動";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 被ダメージログの追加
    /// </summary>
    private void AddTakeDamageLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, long skillFxId, string skillGuid, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeDamage;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillFxId;
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);

                return $"{possess}{monster.name}に{Math.Abs(d.hpChanges)}ダメージ";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 被回復ログの追加
    /// </summary>
    private void AddTakeHealLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, long skillFxId, string skillGuid, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeHeal;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillFxId;
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);

                return $"{possess}{monster.name}の体力を{Math.Abs(d.hpChanges)}回復";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// エネルギー上昇ログの追加
    /// </summary>
    private void AddEnergyUpLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, long skillFxId, string skillGuid, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EnergyUp;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillFxId;
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);

                return $"{possess}{monster.name}のエネルギーが{Math.Abs(d.energyChanges)}増加";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// エネルギー減少ログの追加
    /// </summary>
    private void AddEnergyDownLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, long skillFxId, string skillGuid, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EnergyDown;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillFxId;
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);

                return $"{possess}{monster.name}のエネルギーが{Math.Abs(d.energyChanges)}減少";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常付与ログの追加
    /// </summary>
    private void AddTakeBattleConditionAddLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, SkillEffectMI skillEffect, string skillGuid, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeBattleConditionAdd;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillEffect.skillFxId;
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleConditionMB = battleConditionList.First(m => m.id == skillEffect.battleConditionId);
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);

                return d.isMissed ?
                    $"{possess}{monster.name}への{battleConditionMB.name}の付与が失敗" :
                    $"{possess}{monster.name}に{battleConditionMB.name}を付与";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ステータス変化ログの追加
    /// </summary>
    private void AddTakeStatusChangeLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, SkillEffectMI skillEffect, int value, string skillGuid, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeStatusChange;
        battleLog.doBattleMonsterIndex = doMonsterIndex;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillEffect.skillFxId;
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleConditionMB = battleConditionList.First(m => m.id == skillEffect.battleConditionId);
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);
                var changeText = battleConditionMB.buffType == BuffType.Buff ? "上昇" : "減少";

                return $"{possess}{monster.name}の{battleConditionMB.targetBattleMonsterStatusType}を{Math.Abs(value)}だけ{changeText}";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 蘇生ログの追加
    /// </summary>
    private void AddTakeReviveLog(BattleMonsterIndex doMonsterIndex, List<BeDoneBattleMonsterData> beDoneMonsterDataList, SkillEffectMI skillEffect, string skillGuid, BattleActionType actionType, int skillEffectIndex, BattleConditionInfo battleCondition)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.TakeRevive;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();
        battleLog.skillFxId = skillEffect.skillFxId;
        battleLog.skillGuid = skillGuid;
        battleLog.actionType = actionType;
        battleLog.skillEffectIndex = skillEffectIndex;
        battleLog.battleCondition = battleCondition;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                return $"{possess}{monster.name}が蘇生した";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 死亡ログの追加
    /// </summary>
    private void AddDieLog(List<BeDoneBattleMonsterData> beDoneMonsterDataList)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.Die;
        battleLog.beDoneBattleMonsterDataList = beDoneMonsterDataList.Clone();

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var logList = beDoneMonsterDataList.Select(d =>
            {
                var battleMonster = GetBattleMonster(d.battleMonsterIndex);
                var monster = monsterList.First(m => m.id == battleMonster.monsterId);
                var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
                return $"{possess}{monster.name}が倒れた";
            }).ToList();
            var log = string.Join("\n", logList);
            battleLog.log = log;
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// アクション終了ログの追加
    /// </summary>
    private void AddEndActionLog(BattleMonsterIndex doMonsterIndex)
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EndAction;
        battleLog.doBattleMonsterIndex = doMonsterIndex;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            var battleMonster = GetBattleMonster(doMonsterIndex);
            var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = monsterList.First(m => m.id == battleMonster.monsterId);
            battleLog.log = $"{possess}{monster.name}のアクションが終了しました";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ターン終了ログの追加
    /// </summary>
    private void AddEndTurnLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EndTurn;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = $"ターン{currentTurnCount}が終了しました";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// ウェーブ終了ログの追加
    /// </summary>
    private void AddEndWaveLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.EndWave;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = $"ウェーブ{currentWaveCount}が終了しました";
        }

        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// バトル終了ログの追加
    /// </summary>
    private void AddEndBattleLog()
    {
        var battleLog = GetDefaultLog();
        battleLog.type = BattleLogType.Result;

        if (ApplicationSettingsManager.Instance.isDebugBattleLogMode)
        {
            battleLog.log = currentWinOrLose == WinOrLose.Win ? "バトルに勝利しました" : "バトルに敗北しました";
        }

        battleLogList.Add(battleLog);
    }
}