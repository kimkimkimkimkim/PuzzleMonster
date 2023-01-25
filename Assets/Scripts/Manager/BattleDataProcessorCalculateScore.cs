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
    /// 合計与ダメージを取得します
    /// </summary>
    private int GetTotalGiveDamage(BattleMonsterIndex battleMonsterIndex) {
        return battleLogList
            // ダメージログに絞る
            .Where(l => l.type == BattleLogType.TakeDamage)
            // 指定モンスターのログに絞る
            .Where(l => l.doBattleMonsterIndex.IsSame(battleMonsterIndex))
            // 自傷ダメージは外す
            .Where(l => l.beDoneBattleMonsterDataList.Any(d => d.battleMonsterIndex.IsSame(battleMonsterIndex)))
            // 与えたダメージの合計を取得
            .Sum(l => l.beDoneBattleMonsterDataList.Sum(d => Math.Abs(d.hpChanges)));
    }

    /// <summary>
    /// 合計被ダメージを取得します
    /// </summary>
    private int GetTotalTakeDamage(BattleMonsterIndex battleMonsterIndex) {
        return battleLogList
            // ダメージログに絞る
            .Where(l => l.type == BattleLogType.TakeDamage)
            // 自傷ダメージは外す
            .Where(l => !l.doBattleMonsterIndex.IsSame(battleMonsterIndex))
            // 指定モンスターのログに絞る
            .Where(l => l.beDoneBattleMonsterDataList.Any(d => d.battleMonsterIndex.IsSame(battleMonsterIndex)))
            // 受けたダメージの合計を取得
            .Sum(l => Math.Abs(l.beDoneBattleMonsterDataList.First(d => d.battleMonsterIndex.IsSame(battleMonsterIndex)).hpChanges));
    }

    /// <summary>
    /// 合計与回復量を取得します
    /// </summary>
    private int GetTotalHealing(BattleMonsterIndex battleMonsterIndex) {
        return battleLogList
            // 回復ログに絞る
            .Where(l => l.type == BattleLogType.TakeHeal)
            // 指定モンスターのログに絞る
            .Where(l => l.doBattleMonsterIndex.IsSame(battleMonsterIndex))
            // 与えた回復量の合計を取得
            .Sum(l => l.beDoneBattleMonsterDataList.Sum(d => Math.Abs(d.hpChanges)));
    }

    /// <summary>
    /// 指定したアクション(通常攻撃、アクティブスキル、パッシブスキル)の現時点での発動回数を取得する
    /// </summary>
    private int GetSkillExecuteCount(BattleActionType actionType, int index, SkillExecuteNumLimitType limitType, BattleMonsterIndex battleMonsterIndex) {

        // 対象のモンスターが対象のスキル効果を発動したログに絞り込んだリスト
        var doMonsterExecuteBattleLogList = battleLogList
            .Where(log => log.doBattleMonsterIndex.IsSame(battleMonsterIndex))
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

    /// <summary>
    /// ブロックした回数の取得
    /// </summary>
    private int GetBlockCount(BattleMonsterIndex battleMonsterIndex) {
        // TODO: ブロックした回数取得
        return battleLogList.Where(log => {
            return true;
        }).Count();
    }
}
