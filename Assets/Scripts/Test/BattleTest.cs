using PM.Enum.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// バトル関係のテストを行うクラス
/// </summary>
public static class BattleTest
{
    private static string ELLIPSIS = "=== ";

    public static void Start()
    {
        try
        {
            var monsterId = 0L;
            var battleActionType = BattleActionType.None;
            var battleDataProcessor = new BattleDataProcessor();
            var playerBattleMonsterList = new List<BattleMonsterInfo>();
            var enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>();
            var battleLogList = new List<BattleLogInfo>();
            var battleLog = new BattleLogInfo();
            var logText = "";
            var monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().ToList();

            // id:7, normal
            monsterId = 7;
            battleActionType = BattleActionType.NormalSkill;
            // スキル効果チェック
            battleDataProcessor = new BattleDataProcessor();
            battleDataProcessor.TestInit();
            playerBattleMonsterList = new List<BattleMonsterInfo>() {
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0),
            };
            enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>() {
                new List<BattleMonsterInfo>() {
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 0, 1, isActed: true),
                },
            };
            battleLogList = battleDataProcessor.TestStart(playerBattleMonsterList, enemyBattleMonsterListByWave);
            ErrorIf(monsterId, battleActionType, "対象者に攻撃力ダウンが付与されたか", CheckBattleConditionAdd(battleLogList, playerBattleMonsterList[0].index, enemyBattleMonsterListByWave[0][0].index, out logText), logText, battleLogList);

            // id:16, normal
            monsterId = 16;
            battleActionType = BattleActionType.NormalSkill;
            // スキル効果チェック
            battleDataProcessor = new BattleDataProcessor();
            battleDataProcessor.TestInit();
            playerBattleMonsterList = new List<BattleMonsterInfo>() {
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0),
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0, isActed: true, currentHp: 1),
            };
            enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>() {
                new List<BattleMonsterInfo>() {
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 0, 1, isActed: true),
                },
            };
            battleLogList = battleDataProcessor.TestStart(playerBattleMonsterList, enemyBattleMonsterListByWave);
            ErrorIf(monsterId, battleActionType, "HPが最も低い味方を回復したか（味方）", CheckHeal(battleLogList, playerBattleMonsterList[0].index, playerBattleMonsterList[1].index, out logText), logText, battleLogList);
            // スキル効果チェック
            battleDataProcessor = new BattleDataProcessor();
            battleDataProcessor.TestInit();
            playerBattleMonsterList = new List<BattleMonsterInfo>() {
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0, currentHp: 1),
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0, isActed: true),
            };
            enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>() {
                new List<BattleMonsterInfo>() {
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 0, 1, isActed: true),
                },
            };
            battleLogList = battleDataProcessor.TestStart(playerBattleMonsterList, enemyBattleMonsterListByWave);
            ErrorIf(monsterId, battleActionType, "HPが最も低い味方を回復したか（自分）", CheckHeal(battleLogList, playerBattleMonsterList[0].index, playerBattleMonsterList[0].index, out logText), logText, battleLogList);

            // id:23, normal
            monsterId = 23;
            battleActionType = BattleActionType.NormalSkill;
            // スキル効果チェック
            battleDataProcessor = new BattleDataProcessor();
            battleDataProcessor.TestInit();
            playerBattleMonsterList = new List<BattleMonsterInfo>() {
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0),
            };
            enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>() {
                new List<BattleMonsterInfo>() {
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 0, 1,currentEnergy:ConstManager.Battle.MAX_ENERGY_VALUE, baseSpeed: 0),
                },
            };
            battleLogList = battleDataProcessor.TestStart(playerBattleMonsterList, enemyBattleMonsterListByWave, 2);
            ErrorIf(monsterId, battleActionType, "対象者のエネルギーを減少させたか", CheckEnergyDown(battleLogList, playerBattleMonsterList[0].index, enemyBattleMonsterListByWave[0][0].index, out logText), logText, battleLogList);
            ErrorIf(monsterId, battleActionType, "相手がアルティメットスキルを使ってないか", !CheckUltimateSkill(battleLogList, enemyBattleMonsterListByWave[0][0].index, out logText), logText, battleLogList);
            ErrorIf(monsterId, battleActionType, "相手が通常スキルを使ったか", CheckNormalSkill(battleLogList, enemyBattleMonsterListByWave[0][0].index, out logText), logText, battleLogList);

            // id:61, ultimate
            monsterId = 61;
            battleActionType = BattleActionType.UltimateSkill;
            // スキル効果チェック
            battleDataProcessor = new BattleDataProcessor();
            battleDataProcessor.TestInit();
            playerBattleMonsterList = new List<BattleMonsterInfo>() {
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0,currentEnergy:ConstManager.Battle.MAX_ENERGY_VALUE, baseAttack: 100),
            };
            var battleConditionList = new List<BattleConditionInfo>()
            {
                GetBattleConditionInfo(63,3,500,1),
                GetBattleConditionInfo(63,5,500,2),
                GetBattleConditionInfo(65,3,500,3),
            };
            enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>() {
                new List<BattleMonsterInfo>() {
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 0, 1,isActed:true, battleConditionList: battleConditionList),
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 3, 1,isActed:true, battleConditionList: battleConditionList),
                },
            };
            battleLogList = battleDataProcessor.TestStart(playerBattleMonsterList, enemyBattleMonsterListByWave, 1);
            ErrorIf(monsterId, battleActionType, "燃焼状態のみが解除されたか", CheckBattleConditionRemoveOnly(battleLogList, enemyBattleMonsterListByWave[0][0].index, 63, out logText), logText, battleLogList);
            ErrorIf(monsterId, battleActionType, "出血状態のみが解除されたか", CheckBattleConditionRemoveOnly(battleLogList, enemyBattleMonsterListByWave[0][1].index, 65, out logText), logText, battleLogList);
            ErrorIf(monsterId, battleActionType, "状態異常解除時ダメージを与えているか", CheckTakeDamage(battleLogList, playerBattleMonsterList[0].index, enemyBattleMonsterListByWave[0][1].index, out logText, skillEffectIndex: 3), logText, battleLogList);

            // id:65, ultimate
            monsterId = 65;
            battleActionType = BattleActionType.UltimateSkill;
            // スキル効果チェック
            battleDataProcessor = new BattleDataProcessor();
            battleDataProcessor.TestInit();
            playerBattleMonsterList = new List<BattleMonsterInfo>() {
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0,currentEnergy:ConstManager.Battle.MAX_ENERGY_VALUE),
            };
            battleConditionList = new List<BattleConditionInfo>()
            {
                GetBattleConditionInfo(19,3,50,1),
            };
            enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>() {
                new List<BattleMonsterInfo>() {
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 2, 1,isActed:true),
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 3, 1,isActed:true, battleConditionList: battleConditionList),
                },
            };
            battleLogList = battleDataProcessor.TestStart(playerBattleMonsterList, enemyBattleMonsterListByWave, 1);
            // ErrorIf(monsterId, battleActionType, "スピードダウン状態のモンスターにのみ2つ流血状態を付与しているか", CheckBattleConditionRemoveOnly(battleLogList, enemyBattleMonsterListByWave[0][0].index, 63, out logText), logText, battleLogList);

            // 一個ずつみていく
            // id:1
            //
            monsterId = 1;
            battleDataProcessor = new BattleDataProcessor();
            battleDataProcessor.TestInit();
            playerBattleMonsterList = new List<BattleMonsterInfo>() {
                battleDataProcessor.TestGetBattleMonster(monsterId, 100, true, 0),
            };
            enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>() {
                new List<BattleMonsterInfo>() {
                    battleDataProcessor.TestGetBattleMonster(monsterId, 100, false, 0, 1, isActed: true),
                },
            };
            battleLogList = battleDataProcessor.TestStart(playerBattleMonsterList, enemyBattleMonsterListByWave);
        }
        catch (Exception e)
        {
            Debug.LogError($"{ELLIPSIS}バトルテストエラー");
            Debug.LogError($"{ELLIPSIS}{Newtonsoft.Json.JsonConvert.SerializeObject(e)}");
        }
    }

    private static BattleConditionInfo GetBattleConditionInfo(long battleConditionId, int remainingTurnNum, int actionValue, int order)
    {
        var battleCondition = MasterRecord.GetMasterOf<BattleConditionMB>().Get(battleConditionId);

        return new BattleConditionInfo()
        {
            guid = Guid.NewGuid().ToString(),
            battleConditionId = battleConditionId,
            remainingTurnNum = remainingTurnNum,
            actionValue = actionValue,
            battleConditionSkillEffect = battleCondition.skillEffect,
            grantorSkillEffect = new SkillEffectMI()
            {
                battleConditionId = battleConditionId,
                canRemove = true,
            },
            order = order,
        };
    }

    private static void ErrorIf(long monsterId, BattleActionType battleActionType, string testContent, bool condition, string logText, List<BattleLogInfo> battleLogList)
    {
        var commonText = $"id:{monsterId}, actionType:{battleActionType}, テスト内容:{testContent}\n{logText}";
        if (condition)
        {
            Debug.Log($"{ELLIPSIS}【成功】 {commonText}");
        }
        else
        {
            Debug.Log($"{ELLIPSIS}【失敗】 {commonText}");
            BattleManager.Instance.StartBattleTest(battleLogList.Where(i => IsImportantLog(i)).ToList());
            throw new System.Exception();
        }
    }

    private static bool IsImportantLog(BattleLogInfo battleLog)
    {
        switch (battleLog.type)
        {
            // バトル開始
            case BattleLogType.StartBattle:
            // ウェーブ進行アニメーション
            case BattleLogType.MoveWave:
            // ターン進行アニメーション
            // アクションするモンスターのアクションが決まった
            case BattleLogType.StartAction:
            // アクションスタートアニメーション
            case BattleLogType.StartActionAnimation:
            // アクション失敗
            case BattleLogType.ActionFailed:
            // スキルエフェクト
            case BattleLogType.TakeDamage:
            case BattleLogType.TakeHeal:
            // 状態異常付与
            case BattleLogType.TakeBattleConditionAdd:
            // 状態異常解除前
            case BattleLogType.TakeBattleConditionRemoveBefore:
            // 状態異常解除後
            case BattleLogType.TakeBattleConditionRemoveAfter:
            // 状態異常ターン進行
            case BattleLogType.ProgressBattleConditionTurn:
            // 蘇生
            case BattleLogType.TakeRevive:
            // モンスター戦闘不能アニメーション
            case BattleLogType.Die:
            // アクション終了時アニメーション
            case BattleLogType.EndAction:
            // バトル結果アニメーション
            case BattleLogType.Result:
            // スキル対象見る用
            case BattleLogType.SetSkillTarget:
            // index見る用
            case BattleLogType.StartSkillEffect:
                return true;

            default:
                return false;
        }
    }

    private static string GetBattleLogText(BattleLogInfo battleLog)
    {
        return Newtonsoft.Json.JsonConvert.SerializeObject(battleLog, Newtonsoft.Json.Formatting.Indented);
    }

    /// <summary>
    /// ダメージ付与効果が発動しているかどうかのチェック
    /// </summary>
    private static bool CheckTakeDamage(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, out string logText, int skillEffectIndex = 0, int maxDamage = -1)
    {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.TakeDamage)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .Where(l => l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex)))
            .Where(l => l.skillEffectIndex == skillEffectIndex)
            .Where(l => maxDamage < 0 || l.beDoneBattleMonsterDataList.All(i => Math.Abs(i.hpChanges) <= maxDamage))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }

    /// <summary>
    /// 状態異常付与のスキル効果が発動しているかどうかのチェック（成功、失敗は問わない）
    /// </summary>
    private static bool CheckBattleConditionAdd(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, out string logText)
    {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.TakeBattleConditionAdd)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .Where(l => l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex)))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }

    /// <summary>
    /// 状態異常が解除されているかどうかのチェック
    /// </summary>
    private static bool CheckBattleConditionRemoveOnly(List<BattleLogInfo> battleLogList, BattleMonsterIndex beDoneMonsterIndex, long battleConditionId, out string logText)
    {
        var beforeBattleLogList = battleLogList.Where(l => l.type == BattleLogType.TakeBattleConditionRemoveBefore && l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex))).ToList();
        var afterBattleLogList = battleLogList.Where(l => l.type == BattleLogType.TakeBattleConditionRemoveAfter && l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex))).ToList();

        var targetLog = beforeBattleLogList.Select((log, index) => (log, index)).Where(data =>
        {
            var beforeBattleLog = data.log;
            var afterBattleLog = afterBattleLogList[data.index];

            var beforeBeDoneMonterData = beforeBattleLog.beDoneBattleMonsterDataList.FirstOrDefault(d => d.battleMonsterIndex.IsSame(beDoneMonsterIndex));
            var afterBeDoneMonterData = afterBattleLog.beDoneBattleMonsterDataList.FirstOrDefault(d => d.battleMonsterIndex.IsSame(beDoneMonsterIndex));
            if (beforeBeDoneMonterData == null || afterBeDoneMonterData == null)
            {
                return false;
            }

            var removedBattleConditionList = beforeBeDoneMonterData.battleConditionList
                .Where(beforeC =>
                {
                    // 解除後状態異常リストに存在しないものだけに絞り込む
                    return !afterBeDoneMonterData.battleConditionList.Any(afterC => afterC.guid == beforeC.guid);
                })
                .ToList();
            return removedBattleConditionList.All(c => c.battleConditionId == battleConditionId);
        }).Select(d => d.log).FirstOrDefault();

        logText = targetLog == null ? "" : GetBattleLogText(targetLog);
        return targetLog != null;
    }

    /// <summary>
    /// 回復のスキル効果が発動しているかどうかのチェック
    /// </summary>
    private static bool CheckHeal(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, out string logText)
    {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.TakeHeal)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .Where(l => l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex)))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }

    /// <summary>
    /// エネルギー減少効果が発動しているかどうかのチェック
    /// </summary>
    private static bool CheckEnergyDown(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, out string logText)
    {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.EnergyDown)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .Where(l => l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex)))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }

    /// <summary>
    /// 通常スキルが発動しているかどうかのチェック
    /// </summary>
    private static bool CheckNormalSkill(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, out string logText)
    {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.StartAction)
            .Where(l => l.actionType == BattleActionType.NormalSkill)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }

    /// <summary>
    /// アルティメットスキルが発動しているかどうかのチェック
    /// </summary>
    private static bool CheckUltimateSkill(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, out string logText)
    {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.StartAction)
            .Where(l => l.actionType == BattleActionType.UltimateSkill)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }
}