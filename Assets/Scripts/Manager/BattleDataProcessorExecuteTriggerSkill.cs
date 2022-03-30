using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// トリガー発動関係のスキル発動を扱うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private void ExecuteTriggerSkillIfNeeded(SkillTriggerType triggerType, List<BattleMonsterIndex> battleMonsterIndexList, int triggerTypeOptionValue = 0, BattleMonsterIndex targetBattleMonsterIndex = null, BattleActionType targetBattleActionType = BattleActionType.None, int targetBattleConditionCount = 0)
    {
        battleMonsterIndexList.ForEach(index =>
        {
            ExecuteTriggerSkillIfNeeded(triggerType, index, triggerTypeOptionValue, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount);
        });
    }

    private void ExecuteTriggerSkillIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, int triggerTypeOptionValue = 0, BattleMonsterIndex targetBattleMonsterIndex = null, BattleActionType targetBattleActionType = BattleActionType.None, int targetBattleConditionCount = 0)
    {
        // パッシブスキルを発動
        ExecutePassiveIfNeeded(triggerType, battleMonsterIndex, triggerTypeOptionValue, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount);

        // 状態異常効果を発動
        ExecuteBattleConditionIfNeeded(triggerType, battleMonsterIndex, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount);
    }

    // パッシブスキルを発動
    private void ExecutePassiveIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, int triggerTypeOptionValue, BattleMonsterIndex targetBattleMonsterIndex, BattleActionType targetBattleActionType, int targetBattleConditionCount)
    {
        // チェーンの状況を元に発動可能か判断
        if (!IsValidChain(triggerType, battleMonsterIndex, 0, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount)) return;

        // 発動条件を満たしていなければだめ
        var targetBattleMonster = GetBattleMonster(battleMonsterIndex);
        var targetMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(targetBattleMonster.monsterId);
        var passiveSkillId = ClientMonsterUtil.GetPassiveSkillId(targetMonster.id, targetBattleMonster.level);
        var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(passiveSkillId);
        if (passiveSkill == null || !IsValidActivateCondition(battleMonsterIndex, passiveSkill.activateConditionType)) return;

        // 発動回数制限に達していたらだめ
        if (passiveSkill.limitExecuteNum > 0 && targetBattleMonster.passiveSkillExecuteCount >= passiveSkill.limitExecuteNum) return;

        // 指定したトリガータイプのパッシブスキルを保持していれば発動
        var skillEffectList = passiveSkill.effectList.Where(effect => effect.triggerType == triggerType && effect.triggerTypeOptionValue == triggerTypeOptionValue).Select(effect => (SkillEffectMI)effect).ToList();
        if (skillEffectList.Any())
        {
            var battleChainParticipant = new BattleChainParticipantInfo()
            {
                battleMonsterIndex = battleMonsterIndex,
                battleActionType = BattleActionType.PassiveSkill,
                targetBattleActionType = targetBattleActionType,
                targetBattleMonsterIndex = targetBattleMonsterIndex,
                targetBattleConditionCount = targetBattleConditionCount,
            };
            StartActionStream(battleMonsterIndex, BattleActionType.PassiveSkill, skillEffectList, battleChainParticipant);
        }
    }

    // 状態異常効果を発動
    private void ExecuteBattleConditionIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, BattleMonsterIndex targetBattleMonsterIndex, BattleActionType targetBattleActionType, int targetBattleConditionCount)
    {
        var targetBattleMonster = GetBattleMonster(battleMonsterIndex);
        var targetBattleConditionList = targetBattleMonster.battleConditionList
            .Where(c => c.battleCondition.triggerType == triggerType)
            .Where(c => IsValidActivateCondition(battleMonsterIndex, c.battleCondition.activateConditionType))
            .ToList();

        targetBattleConditionList.ForEach(battleCondition =>
        {
            var battleChainParticipant = new BattleChainParticipantInfo()
            {
                battleMonsterIndex = battleMonsterIndex,
                battleActionType = BattleActionType.PassiveSkill,
                battleConditionCount = battleCondition.order,
                targetBattleActionType = targetBattleActionType,
                targetBattleMonsterIndex = targetBattleMonsterIndex,
                targetBattleConditionCount = targetBattleConditionCount,
            };

            // どの状態異常効果が発動するかによって条件が変わるのでここで判定
            if (IsValidChain(triggerType, battleMonsterIndex, battleCondition.order, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount))
            {
                StartActionStream(battleMonsterIndex, BattleActionType.BattleCondition, new List<SkillEffectMI>() { battleCondition.skillEffect }, battleChainParticipant);
            }
        });
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
        // 対象のトリガータイプがこのリスト内に存在しかつすでに同じ条件のスキルを発動していた場合発動不可
        if (limitExecute1InChainTriggerTypeList.Contains(triggerType) && battleChainParticipantList.Any(p => p.IsSame(battleChainParticipant))) return false;

        // 反撃系は同じモンスターの同じスキルを起因とするものは同一チェイン内で2回以上発動できない
        if (triggerType == SkillTriggerType.OnMeBeExecutedNormalOrUltimateSkill && battleChainParticipantList.Any(p => p.IsSame(battleChainParticipant))) return false;

        return true;
    }
}
