using PM.Enum.Battle;
using System;
using System.Linq;

public static class BattleMonsterInfoExtentions
{
    /// <summary>
    /// HP値変更は毎回ここを通す
    /// 実際に影響を与えた値を返す
    /// </summary>
    public static int ChangeHp(this BattleMonsterInfo monster, int value)
    {
        var existsShield = monster.shield() > 0;

        if (existsShield && value < 0)
        {
            // 計算しやすいように効果量の絶対値に直す
            value = Math.Abs(value);

            // 影響値用にも値を保存
            var initialValue = value;

            // ダメージかつシールドを保持していたら、体力ではなくシールドを削る
            monster.battleConditionList
                .Where(c => c.battleCondition.battleConditionType == BattleConditionType.Shield)
                .OrderBy(c => c.order)
                .ToList()
                .ForEach(c => {
                    if (c.shieldValue <= value)
                    {
                        value -= c.shieldValue;
                        c.shieldValue = 0;
                    }
                    else
                    {
                        c.shieldValue -= value;
                        value = 0;
                    }
                });

            // 耐久値が0になったシールドを解除する
            monster.battleConditionList = monster.battleConditionList.Where(c => {
                var isNotShield = c.battleCondition.battleConditionType != BattleConditionType.Shield;
                var isValidShield = c.battleCondition.battleConditionType == BattleConditionType.Shield && c.shieldValue > 0;
                return isNotShield || isValidShield;
            }).ToList();

            // 最初の効果量からシールド削り後の効果値を引いたものが実際の影響値
            return initialValue - value;
        }
        else
        {
            var tempHp = monster.currentHp + value;
            if (tempHp > monster.maxHp)
            {
                monster.currentHp = monster.maxHp;
                return value - (tempHp - monster.maxHp);
            }
            else if (tempHp < 0)
            {
                monster.currentHp = 0;
                return value - tempHp;
            }
            else
            {
                monster.currentHp = tempHp;
                return value;
            }
        }
    }

    public static void ChangeEnergy(this BattleMonsterInfo monster, int value)
    {
        var tempEnergy = monster.currentEnergy + value;
        if (tempEnergy > ConstManager.Battle.MAX_ENERGY_VALUE)
        {
            monster.currentEnergy = ConstManager.Battle.MAX_ENERGY_VALUE;
        }
        else if (tempEnergy < 0)
        {
            monster.currentEnergy = 0;
        }
        else
        {
            monster.currentEnergy = tempEnergy;
        }
    }

    /// <summary>
    /// 状態異常などを加味した攻撃力
    /// </summary>
    public static float currentAttack(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Attack)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Attack)
            .Sum(c => c.skillEffect.value);
        var value = up - down;
        return BattleUtil.GetRatedValue(monster.baseAttack, value);
    }

    /// <summary>
    /// 状態異常などを加味した防御力
    /// </summary>
    public static float currentDefense(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Defense)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Defense)
            .Sum(c => c.skillEffect.value);
        var value = up - down;
        return BattleUtil.GetRatedValue(monster.baseDefense, value);
    }

    /// <summary>
    /// 状態異常などを加味したスピード
    /// </summary>
    public static float currentSpeed(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Speed)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Speed)
            .Sum(c => c.skillEffect.value);
        var value = up - down;
        return BattleUtil.GetRatedValue(monster.baseSpeed, value);
    }

    /// <summary>
    /// 状態異常などを加味した回復力
    /// </summary>
    public static float currentHeal(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Heal)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.Heal)
            .Sum(c => c.skillEffect.value);
        var value = up - down;
        return BattleUtil.GetRatedValue(monster.baseHeal, value);
    }

    /// <summary>
    /// シールド耐久値
    /// </summary>
    public static int shield(this BattleMonsterInfo monster)
    {
        return monster.battleConditionList.Where(c => c.battleCondition.battleConditionType == BattleConditionType.Shield).Sum(c => c.shieldValue);
    }

    /// <summary>
    /// スキルダメージ率
    /// </summary>
    public static int ultimateSkillDamageRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.UltimateSkillDamageRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.UltimateSkillDamageRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// ブロック率
    /// </summary>
    public static int blockRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.BlockRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.BlockRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// クリティカル率
    /// </summary>
    public static int criticalRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.CriticalRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.CriticalRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// クリティカルダメージ
    /// </summary>
    public static int criticalDamage(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.CriticalDamage)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.CriticalDamage)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// 強化効果免疫率
    /// </summary>
    public static int buffResistRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.BuffResistRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.BuffResistRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// 弱体効果免疫率
    /// </summary>
    public static int debuffResistRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.DebuffResistRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.DebuffResistRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// ダメージ軽減率
    /// </summary>
    public static int damageResistRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.DamageResistRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.DamageResistRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// ラックダメージ率
    /// </summary>
    public static int luckDamageRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.LuckDamageRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.LuckDamageRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// 神聖ダメージ率
    /// </summary>
    public static int holyDamageRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.HolyDamageRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.HolyDamageRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// エネルギー上昇率
    /// </summary>
    public static int energyUpRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.EnergyUpRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.EnergyUpRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }

    /// <summary>
    /// 被回復率（回復を受ける際の回復量上昇率）
    /// </summary>
    public static int healedRate(this BattleMonsterInfo monster)
    {
        var up = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusUp)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.HealedRate)
            .Sum(c => c.skillEffect.value);
        var down = monster.battleConditionList
            .Where(c => c.battleCondition.battleConditionType == BattleConditionType.StatusDown)
            .Where(c => MasterRecord.GetMasterOf<BattleConditionMB>().Get(c.skillEffect.battleConditionId).targetBattleMonsterStatusType == BattleMonsterStatusType.HealedRate)
            .Sum(c => c.skillEffect.value);
        return up - down;
    }
}