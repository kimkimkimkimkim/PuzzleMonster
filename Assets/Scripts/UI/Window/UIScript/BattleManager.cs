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
    private BattleWindowUIScript battleWindow;
    private QuestMB quest;
    private int maxWaveCount;
    private string userMonsterPartyId;
    private List<BattleLogInfo> battleLogList;
    private UserBattleInfo userBattle;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init(long questId, string userMonsterPartyId)
    {
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        maxWaveCount = quest.questWaveIdList.Count;
        this.userMonsterPartyId = userMonsterPartyId;
    }

    /// <summary>
    /// バトルを開始する
    /// フェードインまではする
    /// </summary>
    public IObservable<Unit> BattleStartObservable(long questId, string userMonsterPartyId)
    {
        // 初期化
        Init(questId, userMonsterPartyId);

        // 暗転中にバトルログの取得とバトル実行APIを呼んでおく
        var callbackObservable = new Func<IObservable<Unit>>(() =>
        {
            var quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
            var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);

            var battleDataProcessor = new BattleDataProcessor();
            battleLogList = battleDataProcessor.GetBattleLogList(userMonsterParty, quest);
            var winOrLose = battleLogList.First(l => l.type == BattleLogType.Result).winOrLose;

            return ApiConnection.ExecuteBattle(userMonsterPartyId, questId, winOrLose).Do(res => userBattle = res.userBattle).AsUnitObservable();
        });
        return FadeInObservable(callbackObservable)
            .SelectMany(_ =>
            {
                var observableList = battleLogList.Select(battleLog => PlayAnimationObservable(battleLog)).ToList();
                return Observable.ReturnUnit().Connect(observableList.ToArray());
            })
            .SelectMany(_ => ApiConnection.EndBattle(userBattle.id))
            .SelectMany(_ => BattleResultDialogFactory.Create(new BattleResultDialogRequest() { userBattleId = userBattle.id }))
            .SelectMany(_ => FadeOutObservable())
            .AsUnitObservable();
    }
    
    /// <summary>
    /// 既存のバトルを途中から再開する
    /// フェードインまではする
    /// </summary>
    /*
    public IObservable<BattleResult> BattleResumeObservable(UserBattleInfo userBattle)
    {
        // 初期化
        Init(userBattle.questId, userBattle.userMonsterPartyId);
        
        return FadeInObservable().SelectMany(_ => BattleStartObservable(userBattle));
    }
    */
    
    /// <summary>
    /// バトルを開始する
    /// アニメーション再生とフェードアウトまでする
    /// </summary>
    /*
    private IObservable<Unit> BattleStartObservable(List<BattleLogInfo> battleLogList)
    {
        // アニメーションリストを作成
        this.battleLogList = battleLogList;
        var observableList = battleLogList.Select(battleLog => PlayAnimationObservable(battleLog)).ToList();
        var winOrLose = battleLogList.First(log => log.type == BattleLogType.Result).winOrLose;

        return Observable.ReturnUnit().Connect(observableList.ToArray())
            .SelectMany(_ => ApiConnection.EndBattle(userMonsterPartyId, questId, winOrLose))
            .SelectMany(_ => BattleResultDialogFactory.Create(new BattleResultDialogRequest()))
            .SelectMany(_ => FadeOutObservable());
    }
    */
    
    /// <summary>
    /// バトルログ情報に応じたアニメーションを再生する
    /// </summary>
    private IObservable<Unit> PlayAnimationObservable(BattleLogInfo battleLog){

        return Observable.ReturnUnit()
            .Do(_ =>
            {
                Debug.Log($"--------- {battleLog.type} ---------");
                Debug.Log(battleLog.log);

                if (battleLog.playerBattleMonsterList != null && battleLog.enemyBattleMonsterList != null)
                {
                    var playerMonsterHpLogList = battleLog.playerBattleMonsterList.Select(m => {
                        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                        return $"{monster.name}: {m.currentHp}";
                    });
                    var enemyMonsterHpLogList = battleLog.enemyBattleMonsterList.Select(m => {
                        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                        return $"{monster.name}: {m.currentHp}";
                    });
                    Debug.Log("===================================================");
                    Debug.Log($"【味方】{string.Join(",", playerMonsterHpLogList)}");
                    Debug.Log($"　【敵】{string.Join(",", enemyMonsterHpLogList)}");
                    Debug.Log("===================================================");
                }
            })
            .SelectMany(_ =>
            {
                switch (battleLog.type)
                {
                    // バトル開始
                    case BattleLogType.StartBattle:
                        return Observable.ReturnUnit();
                        
                    // ウェーブ進行アニメーション
                    case BattleLogType.MoveWave:
                        return battleWindow.PlayWaveTitleFxObservable(battleLog.waveCount, maxWaveCount);

                    // ターン進行アニメーション
                    case BattleLogType.MoveTurn:
                        return battleWindow.PlayTurnFxObservable(battleLog.turnCount);
                        
                    // アクションするモンスターのアクションが決まった
                    case BattleLogType.StartAction:
                        return Observable.ReturnUnit();

                    // 今からアクションしますアニメーション
                    case BattleLogType.StartActionAnimation:
                        return battleWindow.PlayStartActionAnimationObservable(GetBattleMonster(battleLog.doBattleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList), battleLog.actionType);

                    // アクション失敗
                    case BattleLogType.ActionFailed:
                        return Observable.ReturnUnit();
                    
                    // スキルエフェクト
                    case BattleLogType.TakeDamage:
                    case BattleLogType.TakeHeal:
                        var takeDamageObservableList = battleLog.beDoneBattleMonsterDataList.Select(d =>
                        {
                            var isPlayer = d.battleMonsterIndex.isPlayer;
                            var battleMonsterList = isPlayer ? battleLog.playerBattleMonsterList : battleLog.enemyBattleMonsterList;
                            var beDoneMonster = battleMonsterList.FirstOrDefault(b => b.index.index == d.battleMonsterIndex.index);
                            return battleWindow.PlayTakeDamageAnimationObservable(d.battleMonsterIndex,battleLog.skillFxId, d.hpChanges, beDoneMonster.currentHp, beDoneMonster.currentEnergy);
                        });
                        return Observable.WhenAll(takeDamageObservableList);
                        
                    // 状態異常付与
                    case BattleLogType.TakeBattleConditionAdd:
                        return Observable.ReturnUnit();
                        
                    // 状態異常解除
                    case BattleLogType.TakeBattleConditionRemove:
                        return Observable.ReturnUnit();
                        
                    // 蘇生
                    case BattleLogType.TakeRevive:
                        return Observable.ReturnUnit();
                        
                    // モンスター戦闘不能アニメーション
                    case BattleLogType.Die:
                        var dieObservableList = battleLog.beDoneBattleMonsterDataList.Select(d =>
                        {
                            return battleWindow.PlayDieAnimationObservable(d.battleMonsterIndex);
                        });
                        return Observable.WhenAll(dieObservableList);
                        
                    // 状態異常ターン進行
                    case BattleLogType.ProgressBattleConditionTurn:
                        return Observable.ReturnUnit();

                    // アクション終了時アニメーション
                    case BattleLogType.EndAction:
                        var endActionBattleMonster = GetBattleMonster(battleLog.doBattleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList);
                        return battleWindow.PlayEnergySliderAnimationObservable(battleLog.doBattleMonsterIndex,endActionBattleMonster.currentEnergy);

                    // バトル結果アニメーション
                    case BattleLogType.Result:
                        return battleLog.winOrLose == WinOrLose.Win ? battleWindow.PlayWinAnimationObservable() : battleWindow.PlayLoseAnimationObservable();
                        
                    default:
                        return Observable.ReturnUnit();
                }
            });
    }

    /// <summary>
    /// 画面遷移（フェードイン）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeInObservable(Func<IObservable<Unit>> callbackObservable)
    {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .SelectMany(res =>
            {
                battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
                battleWindow.Init(userMonsterPartyId, quest.id); // TODO: 途中からの再生に対応できるように
                HeaderFooterManager.Instance.Show(false);
                return Observable.ReturnUnit();
            })
            .SelectMany(_ => Observable.WhenAll(
                VisualFxManager.Instance.PlayQuestTitleFxObservable(quest.name),
                callbackObservable()
            ))
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }

    /// <summary>
    /// 画面遷移（フェードアウト）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeOutObservable()
    {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(_ =>
            {
                // クエストリスト画面が表示されるまで現在の画面を閉じる
                var currentWindowInfo = UIManager.Instance.currentWindowInfo;
                var windowNameList = new List<string>();
                while (currentWindowInfo != null)
                {
                    windowNameList.Add(currentWindowInfo.component.name);
                    currentWindowInfo = currentWindowInfo.parent;
                }
                var questListWindowName = UIManager.Instance.GetUIName<QuestListWindowUIScript>();
                var existsQuestListWindow = windowNameList.Any(name => name == questListWindowName);

                if (existsQuestListWindow)
                {
                    currentWindowInfo = UIManager.Instance.currentWindowInfo;
                    while (currentWindowInfo.component.name != questListWindowName)
                    {
                        UIManager.Instance.CloseWindow();
                        currentWindowInfo = UIManager.Instance.currentWindowInfo;
                    }
                }
            })
            .Do(_ =>
            {
                Destroy(battleWindow.gameObject);
                HeaderFooterManager.Instance.Show(true);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex monsterIndex, List<BattleMonsterInfo> playerBattleMonsterList, List<BattleMonsterInfo> enemyBattleMonsterList)
    {
        if (monsterIndex.isPlayer)
        {
            return playerBattleMonsterList[monsterIndex.index];
        }
        else
        {
            return enemyBattleMonsterList[monsterIndex.index];
        }
    }
}
