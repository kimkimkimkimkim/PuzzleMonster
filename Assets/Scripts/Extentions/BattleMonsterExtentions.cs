using PM.Enum.Battle;
using System;
using System.Linq;

public static class BattleMonsterInfoExtentions {
    /// <summary>
    /// HP値変更は毎回ここを通す
    /// 実際に影響を与えた値を返す
    /// </summary>
    public static int ChangeHp(this BattleMonsterInfo monster, int value) {
        var defaultHp = monster.currentHp;
        var existsShield = monster.currentShield > 0;

        if (existsShield && value < 0) {
            // 計算しやすいように効果量の絶対値に直す
            value = Math.Abs(value);

            // 影響値用にも値を保存
            var initialValue = value;

            // ダメージかつシールドを保持していたら、体力ではなくシールドを削る
            monster.battleConditionList
                .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.battleConditionId).battleConditionType == BattleConditionType.Shield)
                .OrderBy(c => c.order)
                .ToList()
                .ForEach(c => {
                    if (c.shieldValue <= value) {
                        value -= c.shieldValue;
                        c.shieldValue = 0;
                    } else {
                        c.shieldValue -= value;
                        value = 0;
                    }
                });

            // 耐久値が0になったシールドを解除する
            monster.battleConditionList = monster.battleConditionList.Where(c => {
                var battleCondition = MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.battleConditionId);
                var isNotShield = battleCondition.battleConditionType != BattleConditionType.Shield;
                var isValidShield = battleCondition.battleConditionType == BattleConditionType.Shield && c.shieldValue > 0;
                return isNotShield || isValidShield;
            }).ToList();
        } else {
            var tempHp = monster.currentHp + value;
            if (tempHp > monster.maxHp) {
                monster.currentHp = monster.maxHp;
            } else if (tempHp < 0) {
                monster.currentHp = 0;
            } else {
                monster.currentHp = tempHp;
            }
        }

        return monster.currentHp - defaultHp;
    }

    /// <summary>
    /// エネルギー値変更は毎回ここを通す
    /// 実際に影響を与えた値を返す
    /// </summary>
    public static int ChangeEnergy(this BattleMonsterInfo monster, int value) {
        var defaultEnergy = monster.currentEnergy;
        var tempEnergy = monster.currentEnergy + value;
        if (tempEnergy > ConstManager.Battle.MAX_ENERGY_VALUE) {
            monster.currentEnergy = ConstManager.Battle.MAX_ENERGY_VALUE;
        } else if (tempEnergy < 0) {
            monster.currentEnergy = 0;
        } else {
            monster.currentEnergy = tempEnergy;
        }

        return monster.currentEnergy - defaultEnergy;
    }

    /// <summary>
    /// 指定したステータスに応じた状態異常によるステータス値を取得します
    /// </summary>
    private static int GetBattleConditionMonsterStatusValue(BattleMonsterInfo monster, BattleMonsterStatusType statusType) {
        return monster.battleConditionList.Concat(monster.baseBattleConditionList)
            .Select(c => {
                var battleCondition = MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.battleConditionId);
                if (battleCondition.targetBattleMonsterStatusType != statusType) return 0;

                if (battleCondition.battleConditionType == BattleConditionType.StatusUp) {
                    return c.grantorSkillEffect.value;
                } else if (battleCondition.battleConditionType == BattleConditionType.StatusDown) {
                    return -c.grantorSkillEffect.value;
                } else {
                    return 0;
                }
            })
            .Sum();
    }

    /// <summary>
    /// 状態異常などを加味した攻撃力
    /// </summary>
    public static int currentAttack(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.Attack);
        value = Math.Clamp(value, ConstManager.Battle.MAX_ATTACK_DEBUFF_VALUE, ConstManager.Battle.MAX_ATTACK_BUFF_VALUE);
        return (int)BattleUtil.GetRatedValue(monster.baseAttack, value);
    }

    /// <summary>
    /// 状態異常などを加味した防御力
    /// </summary>
    public static int currentDefense(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.Defense);
        value = Math.Clamp(value, ConstManager.Battle.MAX_DEFENSE_DEBUFF_VALUE, ConstManager.Battle.MAX_DEFENSE_BUFF_VALUE);
        return (int)BattleUtil.GetRatedValue(monster.baseDefense, value);
    }

    /// <summary>
    /// 状態異常などを加味したスピード
    /// </summary>
    public static int currentSpeed(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.Speed);
        return (int)BattleUtil.GetRatedValue(monster.baseSpeed, value);
    }

    /// <summary>
    /// 状態異常などを加味した回復力
    /// </summary>
    public static int currentHeal(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.Heal);
        return (int)BattleUtil.GetRatedValue(monster.baseHeal, value);
    }

    /// <summary>
    /// シールド耐久値
    /// </summary>
    public static int shield(this BattleMonsterInfo monster) {
        return monster.battleConditionList
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.battleConditionId).battleConditionType == BattleConditionType.Shield)
            .Sum(c => c.shieldValue);
    }

    /// <summary>
    /// スキルダメージ率
    /// </summary>
    public static int ultimateSkillDamageRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.UltimateSkillDamageRate);
        return monster.baseUltimateSkillDamageRate + value;
    }

    /// <summary>
    /// ブロック率
    /// </summary>
    public static int blockRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.BlockRate);
        return monster.baseBlockRate + value;
    }

    /// <summary>
    /// クリティカル率
    /// </summary>
    public static int criticalRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.CriticalRate);
        return monster.baseCriticalRate + value;
    }

    /// <summary>
    /// クリティカルダメージ
    /// </summary>
    public static int criticalDamage(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.CriticalDamageRate);
        return monster.baseCriticalDamage + value;
    }

    /// <summary>
    /// 強化効果免疫率
    /// </summary>
    public static int buffResistRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.BuffResistRate);
        return value;
    }

    /// <summary>
    /// 弱体効果免疫率
    /// </summary>
    public static int debuffResistRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.DebuffResistRate);
        return monster.baseDebuffResistRate + value;
    }

    /// <summary>
    /// ダメージ軽減率
    /// </summary>
    public static int damageResistRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.DamageResistRate);
        return monster.baseDamageResistRate + value;
    }

    /// <summary>
    /// ラックダメージ率
    /// </summary>
    public static int luckDamageRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.LuckDamageRate);
        return monster.baseLuckDamageRate + value;
    }

    /// <summary>
    /// 神聖ダメージ率
    /// </summary>
    public static int holyDamageRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.HolyDamageRate);
        return monster.baseHolyDamageRate + value;
    }

    /// <summary>
    /// エネルギー上昇率
    /// </summary>
    public static int energyUpRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.EnergyUpRate);
        return monster.baseEnergyUpRate + value;
    }

    /// <summary>
    /// 被回復率（回復を受ける際の回復量上昇率）
    /// </summary>
    public static int healedRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.HealedRate);
        return monster.baseHealedRate + value;
    }

    /// <summary>
    /// 攻撃精度
    /// </summary>
    public static int attackAccuracy(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.AttackAccuracyRate);
        return monster.baseAttackAccuracy + value;
    }

    /// <summary>
    /// アーマー
    /// </summary>
    public static int armor(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.Armor);
        return monster.baseArmor + value;
    }

    /// <summary>
    /// アーマーブレイク率
    /// </summary>
    public static int armorBreakRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.ArmorBreakRate);
        return monster.baseArmorBreakRate + value;
    }

    /// <summary>
    /// 与回復率（回復をする際の回復量上昇率）
    /// </summary>
    public static int healingRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.HealingRate);
        return monster.baseHealingRate + value;
    }

    /// <summary>
    /// 防御貫通率
    /// </summary>
    public static int defensePenetratingRate(this BattleMonsterInfo monster) {
        var value = GetBattleConditionMonsterStatusValue(monster, BattleMonsterStatusType.DefensePenetratingRate);
        return monster.baseDefensePenetratingRate + value;
    }

    public static float GetStatus(this BattleMonsterInfo monster, BattleMonsterStatusType type) {
        switch (type) {
            case BattleMonsterStatusType.Hp:
                return monster.maxHp;

            case BattleMonsterStatusType.Attack:
                return monster.currentAttack;

            case BattleMonsterStatusType.Defense:
                return monster.currentDefense;

            case BattleMonsterStatusType.Heal:
                return monster.currentHeal;

            case BattleMonsterStatusType.Speed:
                return monster.currentSpeed;

            case BattleMonsterStatusType.Sheild:
                return monster.currentShield;

            case BattleMonsterStatusType.UltimateSkillDamageRate:
                return monster.currentUltimateSkillDamageRate;

            case BattleMonsterStatusType.BlockRate:
                return monster.currentBlockRate;

            case BattleMonsterStatusType.CriticalRate:
                return monster.currentCriticalRate;

            case BattleMonsterStatusType.CriticalDamageRate:
                return monster.currentCriticalDamage;

            case BattleMonsterStatusType.BuffResistRate:
                return monster.currentBuffResistRate;

            case BattleMonsterStatusType.DebuffResistRate:
                return monster.currentDebuffResistRate;

            case BattleMonsterStatusType.DamageResistRate:
                return monster.currentDamageResistRate;

            case BattleMonsterStatusType.LuckDamageRate:
                return monster.currentLuckDamageRate;

            case BattleMonsterStatusType.HolyDamageRate:
                return monster.currentHolyDamageRate;

            case BattleMonsterStatusType.EnergyUpRate:
                return monster.currentEnergyUpRate;

            case BattleMonsterStatusType.HealedRate:
                return monster.currentHealedRate;

            case BattleMonsterStatusType.AttackAccuracyRate:
                return monster.currentAttackAccuracy;

            case BattleMonsterStatusType.Armor:
                return monster.currentArmor;

            case BattleMonsterStatusType.ArmorBreakRate:
                return monster.currentArmorBreakRate;

            case BattleMonsterStatusType.HealingRate:
                return monster.currentHealingRate;

            case BattleMonsterStatusType.DefensePenetratingRate:
                return monster.currentDefensePenetratingRate;

            case BattleMonsterStatusType.CurrentHp:
                return monster.currentHp;

            default:
                return 0;
        }
    }

    public static void UpdateCurrentStatus(this BattleMonsterInfo monster, BattleMonsterStatusType type) {
        switch (type) {
            // 最大HPは何もしない
            case BattleMonsterStatusType.Hp:
                break;

            case BattleMonsterStatusType.Attack:
                monster.currentAttack = monster.currentAttack();
                break;

            case BattleMonsterStatusType.Defense:
                monster.currentDefense = monster.currentDefense();
                break;

            case BattleMonsterStatusType.Heal:
                monster.currentHeal = monster.currentHeal();
                break;

            case BattleMonsterStatusType.Speed:
                monster.currentSpeed = monster.currentSpeed();
                break;

            case BattleMonsterStatusType.Sheild:
                monster.currentShield = monster.shield();
                break;

            case BattleMonsterStatusType.UltimateSkillDamageRate:
                monster.currentUltimateSkillDamageRate = monster.ultimateSkillDamageRate();
                break;

            case BattleMonsterStatusType.BlockRate:
                monster.currentBlockRate = monster.blockRate();
                break;

            case BattleMonsterStatusType.CriticalRate:
                monster.currentCriticalRate = monster.criticalRate();
                break;

            case BattleMonsterStatusType.CriticalDamageRate:
                monster.currentCriticalDamage = monster.criticalDamage();
                break;

            case BattleMonsterStatusType.BuffResistRate:
                monster.currentBuffResistRate = monster.buffResistRate();
                break;

            case BattleMonsterStatusType.DebuffResistRate:
                monster.currentDebuffResistRate = monster.debuffResistRate();
                break;

            case BattleMonsterStatusType.DamageResistRate:
                monster.currentDamageResistRate = monster.damageResistRate();
                break;

            case BattleMonsterStatusType.LuckDamageRate:
                monster.currentLuckDamageRate = monster.luckDamageRate();
                break;

            case BattleMonsterStatusType.HolyDamageRate:
                monster.currentHolyDamageRate = monster.holyDamageRate();
                break;

            case BattleMonsterStatusType.EnergyUpRate:
                monster.currentEnergyUpRate = monster.energyUpRate();
                break;

            case BattleMonsterStatusType.HealedRate:
                monster.currentHealedRate = monster.healedRate();
                break;

            case BattleMonsterStatusType.AttackAccuracyRate:
                monster.currentAttackAccuracy = monster.attackAccuracy();
                break;

            case BattleMonsterStatusType.Armor:
                monster.currentArmor = monster.armor();
                break;

            case BattleMonsterStatusType.ArmorBreakRate:
                monster.currentArmorBreakRate = monster.armorBreakRate();
                break;

            case BattleMonsterStatusType.HealingRate:
                monster.currentHealingRate = monster.healingRate();
                break;

            case BattleMonsterStatusType.DefensePenetratingRate:
                monster.currentDefensePenetratingRate = monster.defensePenetratingRate();
                break;

            // 現在HPの場合も何もしない
            case BattleMonsterStatusType.CurrentHp:
                monster.currentHp = monster.currentHp;
                break;

            // Noneの場合はすべてのステータスを更新する
            case BattleMonsterStatusType.None:
                foreach (var statusType in Enum.GetValues(typeof(BattleMonsterStatusType)).Cast<BattleMonsterStatusType>()) {
                    if (statusType != BattleMonsterStatusType.None) {
                        monster.UpdateCurrentStatus(statusType);
                    }
                }
                break;

            default:
                break;
        }
    }

}
