using PM.Enum.Battle;
using PM.Enum.Monster;
using System.Linq;
using UnityEngine;

/// <summary>
/// ダメージ計算を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
	private BattleActionValueData GetActionValue(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect)
	{
		var doBattleMonster = GetBattleMonster(doMonsterIndex);
		var beDoneBattleMonster = GetBattleMonster(beDoneMonsterIndex);

		switch (skillEffect.type)
		{
			case SkillType.Attack:
				switch (skillEffect.valueTargetType)
				{
					// HPを基準にする攻撃は他の要素を含まないダメージで計算
					case ValueTargetType.MyCurrentHP:
					case ValueTargetType.MyMaxHp:
					case ValueTargetType.TargetCurrentHP:
					case ValueTargetType.TargetMaxHp:
						return new BattleActionValueData(){ value = GetHpRateDamageValue(doBattleMonster, beDoneBattleMonster, skillEffect) };
					// それ以外のダメージの場合は含めて計算
					default:
						return GetActionValueWithFactor(doBattleMonster, beDoneBattleMonster, skillEffect);
				}
			case SkillType.Heal:
				return new BattleActionValueData(){ value = GetHealValue(doBattleMonster, beDoneBattleMonster, skillEffect) };
			case SkillType.ConditionAdd:
			case SkillType.ConditionRemove:
			case SkillType.Revive:
			default:
				return new BattleActionValueData();
		}
	}

	/// <summary>
	/// 様々な要因を加味したアクション値を取得する
	/// </summary>
	private BattleActionValueData GetActionValueWithFactor(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect)
	{
		// Incoming Damage × (1 – Reduce Damage %) × [((1 – Armor Mitigation %) × (1 - Armor Break %))  + 70% × Holy Damage % + 30% × Luck Damage % ]
		const float HOLY_DAMAGE_MAGNIFICATION = 70.0f;
		const float LUCK_DAMAGE_MAGNIFICATION = 30.0f;
		const float BLOCK_DAMAGE_REDUCE_RATE = 33.0f;
		
		var coefficient = GetValueCoefficient(skillEffect);
		var incomingDamage = IncomingDamage(doBattleMonster,beDoneBattleMonster, skillEffect);
		var isBlocked = ExecuteProbability(beDoneBattleMonster.blockRate() - doBattleMonster.attackAccuracy());
		var damage =
			(int)(
				incomingDamage.damage														// 基準ダメージ
				* (1 - GetRate(beDoneBattleMonster.damageResistRate()))						// ダメージ軽減分ダメージを軽減
				* (
					(1 - ArmorMitigation(beDoneBattleMonster))								// 防御力分ダメージを軽減
					* (1 - GetRate(doBattleMonster.defensePenetratingRate()))				// 防御貫通率分防御力を無視
					+ HOLY_DAMAGE_MAGNIFICATION * GetRate(doBattleMonster.holyDamageRate()) // 神聖ダメージを加算
					+ LUCK_DAMAGE_MAGNIFICATION * GetRate(doBattleMonster.luckDamageRate()) // ラックダメージを加算
				)
				* (
					isBlocked ?																// ブロックしたかを判定
					((float)(100 - BLOCK_DAMAGE_REDUCE_RATE) / 100) :						// ブロックしていればブロックでの軽減率分ダメージを軽減
					1																		// ブロックしていなければそのまま
				)
			);
		return new BattleActionValueData(){
			value = coefficient * damage,
			isCritical = incomingDamage.isCritical,
			isBlocked = isBlocked
		};
	}

	/// <summary>
	/// HP割合攻撃によるダメージを取得する
	/// </summary>
	private int GetHpRateDamageValue(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect)
	{
		var coefficient = GetValueCoefficient(skillEffect);
		return (int)(coefficient * GetStatusValue(doBattleMonster, beDoneBattleMonster, skillEffect) * GetRate(skillEffect.value));
	}

	/// <summary>
	/// 回復値を取得する
	/// </summary>
	private int GetHealValue(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect)
	{
		var coefficient = GetValueCoefficient(skillEffect);
		return (int)(
			coefficient 
			* GetStatusValue(doBattleMonster, beDoneBattleMonster, skillEffect)							  // 対象のステータス値
			* GetRate(skillEffect.value)																  // ダメージ倍率
			* (1 + GetRate(doBattleMonster.healingRate()))												  // 与回復率分回復量を上昇
			* (1 + GetRate(beDoneBattleMonster.healedRate()))											  // 被回復率分回復量を上昇
		);
	}

	private (float damage, bool isCritical) IncomingDamage(BattleMonsterInfo doMonster, BattleMonsterInfo beDoneMonster, SkillEffectMI skillEffect)
	{
		// 攻撃×攻撃倍率×クリ時(1.5+0.02×クリダメ)
		const float CRITICAL_DAMAGE_MAGNIFICATION = 1.5f;
		const float CRITICAL_DAMAGE_COEFFICIANT = 0.02f;

		var isCritical = ExecuteProbability(doMonster.criticalRate());
		var damage =
			GetStatusValue(doMonster, beDoneMonster, skillEffect)										   // 対象のステータス値
			* GetRate(skillEffect.value, false)															   // ダメージ倍率
			* BattleConditionKiller(doMonster, beDoneMonster)											   // 状態異常特攻倍率
			* BuffTypeNumKiller(doMonster, beDoneMonster)												   // 指定バフタイプの個数特攻倍率
			* MonsterAttributeKiller(doMonster, beDoneMonster)											   // 属性特攻倍率
			* MonsterAttributeCompatibility(doMonster, beDoneMonster)									   // 属性相性
			* AttackAccuracyCompatibility(doMonster, beDoneMonster)										   // 攻撃精度倍率
			* (
				isCritical ?																			   // クリティカルかどうかを判定
				(CRITICAL_DAMAGE_MAGNIFICATION + CRITICAL_DAMAGE_COEFFICIANT * doMonster.criticalDamage()) // クリティカルならクリティカルダメージ
				: 1.0f																					   // クリティカルでなければそのまま
			);
		return (damage, isCritical);
	}

	private float BattleConditionKiller(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
    {
		var rate = doBattleMonster.battleConditionList
			.Where(c => c.battleCondition.battleConditionType == BattleConditionType.BattleConditionKiller)
			.GroupBy(c => c.battleCondition.targetBattleConditionId)
			.Select(group =>
			{
				var targetBattleConditionId = group.Key;
				var existsTargetBattleCondition = beDoneBattleMonster.battleConditionList.Any(c => c.battleCondition.targetBattleConditionId == targetBattleConditionId);

				if (!existsTargetBattleCondition)
				{
					// 相手が対象の状態異常を保持していなければ何もしない
					return 0;
				}
				else
				{
					// 相手が対象の状態異常を保持していれば合計の倍率を返す
					return group.Sum(battleCondition => battleCondition.skillEffect.value);

				}
            })
            .Sum();
		return GetRate(rate + 100);
    }

	private float BuffTypeNumKiller(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
	{
		var rate = doBattleMonster.battleConditionList
			.Where(c => c.battleCondition.battleConditionType == BattleConditionType.BuffTypeNumKiller)
			.GroupBy(c => c.battleCondition.targetBuffType)
			.Select(group =>
			{
				var targetBuffType = group.Key;
				var existsTargetBuffType = beDoneBattleMonster.battleConditionList.Any(c => c.battleCondition.buffType == targetBuffType);

				if (!existsTargetBuffType)
				{
					// 相手が指定のバフタイプの状態異常を保持していなければ何もしない
					return 0;
				}
				else
				{
					// 相手が指定のバフタイプの状態異常を保持していれば合計の倍率を返す
					var num = group.Count();
					return num * 5; // TODO: 個数バフの倍率の計算

				}
			})
			.Sum();
		return GetRate(rate + 100);
	}

	private float MonsterAttributeKiller(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
	{
		var beDoneMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(beDoneBattleMonster.monsterId);
		var rate = doBattleMonster.battleConditionList
			.Where(c => c.battleCondition.battleConditionType == BattleConditionType.MonsterAttributeKiller && c.battleCondition.targetMonsterAttribute == beDoneMonster.attribute)
			.Sum(battleCondition => battleCondition.skillEffect.value);
		return GetRate(rate + 100);
	}

	private float MonsterAttributeCompatibility(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
	{
		const int MONSTER_ATTRIBUTE_COMPATIBILITY_RATE = 30;
		var doMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(doBattleMonster.monsterId);
		var beDoneMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(beDoneBattleMonster.monsterId);

		var rate = doMonster.attribute.IsAdvantageous(beDoneMonster.attribute) ? MONSTER_ATTRIBUTE_COMPATIBILITY_RATE 
			: doMonster.attribute.IsDisadvantage(beDoneMonster.attribute) ? -MONSTER_ATTRIBUTE_COMPATIBILITY_RATE 
			: 0;

		return GetRate(rate + 100);
	}

	private float AttackAccuracyCompatibility(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster)
	{
		const float MONSTER_ATTRIBUTE_COMPATIBILITY_RATE = 0.3f; // 攻撃精度1%につき何%ダメージアップするか
		const float ADVANTAGE_PLUS_VALUE = 15.0f; // 属性有利時に攻撃精度が何%上昇するか
		var doMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(doBattleMonster.monsterId);
		var beDoneMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(beDoneBattleMonster.monsterId);

		var rate = (float)doBattleMonster.attackAccuracy();
		if (doMonster.attribute.IsAdvantageous(beDoneMonster.attribute)) rate += ADVANTAGE_PLUS_VALUE;
		rate *= MONSTER_ATTRIBUTE_COMPATIBILITY_RATE;

		return GetRate(rate + 100);
	}

	private float ArmorMitigation(BattleMonsterInfo beDoneMonster)
	{
		// 防御/(180+22×Level)
		const float DEFENSE_RESISTIVITY = 180.0f;
		const float LEVEL_MAGNIFICATION = 22.0f;

		var armorMitigation = (float)beDoneMonster.currentDefense() / (DEFENSE_RESISTIVITY + LEVEL_MAGNIFICATION * beDoneMonster.level);
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

	private float GetStatusValue(BattleMonsterInfo doMonster, BattleMonsterInfo beDoneMonster, SkillEffectMI skillEffect)
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
				return 0.0f;
		}
	}
	
	private int GetValueCoefficient(SkillEffectMI skillEffect){
		switch(skillEffect.type){
			case SkillType.Attack:
				return -1;
			case SkillType.Heal:
				return 1;
			case SkillType.ConditionAdd:
				var battleCondition = MasterRecord.GetMasterOf<BattleConditionMB>().Get(skillEffect.battleConditionId);
				switch(battleCondition.skillEffect.type){
					case SkillType.Attack:
						return -1;
					case SkillType.Heal:
						return 1;
					default:
						return 0;
				}
			case SkillType.ConditionRemove:
			case SkillType.Revive:
			default:
				return 0;
		}
	}
}

public class BattleActionValueData{
    public int value { get; set; }
    public bool isMissed { get; set; }
    public bool isCritical { get; set; }
    public bool isBlocked { get; set; }
}
