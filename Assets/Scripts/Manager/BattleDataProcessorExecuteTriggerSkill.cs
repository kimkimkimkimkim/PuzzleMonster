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

        // パッシブスキルを取得していない場合は何もしない
        var targetBattleMonster = GetBattleMonster(battleMonsterIndex);
        var targetMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(targetBattleMonster.monsterId);
        var passiveSkillId = ClientMonsterUtil.GetPassiveSkillId(targetMonster.id, targetBattleMonster.level);
        var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(passiveSkillId);
        if (passiveSkill == null) return;

        // 発動条件をみたしたスキルが存在していれば発動
        var skillEffectList = passiveSkill.effectList
            .Where(effect =>
            {
                // 実行者条件を満たしているか
                if (!IsValidActivateCondition(battleMonsterIndex, effect.activateConditionType, effect.activateConditionValue, 0)) return false;

                // TODO: 発動回数条件を満たしているか
                if (passiveSkill.limitExecuteNum > 0 && targetBattleMonster.passiveSkillExecuteCount >= passiveSkill.limitExecuteNum) return false;

                // トリガータイプがあっているか
                if (effect.triggerType != triggerType || effect.triggerTypeOptionValue != triggerTypeOptionValue) return false;

                return true;
            })
            .Select(effect => (SkillEffectMI)effect)
            .ToList();
        if (skillEffectList.Any())
        {
            StartActionStream(battleMonsterIndex, BattleActionType.PassiveSkill, null, skillEffectList);
        }
    }

    // 状態異常効果を発動
    private void ExecuteBattleConditionIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex battleMonsterIndex, BattleMonsterIndex targetBattleMonsterIndex, BattleActionType targetBattleActionType, int targetBattleConditionCount)
    {
        var targetBattleMonster = GetBattleMonster(battleMonsterIndex);
        var targetBattleConditionList = targetBattleMonster.battleConditionList
            .Where(c => {
                // 状態異常効果の発動条件はマスタのスキルエフェクトを参照する
                var battleConditionMB = MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.battleCondition.id);
                if ( battleConditionMB.skillEffect.triggerType != triggerType) return false;
                if (!IsValidActivateCondition(battleMonsterIndex, battleConditionMB.skillEffect.activateConditionType, battleConditionMB.skillEffect.activateConditionValue, battleConditionMB.id)) return false;
                return true;
            })
            .ToList();

        targetBattleConditionList.ForEach(battleCondition =>
        {
            // どの状態異常効果が発動するかによって条件が変わるのでここで判定
            if (IsValidChain(triggerType, battleMonsterIndex, battleCondition.order, targetBattleMonsterIndex, targetBattleActionType, targetBattleConditionCount))
            {
                StartActionStream(battleMonsterIndex, BattleActionType.BattleCondition, battleCondition, new List<SkillEffectMI>() { battleCondition.skillEffect });
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

        // TODO: 発動回数制限処理
        // 対象のトリガータイプがこのリスト内に存在しかつすでに同じ条件のスキルを発動していた場合発動不可
        // if (limitExecute1InChainTriggerTypeList.Contains(triggerType) && battleChainParticipantList.Any(p => p.IsSame(battleChainParticipant))) return false;

        // 反撃系は同じモンスターの同じスキルを起因とするものは同一チェイン内で2回以上発動できない
        // if (triggerType == SkillTriggerType.OnMeBeExecutedNormalOrUltimateSkill && battleChainParticipantList.Any(p => p.IsSame(battleChainParticipant))) return false;

        return true;
    }
}
