using System;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using UniRx;
using GameBase;
using UnityEngine;
using PM.Enum.UI;
using PM.Enum.Monster;

public class BattleManager : SingletonMonoBehaviour<BattleManager>
{
    private IObserver<BattleResult> battleObserver;
    private IObserver<Unit> battleTurnObserver;
    private BattleWindowUIScript battleWindow;
    private BattleResult battleResult;
    private QuestMB quest;
    private int currentTurnCount;
    private int currentTurnCountInWave;
    private int currentWaveCount;
    private int maxWaveCount;
    private string userMonsterPartyId;
    private List<BattleLogInfo> battleLogList;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init(long questId, string userMonsterPartyId)
    {
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        currentTurnCount = 0;
        currentTurnCountInWave = 0;
        currentWaveCount = 0;
        maxWaveCount = quest.questWaveIdList.Count;
        battleResult = new BattleResult() { wol = WinOrLose.Continue };
        this.userMonsterPartyId = userMonsterPartyId;
    }

    public static string GetLogText(BattleLogInfo battleLog)
    {
        var log = battleLog.log;

        if (battleLog.log.Contains("{do}"))
        {
            if (battleLog.doBattleMonsterIndex == null) return log;

            var battleMonsterList = battleLog.doBattleMonsterIndex.isPlayer ? battleLog.playerBattleMonsterList : battleLog.enemyBattleMonsterList;
            var doBattleMonster = battleMonsterList.First(m => m.index.index == battleLog.doBattleMonsterIndex.index);
            // TODO: 実際のモンスターを取得
            var monsterName = $"{(battleLog.doBattleMonsterIndex.isPlayer ? "" : "あいての")}モンスター{battleLog.doBattleMonsterIndex.index + 1}";
            log = log.Replace("{do}", monsterName);
        }

        if (battleLog.log.Contains("{beDone}"))
        {
            if (battleLog.beDoneBattleMonsterIndex == null) return log;

            var battleMonsterList = battleLog.beDoneBattleMonsterIndex.isPlayer ? battleLog.playerBattleMonsterList : battleLog.enemyBattleMonsterList;
            var beDoneBattleMonster = battleMonsterList.First(m => m.index.index == battleLog.beDoneBattleMonsterIndex.index);
            // TODO: 実際のモンスターを取得
            var monsterName = $"{(battleLog.beDoneBattleMonsterIndex.isPlayer ? "" : "あいての")}モンスター{battleLog.beDoneBattleMonsterIndex.index + 1}";
            log = log.Replace("{beDone}", monsterName);
        }

        return log;
    }

    /// <summary>
    /// バトルを開始する
    /// </summary>
    public IObservable<BattleResult> BattleStartObservable(long questId, string userMonsterPartyId)
    {
        // 初期化
        Init(questId, userMonsterPartyId);

        return Observable.Create<BattleResult>(battleObserver => {
            this.battleObserver = battleObserver;

            Observable.ReturnUnit()
                .SelectMany(_ => FadeInObservable())
                .SelectMany(_ =>{
                    // バトルアニメーションの再生
                    var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
                    var battleDataProcessor = new BattleDataProcessor();
                    battleLogList = battleDataProcessor.GetBattleLogList(userMonsterParty, quest);
                    var observableList = battleLogList.Select(battleLog => PlayAnimationObservable(battleLog)).ToList();
                    return Observable.ReturnUnit().Connect(observableList.ToArray());
                })
                .SelectMany(_ => FadeOutObservable())
                .Subscribe();

            return Disposable.Empty;
        });
    }
    
    /// <summary>
    /// バトルログ情報に応じたアニメーションを再生する
    /// </summary>
    private IObservable<Unit> PlayAnimationObservable(BattleLogInfo battleLog){

        return Observable.ReturnUnit()
            .Do(_ =>
            {
                Debug.Log("------------------");
                Debug.Log(GetLogText(battleLog));
                Debug.Log($"{string.Join(" ", battleLog.playerBattleMonsterList.Select(m => m.currentHp))} vs {string.Join(" ", battleLog.enemyBattleMonsterList.Select(m => m.currentHp))}");
            })
            .SelectMany(_ =>
            {
                var isBeDoneMonsterPlayer = battleLog.beDoneBattleMonsterIndex?.isPlayer ?? false;
                var beDoneMonsterList = isBeDoneMonsterPlayer ? battleLog.playerBattleMonsterList : battleLog.enemyBattleMonsterList;
                var beDoneMonster = beDoneMonsterList.FirstOrDefault(m => m.index.index == battleLog.beDoneBattleMonsterIndex?.index);

                switch (battleLog.type)
                {
                    case BattleLogType.MoveWave:
                        return battleWindow.PlayWaveTitleFxObservable(battleLog.waveCount, maxWaveCount);
                    case BattleLogType.StartAttack:
                        return battleWindow.PlayAttackAnimationObservable(battleLog.doBattleMonsterIndex, battleLog.beDoneBattleMonsterIndex);
                    case BattleLogType.TakeDamage:
                        return battleWindow.PlayTakeDamageAnimationObservable(battleLog.beDoneBattleMonsterIndex, battleLog.damage, beDoneMonster.currentHp);
                    case BattleLogType.Die:
                        return battleWindow.PlayDieAnimationObservable(battleLog.beDoneBattleMonsterIndex);
                    case BattleLogType.Result:
                        if (battleLog.winOrLose == WinOrLose.Win)
                        {
                            return battleWindow.PlayWinAnimationObservable();
                        }
                        else
                        {
                            return Observable.ReturnUnit();
                        }
                    default:
                        return Observable.Timer(TimeSpan.FromSeconds(1)).AsUnitObservable();
                }
            });
    }

    /// <summary>
    /// 画面遷移（フェードイン）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeInObservable()
    {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .SelectMany(res =>
            {
                battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
                battleWindow.Init(userMonsterPartyId, quest.id);
                HeaderFooterManager.Instance.Show(false);
                return Observable.ReturnUnit();
            })
            .SelectMany(_ => VisualFxManager.Instance.PlayQuestTitleFxObservable(quest.name))
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }

    /// <summary>
    /// バトル結果ダイアログを表示する
    /// </summary>
    private IObservable<Unit> ShowResultDialogObservable() { 
        return BattleResultDialogFactory.Create(new BattleResultDialogRequest())
            .AsUnitObservable();
    }

    /// <summary>
    /// 画面遷移（フェードアウト）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeOutObservable()
    {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(_ =>
            {
                Destroy(battleWindow.gameObject);
                HeaderFooterManager.Instance.Show(true);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0))
            .Do(res =>
            {
                if (battleObserver != null)
                {
                    battleObserver.OnNext(battleResult);
                    battleObserver.OnCompleted();
                    battleObserver = null;
                }
            });
    }

    /// <summary>
    /// バトルのターン進行を開始する
    /// </summary>
    private void StartTurnProgress()
    {
        Observable.ReturnUnit()
            .Do(_ => Debug.Log($"ターン{currentTurnCount}開始"))
            .SelectMany(_ => CountUpTurnObservable())
            .SelectMany(isMoveNextWave => MoveNextWaveObservable(isMoveNextWave))
            .SelectMany(_ => CreateEnemyObservable())
            .SelectMany(_ => StartPlayerAttackObservable())
            .SelectMany(_ => StartEnemyAttackObservable())
            .SelectMany(_ => JudgeContinueBattleObservable())
            .Where(isContinue => isContinue)
            .RepeatSafe()
            .Subscribe();
    }

    /// <summary>
    /// 各種ターンの計算を行う
    /// </summary>
    private IObservable<bool> CountUpTurnObservable()
    {
        // 勝敗がついていれば何もしない
        if (battleResult.wol != WinOrLose.Continue) return Observable.Return(false);

        var isNoEnemy = true;
        var isMaxWave = currentWaveCount == maxWaveCount;
        var isMoveNextWave = isNoEnemy && !isMaxWave;

        // 敵が全滅かつ最終Waveではないなら次のWaveに移動
        if (isMoveNextWave)
        {
            currentWaveCount++;
            currentTurnCountInWave = 0;
        }

        currentTurnCount++;
        currentTurnCountInWave++;

        return Observable.Return(isMoveNextWave);
    }

    /// <summary>
    /// 次のウェーブに移動する
    /// </summary>
    private IObservable<Unit> MoveNextWaveObservable(bool isMoveNextWave)
    {
        // 勝敗がついていれば何もしない
        if (battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        if (isMoveNextWave)
        {
            return VisualFxManager.Instance.PlayWaveTitleFxObservable(battleWindow._windowFrameRT, currentWaveCount, maxWaveCount);
        }

        return Observable.ReturnUnit();
    }

    /// <summary>
    /// 敵を生成する
    /// </summary>
    private IObservable<Unit> CreateEnemyObservable()
    {
        // 勝敗がついていれば何もしない
        if (battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        // Waveの最初のターンでなければ何もしない
        if (currentTurnCountInWave != 1) return Observable.ReturnUnit();

        var questWaveId = quest.questWaveIdList[currentWaveCount - 1];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);
        var questMonsterList = questWave.questMonsterIdList.Select(id => MasterRecord.GetMasterOf<QuestMonsterMB>().Get(id)).ToList();

        return Observable.ReturnUnit();
    }

    /// <summary>
    /// プレイヤーの攻撃フェイズを開始する
    /// </summary>
    private IObservable<Unit> StartPlayerAttackObservable()
    {
        // 勝敗がついていれば何もしない
        if (battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        JudgeWinOrLose();

        return Observable.ReturnUnit();
    }

    /// <summary>
    /// 敵の攻撃フェイズを開始する
    /// </summary>
    private IObservable<Unit> StartEnemyAttackObservable()
    {
        // 勝敗がついていれば何もしない
        if (battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        JudgeWinOrLose();

        return Observable.ReturnUnit();
    }

    /// <summary>
    /// バトルを続行するか（勝敗がついたか）否かを判定する
    /// </summary>
    private IObservable<bool> JudgeContinueBattleObservable()
    {
        if (battleResult.wol == WinOrLose.Win || battleResult.wol == WinOrLose.Lose)
        {
            // 勝敗がついたのでバトルを終了
            if (battleTurnObserver != null)
            {
                Observable.NextFrame()
                    .Do(_ => {
                        battleTurnObserver.OnNext(Unit.Default);
                        battleTurnObserver.OnCompleted();
                        battleTurnObserver = null;
                    })
                    .Subscribe();
            }
            return Observable.NextFrame().Select(_ => false);
        }
        else
        {
            // まだ続行
            return Observable.NextFrame().Select(_ => true);
        }
    }

    /// <summary>
    /// 勝敗を判定する
    /// </summary>
    private void JudgeWinOrLose()
    {
        if(currentWaveCount == maxWaveCount)
        {
            battleResult.wol = WinOrLose.Win;
        }
        else
        {
            battleResult.wol = WinOrLose.Continue;
        }
    }
}
