using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 状態異常効果関係の処理を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private void ExecuteBattleConditionIfNeeded(SkillTriggerType  triggerType, BattleMonsterIndex targetBattleMonsterIndex)
    {
        // インデックスがnullまたはすでにチェインに参加していたら発動しない
        if (targetBattleMonsterIndex == null || battleChainParticipantList.Any(p => p.battleMonsterIndex.IsSame(targetBattleMonsterIndex) && p.battleActionType == BattleActionType.BattleCondition)) return;
        
        var targetBattleMonster = GetBattleMonster(targetBattleMonsterIndex);
        var skillEffectList = targetBattleMonster.battleConditionList
            .Where(c => c.battleCondition.triggerType == triggerType)
            .Where(c => IsValid(targetBattleMonsterIndex, c.battleCondition.activateConditionType))
            .Select(c => c.skillEffect)
            .ToList();
        if (skillEffectList.Any()) StartActionStream(targetBattleMonsterIndex, BattleActionType.BattleCondition, skillEffectList);
    }

    private void ExecuteBattleConditionIfNeeded(SkillTriggerType  triggerType, List<BattleMonsterIndex> targetBattleMonsterIndexList)
    {
        targetBattleMonsterIndexList.ForEach(index =>
        {
            ExecuteBattleConditionIfNeeded(triggerType, index);
        });
    }
}
