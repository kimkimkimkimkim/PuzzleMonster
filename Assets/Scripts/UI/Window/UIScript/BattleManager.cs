﻿using System;
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
    /// フェードインまではする
    /// </summary>
    public IObservable<Unit> BattleStartObservable(long questId, string userMonsterPartyId)
    {
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
                            return battleWindow.PlayLoseAnimationObservable();
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
