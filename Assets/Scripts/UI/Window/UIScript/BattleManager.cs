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
    private BattleResult battleResult;
    private long questId;
    private QuestMB quest;
    private int maxWaveCount;
    private string userMonsterPartyId;
    private List<BattleLogInfo> battleLogList;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init(long questId, string userMonsterPartyId)
    {
        this.questId = questId;
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        maxWaveCount = quest.questWaveIdList.Count;
        battleResult = new BattleResult() { wol = WinOrLose.Continue };
        this.userMonsterPartyId = userMonsterPartyId;
    }

    /// <summary>
    /// バトルを開始する
    /// フェードインまではする
    /// </summary>
    public IObservable<Unit> BattleStartObservable(long questId, string userMonsterPartyId)
    {
        Init(questId, userMonsterPartyId);

        return FadeInObservable()
            .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
            {
                commonDialogType = CommonDialogType.NoAndYes,
                title = "確認",
                content = "バトルを勝ったことにしますか？\n（いいえを選んだら負けたことになります）",
            }))
            .SelectMany(res =>
            {
                var winOrLose = res.dialogResponseType == DialogResponseType.Yes ? WinOrLose.Win : WinOrLose.Lose;
                return ApiConnection.EndBattle(userMonsterPartyId, questId, winOrLose);
            })
            .SelectMany(res => BattleResultDialogFactory.Create(new BattleResultDialogRequest() { userBattle = res.userBattle }))
            .SelectMany(_ => FadeOutObservable())
            .AsUnitObservable();

        // TODO: バトル機能の実装
        /*
        // 初期化
        Init(questId, userMonsterPartyId);
        
        return FadeInObservable()
            .SelectMany(_ =>
            {
                var quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
                var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);

                var battleDataProcessor = new BattleDataProcessor();
                var battleLogList = battleDataProcessor.GetBattleLogList(userMonsterParty, quest);
                return BattleStartObservable(battleLogList);
            });
        */
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
    
    /// <summary>
    /// バトルログ情報に応じたアニメーションを再生する
    /// </summary>
    private IObservable<Unit> PlayAnimationObservable(BattleLogInfo battleLog){

        return Observable.ReturnUnit()
            .Do(_ =>
            {
                Debug.Log($"--------- {battleLog.type} ---------");
                Debug.Log(battleLog.log);
                Debug.Log($"{string.Join(" ", battleLog.playerBattleMonsterList.Select(m => m.currentHp))} vs {string.Join(" ", battleLog.enemyBattleMonsterList.Select(m => m.currentHp))}");
            })
            .SelectMany(_ =>
            {
                switch (battleLog.type)
                {
                    case BattleLogType.MoveWave:
                        return battleWindow.PlayWaveTitleFxObservable(battleLog.waveCount, maxWaveCount);
                    case BattleLogType.StartAttack:
                        return battleWindow.PlayStartAttackAnimationObservable(battleLog.doBattleMonsterIndex);
                    case BattleLogType.TakeDamage:
                        var takeDamageObservableList = battleLog.beDoneBattleMonsterDataList.Select(d =>
                        {
                            var isPlayer = d.battleMonsterIndex.isPlayer;
                            var battleMonsterList = isPlayer ? battleLog.playerBattleMonsterList : battleLog.enemyBattleMonsterList;
                            var beDoneMonster = battleMonsterList.FirstOrDefault(b => b.index.index == d.battleMonsterIndex.index);
                            return battleWindow.PlayTakeDamageAnimationObservable(d.battleMonsterIndex,battleLog.skillFxId, d.hpChanges, beDoneMonster.currentHp);
                        });
                        return Observable.WhenAll(takeDamageObservableList);
                    case BattleLogType.Die:
                        var dieObservableList = battleLog.beDoneBattleMonsterDataList.Select(d =>
                        {
                            return battleWindow.PlayDieAnimationObservable(d.battleMonsterIndex);
                        });
                        return Observable.WhenAll(dieObservableList);
                    case BattleLogType.Result:
                        if (battleLog.winOrLose == WinOrLose.Win)
                        {
                            return battleWindow.PlayWinAnimationObservable();
                        }
                        else
                        {
                            return battleWindow.PlayLoseAnimationObservable();
                        }
                    default:
                        return Observable.ReturnUnit();
                }
            });
    }

    /// <summary>
    /// 画面遷移（フェードイン）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeInObservable()
    {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(_ =>
            {
                // クエストリスト画面が表示されるまで現在の画面を閉じる
                var currentWindowInfo = UIManager.Instance.currentWindowInfo;
                var windowNameList = new List<string>();
                while(currentWindowInfo != null)
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
            .SelectMany(res =>
            {
                battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
                battleWindow.Init(userMonsterPartyId, quest.id); // TODO: 途中からの再生に対応できるように
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
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }
}
