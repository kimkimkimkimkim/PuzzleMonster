using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 状態異常効果関係の処理を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private void ExecuteBattleConditionIfNeeded(BattleConditionTriggerType triggerType, BattleMonsterIndex actionMonsterIndex = null, List<BattleMonsterIndex> beDoneActionMonsterIndexList = null)
    {
        switch (triggerType) {
            case BattleConditionTriggerType.OnBattleStart:
                ExecuteOnBattleStartBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnWaveStart:
                ExecuteOnWaveStartBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnTurnStart:
                ExecuteOnTurnStartBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnMeActionStart:
                ExecuteOnMeActionStartBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnMeActionEnd:
                ExecuteOnMeActionEndBattleConditionIfNeeded(actionMonsterIndex);
                break;
            case BattleConditionTriggerType.OnMeNormalSkillEnd:
                ExecuteOnMeNormalSkillEndBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnMeUltimateSkillEnd:
                ExecuteOnMeUltimateSkillEndBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnMeTakeDamageEnd:
                ExecuteOnMeTakeDamageEndBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnMeTakeActionBefore:
                ExecuteOnMeTakeActionBeforeBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnMeTakeActionAfter:
                ExecuteOnMeTakeActionAfterBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.EveryTimeEnd:
                ExecuteEveryTimeEndBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnMeDeadEnd:
                ExecuteOnMeDeadEndBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnTurnEnd:
                ExecuteOnTurnEndBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnWaveEnd:
                ExecuteOnWaveEndBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.OnRemoved:
                ExecuteOnRemovedBattleConditionIfNeeded();
                break;
            case BattleConditionTriggerType.Always:
            default:
                break;
        }
    }

    private void ExecuteOnBattleStartBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnWaveStartBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnTurnStartBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnMeActionStartBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnMeActionEndBattleConditionIfNeeded(BattleMonsterIndex actionMonsterIndex)
    {
        if (actionMonsterIndex == null) return;

        var actionBattleMonster = GetBattleMonster(actionMonsterIndex);
        if (actionBattleMonster.isDead) return;

        var skillEffectList = actionBattleMonster.battleConditionList
            .Where(c => c.battleCondition.battleConditionTriggerType == BattleConditionTriggerType.OnMeActionEnd)
            .Select(c => c.skillEffect)
            .ToList();
        if (skillEffectList.Any()) StartActionStream(actionMonsterIndex, BattleActionType.BattleCondition, skillEffectList);
    }

    private void ExecuteOnMeNormalSkillEndBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnMeUltimateSkillEndBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnMeTakeDamageEndBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnMeTakeActionBeforeBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnMeTakeActionAfterBattleConditionIfNeeded()
    {

    }

    private void ExecuteEveryTimeEndBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnMeDeadEndBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnTurnEndBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnWaveEndBattleConditionIfNeeded()
    {

    }

    private void ExecuteOnRemovedBattleConditionIfNeeded()
    {

    }
}