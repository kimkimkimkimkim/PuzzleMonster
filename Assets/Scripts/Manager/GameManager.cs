using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class GameManager
{
    private GameWindowUIScript gameWindowUIScript;
    private List<BattleMonsterInfo> enemyBattleMonsterList;
    private List<BattleMonsterInfo> playerBattleMonsterList;
    private int playerCurrentHp;

    public GameManager(GameWindowUIScript gameWindowUIScript,List<UserMonsterInfo> enemyUserMonsterList,List<UserMonsterInfo> playerUserMonsterList)
    {
        this.gameWindowUIScript = gameWindowUIScript;
        enemyBattleMonsterList = enemyUserMonsterList.Select(u => GameUtil.GetBattleMonster(u)).ToList();
        playerBattleMonsterList = playerUserMonsterList.Select(u => GameUtil.GetBattleMonster(u)).ToList();
        playerCurrentHp = playerUserMonsterList.Sum(u => u.hp);
    }

    /// <summary>
    /// ユーザーのドロップ操作終了時に実行する処理
    /// コマンド発動したモンスターのインデックスを引数に渡す
    /// </summary>
    public IObservable<Unit> OnEndDropOperationObservable(List<int> attackMonsterIndexList)
    {
        return AttackObservable(attackMonsterIndexList);
    }

    /// <summary>
    /// 敵への攻撃
    /// コマンド発動したモンスターのインデックスを引数に渡す
    /// </summary>
    private IObservable<Unit> AttackObservable(List<int> attackMonsterIndexList)
    {
        const float ATTACK_EFFECT_DELAY = 0.1f;

        // 攻撃エフェクト
        var attackAnimationObservableList = attackMonsterIndexList.Select((playerMonsterIndex,index) =>
        {
            var enemyIndex = Attack(playerMonsterIndex);
            var enemyHp = enemyBattleMonsterList[enemyIndex].currentHp;
            return Observable.Timer(TimeSpan.FromSeconds(ATTACK_EFFECT_DELAY * index))
                .SelectMany(_ => gameWindowUIScript.PlayAttackToEnemyAnimationObservable(enemyIndex, enemyHp, playerMonsterIndex));
        }).ToList();

        return Observable.WhenAll(attackAnimationObservableList);
    }

    /// <summary>
    /// 敵への攻撃
    /// 攻撃するモンスターのインデックスを引数に渡す
    /// 攻撃対象の敵モンスターインデックスを返します(エラー値は-1)
    /// </summary>
    private int Attack(int playerMonsterIndex)
    {
        // 敵が全滅していたら最後の敵を対象とする
        var enemyBattleMonsterIndex = enemyBattleMonsterList.FindIndex(b => b.currentHp != 0);
        if (enemyBattleMonsterIndex == -1) return enemyBattleMonsterList.Count - 1;

        // 攻撃するモンスターが取得できない場合はエラー
        if (playerMonsterIndex < 0 || playerMonsterIndex >= playerBattleMonsterList.Count) return -1;
        var playerBattleMonster = playerBattleMonsterList[playerMonsterIndex];

        // ダメージ計算
        var enemyBattleMonster = enemyBattleMonsterList[enemyBattleMonsterIndex];
        enemyBattleMonster.currentHp = Math.Max(0, enemyBattleMonster.currentHp - playerBattleMonster.baseAttack);

        return enemyBattleMonsterIndex;
    }


}
