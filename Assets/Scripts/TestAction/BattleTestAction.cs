using System;
using System.Collections.Generic;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using System.Linq;

public class BattleTestAction : ITestAction {
    public List<TestActionData> GetTestActionDataList() {
        var testActionDataList = new List<TestActionData>();

        testActionDataList.Add(new TestActionData() {
            title = "バトルシミュレーション",
            action = new Action(() => {
                CommonInputDialogFactory.Create(new CommonInputDialogRequest() { contentText = "自分の使用するモンスターIDを入力してください" })
                    .SelectMany(res => {
                        if (long.TryParse(res.inputText, out long monsterId)) {
                            return CommonInputDialogFactory.Create(new CommonInputDialogRequest() { contentText = "自分の使用するモンスターのレベルを入力してください" })
                                .Select(resp => {
                                    if (int.TryParse(resp.inputText, out int monsterLevel)) {
                                        return (isContinued: true, monsterId: monsterId, monsterLevel: monsterLevel);
                                    } else {
                                        return (isContinued: false, monsterId: 0L, monsterLevel: 0);
                                    }
                                });
                        } else {
                            return Observable.Return((isContinued: false, monsterId: 0L, monsterLevel: 0));
                        }
                    })
                    .SelectMany(res => {
                        if (res.isContinued) {
                            var userMonster = new UserMonsterInfo() {
                                monsterId = res.monsterId,
                                customData = new UserMonsterCustomData() {
                                    level = res.monsterLevel,
                                }
                            };
                            var quest = new QuestMB() {
                                id = 0,
                                name = "バトルシミュレーション",
                                questCategoryId = 0,
                                firstRewardItemList = new List<ItemMI>(),
                                dropItemList = new List<ProbabilityItemMI>(),
                                questMonsterListByWave = new List<List<QuestMonsterMI>>()
                                {
                                    new List<QuestMonsterMI>()
                                    {
                                        new QuestMonsterMI()
                                        {
                                            monsterId = res.monsterId,
                                            level = res.monsterLevel,
                                        },
                                    },
                                },
                                displayConditionList = new List<ConditionMI>(),
                                canExecuteConditionList = new List<ConditionMI>(),
                                consumeStamina = 0,
                                limitTurnNum = 99,
                                isLastWaveBoss = true,
                            };
                            return BattleManager.Instance.StartBattleSimulationObservable(new List<UserMonsterInfo>() { userMonster }, quest).AsUnitObservable();
                        } else {
                            return CommonDialogFactory.Create(new CommonDialogRequest() {
                                commonDialogType = CommonDialogType.YesOnly,
                                title = "お知らせ",
                                content = "正常な値が入力されませんでした",
                            }).AsUnitObservable();
                        }
                    })
                    .Subscribe();
            }),
        });

        testActionDataList.Add(new TestActionData() {
            title = "バトルテスト",
            action = new Action(() => {
                try {
                    var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().OrderBy(m => m.id).ToList();
                    var allMonsterNum = 10;
                    var repeatNum = (int)Math.Ceiling(monsterList.Count / (float)allMonsterNum);
                    var monsterLevelList = new List<int>() { 50, 60, 70, 80, 90, 100 };

                    Observable.Interval(TimeSpan.FromSeconds(1.0f))
                        .Do(count => {
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
                            var getMaxLevel = new Func<MonsterMB, int>(monster => {
                                switch (monster.rarity) {
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
                            var allyUserMonsterList = allyMonsterList.Select(m => new UserMonsterInfo() {
                                id = "",
                                monsterId = m.id,
                                num = 1,
                                customData = new UserMonsterCustomData() {
                                    level = Math.Min(level, getMaxLevel(m)),
                                    exp = 0,
                                    grade = 0,
                                    luck = 0,
                                },
                            }).ToList();
                            var enemyQuestMonsterList = enemyMonsterList.Select(m => new QuestMonsterMI() {
                                monsterId = m.id,
                                level = Math.Min(level, getMaxLevel(m)),
                            }).ToList();
                            var quest = new QuestMB() {
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
                            var targetPlayerMonsterHpLogList = targetLog.playerBattleMonsterList.Select(m => {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });
                            var targetEnemyMonsterHpLogList = targetLog.enemyBattleMonsterList.Select(m => {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });
                            Debug.Log("===================================================");
                            Debug.Log($"{count + 1}試合目");
                            Debug.Log($"勝敗: {(targetLog.winOrLose == PM.Enum.Battle.WinOrLose.Win ? "勝利" : "敗北")}");
                            var timeText = $"処理時間: {ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒";
                            if (ts.TotalSeconds >= 5) {
                                Debug.LogError(timeText);
                            } else {
                                Debug.Log(timeText);
                            }
                            Debug.Log($"【味方】{string.Join(",", targetPlayerMonsterHpLogList)}");
                            Debug.Log($"　【敵】{string.Join(",", targetEnemyMonsterHpLogList)}");
                            Debug.Log("===================================================");
                        })
                        .Take(repeatNum * monsterLevelList.Count)
                        .Catch((PMApiException e) => {
                            return Observable.ReturnUnit().Do(_ => Debug.Log($"ERROR?: {e.message}")).Select(_ => 0L);
                        })
                        // .Buffer(repeatNum * monsterLevelList.Count)
                        // .SelectMany(_ => CommonDialogFactory.Create(new CommonDialogRequest() { title = "通知", content = "完了しました", commonDialogType = CommonDialogType.YesOnly }))
                        .Subscribe();
                } catch (PMApiException e) {
                    Debug.Log($"ERROR!: {e.message}");
                }
            }),
        });

        testActionDataList.Add(new TestActionData() {
            title = "ダメージテスト",
            action = new Action(() => {
                try {
                    var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().OrderBy(m => m.id).ToList();
                    var rMonsterId = monsterList.Where(m => m.rarity == PM.Enum.Monster.MonsterRarity.R).Shuffle().First().id;
                    var srMonsterId = monsterList.Where(m => m.rarity == PM.Enum.Monster.MonsterRarity.SR).Shuffle().First().id;
                    var ssrMonsterId = monsterList.Where(m => m.rarity == PM.Enum.Monster.MonsterRarity.SSR).Shuffle().First().id;
                    var monsterIdList = new List<long>() { rMonsterId, srMonsterId, ssrMonsterId };
                    var monsterLevelList = Enumerable.Range(0, 3).Select(i => Math.Max(1, i * 50)).ToList();
                    var userMonsterList = new List<UserMonsterInfo>();
                    monsterIdList.ForEach(id => {
                        var monster = monsterList.First(m => m.id == id);
                        monsterLevelList.ForEach(level => {
                            var userMonster = new UserMonsterInfo() {
                                id = "",
                                monsterId = id,
                                num = 1,
                                customData = new UserMonsterCustomData() {
                                    level = level,
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
                        .Do(count => {
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
                            var quest = new QuestMB() {
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
                            testLog += battleDataProcessor.testLog;

                            // 計測停止
                            sw.Stop();
                            var ts = sw.Elapsed;

                            // ログ出力
                            var targetLog = battleLogList.First(log => log.winOrLose != PM.Enum.Battle.WinOrLose.Continue);
                            var targetPlayerMonsterHpLogList = targetLog.playerBattleMonsterList.Select(m => {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });
                            var targetEnemyMonsterHpLogList = targetLog.enemyBattleMonsterList.Select(m => {
                                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                                return $"{monster.name}: {m.currentHp}";
                            });

                            testLog += $"\n勝敗: {(targetLog.winOrLose == PM.Enum.Battle.WinOrLose.Win ? "勝利" : "敗北")}";
                            testLog += $"\n処理時間: {ts.Hours}時間 {ts.Minutes}分 {ts.Seconds}秒 {ts.Milliseconds}ミリ秒";
                            Debug.Log(testLog);
                        })
                        .Take(repeatNum)
                        .Catch((PMApiException e) => {
                            return Observable.ReturnUnit().Do(_ => Debug.Log($"ERROR?: {e.message}")).Select(_ => 0L);
                        })
                        .Subscribe();
                } catch (PMApiException e) {
                    Debug.Log($"ERROR!: {e.message}");
                }
            }),
        });

        testActionDataList.Add(new TestActionData() {
            title = "バトルスキル効果エラーチェック",
            action = new Action(() => {
                BattleTest.Start();
            }),
        });

        return testActionDataList;
    }
}
