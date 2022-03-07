﻿using PM.Enum.Battle;
using System.Linq;
using UnityEngine;

/// <summary>
/// ダメージ計算を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
	private int GetActionValue(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect)
	{
		var doBattleMonster = GetBattleMonster(doMonsterIndex);
		var beDoneBattleMonster = GetBattleMonster(beDoneMonsterIndex);

		switch (skillEffect.type)
		{
			case SkillType.Damage:
				switch (skillEffect.valueTargetType)
				{
					// HPを基準にする攻撃は他の要素を含まないダメージで計算
					case ValueTargetType.MyCurrentHP:
					case ValueTargetType.MyMaxHp:
					case ValueTargetType.TargetCurrentHP:
					case ValueTargetType.TargetMaxHp:
						return -GetActionValueWithoutFactor(doBattleMonster, beDoneBattleMonster, skillEffect);
					// それ以外のダメージの場合は含めて計算
					default:
						return -GetActionValueWithFactor(doBattleMonster, beDoneBattleMonster, skillEffect);
				}
			case SkillType.Heal:
				return GetActionValueWithoutFactor(doBattleMonster, beDoneBattleMonster, skillEffect);
			case SkillType.ConditionAdd:
			case SkillType.ConditionRemove:
			case SkillType.Revive:
			default:
				return 0;
		}
	}

	/// <summary>
	/// 様々な要因を加味したアクション値を取得する
	/// </summary>
	private int GetActionValueWithFactor(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect)
	{
		// Incoming Damage × (1 – Reduce Damage %) × [(1 – Armor Mitigation %  + 70% × Holy Damage % + 30% × Luck Damage % ]
		const float HOLY_DAMAGE_MAGNIFICATION = 70.0f;
		const float LUCK_DAMAGE_MAGNIFICATION = 30.0f;
		const float BLOCK_DAMAGE_REDUCE_RATE = 33.0f;

		var isBlock = ExecuteProbability(beDoneBattleMonster.blockRate());
		return
			(int)(
				IncomingDamage(doBattleMonster,beDoneBattleMonster, skillEffect)			// 基準ダメージ
				* (1 - GetRate(beDoneBattleMonster.damageResistRate()))						// ダメージ軽減分ダメージを軽減
				* (
					(1 - ArmorMitigation(beDoneBattleMonster))								// 防御力分ダメージを軽減
					+ HOLY_DAMAGE_MAGNIFICATION * GetRate(doBattleMonster.holyDamageRate()) // 神聖ダメージを上乗せ
					+ LUCK_DAMAGE_MAGNIFICATION * GetRate(doBattleMonster.luckDamageRate()) // ラックダメージを上乗せ
				)
				* (
					isBlock ?																// ブロックしたかを判定
					((float)(100 - BLOCK_DAMAGE_REDUCE_RATE) / 100) :						// ブロックしていればブロックでの軽減率分ダメージを軽減
					1																		// ブロックしていなければそのまま
				)
			);
	}

	/// <summary>
	/// 様々な要因を加味しないアクション値を取得する
	/// </summary>
	private int GetActionValueWithoutFactor(BattleMonsterInfo doBattleMonster, BattleMonsterInfo beDoneBattleMonster, SkillEffectMI skillEffect)
	{
		return (int)(GetStatusValue(doBattleMonster, beDoneBattleMonster, skillEffect) * GetRate(skillEffect.value));
	}

	private float IncomingDamage(BattleMonsterInfo doMonster, BattleMonsterInfo beDoneMonster, SkillEffectMI skillEffect)
	{
		// 攻撃×攻撃倍率×クリ時(1.5+0.02×クリダメ)
		const float CRITICAL_DAMAGE_MAGNIFICATION = 1.5f;
		const float CRITICAL_DAMAGE_COEFFICIANT = 0.02f;

		var isCritical = ExecuteProbability(doMonster.criticalRate());
		return 
			GetStatusValue(doMonster, beDoneMonster, skillEffect)										   // 対象のステータス値
			* GetRate(skillEffect.value, false)															   // ダメージ倍率
			* BattleConditionKiller(doMonster, beDoneMonster)											   // 状態異常特攻倍率
			* BuffTypeNumKiller(doMonster, beDoneMonster)												   // 指定バフタイプの個数特攻倍率
			* MonsterAttributeKiller(doMonster, beDoneMonster)											   // 属性特攻倍率
			* (
				isCritical ?																			   // クリティカルかどうかを判定
				(CRITICAL_DAMAGE_MAGNIFICATION + CRITICAL_DAMAGE_COEFFICIANT * doMonster.criticalDamage()) // クリティカルならクリティカルダメージ
				: 1.0f																					   // クリティカルでなければそのまま
			);
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
					return group.Sum(battleCondition => battleCondition.value);

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
			.Sum(battleCondition => battleCondition.value);
		return GetRate(rate + 100);
	}

	private float ArmorMitigation(BattleMonsterInfo beDoneMonster)
	{
		// 防御/(180+20×Level)
		const float DEFENSE_RESISTIVITY = 180.0f;
		const float LEVEL_MAGNIFICATION = 20.0f;

		var armorMitigation = (float)beDoneMonster.currentDefense() / (DEFENSE_RESISTIVITY + LEVEL_MAGNIFICATION * beDoneMonster.level);
		return Mathf.Clamp(armorMitigation, 0.0f, 1.0f);
	}

	private float GetRate(int rateStatusValue, bool isClamp = true)
	{
		var rate = (float)rateStatusValue / 100;
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
}