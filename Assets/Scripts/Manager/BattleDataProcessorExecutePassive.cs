using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// パッシブスキル効果関係の処理を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private void ExecutePassiveIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex targetBattleMonsterIndex)
    {
        // インデックスがnullまたはすでにチェインに参加していたら発動しない
        if (targetBattleMonsterIndex == null || battleChainParticipantList.Any(p => p.battleMonsterIndex.IsSame(targetBattleMonsterIndex) && p.battleActionType == BattleActionType.PassiveSkill)) return;

        // 発動条件を満たしていなければだめ
        var targetBattleMonster = GetBattleMonster(targetBattleMonsterIndex);
        var targetMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(targetBattleMonster.monsterId);
        var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(targetMonster.passiveSkillId);
        if (!IsValid(targetBattleMonsterIndex, passiveSkill.activateConditionType)) return;

        // 指定したトリガータイプのパッシブスキルを保持していれば発動
        var skillEffectList = passiveSkill.effectList.Where(effect => effect.triggerType == triggerType).Select(effect => (SkillEffectMI)effect).ToList();
        if (skillEffectList.Any()) StartActionStream(targetBattleMonsterIndex, BattleActionType.PassiveSkill, skillEffectList);
    }

    private void ExecutePassiveIfNeeded(SkillTriggerType triggerType, List<BattleMonsterIndex> targetBattleMonsterIndexList)
    {
        targetBattleMonsterIndexList.ForEach(index =>
        {
            ExecutePassiveIfNeeded(triggerType, index);
        });
    }
}
