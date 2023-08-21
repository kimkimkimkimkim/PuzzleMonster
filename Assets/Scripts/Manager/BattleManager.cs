﻿using System;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using UniRx;
using GameBase;
using UnityEngine;
using PM.Enum.UI;
using PM.Enum.Monster;

public class BattleManager : SingletonMonoBehaviour<BattleManager> {
    private BattleWindowUIScript battleWindow;
    private QuestMB quest;
    private int maxWaveCount;
    private string userBattleId;
    private List<UserMonsterInfo> partyUserMonsterList;
    private List<BattleLogInfo> battleLogList;

    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init(long questId, string userMonsterPartyId) {
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        maxWaveCount = quest.questMonsterListByWave.Count;
        var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
        partyUserMonsterList = userMonsterParty.userMonsterIdList.Select(userMonsterId => {
            return ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
        }).ToList();
    }

    /// <summary>
    /// バトルを開始する
    /// フェードインまではする
    /// </summary>
    public IObservable<Unit> StartBattleObservable(long questId, string userMonsterPartyId) {
        // 初期化
        Init(questId, userMonsterPartyId);

        // 暗転中にバトルログの取得とバトル実行APIを呼んでおく
        var callbackObservable = new Func<IObservable<Unit>>(() => {
            var quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
            var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
            var userMonsterList = userMonsterParty.userMonsterIdList.Select(userMonsterId => {
                return ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            }).ToList();

            var battleDataProcessor = new BattleDataProcessor();
            battleLogList = battleDataProcessor.GetBattleLogList(userMonsterList, quest);
            var winOrLose = battleLogList.First(l => l.type == BattleLogType.Result).winOrLose;

            return ApiConnection.ExecuteBattle(userMonsterPartyId, questId, winOrLose)
                .Do(res => {
                    userBattleId = res.userBattle.id;

                    // 再開用にバトル情報をクライアントに保存しておく
                    SaveDataUtil.Battle.SetResumeQuestId(questId);
                    SaveDataUtil.Battle.SetResumeUserMonsterPartyId(userMonsterPartyId);
                    SaveDataUtil.Battle.SetResumeUserBattleId(res.userBattle.id);
                    SaveDataUtil.Battle.SetResumeBattleLogList(battleLogList);
                })
                .AsUnitObservable();
        });

        // バトルを実行
        return PlayBattleObservable(callbackObservable);
    }

    /// <summary>
    /// 正常に終了しなかったバトルを再開する
    /// </summary>
    public IObservable<Unit> ResumeBattleObservable(long questId, string userMonsterPartyId, string userBattleId, List<BattleLogInfo> battleLogList) {
        // 初期化
        Init(questId, userMonsterPartyId);
        this.userBattleId = userBattleId;
        this.battleLogList = battleLogList;

        // バトルを実行
        return PlayBattleObservable();
    }

    private IObservable<Unit> PlayBattleObservable(Func<IObservable<Unit>> callbackObservable = null) {
        var beforeRank = ApplicationContext.userData.rank;
        return FadeInObservable(callbackObservable)
            .SelectMany(_ => {
                var observableList = battleLogList.Select(battleLog => PlayAnimationObservable(battleLog)).ToList();
                return Observable.ReturnUnit().Connect(observableList.ToArray());
            })
            .SelectMany(_ => ApiConnection.EndBattle(userBattleId))
            .Do(_ => {
                // バトルが正常に終了したので再開用データを削除
                SaveDataUtil.Battle.ClearAllResumeSaveData();
            })
            .SelectMany(res => {
                var resultLog = battleLogList.First(l => l.type == BattleLogType.Result);
                return BattleResultDialogFactory.Create(new BattleResultDialogRequest() {
                    winOrLose = resultLog.winOrLose,
                    playerBattleMonsterList = resultLog.playerBattleMonsterList,
                    enemyBattleMonsterListByWave = resultLog.enemyBattleMonsterListByWave,
                })
                    .SelectMany(_ => {
                        if (resultLog.winOrLose == WinOrLose.Win) {
                            var rewardItemList = ItemUtil.GetRewardItemList(res.userBattle);
                            return RewardReceiveDialogFactory.Create(new RewardReceiveDialogRequest() {
                                rewardItemList = rewardItemList,
                            }).AsUnitObservable();
                        } else {
                            return Observable.ReturnUnit();
                        }
                    });
            })
            .SelectMany(_ => {
                var afterRank = ApplicationContext.userData.rank;
                if (afterRank > beforeRank) {
                    var beforeStamina = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.rank == beforeRank);
                    var afterStamina = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.rank == afterRank);
                    if (beforeStamina != null && afterStamina != null) {
                        return PlayerRankUpDialogFactory.Create(new PlayerRankUpDialogRequest() {
                            beforeRank = beforeRank,
                            afterRank = afterRank,
                            beforeMaxStamina = beforeStamina.stamina,
                            afterMaxStamina = afterStamina.stamina,
                        }).AsUnitObservable();
                    } else {
                        return Observable.ReturnUnit();
                    }
                } else {
                    return Observable.ReturnUnit();
                }
            })
            .SelectMany(_ => FadeOutObservable())
            .AsUnitObservable();
    }

    /// <summary>
    /// バトルログ情報に応じたアニメーションを再生する
    /// </summary>
    private IObservable<Unit> PlayAnimationObservable(BattleLogInfo battleLog) {
        return Observable.ReturnUnit()
            .Do(_ => {
                Debug.Log($"--------- {battleLog.type} ---------");
                Debug.Log(battleLog.log);

                if (battleLog.playerBattleMonsterList != null && battleLog.enemyBattleMonsterList != null) {
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
            .SelectMany(_ => {
                switch (battleLog.type) {
                    // バトル開始
                    case BattleLogType.StartBattle:
                        battleWindow.SetBattleMonsterList(battleLog.playerBattleMonsterList);
                        return Observable.ReturnUnit();

                    // ウェーブ進行アニメーション
                    case BattleLogType.MoveWave:
                        battleWindow.SetBattleMonsterList(battleLog.enemyBattleMonsterList);
                        return battleWindow.PlayWaveTitleFxObservable(battleLog.waveCount, maxWaveCount)
                            .SelectMany(res => {
                                var isPlayBossWaveEffect = battleLog.waveCount == maxWaveCount;
                                if (isPlayBossWaveEffect && quest.isLastWaveBoss) {
                                    return battleWindow.PlayBossWaveAnimationObservable();
                                } else {
                                    return Observable.ReturnUnit();
                                }
                            });

                    // ステータス変化
                    case BattleLogType.TakeStatusChange: {
                            var battleMonster = GetBattleMonster(battleLog.doBattleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList);
                            battleWindow.UpdateBattleMonster(battleMonster);
                            battleWindow.UpdateHpSlider(battleMonster);
                            return Observable.ReturnUnit();
                        }

                    // ターン進行アニメーション
                    case BattleLogType.MoveTurn:
                        return battleWindow.PlayTurnFxObservable(battleLog.turnCount);

                    // アクションするモンスターのアクションが決まった
                    case BattleLogType.StartAction:
                        // 何もしない
                        return Observable.ReturnUnit();

                    // アクションスタートアニメーション
                    case BattleLogType.StartActionAnimation: {
                            var battleMonster = GetBattleMonster(battleLog.doBattleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList);
                            battleWindow.UpdateBattleMonster(battleMonster);
                            return Observable.WhenAll(
                                battleWindow.ShowSkillInfoObservable(battleMonster, battleLog.actionType),
                                battleWindow.PlayAttackAnimationObservable(battleMonster, battleLog.actionType)
                            );
                        }

                    // アクション失敗
                    case BattleLogType.ActionFailed:
                        return battleWindow.PlayActionFailedAnimationObservable(battleLog.doBattleMonsterIndex);

                    // スキルエフェクト
                    case BattleLogType.TakeDamage:
                    case BattleLogType.TakeHeal:
                        var takeDamageObservableList = battleLog.beDoneBattleMonsterDataList.Select(d => {
                            var beDoneMonster = GetBattleMonster(d.battleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList);
                            battleWindow.UpdateBattleMonster(beDoneMonster);
                            return battleWindow.PlayTakeDamageAnimationObservable(battleLog.actionType, d, battleLog.skillFxId, beDoneMonster.currentHp, beDoneMonster.currentEnergy, beDoneMonster.currentShield);
                        }).ToList();
                        return Observable.WhenAll(takeDamageObservableList);

                    // 状態異常付与
                    case BattleLogType.TakeBattleConditionAdd:
                    // 状態異常解除前
                    case BattleLogType.TakeBattleConditionRemoveBefore:
                    // 状態異常解除後
                    case BattleLogType.TakeBattleConditionRemoveAfter:
                    // 状態異常ターン進行
                    case BattleLogType.ProgressBattleConditionTurn:
                        battleLog.beDoneBattleMonsterDataList.ForEach(d => {
                            var battleMonster = GetBattleMonster(d.battleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList);
                            battleWindow.UpdateBattleMonster(battleMonster);
                            battleWindow.RefreshBattleCondition(battleMonster);
                        });
                        return Observable.ReturnUnit();

                    // 蘇生
                    case BattleLogType.TakeRevive:
                        return Observable.ReturnUnit();

                    // モンスター戦闘不能アニメーション
                    case BattleLogType.Die:
                        var dieObservableList = battleLog.beDoneBattleMonsterDataList.Select(d => {
                            var battleMonster = GetBattleMonster(d.battleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList);
                            battleWindow.UpdateBattleMonster(battleMonster);
                            return battleWindow.PlayDieAnimationObservable(d.battleMonsterIndex);
                        });
                        return Observable.WhenAll(dieObservableList);

                    // アクション終了時アニメーション
                    case BattleLogType.EndAction:
                        var endActionBattleMonster = GetBattleMonster(battleLog.doBattleMonsterIndex, battleLog.playerBattleMonsterList, battleLog.enemyBattleMonsterList);
                        battleWindow.UpdateBattleMonster(endActionBattleMonster);
                        return battleWindow.PlayEnergySliderAnimationObservable(battleLog.doBattleMonsterIndex, endActionBattleMonster.currentEnergy);

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
    private IObservable<Unit> FadeInObservable(Func<IObservable<Unit>> callbackObservable) {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(res => {
                HeaderFooterManager.Instance.Show(false);
            })
            .SelectMany(_ => Observable.WhenAll(
                VisualFxManager.Instance.PlayQuestTitleFxObservable(quest.name),
                Observable.ReturnUnit().SelectMany(res => {
                    if (callbackObservable != null) {
                        return callbackObservable();
                    } else {
                        return Observable.ReturnUnit();
                    }
                })
            ))
            .Do(_ => {
                battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
                battleWindow.Init(partyUserMonsterList, quest, userBattleId);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }

    /// <summary>
    /// 画面遷移（フェードアウト）時処理を実行
    /// </summary>
    public IObservable<Unit> FadeOutObservable() {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(_ => {
                // クエストリスト画面が表示されるまで現在の画面を閉じる
                var currentWindowInfo = UIManager.Instance.currentWindowInfo;
                var windowNameList = new List<string>();
                while (currentWindowInfo != null) {
                    windowNameList.Add(currentWindowInfo.component.name);
                    currentWindowInfo = currentWindowInfo.parent;
                }
                var questListWindowName = UIManager.Instance.GetUIName<QuestListWindowUIScript>();
                var existsQuestListWindow = windowNameList.Any(name => name == questListWindowName);

                if (existsQuestListWindow) {
                    currentWindowInfo = UIManager.Instance.currentWindowInfo;
                    while (currentWindowInfo.component.name != questListWindowName) {
                        UIManager.Instance.CloseWindow();
                        currentWindowInfo = UIManager.Instance.currentWindowInfo;
                    }
                }
            })
            .Do(_ => {
                Destroy(battleWindow.gameObject);
                HeaderFooterManager.Instance.Show(true);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex monsterIndex, List<BattleMonsterInfo> playerBattleMonsterList, List<BattleMonsterInfo> enemyBattleMonsterList) {
        if (monsterIndex.isPlayer) {
            return playerBattleMonsterList.First(battleMonster => battleMonster.index.IsSame(monsterIndex));
        } else {
            return enemyBattleMonsterList.First(battleMonster => battleMonster.index.IsSame(monsterIndex));
        }
    }

    #region SIMULATION

    public IObservable<Unit> StartBattleSimulationObservable(List<UserMonsterInfo> userMonsterList, QuestMB quest) {
        // 初期化
        this.quest = quest;
        maxWaveCount = quest.questMonsterListByWave.Count;
        partyUserMonsterList = userMonsterList;

        // バトルログの取得を行う
        var battleDataProcessor = new BattleDataProcessor();
        battleLogList = battleDataProcessor.GetBattleLogList(userMonsterList, quest);

        // バトルを実行
        return FadeInObservable(null)
            .SelectMany(_ => {
                var observableList = battleLogList.Select(battleLog => PlayAnimationObservable(battleLog)).ToList();
                return Observable.ReturnUnit().Connect(observableList.ToArray());
            })
            .SelectMany(res => {
                var resultLog = battleLogList.First(l => l.type == BattleLogType.Result);
                return BattleResultDialogFactory.Create(new BattleResultDialogRequest() {
                    winOrLose = resultLog.winOrLose,
                    playerBattleMonsterList = resultLog.playerBattleMonsterList,
                    enemyBattleMonsterListByWave = resultLog.enemyBattleMonsterListByWave,
                });
            })
            .SelectMany(_ => FadeOutObservable())
            .AsUnitObservable();
    }

    public IObservable<Unit> StartBattleTestObservable(List<BattleLogInfo> battleLogList) {
        battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
        Debug.Log("===========");
        return battleWindow.StartTestObservable(battleLogList);
    }

    #endregion SIMULATION
}
