using PM.Enum.Battle;
using System;
using System.Linq;
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
			case SkillType.Attack:
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

    /// <summary>
    /// 指定したアクション(通常攻撃、アクティブスキル、パッシブスキル)の現時点での発動回数を取得する
    /// 敵のモンスターの場合は現在のウェーブのモンスターのみ指定可能
    /// </summary>
    private int GetSkillExecuteCount(BattleActionType actionType, int index, SkillExecuteNumLimitType limitType, BattleMonsterIndex battleMonsterIndex) {

        // 対象のモンスターが対象のスキル効果を発動したログに絞り込んだリスト
        var doMonsterExecuteBattleLogList = battleLogList
            .Where(log => {
                if (battleMonsterIndex.isPlayer) {
                    return log.doBattleMonsterIndex.IsSame(battleMonsterIndex);
                } else {
                    return log.waveCount == currentWaveCount && log.doBattleMonsterIndex.IsSame(battleMonsterIndex);
                }
            })
            .Where(log => log.actionType == actionType && log.skillEffectIndex == index)
            .ToList();
        switch (limitType) {
            case SkillExecuteNumLimitType.None:
                return 0;
            case SkillExecuteNumLimitType.InBattle:
                return doMonsterExecuteBattleLogList.Count();
            case SkillExecuteNumLimitType.InWave:
                return doMonsterExecuteBattleLogList.Where(log => log.waveCount == currentWaveCount).Count();
            case SkillExecuteNumLimitType.InTurn:
                return doMonsterExecuteBattleLogList.Where(log => log.turnCount == currentTurnCount).Count();
            case SkillExecuteNumLimitType.InStream:
                return 0;
            case SkillExecuteNumLimitType.InEffectOnOwnEffect:
                return 0;
            default:
                return 0;
        }
    }
}
