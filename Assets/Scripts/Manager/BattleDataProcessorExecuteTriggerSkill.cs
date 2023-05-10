using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// トリガー発動関係のスキル発動を扱うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private void ExecuteTriggerSkillIfNeeded(SkillTriggerType triggerType, List<BattleMonsterIndex> battleMonsterIndexList, int triggerTypeOptionValue = 0, BattleMonsterIndex targetBattleMonsterIndex = null, BattleActionType targetBattleActionType = BattleActionType.None, int targetBattleConditionCount = 0, TriggerSkillData triggerSkillData = null, string battleConditionGuid = "")
    {
        battleMonsterIndexList.ForEach(index =>
        {
            ExecuteTriggerSkillIfNeeded(triggerType, index, triggerTypeOptionValue, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount, triggerSkillData, battleConditionGuid);
        });
    }

    private void ExecuteTriggerSkillIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, int triggerTypeOptionValue = 0, BattleMonsterIndex targetBattleMonsterIndex = null, BattleActionType targetBattleActionType = BattleActionType.None, int targetBattleConditionCount = 0, TriggerSkillData triggerSkillData = null, string battleConditionGuid = "")
    {
        // パッシブスキルを発動
        ExecutePassiveIfNeeded(triggerType, battleMonsterIndex, triggerTypeOptionValue, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount, triggerSkillData, battleConditionGuid);

        // 状態異常効果を発動
        ExecuteBattleConditionIfNeeded(triggerType, battleMonsterIndex, triggerTypeOptionValue, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount, triggerSkillData, battleConditionGuid);
    }

    // パッシブスキルを発動
    private void ExecutePassiveIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, int triggerTypeOptionValue, BattleMonsterIndex targetBattleMonsterIndex, BattleActionType targetBattleActionType, int targetBattleConditionCount, TriggerSkillData triggerSkillData, string battleConditionGuid)
    {
        // チェーンの状況を元に発動可能か判断
        if (!IsValidChain(triggerType, battleMonsterIndex, 0, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount)) return;

        // 発動条件をみたしたスキルが存在していれば発動
        var targetBattleMonster = GetBattleMonster(battleMonsterIndex);
        var targetMonster = monsterList.First(m => m.id == targetBattleMonster.monsterId);
        var battleSkillEffectList = targetBattleMonster.passiveSkill.effectList
            .Select((effect, index) =>
            {
                var isActive = false;
                if (effect.triggerType != triggerType || effect.triggerTypeOptionValue != triggerTypeOptionValue)
                {
                    // トリガータイプがあっているか
                    isActive = false;
                }
                else if (!IsValidLimitExecuteNum(targetBattleMonster, effect, index, triggerSkillData))
                {
                    // 発動回数条件を満たしているか
                    isActive = false;
                }
                else if (!IsValidActivateCondition(battleMonsterIndex, effect.doMonsterActivateConditionType, effect.doMonsterActivateConditionValue, 0))
                {
                    // 実行者条件を満たしているか
                    isActive = false;
                }
                else
                {
                    isActive = true;
                }

                return new BattleSkillEffectMI() { isActive = isActive, skillEffect = effect };
            })
            .ToList();
        if (battleSkillEffectList.Any(b => b.isActive))
        {
            StartActionStream(battleMonsterIndex, BattleActionType.PassiveSkill, null, battleSkillEffectList, triggerSkillData);
        }
    }

    // 状態異常効果を発動
    private void ExecuteBattleConditionIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, int triggerTypeOptionValue, BattleMonsterIndex targetBattleMonsterIndex, BattleActionType targetBattleActionType, int targetBattleConditionCount, TriggerSkillData triggerSkillData, string battleConditionGuid)
    {
        var targetBattleMonster = GetBattleMonster(battleMonsterIndex);
        var targetBattleConditionList = targetBattleMonster.battleConditionList
            .Where(c =>
            {
                // 状態異常効果の発動条件はマスタのスキルエフェクトを参照する
                if (c.battleConditionSkillEffect.triggerType != triggerType || c.battleConditionSkillEffect.triggerTypeOptionValue != triggerTypeOptionValue) return false;
                if (!IsValidActivateCondition(battleMonsterIndex, c.battleConditionSkillEffect.doMonsterActivateConditionType, c.battleConditionSkillEffect.doMonsterActivateConditionValue, c.battleConditionId)) return false;
                if (battleConditionGuid != "" && c.guid != battleConditionGuid) return false;
                return true;
            })
            .ToList();

        targetBattleConditionList.ForEach(battleCondition =>
        {
            // どの状態異常効果が発動するかによって条件が変わるのでここで判定
            if (IsValidChain(triggerType, battleMonsterIndex, battleCondition.order, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount))
            {
                var skillEffect = battleCondition.grantorSkillEffect;
                skillEffect.type = battleCondition.battleConditionSkillEffect.type;
                skillEffect.doMonsterActivateConditionType = battleCondition.battleConditionSkillEffect.doMonsterActivateConditionType;
                skillEffect.doMonsterActivateConditionValue = battleCondition.battleConditionSkillEffect.doMonsterActivateConditionValue;
                skillEffect.activateConditionType = battleCondition.battleConditionSkillEffect.activateConditionType;
                skillEffect.activateConditionValue = battleCondition.battleConditionSkillEffect.activateConditionValue;
                skillEffect.activateProbability = battleCondition.battleConditionSkillEffect.activateProbability;
                skillEffect.skillTargetType = battleCondition.battleConditionSkillEffect.skillTargetType;
                skillEffect.skillFxId = battleCondition.battleConditionSkillEffect.skillFxId;

                var battleSkillEffectList = new List<BattleSkillEffectMI>() { new BattleSkillEffectMI() { isActive = true, skillEffect = skillEffect } };
                StartActionStream(battleMonsterIndex, BattleActionType.BattleCondition, battleCondition, battleSkillEffectList, triggerSkillData);
            }
        });
    }

    private bool IsValidLimitExecuteNum(BattleMonsterInfo battleMonster, PassiveSkillEffectMI passiveSkillEffect, int skillEffectIndex, TriggerSkillData triggerSkillData)
    {
        if (passiveSkillEffect.limitExecuteNum <= 0) return true;

        switch (passiveSkillEffect.executeNumLimitType)
        {
            case SkillExecuteNumLimitType.InBattle:
                {
                    var battleLogList = this.battleLogList
                        .Where(l => l.type == BattleLogType.StartSkillEffect && l.actionType == BattleActionType.PassiveSkill && l.skillEffectIndex == skillEffectIndex)
                        .Where(l => l.doBattleMonsterIndex.IsSame(battleMonster.index))
                        .ToList();
                    return battleLogList.Count < passiveSkillEffect.limitExecuteNum;
                }
            case SkillExecuteNumLimitType.InWave:
                {
                    var battleLogList = this.battleLogList
                        .Where(l => l.type == BattleLogType.StartSkillEffect && l.actionType == BattleActionType.PassiveSkill && l.skillEffectIndex == skillEffectIndex)
                        .Where(l => l.doBattleMonsterIndex.IsSame(battleMonster.index))
                        .Where(l => l.waveCount == currentWaveCount)
                        .ToList();
                    return battleLogList.Count < passiveSkillEffect.limitExecuteNum;
                }
            case SkillExecuteNumLimitType.InTurn:
                {
                    var battleLogList = this.battleLogList
                        .Where(l => l.type == BattleLogType.StartSkillEffect && l.actionType == BattleActionType.PassiveSkill && l.skillEffectIndex == skillEffectIndex)
                        .Where(l => l.doBattleMonsterIndex.IsSame(battleMonster.index))
                        .Where(l => l.waveCount == currentWaveCount && l.turnCount == currentTurnCount)
                        .ToList();
                    return battleLogList.Count < passiveSkillEffect.limitExecuteNum;
                }
            case SkillExecuteNumLimitType.InStream:
                {
                    var targetIndex = this.battleLogList.FindLastIndex(log =>
                    {
                        if (log.type != BattleLogType.StartTurnAction) return false;
                        if (log.waveCount != currentWaveCount) return false;
                        if (log.turnCount != currentTurnCount) return false;
                        return true;
                    });
                    if (targetIndex < 0)
                    {
                        return false;
                    }
                    else
                    {
                        var battleLogList = this.battleLogList
                            .Where((log, index) => index >= targetIndex)
                            .Where(l => l.type == BattleLogType.StartSkillEffect && l.actionType == BattleActionType.PassiveSkill && l.skillEffectIndex == skillEffectIndex)
                            .Where(l => l.doBattleMonsterIndex.IsSame(battleMonster.index))
                            .ToList();
                        return battleLogList.Count < passiveSkillEffect.limitExecuteNum;
                    }
                }
            case SkillExecuteNumLimitType.InEffectOnOwnEffect:
                {
                    var targetIndex = this.battleLogList.FindLastIndex(log =>
                    {
                        if (log.type != BattleLogType.StartTurnAction) return false;
                        if (log.waveCount != currentWaveCount) return false;
                        if (log.turnCount != currentTurnCount) return false;
                        return true;
                    });
                    if (targetIndex < 0)
                    {
                        return false;
                    }
                    else
                    {
                        var battleLogList = this.battleLogList
                            .Where((log, index) => index >= targetIndex)
                            .Where(l => l.type == BattleLogType.TriggerSkill && l.actionType == BattleActionType.PassiveSkill && l.skillEffectIndex == skillEffectIndex)
                            .Where(l => l.doBattleMonsterIndex.IsSame(battleMonster.index))
                            .Where(l => l.triggerSkillData != null && l.triggerSkillData.battleMonsterIndex.IsSame(triggerSkillData.battleMonsterIndex) && l.triggerSkillData.battleActionType == triggerSkillData.battleActionType && l.triggerSkillData.skillEffectIndex == triggerSkillData.skillEffectIndex)
                            .ToList();
                        return battleLogList.Count < passiveSkillEffect.limitExecuteNum;
                    }
                }
            case SkillExecuteNumLimitType.None:
            default:
                return false;
        }
    }

    private bool IsValidChain(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, int battleConditionCount, BattleMonsterIndex targetBattleMonsterIndex, BattleActionType targetBattleActionType, int targetBattleConditionCount)
    {
        // インデックスがnullなら発動不可
        if (battleMonsterIndex == null) return false;

        var battleChainParticipant = new BattleChainParticipantInfo()
        {
            battleMonsterIndex = battleMonsterIndex,
            battleActionType = BattleActionType.PassiveSkill,
            battleConditionCount = battleConditionCount,
            targetBattleActionType = targetBattleActionType,
            targetBattleMonsterIndex = targetBattleMonsterIndex,
            targetBattleConditionCount = targetBattleConditionCount,
        };

        // チェーン内での発動制限回数が無条件で1回であるトリガータイプリストを取得
        var limitExecute1InChainTriggerTypeList = new List<SkillTriggerType>()
        {
            SkillTriggerType.OnMeActionStart,
            SkillTriggerType.OnMeActionEnd,
        };

        // TODO: 発動回数制限処理
        // 対象のトリガータイプがこのリスト内に存在しかつすでに同じ条件のスキルを発動していた場合発動不可
        // if (limitExecute1InChainTriggerTypeList.Contains(triggerType) && battleChainParticipantList.Any(p => p.IsSame(battleChainParticipant))) return false;

        // 反撃系は同じモンスターの同じスキルを起因とするものは同一チェイン内で2回以上発動できない
        // if (triggerType == SkillTriggerType.OnMeBeExecutedNormalOrUltimateSkill && battleChainParticipantList.Any(p => p.IsSame(battleChainParticipant))) return false;

        return true;
    }
}