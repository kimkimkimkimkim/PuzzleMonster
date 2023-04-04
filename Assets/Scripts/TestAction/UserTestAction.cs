﻿using System;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class UserTestAction : ITestAction
{
    public List<TestActionData> GetTestActionDataList()
    {
        var testActionDataList = new List<TestActionData>();

        testActionDataList.Add(new TestActionData()
        {
            title = "セーブデータ削除",
            action = new Action(() =>
            {
                SaveDataUtil.Clear();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "オーブ追加",
            action = new Action(() =>
            {
                const long debugOrbBundleId = 9001001;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugOrbBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "オーブを追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
                    .Do(res => HeaderFooterManager.Instance.UpdatePropertyPanelText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "オーブの追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "コイン追加",
            action = new Action(() =>
            {
                const long debugCoinBundleId = 9001002;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugCoinBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "コインを追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
                    .Do(res => HeaderFooterManager.Instance.UpdatePropertyPanelText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "コインの追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "プレイヤー経験値追加",
            action = new Action(() =>
            {
                const long debugPlayerExpBundleId = 9001003;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugPlayerExpBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "プレイヤー経験値を追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
                    .Do(res => HeaderFooterManager.Instance.UpdatePropertyPanelText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "プレイヤー経験値の追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "モンスター経験値追加",
            action = new Action(() =>
            {
                const long debugMonsterExpBundleId = 9001004;
                var itemId = ItemUtil.GetItemId(ItemType.Bundle, debugMonsterExpBundleId);
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "モンスター経験値を追加します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.GrantItemsToUser(itemId))
                    .Do(res => HeaderFooterManager.Instance.UpdatePropertyPanelText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "モンスター経験値の追加が完了しました",
                    }))
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "モンスター付与",
            action = new Action(() =>
            {
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "モンスターを付与します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.DevelopGrantAllMonster())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "モンスターの付与が完了しました",
                    }))
                    .Subscribe();
            })
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "資産付与",
            action = new Action(() =>
            {
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = "資産を付与します"
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.DevelopGrantAllProperty())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "資産の付与が完了しました",
                    }))
                    .Subscribe();
            })
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "スタミナ消費(5)",
            action = new Action(() =>
            {
                var consumeStamina = 5;

                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.NoAndYes,
                    title = "確認",
                    content = $"スタミナを{consumeStamina}消費します",
                })
                    .Where(res => res.dialogResponseType == DialogResponseType.Yes)
                    .SelectMany(_ => ApiConnection.DevelopConsumeStamina(consumeStamina))
                    .Do(_ => HeaderFooterManager.Instance.SetStaminaText())
                    .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = "スタミナを消費しました",
                    }))
                    .Subscribe();
            })
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "バトルシミュレーション",
            action = new Action(() =>
            {
                DevelopInputBattleSimulationInfoWindowFactory.Create(new DevelopInputBattleSimulationInfoWindowRequest()).Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "バトルテスト",
            action = new Action(() =>
            {
                try
                {
                    var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().OrderBy(m => m.id).ToList();
                    var allMonsterNum = 10;
                    var repeatNum = (int)Math.Ceiling(monsterList.Count / (float)allMonsterNum);
                    var monsterLevelList = new List<int>() { 50, 60, 70, 80, 90, 100 };

                    Observable.Interval(TimeSpan.FromSeconds(1.0f))
                        .Do(count =>
                        {
                            // 計測開始
                            var sw = new System.Diagnostics.Stopwatch();
                            sw.Start();

                            var selectMonsterIndex = (int)Math.Floor((float)count / monsterLevelList.Count);
                            var monsterLevelListIndex = (int)(count % monsterLevelList.Count);

                            var initialId = selectMonsterIndex * allMonsterNum + 1;
                            var lastId = Math.Min((selectMonsterIndex + 1) * allMonsterNum, monsterList.Count);
                            var monsterNum = lastId - initialId + 1;
                            if (monsterNum < 2) throw new Exception();

                            var allyMonsterNum = (int)Math.Ceiling(monsterNum / 2.0f);
                            var allyMonsterList = monsterList.Where(m => initialId <= m.id && m.id < initialId + allyMonsterNum).ToList();
                            var enemyMonsterList = monsterList.Where(m => initialId + allyMonsterNum <= m.id && m.id <= lastId).ToList();

                            var level = monsterLevelList[monsterLevelListIndex];
                            var getMaxLevel = new Func<MonsterMB, int>(monster =>
                            {
                                switch (monster.rarity)
                                {
                                    case PM.Enum.Monster.MonsterRarity.R:
                                        return 80;

                                    case PM.Enum.Monster.MonsterRarity.SR:
                                        return 90;

                                    case PM.Enum.Monster.MonsterRarity.SSR:
                                        return 100;

                                    default:
                                        return 10;
                                }
                            });
                            var allyUserMonsterList = allyMonsterList.Select(m => new UserMonsterInfo()
                            {
                                id = "",
                                monsterId = m.id,
                                num = 1,
                                customData = new UserMonsterCustomData()
                                {
                                    level = Math.Min(level, getMaxLevel(m)),
                                    exp = 0,
                                    grade = 0,
                                    luck = 0,
                                },
                            }).ToList();
                            var enemyQuestMonsterList = enemyMonsterList.Select(m => new QuestMonsterMI()
                            {
                                monsterId = m.id,
                                level = Math.Min(level, getMaxLevel(m)),
                            }).ToList();
                            var quest = new QuestMB()
                            {
                                id = 0,
                                name = "バトルテスト",
                                questCategoryId = 0,
                                firstRewardItemList = new List<ItemMI>(),
                                dropItemList = new List<ProbabilityItemMI>(),
                                questMonsterListByWave = new List<List<QuestMonsterMI>>() { enemyQuestMonsterList },
                                displayConditionList = new List<ConditionMI>(),
                                canExecuteConditionList = new List<ConditionMI>(),
                                consumeStamina = 0,
                                limitTurnNum = 99,
                                isLastWaveBoss = true,
                            };
                            var battleDataProcessor = new BattleDataProcessor();
                            var battleLogList = battleDataProcessor.GetBattleLogList(allyUserMonsterList, quest);

                            // 計測停止
                            sw.Stop();
                            var ts = sw.Elapsed;

                            // ログ出力
                            var targetLog = battleLogList.First(log => log.winOrLose != PM.Enum.Battle.WinOrLose.Continue);
                            var targetPlayerMonsterHpLogList = targetLog.playerBattleMonsterList.Select(m =>
                            {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });
                            var targetEnemyMonsterHpLogList = targetLog.enemyBattleMonsterList.Select(m =>
                            {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });
                            Debug.Log("===================================================");
                            Debug.Log($"{count + 1}試合目");
                            Debug.Log($"勝敗: {(targetLog.winOrLose == PM.Enum.Battle.WinOrLose.Win ? "勝利" : "敗北")}");
                            Debug.Log($"処理時間: {ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒");
                            Debug.Log($"【味方】{string.Join(",", targetPlayerMonsterHpLogList)}");
                            Debug.Log($"　【敵】{string.Join(",", targetEnemyMonsterHpLogList)}");
                            Debug.Log("===================================================");
                        })
                        .Take(repeatNum * monsterLevelList.Count)
                        .Catch((PMApiException e) =>
                        {
                            return Observable.ReturnUnit().Do(_ => Debug.Log($"ERROR?: {e.message}")).Select(_ => 0L);
                        })
                        .Subscribe();
                }
                catch (PMApiException e)
                {
                    Debug.Log($"ERROR!: {e.message}");
                }
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "ダメージテスト",
            action = new Action(() =>
            {
                try
                {
                    var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().OrderBy(m => m.id).ToList();
                    var rMonsterId = monsterList.Where(m => m.rarity == PM.Enum.Monster.MonsterRarity.R).Shuffle().First().id;
                    var srMonsterId = monsterList.Where(m => m.rarity == PM.Enum.Monster.MonsterRarity.SR).Shuffle().First().id;
                    var ssrMonsterId = monsterList.Where(m => m.rarity == PM.Enum.Monster.MonsterRarity.SSR).Shuffle().First().id;
                    var monsterIdList = new List<long>() { rMonsterId, srMonsterId, ssrMonsterId };
                    var monsterLevelList = Enumerable.Range(0, 3).Select(i => Math.Max(1, i * 50)).ToList();
                    var getMaxLevel = new Func<MonsterMB, int>(monster =>
                    {
                        switch (monster.rarity)
                        {
                            case PM.Enum.Monster.MonsterRarity.R:
                                return 80;

                            case PM.Enum.Monster.MonsterRarity.SR:
                                return 90;

                            case PM.Enum.Monster.MonsterRarity.SSR:
                                return 100;

                            default:
                                return 10;
                        }
                    });
                    var userMonsterList = new List<UserMonsterInfo>();
                    monsterIdList.ForEach(id =>
                    {
                        var monster = monsterList.First(m => m.id == id);
                        monsterLevelList.ForEach(level =>
                        {
                            var userMonster = new UserMonsterInfo()
                            {
                                id = "",
                                monsterId = id,
                                num = 1,
                                customData = new UserMonsterCustomData()
                                {
                                    level = Math.Min(level, getMaxLevel(monster)),
                                    exp = 0,
                                    grade = 0,
                                    luck = 0,
                                },
                            };
                            userMonsterList.Add(userMonster);
                        });
                    });
                    var repeatNum = userMonsterList.Count * userMonsterList.Count;

                    Observable.Interval(TimeSpan.FromSeconds(0.1f))
                        .Do(count =>
                        {
                            // 計測開始
                            var sw = new System.Diagnostics.Stopwatch();
                            sw.Start();

                            var index = (int)count; // 0～
                            var allyIndex = index / userMonsterList.Count;
                            var enemyIndex = index % userMonsterList.Count;
                            var allyUserMonster = userMonsterList[allyIndex];
                            var enemyUserMonster = userMonsterList[enemyIndex];
                            var allyMonster = monsterList.First(m => m.id == allyUserMonster.monsterId);
                            var enemyMonster = monsterList.First(m => m.id == enemyUserMonster.monsterId);
                            var allyUserMonsterList = new List<UserMonsterInfo>() { allyUserMonster };
                            var enemyQuestMonsterList = new List<QuestMonsterMI>() { new QuestMonsterMI()
                            {
                                monsterId = enemyUserMonster.monsterId,
                                level = enemyUserMonster.customData.level,
                            } };
                            var quest = new QuestMB()
                            {
                                id = 0,
                                name = "バトルテスト",
                                questCategoryId = 0,
                                firstRewardItemList = new List<ItemMI>(),
                                dropItemList = new List<ProbabilityItemMI>(),
                                questMonsterListByWave = new List<List<QuestMonsterMI>>() { enemyQuestMonsterList },
                                displayConditionList = new List<ConditionMI>(),
                                canExecuteConditionList = new List<ConditionMI>(),
                                consumeStamina = 0,
                                limitTurnNum = 99,
                                isLastWaveBoss = true,
                            };
                            var testLog = "";
                            testLog += $"\n{index + 1}試合目";
                            testLog += $"\n味方: {allyMonster.rarity} Lv.{allyUserMonster.customData.level} {allyMonster.name}";
                            testLog += $"\n　敵: {enemyMonster.rarity} Lv.{enemyUserMonster.customData.level} {enemyMonster.name}";

                            var battleDataProcessor = new BattleDataProcessor();
                            var battleLogList = battleDataProcessor.GetBattleLogList(allyUserMonsterList, quest);
                            testLog += battleDataProcessor.GetTestLog();

                            // 計測停止
                            sw.Stop();
                            var ts = sw.Elapsed;

                            // ログ出力
                            var targetLog = battleLogList.First(log => log.winOrLose != PM.Enum.Battle.WinOrLose.Continue);
                            var targetPlayerMonsterHpLogList = targetLog.playerBattleMonsterList.Select(m =>
                            {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });
                            var targetEnemyMonsterHpLogList = targetLog.enemyBattleMonsterList.Select(m =>
                            {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });

                            testLog += $"\n勝敗: {(targetLog.winOrLose == PM.Enum.Battle.WinOrLose.Win ? "勝利" : "敗北")}";
                            testLog += $"\n処理時間: {ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒";
                            Debug.Log(testLog);
                        })
                        .Take(repeatNum)
                        .Catch((PMApiException e) =>
                        {
                            return Observable.ReturnUnit().Do(_ => Debug.Log($"ERROR?: {e.message}")).Select(_ => 0L);
                        })
                        .Subscribe();
                }
                catch (PMApiException e)
                {
                    Debug.Log($"ERROR!: {e.message}");
                }
            }),
        });

        testActionDataList.Add(new TestActionData()
        {
            title = "作業用",
            action = new Action(() =>
            {
            }),
        });

        return testActionDataList;
    }
}