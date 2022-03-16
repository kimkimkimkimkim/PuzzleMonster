using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// トリガー発動関係のスキル発動を扱うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private void ExecuteTriggerSkillIfNeeded(SkillTriggerType triggerType, List<BattleMonsterIndex> targetBattleMonsterIndexList, int triggerTypeOptionValue = 0)
    {
        targetBattleMonsterIndexList.ForEach(index =>
        {
            ExecuteTriggerSkillIfNeeded(triggerType, index, triggerTypeOptionValue);
        });
    }

    private void ExecuteTriggerSkillIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex targetBattleMonsterIndex, int triggerTypeOptionValue = 0)
    {
        // パッシブスキルを発動
        ExecutePassiveIfNeeded(triggerType, targetBattleMonsterIndex, triggerTypeOptionValue);

        // 状態異常効果を発動
        ExecuteBattleConditionIfNeeded(triggerType, targetBattleMonsterIndex);
    }

    // パッシブスキルを発動
    private void ExecutePassiveIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex targetBattleMonsterIndex, int triggerTypeOptionValue = 0)
    {
        // インデックスがnullまたはすでにチェインに参加していたら発動しない
        if (targetBattleMonsterIndex == null || battleChainParticipantList.Any(p => p.battleMonsterIndex.IsSame(targetBattleMonsterIndex) && p.battleActionType == BattleActionType.PassiveSkill)) return;

        // 発動条件を満たしていなければだめ
        var targetBattleMonster = GetBattleMonster(targetBattleMonsterIndex);
        var targetMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(targetBattleMonster.monsterId);
        var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(targetMonster.passiveSkillId);
        if (!IsValidActivateCondition(targetBattleMonsterIndex, passiveSkill.activateConditionType)) return;

        // 発動回数制限に達していたらだめ
        if (passiveSkill.limitExecuteNum > 0 && targetBattleMonster.passiveSkillExecuteCount >= passiveSkill.limitExecuteNum) return;

        // 指定したトリガータイプのパッシブスキルを保持していれば発動
        var skillEffectList = passiveSkill.effectList.Where(effect => effect.triggerType == triggerType && effect.triggerTypeOptionValue == triggerTypeOptionValue).Select(effect => (SkillEffectMI)effect).ToList();
        if (skillEffectList.Any()) StartActionStream(targetBattleMonsterIndex, BattleActionType.PassiveSkill, skillEffectList);
    }

    // 状態異常効果を発動
    private void ExecuteBattleConditionIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex targetBattleMonsterIndex)
    {
        // インデックスがnullまたはすでにチェインに参加していたら発動しない
        if (targetBattleMonsterIndex == null || battleChainParticipantList.Any(p => p.battleMonsterIndex.IsSame(targetBattleMonsterIndex) && p.battleActionType == BattleActionType.BattleCondition)) return;

        var targetBattleMonster = GetBattleMonster(targetBattleMonsterIndex);
        var skillEffectList = targetBattleMonster.battleConditionList
            .Where(c => c.battleCondition.triggerType == triggerType)
            .Where(c => IsValidActivateCondition(targetBattleMonsterIndex, c.battleCondition.activateConditionType))
            .Select(c => c.skillEffect)
            .ToList();
        if (skillEffectList.Any()) StartActionStream(targetBattleMonsterIndex, BattleActionType.BattleCondition, skillEffectList);
    }
}
