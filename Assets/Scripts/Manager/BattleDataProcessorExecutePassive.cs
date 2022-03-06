using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// パッシブスキル効果関係の処理を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private void ExecutePassiveIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex actionMonsterIndex = null, List<BattleMonsterIndex> beDoneActionMonsterIndexList = null)
    {
        switch (triggerType)
        {
            case SkillTriggerType.OnBattleStart:
                ExecuteOnBattleStartPassiveIfNeeded();
                break;
            case SkillTriggerType.OnWaveStart:
                ExecuteOnWaveStartPassiveIfNeeded();
                break;
            case SkillTriggerType.OnTurnStart:
                ExecuteOnTurnStartPassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeActionStart:
                ExecuteOnMeActionStartPassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeActionEnd:
                ExecuteOnMeActionEndPassiveIfNeeded(actionMonsterIndex);
                break;
            case SkillTriggerType.OnMeNormalSkillEnd:
                ExecuteOnMeNormalSkillEndPassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeUltimateSkillEnd:
                ExecuteOnMeUltimateSkillEndPassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeTakeDamageEnd:
                ExecuteOnMeTakeDamageEndPassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeTakeActionBefore:
                ExecuteOnMeTakeActionBeforePassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeTakeActionAfter:
                ExecuteOnMeTakeActionAfterPassiveIfNeeded();
                break;
            case SkillTriggerType.EveryTimeEnd:
                ExecuteEveryTimeEndPassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeDeadEnd:
                ExecuteOnMeDeadEndPassiveIfNeeded();
                break;
            case SkillTriggerType.OnTurnEnd:
                ExecuteOnTurnEndPassiveIfNeeded();
                break;
            case SkillTriggerType.OnWaveEnd:
                ExecuteOnWaveEndPassiveIfNeeded();
                break;
            default:
                break;
        }
    }

    private void ExecuteOnBattleStartPassiveIfNeeded()
    {
        GetAllMonsterList()
            .ForEach(m => {
                if (chainParticipantMonsterIndexList.Contains(m.index)) return;
                if (m.isDead) return;

                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                var effectList = passiveSkill.effectList.Where(effect => effect.triggerType == SkillTriggerType.OnBattleStart).Select(effect => (SkillEffectMI)effect).ToList();
                if (!effectList.Any()) return;

                StartActionStream(m.index, BattleActionType.PassiveSkill, effectList);
                chainParticipantMonsterIndexList.Clear();
            });
    }

    private void ExecuteOnWaveStartPassiveIfNeeded()
    {

    }

    private void ExecuteOnTurnStartPassiveIfNeeded()
    {

    }

    private void ExecuteOnMeActionStartPassiveIfNeeded()
    {

    }

    private void ExecuteOnMeActionEndPassiveIfNeeded(BattleMonsterIndex actionMonsterIndex)
    {
        if (actionMonsterIndex == null || chainParticipantMonsterIndexList.Contains(actionMonsterIndex)) return;

        var actionBattleMonster = GetBattleMonster(actionMonsterIndex);
        if (actionBattleMonster.isDead) return;

        var actionMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(actionBattleMonster.monsterId);
        var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(actionMonster.passiveSkillId);
        var skillEffectList = passiveSkill.effectList.Where(effect => effect.triggerType == SkillTriggerType.OnMeActionEnd).Select(effect => (SkillEffectMI)effect).ToList();
        if (skillEffectList.Any()) StartActionStream(actionMonsterIndex, BattleActionType.PassiveSkill, skillEffectList);
    }

    private void ExecuteOnMeNormalSkillEndPassiveIfNeeded()
    {

    }

    private void ExecuteOnMeUltimateSkillEndPassiveIfNeeded()
    {

    }

    private void ExecuteOnMeTakeDamageEndPassiveIfNeeded()
    {

    }

    private void ExecuteOnMeTakeActionBeforePassiveIfNeeded()
    {

    }

    private void ExecuteOnMeTakeActionAfterPassiveIfNeeded()
    {

    }

    private void ExecuteEveryTimeEndPassiveIfNeeded()
    {

    }

    private void ExecuteOnMeDeadEndPassiveIfNeeded()
    {

    }

    private void ExecuteOnTurnEndPassiveIfNeeded()
    {

    }

    private void ExecuteOnWaveEndPassiveIfNeeded()
    {

    }
}