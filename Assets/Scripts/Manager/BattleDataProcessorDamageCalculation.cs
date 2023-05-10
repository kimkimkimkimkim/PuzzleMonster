using PM.Enum.Battle;
using PM.Enum.Monster;
using System.Linq;
using UnityEngine;

/// <summary>
/// ダメージ計算を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
    private BattleActionValueData GetActionValue(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect, BattleActionType actionType, string skillGuid, int skillEffectIndex, BattleConditionInfo battleConditionInfo)
    {
        var doBattleMonster = GetBattleMonster(doMonsterIndex);
        var beDoneBattleMonster = GetBattleMonster(beDoneMonsterIndex);

        var data = new BattleActionValueData();

        // 固定値タイプの状態異常効果であれば先に返す
        if (actionType == BattleActionType.BattleCondition && battleConditionInfo != null)
        {
            var battleConditionMB = battleConditionList.First(m => m.id == battleConditionInfo.battleConditionId);
            if (battleConditionMB.actionValueType == BattleConditionActionValueType.Fixed)
            {
                data.value = battleConditionInfo.actionValue;
                return data;
            }
        }

        switch (skillEffect.type)
        {
            case SkillType.Attack:
            case SkillType.Damage:
            case SkillType.WithoutFactorDamage:
                switch (skillEffect.valueTargetType)
                {
                    // HPを基準を参照する系は他の要素を含まないダメージで計算
                    case ValueTargetType.MyCurrentHP:
                    case ValueTargetType.MyMaxHp:
                    case ValueTargetType.TargetCurrentHP:
                    case ValueTargetType.TargetMaxHp:
                        data.value = GetActionValueReferenceHp(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex);
                        break;
                    // ダメージを参照する系の値を取得
                    case ValueTargetType.FirstElementDamage:
                    case ValueTargetType.JustBeforeElementDamage:
                    case ValueTargetType.JustBeforeElementRemoveBattleConditionRemainDamage:
                    case ValueTargetType.AllBeforeElementRemoveBattleConditionRemainDamage:
                    case ValueTargetType.JustBeforeTakeDamage:
                        data = new BattleActionValueData() { value = GetActionValueReferenceDamage(doBattleMonster, beDoneBattleMonster, skillEffect, actionType, skillGuid, skillEffectIndex) };
                        break;
                    // それ以外のダメージの場合は含めて計算
                    default:
                        data = GetActionValueWithFactor(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex);
                        break;
                }
                break;

            case SkillType.Heal:
                data.value = GetHealValue(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex);
                break;

            case SkillType.Revive:
                data.value = GetActionValueRevive(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex);
                break;

            case SkillType.EnergyUp:
            case SkillType.EnergyDown:
                data.value = GetActionValueEnergy(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex);
                break;

            case SkillType.ConditionAdd:
            case SkillType.ConditionRemove:
            case SkillType.Status:
            default:
                data = new BattleActionValueData();
                break;
        }

        var doMonsterSide = doMonsterIndex.isPlayer ? "味" : "敵";
        var beDoneMonsterSide = beDoneMonsterIndex.isPlayer ? "味" : "敵";
        var arrow =
            doMonsterIndex.isPlayer && !beDoneMonsterIndex.isPlayer ? ">>>>>>>>>>"
            : !doMonsterIndex.isPlayer && beDoneMonsterIndex.isPlayer ? "<<<<<<<<<<"
            : "----------";
        testLog += $"\n{actionType.ToString()[0]}: {doMonsterSide}({doBattleMonster.maxHp}) {arrow} {beDoneMonsterSide}({beDoneBattleMonster.maxHp}) へ {data.value}";
        return data;
    }

    /// <summary>
    /// 様々な要因を加味したアクション値を取得する
    /// </summary>
    private BattleActionValueData GetActionValueWithFactor(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        // Incoming Damage × (1 – Reduce Damage %) × [((1 – Armor Mitigation %) × (1 - Armor Break %))  + 70% × Holy Damage % + 30% × Luck Damage % ]
        const float HOLY_DAMAGE_MAGNIFICATION = 70.0f;
        const float LUCK_DAMAGE_MAGNIFICATION = 30.0f;
        const float BLOCK_DAMAGE_REDUCE_RATE = 33.0f;

        var coefficient = GetValueCoefficient(skillEffect);
        var incomingDamage = IncomingDamage(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex);
        var isBlocked = ExecuteProbability(beDoneBattleMonster.blockRate() - doBattleMonster.attackAccuracy());
        var damage =
            (int)(
                incomingDamage.damage                                                       // 基準ダメージ
                * (1 - GetRate(beDoneBattleMonster.damageResistRate()))                     // ダメージ軽減分ダメージを軽減
                * (
                    (1 - ArmorMitigation(beDoneBattleMonster))                              // 防御力分ダメージを軽減
                    * (1 - GetRate(doBattleMonster.defensePenetratingRate()))               // 防御貫通率分防御力を無視
                    + HOLY_DAMAGE_MAGNIFICATION * GetRate(doBattleMonster.holyDamageRate()) // 神聖ダメージを加算
                    + LUCK_DAMAGE_MAGNIFICATION * GetRate(doBattleMonster.luckDamageRate()) // ラックダメージを加算
                )
                * (
                    isBlocked ?                                                             // ブロックしたかを判定
                    ((float)(100 - BLOCK_DAMAGE_REDUCE_RATE) / 100) :                       // ブロックしていればブロックでの軽減率分ダメージを軽減
                    1                                                                       // ブロックしていなければそのまま
                )
            );
        return new BattleActionValueData()
        {
            value = coefficient * damage,
            isCritical = incomingDamage.isCritical,
            isBlocked = isBlocked
        };
    }

    /// <summary>
    /// HPを参照するタイプのアクション値を取得する
    /// </summary>
    private int GetActionValueReferenceHp(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        var coefficient = GetValueCoefficient(skillEffect);
        return (int)(coefficient * GetStatusValue(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex) * GetRate(skillEffect.value));
    }

    /// <summary>
    /// 様々な要因を加味しないアクション値を取得する
    /// </summary>
    private int GetActionValueWithoutFactor(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        var coefficient = GetValueCoefficient(skillEffect);
        return (int)(coefficient * GetStatusValue(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex) * GetRate(skillEffect.value));
    }

    /// <summary>
    /// ダメージを参照するタイプのアクション値を取得する
    /// </summary>
    private int GetActionValueReferenceDamage(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect, BattleActionType actionType, string skillGuid, int skillEffectIndex)
    {
        var coefficient = GetValueCoefficient(skillEffect);
        var value = 0;
        switch (skillEffect.valueTargetType)
        {
            case ValueTargetType.FirstElementDamage:
                {
                    var battleLog = battleLogList
                        .Where(l => l.waveCount == currentWaveCount && l.turnCount == currentTurnCount)
                        .Where(l => l.doBattleMonsterIndex.IsSame(doBattleMonster.index))
                        .Where(l => l.type == BattleLogType.TakeDamage && l.skillGuid == skillGuid && l.skillEffectIndex == 0)
                        .FirstOrDefault();
                    value = battleLog == null ? 0 : -battleLog.beDoneBattleMonsterDataList.Sum(d => d.hpChanges);
                    break;
                }
            case ValueTargetType.JustBeforeElementDamage:
                {
                    var battleLog = battleLogList
                        .Where(l => l.waveCount == currentWaveCount && l.turnCount == currentTurnCount)
                        .Where(l => l.doBattleMonsterIndex.IsSame(doBattleMonster.index))
                        .Where(l => l.type == BattleLogType.TakeDamage && l.skillGuid == skillGuid && l.skillEffectIndex == skillEffectIndex - 1)
                        .FirstOrDefault();
                    value = battleLog == null ? 0 : -battleLog.beDoneBattleMonsterDataList.Sum(d => d.hpChanges);
                    break;
                }
            case ValueTargetType.JustBeforeElementRemoveBattleConditionRemainDamage:
                value = GetRemovedBattleConditionRemainDamage(beDoneBattleMonster, skillGuid, skillEffectIndex - 1);
                break;

            case ValueTargetType.AllBeforeElementRemoveBattleConditionRemainDamage:
                {
                    var v = 0;
                    for (var i = 0; i < skillEffectIndex; i++)
                    {
                        v += GetRemovedBattleConditionRemainDamage(beDoneBattleMonster, skillGuid, i);
                    }
                    value = v;
                    break;
                }
            case ValueTargetType.JustBeforeTakeDamage:
                {
                    var battleLogIndex = battleLogList.FindLastIndex(log =>
                    {
                        if (log.type != BattleLogType.TakeDamage) return false;
                        if (!log.beDoneBattleMonsterDataList.Any(d => d.battleMonsterIndex.IsSame(doBattleMonster.index))) return false;
                        return true;
                    });
                    if (battleLogIndex > 0)
                    {
                        var battleLog = battleLogList[battleLogIndex];
                        value = -battleLog.beDoneBattleMonsterDataList.First(d => d.battleMonsterIndex.IsSame(doBattleMonster.index)).hpChanges;
                    }
                }
                break;

            default:
                break;
        }
        return (int)(coefficient * value * GetRate(skillEffect.value));
    }

    /// <summary>
    /// 指定したスキル効果で解除した状態異常の残りダメージを取得する
    /// </summary>
    private int GetRemovedBattleConditionRemainDamage(BattleMonsterInfo beDoneBattleMonster, string skillGuid, int skillEffectIndex)
    {
        var beforeBattleLog = battleLogList.FirstOrDefault(l => l.type == BattleLogType.TakeBattleConditionRemoveBefore && l.skillGuid == skillGuid && l.skillEffectIndex == skillEffectIndex);
        var afterBattleLog = battleLogList.FirstOrDefault(l => l.type == BattleLogType.TakeBattleConditionRemoveAfter && l.skillGuid == skillGuid && l.skillEffectIndex == skillEffectIndex);
        if (beforeBattleLog == null || afterBattleLog == null) return 0;

        var beforeBeDoneMonterData = beforeBattleLog.beDoneBattleMonsterDataList.FirstOrDefault(d => d.battleMonsterIndex.IsSame(beDoneBattleMonster.index));
        var afterBeDoneMonterData = afterBattleLog.beDoneBattleMonsterDataList.FirstOrDefault(d => d.battleMonsterIndex.IsSame(beDoneBattleMonster.index));
        if (beforeBeDoneMonterData == null || afterBeDoneMonterData == null) return 0;

        var removedBattleConditionList = beforeBeDoneMonterData.battleConditionList
            .Where(beforeC =>
            {
                // 解除後状態異常リストに存在しないものだけに絞り込む
                return !afterBeDoneMonterData.battleConditionList.Any(afterC => afterC.guid == beforeC.guid);
            })
            .ToList();
        return removedBattleConditionList.Sum(c => c.remainingTurnNum * c.actionValue);
    }

    /// <summary>
    /// 回復値を取得する
    /// </summary>
    private int GetHealValue(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        var coefficient = GetValueCoefficient(skillEffect);
        return (int)(
            coefficient
            * GetStatusValue(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex)                              // 対象のステータス値
            * GetRate(skillEffect.value)                                                                  // ダメージ倍率
            * (1 + GetRate(doBattleMonster.healingRate()))                                                // 与回復率分回復量を上昇
            * (1 + GetRate(beDoneBattleMonster.healedRate()))                                             // 被回復率分回復量を上昇
        );
    }

    /// <summary>
	/// 蘇生時の体力を取得する
	/// </summary>
	private int GetActionValueRevive(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        var coefficient = GetValueCoefficient(skillEffect);
        return (int)(coefficient * GetStatusValue(doBattleMonster, beDoneBattleMonster, skillEffect, skillGuid, skillEffectIndex) * GetRate(skillEffect.value));
    }

    /// <summary>
	/// エネルギー変動時のアクション値を取得する
	/// </summary>
	private int GetActionValueEnergy(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        var coefficient = GetValueCoefficient(skillEffect);
        return (int)(coefficient * skillEffect.value);
    }

    private (float damage, bool isCritical) IncomingDamage(BattleMonsterInfo doMonster, BattleMonsterInfo beDoneMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        // 攻撃×攻撃倍率×クリ時(1.5+0.02×クリダメ)
        const float CRITICAL_DAMAGE_MAGNIFICATION = 1.5f;
        const float CRITICAL_DAMAGE_COEFFICIANT = 0.02f;

        var isCritical = ExecuteProbability(doMonster.criticalRate());
        var damage =
            GetStatusValue(doMonster, beDoneMonster, skillEffect, skillGuid, skillEffectIndex)             // 対象のステータス値
            * GetRate(skillEffect.value, false)                                                            // ダメージ倍率
            * BattleConditionKiller(doMonster, beDoneMonster)                                              // 状態異常特攻倍率
            * BuffTypeNumKiller(doMonster, beDoneMonster)                                                  // 指定バフタイプの個数特攻倍率
            * MonsterAttributeKiller(doMonster, beDoneMonster)                                             // 属性特攻倍率
            * MonsterAttributeCompatibility(doMonster, beDoneMonster)                                      // 属性相性
            * AttackAccuracyCompatibility(doMonster, beDoneMonster)                                        // 攻撃精度倍率
            * (
                isCritical ?                                                                               // クリティカルかどうかを判定
                (CRITICAL_DAMAGE_MAGNIFICATION + CRITICAL_DAMAGE_COEFFICIANT * doMonster.criticalDamage()) // クリティカルならクリティカルダメージ
                : 1.0f                                                                                     // クリティカルでなければそのまま
            );
        return (damage, isCritical);
    }

    private float BattleConditionKiller(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
    {
        var rate = doBattleMonster.battleConditionList
            .Sum(c =>
            {
                var battleCondition = battleConditionList.First(m => m.id == c.battleConditionId);

                // 状態異常特攻でなければ何もしない
                if (battleCondition.battleConditionType != BattleConditionType.BattleConditionKiller) return 0;

                var existsTargetBattleCondition = beDoneBattleMonster.battleConditionList.Any(condition => condition.battleConditionId == battleCondition.targetBattleConditionId);
                if (existsTargetBattleCondition)
                {
                    // 相手が対象の状態異常を保持していれば倍率を返す
                    return c.grantorSkillEffect.value;
                }
                else
                {
                    // 相手が対象の状態異常を保持していなければ何もしない
                    return 0;
                }
            });
        return GetRate(rate + 100);
    }

    private float BuffTypeNumKiller(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
    {
        var rate = doBattleMonster.battleConditionList
            .Sum(c =>
            {
                var battleCondition = battleConditionList.First(m => m.id == c.battleConditionId);

                // バフタイプ個数特攻でなければ何もしない
                if (battleCondition.battleConditionType != BattleConditionType.BuffTypeNumKiller) return 0;

                // 対象のバフタイプの状態異常を取得して倍率を返す
                var targetBattleConditionNum = beDoneBattleMonster.battleConditionList
                    .Where(condition => battleConditionList.First(m => m.id == condition.battleConditionId).buffType == battleCondition.targetBuffType)
                    .Count();
                return targetBattleConditionNum * 5; // TODO: 個数バフの倍率の計算
            });
        return GetRate(rate + 100);
    }

    private float MonsterAttributeKiller(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
    {
        var beDoneMonster = monsterList.First(m => m.id == beDoneBattleMonster.monsterId);
        var rate = doBattleMonster.battleConditionList
            .Sum(c =>
            {
                var battleCondition = battleConditionList.First(m => m.id == c.battleConditionId);

                // 属性特攻でなければ何もしない
                if (battleCondition.battleConditionType != BattleConditionType.MonsterAttributeKiller) return 0;

                // 対象の属性でなければ何もしない
                if (battleCondition.targetMonsterAttribute != beDoneMonster.attribute) return 0;

                return c.grantorSkillEffect.value;
            });
        return GetRate(rate + 100);
    }

    private float MonsterAttributeCompatibility(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
    {
        const int MONSTER_ATTRIBUTE_COMPATIBILITY_RATE = 15;
        var doMonster = monsterList.First(m => m.id == doBattleMonster.monsterId);
        var beDoneMonster = monsterList.First(m => m.id == beDoneBattleMonster.monsterId);

        var rate = doMonster.attribute.IsAdvantageous(beDoneMonster.attribute) ? MONSTER_ATTRIBUTE_COMPATIBILITY_RATE
            : doMonster.attribute.IsDisadvantage(beDoneMonster.attribute) ? -MONSTER_ATTRIBUTE_COMPATIBILITY_RATE
            : 0;

        return GetRate(rate + 100);
    }

    private float AttackAccuracyCompatibility(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
    {
        const float MONSTER_ATTRIBUTE_COMPATIBILITY_RATE = 0.3f; // 攻撃精度1%につき何%ダメージアップするか
        const float ADVANTAGE_PLUS_VALUE = 15.0f; // 属性有利時に攻撃精度が何%上昇するか
        var doMonster = monsterList.First(m => m.id == doBattleMonster.monsterId);
        var beDoneMonster = monsterList.First(m => m.id == beDoneBattleMonster.monsterId);

        var rate = (float)doBattleMonster.attackAccuracy();
        if (doMonster.attribute.IsAdvantageous(beDoneMonster.attribute)) rate += ADVANTAGE_PLUS_VALUE;
        rate *= MONSTER_ATTRIBUTE_COMPATIBILITY_RATE;

        return GetRate(rate + 100);
    }

    private float ArmorMitigation(BattleMonsterInfo beDoneMonster)
    {
        // 防御/(180+22×Level)
        const float DEFENSE_RESISTIVITY = 300.0f;
        const float LEVEL_MAGNIFICATION = 260.0f;

        var armorMitigation = (float)beDoneMonster.currentDefense() / (DEFENSE_RESISTIVITY + LEVEL_MAGNIFICATION * beDoneMonster.level);

        var monster = monsterList.First(m => m.id == beDoneMonster.monsterId);
        // Debug.Log($"{monster.rarity} Lv.{beDoneMonster.level} {monster.name}, cd:{beDoneMonster.currentDefense()}, am:{armorMitigation}");

        return Mathf.Clamp(armorMitigation, 0.0f, 1.0f);
    }

    private float GetRate(int rateStatusValue, bool isClamp = true)
    {
        var rate = (float)rateStatusValue / 100;
        return isClamp ? Mathf.Clamp(rate, 0.0f, 1.0f) : rate;
    }

    private float GetRate(float rateStatusValue, bool isClamp = true)
    {
        var rate = rateStatusValue / 100.0f;
        return isClamp ? Mathf.Clamp(rate, 0.0f, 1.0f) : rate;
    }

    private float GetStatusValue(BattleMonsterInfo doMonster, BattleMonsterInfo beDoneMonster, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex)
    {
        switch (skillEffect.valueTargetType)
        {
            case ValueTargetType.MyCurrentHP:
                return doMonster.currentHp;

            case ValueTargetType.MyCurrentAttack:
                return doMonster.currentAttack();

            case ValueTargetType.MyCurrentDefense:
                return doMonster.currentDefense();

            case ValueTargetType.MyCurrentHeal:
                return doMonster.currentHeal();

            case ValueTargetType.MyCurrentSpeed:
                return doMonster.currentSpeed();

            case ValueTargetType.MyMaxHp:
                return doMonster.maxHp;

            case ValueTargetType.TargetCurrentHP:
                return beDoneMonster.currentHp;

            case ValueTargetType.TargetCurrentAttack:
                return beDoneMonster.currentAttack();

            case ValueTargetType.TargetCurrentDefense:
                return beDoneMonster.currentDefense();

            case ValueTargetType.TargetCurrentHeal:
                return beDoneMonster.currentHeal();

            case ValueTargetType.TargetCurrentSpeed:
                return beDoneMonster.currentSpeed();

            case ValueTargetType.TargetMaxHp:
                return beDoneMonster.maxHp;

            default:
                return 1.0f;
        }
    }

    private int GetValueCoefficient(SkillEffectMI skillEffect)
    {
        switch (skillEffect.type)
        {
            case SkillType.Attack:
            case SkillType.Damage:
            case SkillType.WithoutFactorDamage:
            case SkillType.EnergyDown:
                return -1;

            case SkillType.Heal:
            case SkillType.Status:
            case SkillType.Revive:
            case SkillType.EnergyUp:
                return 1;

            case SkillType.ConditionAdd:
                var battleCondition = battleConditionList.First(m => m.id == skillEffect.battleConditionId);
                switch (battleCondition.skillEffect.type)
                {
                    case SkillType.Attack:
                        return -1;

                    case SkillType.Heal:
                        return 1;

                    default:
                        return 0;
                }
            case SkillType.ConditionRemove:
            default:
                return 0;
        }
    }
}

public class BattleActionValueData
{
    public int value { get; set; }
    public bool isMissed { get; set; }
    public bool isCritical { get; set; }
    public bool isBlocked { get; set; }
}