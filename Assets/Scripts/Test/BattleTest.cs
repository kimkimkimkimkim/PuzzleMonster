using PM.Enum.Battle;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// バトル関係のテストを行うクラス
/// </summary>
public static class BattleTest {
    private static string ELLIPSIS = "=== ";
    public static void Start() {
        try {
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
            // 対象者に攻撃力ダウンが付与されたか
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
            ErrorIf(monsterId, battleActionType, "対象者に攻撃力ダウンが付与されたか", CheckBattleConditionAdd(battleLogList, playerBattleMonsterList[0].index, enemyBattleMonsterListByWave[0][0].index, out logText), logText);

            // id:16, normal
            monsterId = 16;
            battleActionType = BattleActionType.NormalSkill;
            // HPが最も低い味方を回復したか（味方）
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
            ErrorIf(monsterId, battleActionType, "HPが最も低い味方を回復したか（味方）", CheckHeal(battleLogList, playerBattleMonsterList[0].index, playerBattleMonsterList[1].index, out logText), logText);
            // HPが最も低い味方を回復したか（自分）
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
            ErrorIf(monsterId, battleActionType, "HPが最も低い味方を回復したか（自分）", CheckHeal(battleLogList, playerBattleMonsterList[0].index, playerBattleMonsterList[0].index, out logText), logText);

        } catch {
            Debug.LogError($"{ELLIPSIS}バトルテストエラー");
        }
    }

    private static void ErrorIf(long monsterId, BattleActionType battleActionType, string testContent, bool condition, string logText) {
        var commonText = $"id:{monsterId}, actionType:{battleActionType}, テスト内容:{testContent}\n{logText}";
        if (condition) {
            Debug.Log($"{ELLIPSIS}【成功】 {commonText}");
        } else {
            Debug.Log($"{ELLIPSIS}【失敗】 {commonText}");
            throw new System.Exception();
        }
    }

    private static string GetBattleLogText(BattleLogInfo battleLog) {
        return Newtonsoft.Json.JsonConvert.SerializeObject(battleLog, Newtonsoft.Json.Formatting.Indented);
    }

    /// <summary>
    /// 状態異常付与のスキル効果が発動しているかどうかのチェック（成功、失敗は問わない）
    /// </summary>
    /// <param name="doMonsterIndex">状態異常付与スキルの発動者</param>
    /// <param name="beDoneMonsterIndex">状態異常付与スキルを受ける方</param>
    private static bool CheckBattleConditionAdd(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, out string logText) {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.TakeBattleConditionAdd)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .Where(l => l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex)))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }

    /// <summary>
    /// 回復のスキル効果が発動しているかどうかのチェック（成功、失敗は問わない）
    /// </summary>
    /// <param name="doMonsterIndex">スキルの発動者</param>
    /// <param name="beDoneMonsterIndex">スキルを受ける方</param>
    private static bool CheckHeal(List<BattleLogInfo> battleLogList, BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, out string logText) {
        var battleLog = battleLogList
            .Where(l => l.type == BattleLogType.TakeHeal)
            .Where(l => l.doBattleMonsterIndex.IsSame(doMonsterIndex))
            .Where(l => l.beDoneBattleMonsterDataList.Any(i => i.battleMonsterIndex.IsSame(beDoneMonsterIndex)))
            .FirstOrDefault();
        logText = GetBattleLogText(battleLog);
        return battleLog != null;
    }
}
