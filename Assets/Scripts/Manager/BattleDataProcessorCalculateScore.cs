using PM.Enum.Battle;
using System;
/// <summary>
/// スコア集計を行うクラス
/// </summary>
public partial class BattleDataProcessor
{
	/// <summary>
	/// スコア集計を行うクラス
	/// actualValue: ダメージ計算後の値ではなく実際にHPなどに影響を与えた値
	/// </summary>
	private void AddScore(BattleMonsterIndex doBattleMonsterIndex, BattleMonsterIndex beDoneBattleMonsterIndex, SkillType skillType, int effectValue)
	{
		effectValue = Math.Abs(effectValue);
		var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
		var beDoneBattleMonster = GetBattleMonster(beDoneBattleMonsterIndex);
		switch(skillType)
		{
			case SkillType.Damage:
				// 与ダメージ
				doBattleMonster.totalGiveDamage += effectValue;

				// 被ダメージ
				beDoneBattleMonster.totalTakeDamage += effectValue;
				break;
			case SkillType.Heal:
				// 与回復量
				doBattleMonster.totalHealing += effectValue;
				break;
		}
	}
}
