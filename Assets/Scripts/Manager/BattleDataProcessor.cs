using PM.Enum.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using GameBase;
using System.Diagnostics;
using Newtonsoft.Json;

public partial class BattleDataProcessor {
    private int currentWaveCount;
    private int currentTurnCount;
    private QuestMB quest;
    private List<MonsterMB> monsterList;
    private List<BattleConditionMB> battleConditionList;
    private List<NormalSkillMB> normalSkillList;
    private List<UltimateSkillMB> ultimateSkillList;
    private List<PassiveSkillMB> passiveSkillList;
    private List<BattleLogInfo> battleLogList = new List<BattleLogInfo>();
    private List<BattleMonsterInfo> playerBattleMonsterList = new List<BattleMonsterInfo>(); // nullは許容しない（もともと表示されないモンスター用のデータは排除されている）
    private List<BattleMonsterInfo> enemyBattleMonsterList = new List<BattleMonsterInfo>(); // nullは許容しない（もともと表示されないモンスター用のデータは排除されている）
    private List<List<BattleMonsterInfo>> enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>();
    private WinOrLose currentWinOrLose;

    public string testLog { get; private set; } = "";

    private void Init(List<UserMonsterInfo> userMonsterList, QuestMB quest) {
        this.quest = quest;
        this.quest.limitTurnNum = 25;

        monsterList = MasterRecord.GetMasterOf<MonsterMB>().GetAll().ToList();
        battleConditionList = MasterRecord.GetMasterOf<BattleConditionMB>().GetAll().ToList();
        normalSkillList = MasterRecord.GetMasterOf<NormalSkillMB>().GetAll().ToList();
        ultimateSkillList = MasterRecord.GetMasterOf<UltimateSkillMB>().GetAll().ToList();
        passiveSkillList = MasterRecord.GetMasterOf<PassiveSkillMB>().GetAll().ToList();

        currentWaveCount = 0;
        currentTurnCount = 0;
        currentWinOrLose = WinOrLose.Continue;

        SetPlayerBattleMonsterList(userMonsterList);
    }

    private Stopwatch GetBattleLogListStopwatch = new Stopwatch();
    private string GetBatleLogListStopwatchErrorMessage = "elapsed over 10 minetes.";
    public List<BattleLogInfo> GetBattleLogList(List<UserMonsterInfo> userMonsterList, QuestMB quest) {
        try {
            GetBattleLogListStopwatch.Start();
            Init(userMonsterList, quest);

            // バトル処理を開始する
            while (currentWinOrLose == WinOrLose.Continue) {
                PlayLoop();
            }
        } catch (Exception e) {
            // バトル処理中にエラーが発生したらそこまでのログを出力する
            battleLogList.ForEach(battleLog => {
                UnityEngine.Debug.Log($"--------- {battleLog.type} ---------");
                UnityEngine.Debug.Log(battleLog.log);

                if (battleLog.playerBattleMonsterList != null && battleLog.enemyBattleMonsterList != null) {
                    var playerMonsterHpLogList = battleLog.playerBattleMonsterList.Select(m => {
                        var monster = monsterList.First(mons => mons.id == m.monsterId);
                        return $"{monster.name}: {m.currentHp}";
                    });
                    var enemyMonsterHpLogList = battleLog.enemyBattleMonsterList.Select(m => {
                        var monster = monsterList.First(mons => mons.id == m.monsterId);
                        return $"{monster.name}: {m.currentHp}";
                    });
                    UnityEngine.Debug.Log("===================================================");
                    UnityEngine.Debug.Log($"【味方】{string.Join(",", playerMonsterHpLogList)}");
                    UnityEngine.Debug.Log($"　【敵】{string.Join(",", enemyMonsterHpLogList)}");
                    UnityEngine.Debug.Log("===================================================");
                }
            });

            // エラー直前のモンスター情報を表示する
            var battleMonsterList = playerBattleMonsterList.Concat(enemyBattleMonsterList).ToList();
            battleMonsterList.ForEach(b => {
                var possessedText = b.index.isPlayer ? "味方" : "敵";
                var monsterName = monsterList.First(m => m.id == b.monsterId).name;
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(b, Newtonsoft.Json.Formatting.Indented);
                UnityEngine.Debug.Log($"【{possessedText}の{monsterName}】");
                UnityEngine.Debug.Log(json);
            });

            var pmApiException = new PMApiException() { message = e.ToString() };
            UnityEngine.Debug.Log(JsonConvert.SerializeObject(e));
            if (e.Message != GetBatleLogListStopwatchErrorMessage) throw pmApiException;
        }

        return battleLogList;
    }

    private Stopwatch StartBattleIfNeededOneShot = new Stopwatch();
    private Stopwatch StartBattleIfNeededTotal = new Stopwatch();
    private Stopwatch MoveWaveIfNeededOneShot = new Stopwatch();
    private Stopwatch MoveWaveIfNeededTotal = new Stopwatch();
    private Stopwatch MoveTurnIfNeededOneShot = new Stopwatch();
    private Stopwatch MoveTurnIfNeededTotal = new Stopwatch();
    private Stopwatch GetNormalActionerOneShot = new Stopwatch();
    private Stopwatch GetNormalActionerTotal = new Stopwatch();
    private Stopwatch GetNormalActionerActionTypeOneShot = new Stopwatch();
    private Stopwatch GetNormalActionerActionTypeTotal = new Stopwatch();
    private Stopwatch AddStartTurnActionLogOneShot = new Stopwatch();
    private Stopwatch AddStartTurnActionLogTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeTurnActionStartOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeTurnActionStartTotal = new Stopwatch();
    private Stopwatch CanActionOneShot = new Stopwatch();
    private Stopwatch CanActionTotal = new Stopwatch();
    private Stopwatch GetSkillEffectListOneShot = new Stopwatch();
    private Stopwatch GetSkillEffectListTotal = new Stopwatch();
    private Stopwatch StartActionStreamOneShot = new Stopwatch();
    private Stopwatch StartActionStreamTotal = new Stopwatch();
    private Stopwatch ActionFailedOneShot = new Stopwatch();
    private Stopwatch ActionFailedTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeActionEndOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeActionEndTotal = new Stopwatch();
    private Stopwatch AddEndTurnActionLogOneShot = new Stopwatch();
    private Stopwatch AddEndTurnActionLogTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeTurnActionEndOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeTurnActionEndTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnTargetBattleConditionAddedAndMeTurnActionEndOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnTargetBattleConditionAddedAndMeTurnActionEndTotal = new Stopwatch();
    private Stopwatch ProgressBattleConditionTurnIfNeededOneShot = new Stopwatch();
    private Stopwatch ProgressBattleConditionTurnIfNeededTotal = new Stopwatch();
    private Stopwatch EndTurnIfNeededOneShot = new Stopwatch();
    private Stopwatch EndTurnIfNeededTotal = new Stopwatch();
    private Stopwatch EndWaveIfNeededOneShot = new Stopwatch();
    private Stopwatch EndWaveIfNeededTotal = new Stopwatch();
    private Stopwatch EndBattleIfNeededOneShot = new Stopwatch();
    private Stopwatch EndBattleIfNeededTotal = new Stopwatch();

    private void ConsoleStopwatch(string name, Stopwatch oneshot, Stopwatch total) {
        var oneshotBorderMilliSeconds = 50;
        var totalBorderMilliSeconds = 1000;
        var logText = $"{name} oneshot:{oneshot.Elapsed.Hours}:{oneshot.Elapsed.Minutes}:{oneshot.Elapsed.Seconds}:{oneshot.Elapsed.Milliseconds}, total:{total.Elapsed.Hours}:{total.Elapsed.Minutes}:{total.Elapsed.Seconds}:{total.Elapsed.Milliseconds}";
        if (oneshot.ElapsedMilliseconds >= oneshotBorderMilliSeconds || total.ElapsedMilliseconds >= totalBorderMilliSeconds) {
            UnityEngine.Debug.LogError(logText);
        } else {
            UnityEngine.Debug.Log(logText);
        }
        oneshot.Reset();
        total.Stop();
    }

    private void PlayLoop() {
        StartBattleIfNeededOneShot.Start();
        StartBattleIfNeededTotal.Start();
        // バトルを開始する
        StartBattleIfNeeded();
        ConsoleStopwatch("StartBattleIfNeeded", StartBattleIfNeededOneShot, StartBattleIfNeededTotal);

        // ウェーブを進行する
        MoveWaveIfNeededOneShot.Start();
        MoveWaveIfNeededTotal.Start();
        var isWaveMove = MoveWaveIfNeeded();
        ConsoleStopwatch("MoveWaveIfNeeded", MoveWaveIfNeededOneShot, MoveWaveIfNeededTotal);

        // ターンを進行する
        MoveTurnIfNeededOneShot.Start();
        MoveTurnIfNeededTotal.Start();
        MoveTurnIfNeeded(isWaveMove);
        ConsoleStopwatch("MoveTurnIfNeeded", MoveTurnIfNeededOneShot, MoveTurnIfNeededTotal);

        // アクション実行者を取得する
        GetNormalActionerOneShot.Start();
        GetNormalActionerTotal.Start();
        var actionMonsterIndex = GetNormalActioner();
        ConsoleStopwatch("GetNormalActioner", GetNormalActionerOneShot, GetNormalActionerTotal);

        // アクションストリームを開始する
        if (actionMonsterIndex != null) {
            GetNormalActionerActionTypeOneShot.Start();
            GetNormalActionerActionTypeTotal.Start();
            var actionType = GetNormalActionerActionType(actionMonsterIndex);
            ConsoleStopwatch("GetNormalActionerActionType", GetNormalActionerActionTypeOneShot, GetNormalActionerActionTypeTotal);

            // ターンアクション開始ログの追加
            AddStartTurnActionLogOneShot.Start();
            AddStartTurnActionLogTotal.Start();
            AddStartTurnActionLog(actionMonsterIndex);
            ConsoleStopwatch("AddStartTurnActionLog", AddStartTurnActionLogOneShot, AddStartTurnActionLogTotal);

            // 状態異常を確認して行動できるかチェック
            CanActionOneShot.Start();
            CanActionTotal.Start();
            var canAction = CanAction(actionMonsterIndex, actionType);
            ConsoleStopwatch("CanAction", CanActionOneShot, CanActionTotal);

            if (canAction) {
                // アクション開始
                GetSkillEffectListOneShot.Start();
                GetSkillEffectListTotal.Start();
                var battleSkillEffectList = GetSkillEffectList(actionMonsterIndex, actionType).Select(m => new BattleSkillEffectMI() { isActive = true, skillEffect = m }).ToList();
                ConsoleStopwatch("GetSkillEffectList", GetSkillEffectListOneShot, GetSkillEffectListTotal);

                StartActionStreamOneShot.Start();
                StartActionStreamTotal.Start();
                StartActionStream(actionMonsterIndex, actionType, null, battleSkillEffectList, null);
                ConsoleStopwatch("StartActionStream", StartActionStreamOneShot, StartActionStreamTotal);
            } else {
                // アクション失敗
                ActionFailedOneShot.Start();
                ActionFailedTotal.Start();
                ActionFailed(actionMonsterIndex, actionType);
                ConsoleStopwatch("ActionFailed", ActionFailedOneShot, ActionFailedTotal);
            }

            // ターンアクション終了ログの追加
            AddEndTurnActionLogOneShot.Start();
            AddEndTurnActionLogTotal.Start();
            AddEndTurnActionLog(actionMonsterIndex);
            ConsoleStopwatch("AddEndTurnActionLog", AddEndTurnActionLogOneShot, AddEndTurnActionLogTotal);

            // ターンアクション終了時トリガースキルの発動
            ExecuteTriggerSkillIfNeededOnMeTurnActionEndOneShot.Start();
            ExecuteTriggerSkillIfNeededOnMeTurnActionEndTotal.Start();
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeTurnActionEnd, actionMonsterIndex);
            ConsoleStopwatch("ExecuteTriggerSkillIfNeededOnMeTurnActionEnd", ExecuteTriggerSkillIfNeededOnMeTurnActionEndOneShot, ExecuteTriggerSkillIfNeededOnMeTurnActionEndTotal);

            ExecuteTriggerSkillIfNeededOnTargetBattleConditionAddedAndMeTurnActionEndOneShot.Start();
            ExecuteTriggerSkillIfNeededOnTargetBattleConditionAddedAndMeTurnActionEndTotal.Start();
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnTargetBattleConditionAddedAndMeTurnActionEnd, actionMonsterIndex);
            ConsoleStopwatch("ExecuteTriggerSkillIfNeededOnTargetBattleConditionAddedAndMeTurnActionEnd", ExecuteTriggerSkillIfNeededOnTargetBattleConditionAddedAndMeTurnActionEndOneShot, ExecuteTriggerSkillIfNeededOnTargetBattleConditionAddedAndMeTurnActionEndTotal);

            // 状態異常のターンを経過させる
            ProgressBattleConditionTurnIfNeededOneShot.Start();
            ProgressBattleConditionTurnIfNeededTotal.Start();
            ProgressBattleConditionTurnIfNeeded(actionMonsterIndex);
            ConsoleStopwatch("ProgressBattleConditionTurnIfNeeded", ProgressBattleConditionTurnIfNeededOneShot, ProgressBattleConditionTurnIfNeededTotal);
        }

        // ターンを終了する
        EndTurnIfNeededOneShot.Start();
        EndTurnIfNeededTotal.Start();
        var isTurnEnd = EndTurnIfNeeded();
        ConsoleStopwatch("EndTurnIfNeeded", EndTurnIfNeededOneShot, EndTurnIfNeededTotal);

        // ウェーブを終了する
        EndWaveIfNeededOneShot.Start();
        EndWaveIfNeededTotal.Start();
        EndWaveIfNeeded();
        ConsoleStopwatch("EndWaveIfNeeded", EndWaveIfNeededOneShot, EndWaveIfNeededTotal);

        // バトルを終了する
        EndBattleIfNeededOneShot.Start();
        EndBattleIfNeededTotal.Start();
        EndBattleIfNeeded(isTurnEnd);
        ConsoleStopwatch("EndBattleIfNeeded", EndBattleIfNeededOneShot, EndBattleIfNeededTotal);
    }

    private void StartBattleIfNeeded() {
        // ウェーブが0じゃなければスキップ
        if (currentWaveCount > 0) return;

        // バトル開始ログの差し込み
        AddStartBattleLog();

        // バトル開始時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnBattleStart, GetAllMonsterList().OrderByDescending(m => m.currentSpeed()).Select(m => m.index).ToList());
    }

    // 通常アクション実行者を取得
    // いなければnullを返す
    private BattleMonsterIndex GetNormalActioner() {
        // プレイヤーと敵のモンスターを合成したリストを取得
        var allMonsterList = GetAllMonsterList();

        // 次のアクション実行者を取得
        var actioner = allMonsterList.Where(b => !b.isActed && !b.isDead).OrderByDescending(b => b.currentSpeed()).ThenBy(_ => Guid.NewGuid()).FirstOrDefault();

        // アクション実行者を設定
        return actioner?.index;
    }

    // 通常アクション実行者のアクションタイプを取得
    private BattleActionType GetNormalActionerActionType(BattleMonsterIndex monsterIndex) {
        var battleMonster = GetBattleMonster(monsterIndex);
        return battleMonster.currentEnergy >= ConstManager.Battle.MAX_ENERGY_VALUE ? BattleActionType.UltimateSkill : BattleActionType.NormalSkill;
    }

    private Stopwatch StartActionOneShot = new Stopwatch();
    private Stopwatch StartActionTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeActionStartOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeActionStartTotal = new Stopwatch();
    private Stopwatch GetBeDoneMonsterIndexListOneShot = new Stopwatch();
    private Stopwatch GetBeDoneMonsterIndexListTotal = new Stopwatch();
    private Stopwatch ExecuteActionOneShot = new Stopwatch();
    private Stopwatch ExecuteActionTotal = new Stopwatch();
    private Stopwatch AddSkillEffectFailedOfProbabilityMissLogOneShot = new Stopwatch();
    private Stopwatch AddSkillEffectFailedOfProbabilityMissLogTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeNormalSkillEndOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeNormalSkillEndTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeUltimateSkillEndOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeUltimateSkillEndTotal = new Stopwatch();
    private Stopwatch ExecuteDieIfNeededOneShot = new Stopwatch();
    private Stopwatch ExecuteDieIfNeededTotal = new Stopwatch();
    private Stopwatch EndActionOneShot = new Stopwatch();
    private Stopwatch EndActionTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeActionEndSuccessOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededOnMeActionEndSuccessTotal = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededEveryTimeEndOneShot = new Stopwatch();
    private Stopwatch ExecuteTriggerSkillIfNeededEveryTimeEndTotal = new Stopwatch();

    // アクション実行者とアクション内容を受け取りアクションを実行する
    private void StartActionStream(BattleMonsterIndex actionMonsterIndex, BattleActionType actionType, BattleConditionInfo battleCondition, List<BattleSkillEffectMI> battleSkillEffectList, TriggerSkillData triggerSkillData) {
        // 10分以上たっていたらエラー発生
        if (GetBattleLogListStopwatch.Elapsed.Minutes >= 10) {
            throw new Exception(GetBatleLogListStopwatchErrorMessage);
        }

        // アクションを開始する
        StartActionOneShot.Start();
        StartActionTotal.Start();
        StartAction(actionMonsterIndex, actionType, battleCondition);
        ConsoleStopwatch("StartAction", StartActionOneShot, StartActionTotal);

        // 各効果の実行
        var battleConditionId = battleCondition != null ? battleCondition.battleConditionId : 0;
        var skillGuid = Guid.NewGuid().ToString();
        battleSkillEffectList.ForEach((battleSkillEffect, index) => {
            if (battleSkillEffect.isActive && IsValidActivateCondition(actionMonsterIndex, battleSkillEffect.skillEffect.doMonsterActivateConditionType, battleSkillEffect.skillEffect.doMonsterActivateConditionValue, battleConditionId)) {
                var skillEffect = battleSkillEffect.skillEffect;

                // アクションの対象を選択する
                GetBeDoneMonsterIndexListOneShot.Start();
                GetBeDoneMonsterIndexListTotal.Start();
                var beDoneMonsterIndexList = GetBeDoneMonsterIndexList(actionMonsterIndex, skillEffect, skillGuid, index, actionType, battleCondition, triggerSkillData);
                ConsoleStopwatch("GetBeDoneMonsterIndexList", GetBeDoneMonsterIndexListOneShot, GetBeDoneMonsterIndexListTotal);

                // アクション処理を実行する
                ExecuteActionOneShot.Start();
                ExecuteActionTotal.Start();
                ExecuteAction(actionMonsterIndex, actionType, beDoneMonsterIndexList, skillGuid, skillEffect, index, battleCondition, triggerSkillData);
                ConsoleStopwatch("ExecuteAction", ExecuteActionOneShot, ExecuteActionTotal);
            }
        });

        // ウルトを発動したとき
        if (actionType == BattleActionType.UltimateSkill) {
            var targetTriggerSkillData = new TriggerSkillData() { battleMonsterIndex = actionMonsterIndex, battleActionType = actionType, skillGuid = skillGuid, skillEffectIndex = 0 };
            // 味方
            if (actionMonsterIndex.isPlayer) {
                var playerBattleMonsterIndexList = GetAllMonsterList().Where(m => m.index.isPlayer).Select(m => m.index).ToList();
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.AfterAllyUltimateSkill, playerBattleMonsterIndexList, 0, actionMonsterIndex, actionType, 0, targetTriggerSkillData);
            } else {
                var enemyBattleMonsterIndexList = GetAllMonsterList().Where(m => !m.index.isPlayer).Select(m => m.index).ToList();
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.AfterAllyUltimateSkill, enemyBattleMonsterIndexList, 0, actionMonsterIndex, actionType, 0, targetTriggerSkillData);
            }
        }

        // 死亡処理を実行
        ExecuteDieIfNeededOneShot.Start();
        ExecuteDieIfNeededTotal.Start();
        ExecuteDieIfNeeded();
        ConsoleStopwatch("ExecuteDieIfNeeded", ExecuteDieIfNeededOneShot, ExecuteDieIfNeededTotal);

        // アクションを終了する
        EndActionOneShot.Start();
        EndActionTotal.Start();
        EndAction(actionMonsterIndex, actionType);
        ConsoleStopwatch("EndAction", EndActionOneShot, EndActionTotal);
    }

    private void ActionFailed(BattleMonsterIndex actionMonsterIndex, BattleActionType actionType) {
        if (actionType == BattleActionType.NormalSkill || actionType == BattleActionType.UltimateSkill) {
            // 通常攻撃あるいはウルトの時にのみ行動済みフラグを立てる
            var battleMonster = GetBattleMonster(actionMonsterIndex);
            battleMonster.isActed = true;
        }

        // アクション失敗ログの差し込み
        AddActionFailedLog(actionMonsterIndex);
    }

    /// <summary>
    /// 状態異常のターンを経過させる
    /// </summary>
    private void ProgressBattleConditionTurnIfNeeded(BattleMonsterIndex battleMonsterIndex) {
        var isRemoved = false;
        var isProgress = false;
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        battleMonster.battleConditionList.ForEach(battleCondition => {
            // 継続ターンがあるものに関しては残りターンをデクリメント
            if (battleCondition.remainingTurnNum > 0) {
                battleCondition.remainingTurnNum--;
                isProgress = true;
                if (battleCondition.remainingTurnNum == 0) {
                    // 解除出来たら解除時状態異常効果を発動
                    isRemoved = true;
                }
            }
        });

        // 状態異常のターンが一つも進行しなければ何もしない
        if (!isProgress) return;

        var beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>() { new BeDoneBattleMonsterData() { battleMonsterIndex = battleMonsterIndex, battleConditionList = battleMonster.battleConditionList } };

        // 状態異常ターン進行ログの差し込み
        AddProgressBattleConditionTurnLog(beDoneBattleMonsterDataList);

        // 何も解除されなかったら何もしない
        if (!isRemoved) return;

        // 状態異常解除前ログの差し込み
        AddTakeBattleConditionRemoveBeforeLog(beDoneBattleMonsterDataList.Clone(), "", BattleActionType.ProgressBattleConditionTurn, 0);

        // ターンが切れている状態異常を削除する
        var i = 0;
        while (battleMonster.battleConditionList.Any(battleCondition => battleCondition.remainingTurnNum == 0) && i < 100) {
            i++;
            var guid = battleMonster.battleConditionList.First(battleCondition => battleCondition.remainingTurnNum == 0).guid;
            RemoveBattleCondition(battleMonster.index, guid);
        }

        if (i == 100) {
            UnityEngine.Debug.LogError("ProgressBattleConditionTurnIfNeeded Error");
        }

        battleMonster = GetBattleMonster(battleMonsterIndex);
        beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>() { new BeDoneBattleMonsterData() { battleMonsterIndex = battleMonsterIndex, battleConditionList = battleMonster.battleConditionList } };

        // 状態異常解除後ログの差し込み
        AddTakeBattleConditionRemoveAfterLog(beDoneBattleMonsterDataList.Clone(), "", BattleActionType.ProgressBattleConditionTurn, 0);
    }

    /// <summary>
    /// ウェーブ進行が必要ならウェーブを進行させる
    /// ウェーブ進行したか否かを返す
    /// </summary>
    private bool MoveWaveIfNeeded() {
        // 敵が全滅していたら実行、残っていたらスキップ
        if (enemyBattleMonsterList.Any(m => !m.isDead)) return false;

        // 現在が最終ウェーブであればスキップ
        if (currentWaveCount >= quest.questMonsterListByWave.Count) return false;

        // ウェーブ数をインクリメント
        currentWaveCount++;

        // 敵モンスターデータを更新
        RefreshEnemyBattleMonsterList(currentWaveCount);

        // ウェーブ進行ログの差し込み
        AddMoveWaveLog();

        // ウェーブ開始時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnWaveStart, GetAllMonsterList().OrderByDescending(m => m.currentSpeed()).Select(m => m.index).ToList());

        return true;
    }

    private void MoveTurnIfNeeded(bool isForce) {
        // すべてのモンスターが行動済みかつ0ターン目でなければ実行そうでなければスキップ
        if (((playerBattleMonsterList.Any(b => !b.isActed && !b.isDead) || enemyBattleMonsterList.Any(b => !b.isActed && !b.isDead)) && currentTurnCount > 0) && !isForce) return;

        // ターン数をインクリメント
        currentTurnCount++;

        // すべてのモンスターの行動済みフラグをもどす
        var allMonsterList = GetAllMonsterList();
        allMonsterList.ForEach(m => m.isActed = false);

        // ターン進行ログの差し込み
        AddMoveTurnLog();
    }

    private void StartAction(BattleMonsterIndex monsterIndex, BattleActionType actionType, BattleConditionInfo battleCondition) {
        // アクション開始ログの差し込み
        AddStartActionLog(monsterIndex, actionType, battleCondition);
    }

    private void StartActionAnimation(BattleMonsterIndex monsterIndex, BattleActionType actionType, BattleConditionInfo battleCondition) {
        // アクションアニメーション開始ログの差し込み
        AddStartActionAnimationLog(monsterIndex, actionType, battleCondition);
    }

    private void ExecuteAction(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterIndexDataSet> beDoneMonsterIndexDataSetList, string skillGuid, SkillEffectMI skillEffect, int skillEffectIndex, BattleConditionInfo battleCondition, TriggerSkillData triggerSkillData) {
        // スキル対象決定ログの差し込み
        AddSetSkillTargetLog(doMonsterIndex, skillGuid, actionType, skillEffectIndex, beDoneMonsterIndexDataSetList.Select(d => d.battleMonsterIndex).ToList(), battleCondition);

        // スキル効果の発動確率判定
        // 発動確率が0の場合は直前のスキル効果要素の発動状態を参照
        var isExecutedBeforeEffect = battleLogList.Where(l => l.skillGuid == skillGuid && l.skillEffectIndex == skillEffectIndex - 1).Any(log => log.type == BattleLogType.StartSkillEffect);
        beDoneMonsterIndexDataSetList.ForEach(d => {
            if (d.isSkillTarget) {
                var isExecute = (skillEffect.activateProbability > 0 && ExecuteProbability(skillEffect, doMonsterIndex, d.battleMonsterIndex)) || (skillEffect.activateProbability <= 0 && isExecutedBeforeEffect);
                d.isMissed = !isExecute;
            }
        });
        var missedBeDoneMonsterDataList = beDoneMonsterIndexDataSetList.Where(d => d.isMissed).Select(d => new BeDoneBattleMonsterData() {
            battleMonsterIndex = d.battleMonsterIndex,
            isMissed = true,
        }).ToList();
        if (missedBeDoneMonsterDataList.Any()) {
            // 確率による失敗ログの追加
            AddSkillEffectFailedOfProbabilityMissLogOneShot.Start();
            AddSkillEffectFailedOfProbabilityMissLogTotal.Start();
            AddSkillEffectFailedOfProbabilityMissLog(doMonsterIndex, missedBeDoneMonsterDataList, actionType, skillEffectIndex, null);
            ConsoleStopwatch("AddSkillEffectFailedOfProbabilityMissLog", AddSkillEffectFailedOfProbabilityMissLogOneShot, AddSkillEffectFailedOfProbabilityMissLogTotal);
        }

        // 対象モンスターが存在しない場合はなにもしない
        var beDoneMonsterIndexList = beDoneMonsterIndexDataSetList.Where(d => d.isSkillTarget && !d.isMissed).Select(d => d.battleMonsterIndex).ToList();
        if (!beDoneMonsterIndexList.Any()) return;

        var allMonsterList = GetAllMonsterList();
        var beDoneMonsterList = allMonsterList.Where(m => beDoneMonsterIndexList.Any(index => index.IsSame(m.index))).ToList();

        // スキル効果ログの差し込み
        AddStartSkillEffectLog(doMonsterIndex, skillGuid, actionType, skillEffectIndex, beDoneMonsterIndexList, battleCondition);

        if (triggerSkillData != null) {
            // トリガースキルログの差し込み
            AddTriggerSkillLog(doMonsterIndex, skillGuid, actionType, skillEffectIndex, beDoneMonsterIndexList, battleCondition, triggerSkillData);
        }

        // スキル効果の実行
        var skillType = skillEffect.type;
        switch (skillType) {
            case SkillType.Attack:
                ExecuteAttack(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.Heal:
                ExecuteHeal(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.ConditionAdd:
                ExecuteBattleConditionAdd(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.ConditionRemove:
                ExecuteBattleConditionRemove(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.Revive:
                ExecuteRevive(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.EnergyUp:
                ExecuteEnergyUp(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.EnergyDown:
                ExecuteEnergyDown(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.Status:
                ExecuteStatus(doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.Damage:
                ExecuteDamage(SkillType.Damage, doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            case SkillType.WithoutFactorDamage:
                ExecuteDamage(SkillType.WithoutFactorDamage, doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
                break;

            default:
                break;
        }
    }

    private void ExecuteAttack(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        // アタックアニメーションを実行
        StartActionAnimation(doMonsterIndex, actionType, battleCondition);

        ExecuteDamage(SkillType.Attack, doMonsterIndex, actionType, beDoneMonsterList, skillEffect, skillGuid, skillEffectIndex, battleCondition);
    }

    private void ExecuteDamage(SkillType skilType, BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        // スキル効果処理
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            var actionValue = GetActionValue(doMonsterIndex, m.index, skillEffect, actionType, skillGuid, skillEffectIndex, battleCondition);

            // 攻撃してきたモンスターの更新
            if (actionType == BattleActionType.NormalSkill || actionType == BattleActionType.UltimateSkill) m.currentBeDoneAttackedMonsterIndex = doMonsterIndex;

            // 効果量を反映
            // 攻撃でも回復でも加算
            var effectValue = m.ChangeHp(actionValue.value);

            // スコア計算
            AddScore(doMonsterIndex, m.index, skilType, effectValue);

            // エネルギーを上昇させる
            if (actionType != BattleActionType.BattleCondition) m.ChangeEnergy(ConstManager.Battle.ENERGY_RISE_VALUE_ON_TAKE_DAMAGE);

            return new BeDoneBattleMonsterData() {
                battleMonsterIndex = m.index,
                hpChanges = actionValue.value,
                isCritical = actionValue.isCritical,
                isBlocked = actionValue.isBlocked,
            };
        }).ToList();

        // 被ダメージログの差し込み
        AddTakeDamageLog(doMonsterIndex, beDoneMonsterDataList, skillEffect.skillFxId, skillGuid, actionType, skillEffectIndex, battleCondition);

        // トリガースキルを発動する
        var doBattleMonster = GetBattleMonster(doMonsterIndex);
        var beDoneBattleMonsterIndexList = beDoneMonsterDataList.Select(d => d.battleMonsterIndex).ToList();
        var beDoneBattleMonsterDataWithoutDoMonsterList = beDoneMonsterDataList.Where(d => !d.battleMonsterIndex.IsSame(doMonsterIndex)).ToList();
        var beDoneBattleMonsterIndexWithoutDoMonsterList = beDoneBattleMonsterDataWithoutDoMonsterList.Select(d => d.battleMonsterIndex).ToList();
        var allBattleMonsterList = GetAllMonsterList();
        var playerBattleMonsterIndexList = allBattleMonsterList.Where(m => m.index.isPlayer).Select(m => m.index).ToList();
        var enemyBattleMonsterIndexList = allBattleMonsterList.Where(m => !m.index.isPlayer).Select(m => m.index).ToList();
        var existsPlayer = beDoneBattleMonsterIndexList.Any(i => i.isPlayer);
        var existsEnemy = beDoneBattleMonsterIndexList.Any(i => !i.isPlayer);
        var triggerSkillData = new TriggerSkillData() { battleMonsterIndex = doMonsterIndex, battleActionType = actionType, skillGuid = skillGuid, skillEffectIndex = skillEffectIndex };

        // 自身がクリティカルを発動した時
        if (beDoneMonsterDataList.Any(d => d.isCritical)) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeExecuteCriticcalAfter, doMonsterIndex, 0, doMonsterIndex, actionType, 0, triggerSkillData);

        // 自身以外からクリティカルを受けた時
        beDoneBattleMonsterDataWithoutDoMonsterList.Where(d => d.isCritical).ToList().ForEach(d => {
            // 自身がクリティカルを受けた時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeAttackedCritical, d.battleMonsterIndex, 0, doMonsterIndex, actionType, 0, triggerSkillData);
        });

        // 自身以外からブロックした時
        beDoneBattleMonsterDataWithoutDoMonsterList.Where(d => d.isBlocked).ToList().ForEach(d => {
            // 自身がブロックした時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBlocked, d.battleMonsterIndex, 0, doMonsterIndex, actionType, 0, triggerSkillData);

            // 自身が指定回数ブロックしたとき
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBlocked, d.battleMonsterIndex, GetBlockCount(d.battleMonsterIndex), doMonsterIndex, actionType, 0, triggerSkillData);

            // 敵がブロックした時
            if (d.battleMonsterIndex.isPlayer) {
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBlocked, enemyBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0, triggerSkillData);
            } else {
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBlocked, playerBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0, triggerSkillData);
            }
        });

        if (skillEffect.type == SkillType.Attack) {
            // 攻撃した時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeAttacked, doMonsterIndex, 0, doMonsterIndex, actionType, 0, triggerSkillData);
        }

        if (skillEffect.type == SkillType.Attack) {
            // 自身以外から特定状態異常の相手に攻撃されたとき
            GetBattleMonster(doMonsterIndex).battleConditionList.ForEach(c => {
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeAttackedBattleCondition, beDoneBattleMonsterIndexWithoutDoMonsterList, (int)c.battleConditionId, doMonsterIndex, actionType, 0, triggerSkillData);
            });

            // 自身以外から攻撃された時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeAttacked, beDoneBattleMonsterIndexWithoutDoMonsterList, 0, doMonsterIndex, actionType, 0, triggerSkillData);
        }

        // 自身以外からの特定ステータスの高低によるトリガー
        beDoneBattleMonsterIndexWithoutDoMonsterList.ForEach(index => {
            foreach (BattleMonsterStatusType type in Enum.GetValues(typeof(BattleMonsterStatusType))) {
                if (doBattleMonster.GetStatus(type) >= GetBattleMonster(index).GetStatus(type)) {
                    ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeAttackedLowerStatus, index, (int)type, doMonsterIndex, actionType, 0, triggerSkillData);
                }
            }
        });

        // 自身以外から通常攻撃またはウルトを受けたとき
        if (actionType == BattleActionType.NormalSkill || actionType == BattleActionType.UltimateSkill) {
            // 反撃系はトリガー発動の要因も渡す
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeExecutedNormalOrUltimateSkill, beDoneBattleMonsterIndexWithoutDoMonsterList, 0, doMonsterIndex, actionType, 0, triggerSkillData);
        }

        // 自身以外から通常攻撃を受けたとき
        if (actionType == BattleActionType.NormalSkill) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeExecutedNormalSkill, beDoneBattleMonsterIndexWithoutDoMonsterList, 0, doMonsterIndex, actionType, 0, triggerSkillData);

        // ダメージを受けたとき
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeTakeDamageEnd, beDoneBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0, triggerSkillData);
    }

    private void ExecuteHeal(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            var actionValue = GetActionValue(doMonsterIndex, m.index, skillEffect, actionType, skillGuid, skillEffectIndex, battleCondition);

            // 効果量を反映
            // 攻撃でも回復でも加算
            var effectValue = m.ChangeHp(actionValue.value);

            // スコア計算
            AddScore(doMonsterIndex, m.index, SkillType.Heal, effectValue);

            return new BeDoneBattleMonsterData() {
                battleMonsterIndex = m.index,
                hpChanges = actionValue.value,
            };
        }).ToList();

        // 被回復ログの差し込み
        AddTakeHealLog(doMonsterIndex, beDoneMonsterDataList, skillEffect.skillFxId, skillGuid, actionType, skillEffectIndex, battleCondition);
    }

    private void ExecuteBattleConditionAdd(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        var battleConditionMB = this.battleConditionList.First(m => m.id == skillEffect.battleConditionId);

        var beDoneMonsterDataList = beDoneMonsterList.Select(battleMonster => {
            // 状態異常を付与
            var battleConditionList = new List<BattleConditionInfo>();
            var battleConditionInfo = AddBattleCondition(doMonsterIndex, battleMonster.index, skillEffect, battleConditionMB.id, actionType, skillGuid, skillEffectIndex);
            battleConditionList.Add(battleConditionInfo);

            return new BeDoneBattleMonsterData() {
                battleMonsterIndex = battleMonster.index,
                battleConditionList = battleConditionList,
            };
        }).ToList();

        // 状態異常付与ログの差し込み
        AddTakeBattleConditionAddLog(doMonsterIndex, beDoneMonsterDataList, skillEffect, skillGuid, actionType, skillEffectIndex, battleCondition);

        // 状態異常付与時トリガースキルを発動する
        var beAddedBattleMonsterDataList = beDoneMonsterDataList.Where(d => !d.isMissed).ToList();
        var allBattleMonsterList = GetAllMonsterList();
        var playerBattleMonsterIndexList = allBattleMonsterList.Where(m => m.index.isPlayer).Select(m => m.index).ToList();
        var enemyBattleMonsterIndexList = allBattleMonsterList.Where(m => !m.index.isPlayer).Select(m => m.index).ToList();
        var triggerSkillData = new TriggerSkillData() { battleMonsterIndex = doMonsterIndex, battleActionType = actionType, skillGuid = skillGuid, skillEffectIndex = skillEffectIndex };

        beAddedBattleMonsterDataList.ForEach(battleMonsterData => {
            var battleMonsterIndex = battleMonsterData.battleMonsterIndex;
            battleMonsterData.battleConditionList.ForEach(battleConditionInfo => {
                // 自身が付与された時
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeAddedBattleCondition, battleMonsterIndex, 0, doMonsterIndex, actionType, 0, triggerSkillData, battleConditionInfo.guid);
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnTargetBattleConditionAddedAndMeTurnActionEnd, battleMonsterIndex, 0, doMonsterIndex, actionType, 0, triggerSkillData, battleConditionInfo.guid);

                // 味方が付与された時
                if (battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyBeAddedBattleCondition, playerBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0, triggerSkillData, battleConditionInfo.guid);
                if (!battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyBeAddedBattleCondition, enemyBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0, triggerSkillData, battleConditionInfo.guid);

                // 敵が付与された時
                if (battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBeAddedBattleCondition, enemyBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0, triggerSkillData, battleConditionInfo.guid);
                if (!battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBeAddedBattleCondition, playerBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0, triggerSkillData, battleConditionInfo.guid);
            });
        });
    }

    private void ExecuteBattleConditionRemove(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        var battleConditionMB = battleConditionList.First(m => m.id == skillEffect.battleConditionId);

        var beforeBeDoneMonsterDataList = beDoneMonsterList
            .Select(battleMonster => new BeDoneBattleMonsterData() { battleMonsterIndex = battleMonster.index, battleConditionList = battleMonster.battleConditionList })
            .ToList()
            .Clone();

        var beDoneMonsterDataList = beDoneMonsterList
            .Where(battleMonster => {
                var isRemoved = false;

                var battleConditionInfo = battleMonster.battleConditionList.OrderBy(c => c.order).FirstOrDefault(c => c.grantorSkillEffect.battleConditionId == skillEffect.battleConditionId && c.grantorSkillEffect.canRemove);
                var isAll = skillEffect.removeBattleConsitionNum == 0;
                var count = 0;
                while (((isAll && battleConditionInfo != null) || (battleConditionInfo != null && count < skillEffect.removeBattleConsitionNum)) && count < 100) {
                    RemoveBattleCondition(battleMonster.index, battleConditionInfo.guid);

                    isRemoved = true;
                    count++;
                    battleConditionInfo = GetBattleMonster(battleMonster.index).battleConditionList.OrderBy(c => c.order).FirstOrDefault(c => c.grantorSkillEffect.battleConditionId == skillEffect.battleConditionId && c.grantorSkillEffect.canRemove);
                }

                if (count == 100) {
                    UnityEngine.Debug.LogError("ExecuteBattleConditionRemove Error");
                }

                return isRemoved;
            })
            .Select(battleMonster => new BeDoneBattleMonsterData() { battleMonsterIndex = battleMonster.index, battleConditionList = battleMonster.battleConditionList })
            .ToList()
            .Clone();

        if (beDoneMonsterDataList.Any()) {
            // 状態異常解除前ログの差し込み
            AddTakeBattleConditionRemoveBeforeLog(beforeBeDoneMonsterDataList, skillGuid, actionType, skillEffectIndex);

            // 状態異常解除後ログの差し込み
            AddTakeBattleConditionRemoveAfterLog(beDoneMonsterDataList, skillGuid, actionType, skillEffectIndex);
        }
    }

    private void ExecuteRevive(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            // 蘇生時は蘇生後のHPが返ってくる
            var hp = GetActionValue(doMonsterIndex, m.index, skillEffect, actionType, skillGuid, skillEffectIndex, battleCondition);

            // 効果量を反映
            var effectValue = m.ChangeHp(hp.value);

            // 死亡フラグを折る
            m.isDead = false;

            return new BeDoneBattleMonsterData() {
                battleMonsterIndex = m.index,
                hpChanges = effectValue,
            };
        }).ToList();

        // 蘇生ログの差し込み
        AddTakeReviveLog(doMonsterIndex, beDoneMonsterDataList, skillEffect, skillGuid, actionType, skillEffectIndex, battleCondition);
    }

    private void ExecuteEnergyUp(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            // アクション値を取得
            var actionValue = GetActionValue(doMonsterIndex, m.index, skillEffect, actionType, skillGuid, skillEffectIndex, battleCondition);

            // 効果量を反映
            var effectValue = m.ChangeEnergy(actionValue.value);

            return new BeDoneBattleMonsterData() {
                battleMonsterIndex = m.index,
                energyChanges = effectValue,
            };
        }).ToList();

        // エネルギー上昇ログの差し込み
        AddEnergyUpLog(doMonsterIndex, beDoneMonsterDataList, skillEffect.skillFxId, skillGuid, actionType, skillEffectIndex, battleCondition);
    }

    private void ExecuteEnergyDown(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            // アクション値を取得
            var actionValue = GetActionValue(doMonsterIndex, m.index, skillEffect, actionType, skillGuid, skillEffectIndex, battleCondition);

            // 効果量を反映
            var effectValue = m.ChangeEnergy(actionValue.value);

            return new BeDoneBattleMonsterData() {
                battleMonsterIndex = m.index,
                energyChanges = effectValue,
            };
        }).ToList();

        // エネルギー上昇ログの差し込み
        AddEnergyDownLog(doMonsterIndex, beDoneMonsterDataList, skillEffect.skillFxId, skillGuid, actionType, skillEffectIndex, battleCondition);
    }

    private void ExecuteStatus(BattleMonsterIndex doMonsterIndex, BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleConditionInfo battleCondition) {
        // ステータスの変更
        var battleConditionMB = battleConditionList.First(m => m.id == skillEffect.battleConditionId);
        var value = battleConditionMB.buffType == BuffType.Buff ? skillEffect.value : -skillEffect.value;
        var beDoneMonsterDataList = beDoneMonsterList.Select(battleMonster => {
            switch (battleConditionMB.targetBattleMonsterStatusType) {
                // 実数値系のステータスは実数値を加算する
                case BattleMonsterStatusType.Hp:
                    battleMonster.maxHp += (int)(value * battleMonster.maxHp / 100.0f);
                    battleMonster.currentHp = battleMonster.maxHp;
                    break;

                case BattleMonsterStatusType.Attack:
                    battleMonster.baseAttack += (int)(value * battleMonster.baseAttack / 100.0f);
                    break;

                case BattleMonsterStatusType.Defense:
                    battleMonster.baseDefense += (int)(value * battleMonster.baseDefense / 100.0f);
                    break;

                case BattleMonsterStatusType.Armor:
                    battleMonster.baseArmor += (int)(value * battleMonster.baseArmor / 100.0f);
                    break;
                // 実数値系の中でもスピードだけはそのまま加算する
                case BattleMonsterStatusType.Speed:
                    battleMonster.baseSpeed += value;
                    break;
                // パーセント系のステータスはそのまま加算する
                case BattleMonsterStatusType.Sheild:
                    battleMonster.baseSheild += value;
                    break;

                case BattleMonsterStatusType.UltimateSkillDamageRate:
                    battleMonster.baseUltimateSkillDamageRate += value;
                    break;

                case BattleMonsterStatusType.BlockRate:
                    battleMonster.baseBlockRate += value;
                    break;

                case BattleMonsterStatusType.CriticalRate:
                    battleMonster.baseCriticalRate += value;
                    break;

                case BattleMonsterStatusType.CriticalDamageRate:
                    battleMonster.baseCriticalDamage += value;
                    break;

                case BattleMonsterStatusType.BuffResistRate:
                    battleMonster.baseBuffResistRate += value;
                    break;

                case BattleMonsterStatusType.DebuffResistRate:
                    battleMonster.baseDebuffResistRate += value;
                    break;

                case BattleMonsterStatusType.DamageResistRate:
                    battleMonster.baseDamageResistRate += value;
                    break;

                case BattleMonsterStatusType.LuckDamageRate:
                    battleMonster.baseLuckDamageRate += value;
                    break;

                case BattleMonsterStatusType.HolyDamageRate:
                    battleMonster.baseHolyDamageRate += value;
                    break;

                case BattleMonsterStatusType.EnergyUpRate:
                    battleMonster.baseEnergyUpRate += value;
                    break;

                case BattleMonsterStatusType.HealedRate:
                    battleMonster.baseHealedRate += value;
                    break;

                case BattleMonsterStatusType.AttackAccuracyRate:
                    battleMonster.baseAttackAccuracy += value;
                    break;

                case BattleMonsterStatusType.ArmorBreakRate:
                    battleMonster.baseArmorBreakRate += value;
                    break;

                case BattleMonsterStatusType.HealingRate:
                    battleMonster.baseHealingRate += value;
                    break;

                default:
                    var battleConditionInfo = GetBattleCondition(doMonsterIndex, battleMonster.index, skillEffect, skillEffect.battleConditionId, 0, actionType, skillGuid, skillEffectIndex);
                    battleMonster.baseBattleConditionList.Add(battleConditionInfo);
                    break;
            }

            return new BeDoneBattleMonsterData() {
                battleMonsterIndex = battleMonster.index,
            };
        }).ToList();

        // ステータス変化ログの差し込み
        AddTakeStatusChangeLog(doMonsterIndex, beDoneMonsterDataList, skillEffect, value, skillGuid, actionType, skillEffectIndex, battleCondition);
    }

    private void ExecuteDieIfNeeded() {
        var allBattleMonsterList = GetAllMonsterList();
        var dieBattleMonsterList = allBattleMonsterList.Where(m => !m.isDead && m.currentHp <= 0).ToList();

        // 倒れたモンスターがいなければ何もしない
        if (!dieBattleMonsterList.Any()) return;

        // 死亡判定フラグを立てる
        dieBattleMonsterList.ForEach(m => m.isDead = true);

        // ログに渡す用のリストを作成
        var beDoneBattleMonsterDataList = dieBattleMonsterList.Clone().Select(m => new BeDoneBattleMonsterData() { battleMonsterIndex = m.index }).ToList();

        // 死亡ログを差し込む
        AddDieLog(beDoneBattleMonsterDataList);

        // 戦闘不能時トリガースキルを発動する
        var existsPlayer = dieBattleMonsterList.Any(m => m.index.isPlayer);
        var existsEnemy = dieBattleMonsterList.Any(m => !m.index.isPlayer);
        var playerBattleMonsterIndexList = allBattleMonsterList.Where(m => m.index.isPlayer).Select(m => m.index).ToList();
        var enemyBattleMonsterIndexList = allBattleMonsterList.Where(m => !m.index.isPlayer).Select(m => m.index).ToList();

        dieBattleMonsterList.Select(m => m.index).ToList().ForEach(battleMonsterIndex => {
            // 自分が戦闘不能時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeDeadEnd, battleMonsterIndex);

            // 敵が戦闘不能時
            if (battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyDead, enemyBattleMonsterIndexList);
            if (!battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyDead, playerBattleMonsterIndexList);
        });
    }

    private void EndAction(BattleMonsterIndex doMonsterIndex, BattleActionType actionType) {
        var battleMonster = GetBattleMonster(doMonsterIndex);

        // エネルギー計算処理を行う
        switch (actionType) {
            case BattleActionType.NormalSkill:
                battleMonster.ChangeEnergy(ConstManager.Battle.ENERGY_RISE_VALUE_ON_ACT);
                battleMonster.isActed = true;
                break;

            case BattleActionType.UltimateSkill:
                battleMonster.currentEnergy = 0;
                battleMonster.isActed = true;
                break;

            default:
                break;
        }

        // アクション終了ログを差し込む
        AddEndActionLog(doMonsterIndex);
    }

    /// <summary>
    /// 現在のターンが終了すればターン終了時処理を実行
    /// ターンが終了するか否かを返す
    /// </summary>
    private bool EndTurnIfNeeded() {
        // 一体でも未行動のモンスターが存在すれば実行しない
        var isNotEnd = GetAllMonsterList().Any(m => !m.isActed && !m.isDead);
        if (isNotEnd) return false; ;

        // ターン終了ログを差し込む
        AddEndTurnLog();

        // ターン終了時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnTurnEnd, GetAllMonsterList().Select(m => m.index).ToList());

        return true;
    }

    private void EndWaveIfNeeded() {
        // 敵に戦えるモンスターが一体でもいれば何もしない
        var existsEnemy = enemyBattleMonsterList.Any(m => !m.isDead);
        if (existsEnemy) return;

        // ウェーブ終了ログを差し込む
        AddEndWaveLog();

        // Wave毎の敵情報リストの更新
        enemyBattleMonsterListByWave.Add(enemyBattleMonsterList);
    }

    private void EndBattleIfNeeded(bool isTurnEnd) {
        // 味方が全滅あるいは最終ウェーブで敵が全滅ならバトル終了
        var existsAlly = playerBattleMonsterList.Any(m => !m.isDead);
        var existsEnemy = enemyBattleMonsterList.Any(m => !m.isDead);
        var isEndBattle = !existsAlly || (!existsEnemy && currentWaveCount >= quest.questMonsterListByWave.Count);

        // バトルが終了していなければ続行、バトル終了かつ味方が残っていれば勝利
        currentWinOrLose =
            !isEndBattle ? WinOrLose.Continue
            : existsAlly ? WinOrLose.Win
            : WinOrLose.Lose;

        // このタイミングでターンが終了しかつ現在のターンが上限ターンかつ決着がついていなければ敗北
        if (isTurnEnd && currentWinOrLose == WinOrLose.Continue && currentTurnCount >= quest.limitTurnNum) currentWinOrLose = WinOrLose.Lose;

        // バトル続行なら何もしない
        if (currentWinOrLose == WinOrLose.Continue) return;

        // Wave毎の敵情報リストの更新
        // 敵が全滅していない場合はそのWaveの敵情報リストは未更新なのでここで更新する
        if (existsEnemy) enemyBattleMonsterListByWave.Add(enemyBattleMonsterList);

        // バトル終了ログの差し込み
        AddEndBattleLog();
    }

    /// <summary>
    /// 状態異常を確認して行動できるかをチェック
    /// </summary>
    private bool CanAction(BattleMonsterIndex battleMonsterIndex, BattleActionType actionType) {
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        switch (actionType) {
            case BattleActionType.NormalSkill:
                return !battleMonster.battleConditionList.Any(c => {
                    var battleCondition = battleConditionList.First(m => m.id == c.battleConditionId);
                    return battleCondition.battleConditionType == BattleConditionType.NormalAndUltimateAndPassiveSkillUnavailable || battleCondition.battleConditionType == BattleConditionType.NormalSkillUnavailable;
                });
            case BattleActionType.UltimateSkill:
                return !battleMonster.battleConditionList.Any(c => {
                    var battleCondition = battleConditionList.First(m => m.id == c.battleConditionId);
                    return battleCondition.battleConditionType == BattleConditionType.NormalAndUltimateAndPassiveSkillUnavailable || battleCondition.battleConditionType == BattleConditionType.UltimateSkillUnavailable;
                });
            case BattleActionType.PassiveSkill:
                return !battleMonster.battleConditionList.Any(c => {
                    var battleCondition = battleConditionList.First(m => m.id == c.battleConditionId);
                    return battleCondition.battleConditionType == BattleConditionType.NormalAndUltimateAndPassiveSkillUnavailable || battleCondition.battleConditionType == BattleConditionType.PassiveSkillUnavailable;
                });
            case BattleActionType.BattleCondition:
            default:
                return true;
        }
    }

    /// <summary>
    /// 状態異常情報を付与する
    /// </summary>
    private BattleConditionInfo AddBattleCondition(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect, long battleConditionId, BattleActionType actionType, string skillGuid, int skillEffectIndex) {
        var beDoneBattleMonster = GetBattleMonster(beDoneMonsterIndex);
        var battleCondition = GetBattleCondition(doMonsterIndex, beDoneMonsterIndex, skillEffect, battleConditionId, beDoneBattleMonster.battleConditionCount, actionType, skillGuid, skillEffectIndex);

        // 状態異常を付与しカウントをインクリメント
        beDoneBattleMonster.battleConditionList.Add(battleCondition.Clone());
        beDoneBattleMonster.battleConditionCount++;

        return battleCondition.Clone();
    }

    /// <summary>
    /// 状態異常情報を作成して返す
    /// </summary>
    private BattleConditionInfo GetBattleCondition(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect, long battleConditionId, int order, BattleActionType actionType, string skillGuid, int skillEffectIndex) {
        var battleConditionMB = battleConditionList.First(m => m.id == battleConditionId);

        // スキルタイプはマスタのスキルタイプに上書きする
        skillEffect = skillEffect.Clone();
        skillEffect.type = battleConditionMB.skillEffect.type;

        var calculatedValue = battleConditionMB.battleConditionType == BattleConditionType.Action && battleConditionMB.actionValueType == BattleConditionActionValueType.Fixed ? GetActionValue(doMonsterIndex, beDoneMonsterIndex, skillEffect, actionType, skillGuid, skillEffectIndex, null).value : 0;
        var shieldValue = battleConditionMB.battleConditionType == BattleConditionType.Shield ? skillEffect.value : 0;

        var battleCondition = new BattleConditionInfo() {
            guid = Guid.NewGuid().ToString(),
            grantorBattleMonsterIndex = doMonsterIndex,
            battleConditionId = battleConditionMB.id,
            grantorSkillEffect = skillEffect,
            battleConditionSkillEffect = battleConditionMB.skillEffect,
            remainingTurnNum = skillEffect.durationTurnNum,
            actionValue = calculatedValue,
            shieldValue = shieldValue,
            order = order,
        };
        return battleCondition.Clone();
    }

    /// <summary>
    /// 状態異常を解除する
    /// guidでは特定できないからorderで
    /// </summary>
    private void RemoveBattleCondition(BattleMonsterIndex battleMonsterIndex, string guid) {
        var battleMonster = GetBattleMonster(battleMonsterIndex);

        // 状態異常を解除する
        battleMonster.battleConditionList = battleMonster.battleConditionList.Where(c => !c.grantorSkillEffect.canRemove || c.guid != guid).ToList();

        // 順序情報を更新する
        battleMonster.battleConditionList.OrderBy(c => c.order).ForEach((c, index) => c.order = index + 1);
    }

    private List<BattleMonsterInfo> GetAllMonsterList() {
        var allMonsterList = new List<BattleMonsterInfo>();
        allMonsterList.AddRange(playerBattleMonsterList);
        allMonsterList.AddRange(enemyBattleMonsterList);
        return allMonsterList;
    }

    private string GetSkillName(BattleMonsterInfo battleMonster, BattleActionType actionType, BattleConditionInfo battleCondition) {
        switch (actionType) {
            case BattleActionType.NormalSkill:
                return battleMonster.normalSkill.name;

            case BattleActionType.UltimateSkill:
                return battleMonster.ultimateSkill.name;

            case BattleActionType.PassiveSkill:
                return battleMonster.passiveSkill?.name ?? "-";

            case BattleActionType.BattleCondition:
                return battleConditionList.First(m => m.id == battleCondition.battleConditionId).name;

            default:
                return "";
        }
    }

    private List<SkillEffectMI> GetSkillEffectList(BattleMonsterIndex monsterIndex, BattleActionType actionType) {
        var battleMonster = GetBattleMonster(monsterIndex);
        switch (actionType) {
            case BattleActionType.NormalSkill:
                return battleMonster.normalSkill.effectList.Select(m => (SkillEffectMI)m).ToList();

            case BattleActionType.UltimateSkill:
                return battleMonster.ultimateSkill.effectList.Select(m => (SkillEffectMI)m).ToList();

            case BattleActionType.PassiveSkill:
                return battleMonster.passiveSkill.effectList.Select(m => (SkillEffectMI)m).ToList();

            default:
                return new List<SkillEffectMI>();
        }
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex monsterIndex) {
        if (monsterIndex.isPlayer) {
            return playerBattleMonsterList.First(battleMonster => battleMonster.index.IsSame(monsterIndex));
        } else {
            return enemyBattleMonsterList.First(battleMonster => battleMonster.index.IsSame(monsterIndex));
        }
    }

    private List<BattleMonsterIndexDataSet> GetBeDoneMonsterIndexList(BattleMonsterIndex doMonsterIndex, SkillEffectMI skillEffect, string skillGuid, int skillEffectIndex, BattleActionType actionType, BattleConditionInfo battleCondition, TriggerSkillData triggerSkillData = null) {
        var isDoMonsterPlayer = doMonsterIndex.isPlayer;
        var battleConditionId = battleCondition != null ? battleCondition.battleConditionId : 0;
        var allyBattleMonsterList = isDoMonsterPlayer ? this.playerBattleMonsterList : this.enemyBattleMonsterList;
        var enemyBattleMonsterList = isDoMonsterPlayer ? this.enemyBattleMonsterList : this.playerBattleMonsterList;
        allyBattleMonsterList = allyBattleMonsterList.Where(b => IsValidActivateCondition(b, skillEffect.activateConditionType, skillEffect.activateConditionValue, battleConditionId)).ToList();
        enemyBattleMonsterList = enemyBattleMonsterList.Where(b => IsValidActivateCondition(b, skillEffect.activateConditionType, skillEffect.activateConditionValue, battleConditionId)).ToList();

        var battleMonsterIndexList = new List<BattleMonsterIndex>();
        switch (skillEffect.skillTargetType) {
            case SkillTargetType.Myself:
                battleMonsterIndexList = allyBattleMonsterList.Where(m => m.index.IsSame(doMonsterIndex)).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAll:
                battleMonsterIndexList = allyBattleMonsterList.Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAll:
                battleMonsterIndexList = enemyBattleMonsterList.Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllRandom1:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(1).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllRandom2:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(2).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllRandom3:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(3).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllRandom4:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(4).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllRandom5:
                battleMonsterIndexList = allyBattleMonsterList.Shuffle().Take(5).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllRandom1:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(1).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllRandom2:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(2).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllRandom3:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(3).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllRandom4:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(4).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllRandom5:
                battleMonsterIndexList = enemyBattleMonsterList.Shuffle().Take(5).Select(b => b.index).ToList();
                break;

            case SkillTargetType.DoAttack:
                var doMonster = GetBattleMonster(doMonsterIndex);
                var isBeAttackedValid = doMonster.currentBeDoneAttackedMonsterIndex == null ? false : IsValidActivateCondition(doMonster.currentBeDoneAttackedMonsterIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue, battleConditionId);
                battleMonsterIndexList = isBeAttackedValid ? new List<BattleMonsterIndex>() { doMonster.currentBeDoneAttackedMonsterIndex } : new List<BattleMonsterIndex>();
                break;

            case SkillTargetType.BeAttacked: {
                    // 実際に攻撃したモンスターを取得するときはStartSkillEffectを参照する
                    var triggerSkillGuid = triggerSkillData != null ? triggerSkillData.skillGuid : "";
                    var triggerSkillEffectIndex = triggerSkillData != null ? triggerSkillData.skillEffectIndex : -1;
                    var targetLog = battleLogList.FirstOrDefault(log => log.type == BattleLogType.StartSkillEffect && log.skillGuid == triggerSkillGuid && log.skillEffectIndex == triggerSkillEffectIndex);
                    if (targetLog != null) {
                        return targetLog.beDoneBattleMonsterDataList.Select(d => {
                            return new BattleMonsterIndexDataSet() {
                                isSkillTarget = IsValidActivateCondition(d.battleMonsterIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue, battleConditionId),
                                battleMonsterIndex = d.battleMonsterIndex,
                            };
                        }).ToList();
                    }
                    break;
                }
            case SkillTargetType.AllyFrontAll: {
                    var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    battleMonsterIndexList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    break;
                }
            case SkillTargetType.AllyBackAll: {
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    battleMonsterIndexList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    break;
                }
            case SkillTargetType.EnemyFrontAll: {
                    var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    battleMonsterIndexList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackAll: {
                    var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    battleMonsterIndexList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    break;
                }
            case SkillTargetType.AllyMostFront:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.index.index).Take(1).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyMostFront:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.index.index).Take(1).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllHPLowest1:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(1).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllHPLowest2:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(2).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllHPLowest3:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(3).Select(b => b.index).ToList();
                break;

            case SkillTargetType.AllyAllHPLowest4:
                battleMonsterIndexList = allyBattleMonsterList.OrderBy(b => b.currentHp).Take(4).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllHPLowest1:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(1).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllHPLowest2:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(2).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllHPLowest3:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(3).Select(b => b.index).ToList();
                break;

            case SkillTargetType.EnemyAllHPLowest4:
                battleMonsterIndexList = enemyBattleMonsterList.OrderBy(b => b.currentHp).Take(4).Select(b => b.index).ToList();
                break;

            case SkillTargetType.JustBeforeElementTarget: {
                    var targetLog = battleLogList.FirstOrDefault(log => log.type == BattleLogType.SetSkillTarget && log.skillGuid == skillGuid && log.skillEffectIndex == skillEffectIndex - 1);
                    if (targetLog != null) {
                        return targetLog.beDoneBattleMonsterDataList.Select(d => {
                            return new BattleMonsterIndexDataSet() {
                                isSkillTarget = IsValidActivateCondition(d.battleMonsterIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue, battleConditionId),
                                battleMonsterIndex = d.battleMonsterIndex,
                            };
                        }).ToList();
                    }
                    break;
                }
            case SkillTargetType.AllyFrontRandom1: {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.AllyFrontRandom2: {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.AllyBackRandom1: {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.AllyBackRandom2: {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.AllyBackRandom3: {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(3).ToList();
                    break;
                }
            case SkillTargetType.EnemyFrontRandom1: {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.EnemyFrontRandom2: {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackRandom1: {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackRandom2: {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackRandom3: {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(3).ToList();
                    break;
                }
            case SkillTargetType.FirstElementTarget: {
                    // すでに対象にしたモンスター
                    // リストの一番最初の要素の効果の対象
                    var targetLog = battleLogList.FirstOrDefault(log => log.type == BattleLogType.SetSkillTarget && log.skillGuid == skillGuid && log.skillEffectIndex == 0);
                    if (targetLog != null) {
                        return targetLog.beDoneBattleMonsterDataList.Select(d => {
                            return new BattleMonsterIndexDataSet() {
                                isSkillTarget = IsValidActivateCondition(d.battleMonsterIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue, battleConditionId),
                                battleMonsterIndex = d.battleMonsterIndex,
                            };
                        }).ToList();
                    }
                    break;
                }
            case SkillTargetType.JustBeforeElementTargetOnlySuccess: {
                    // すでに対象にしたモンスター
                    // リストの直前の要素の効果の対象
                    // その要素の効果が成功した時のみ
                    var targetLog = battleLogList.FirstOrDefault(log => log.type == BattleLogType.StartSkillEffect && log.skillGuid == skillGuid && log.skillEffectIndex == skillEffectIndex - 1);
                    if (targetLog != null) {
                        return targetLog.beDoneBattleMonsterDataList.Select(d => {
                            return new BattleMonsterIndexDataSet() {
                                isSkillTarget = !d.isMissed && IsValidActivateCondition(d.battleMonsterIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue, battleConditionId),
                                battleMonsterIndex = d.battleMonsterIndex,
                            };
                        }).ToList();
                    }
                    break;
                }
            case SkillTargetType.None:
            default:
                battleMonsterIndexList = new List<BattleMonsterIndex>();
                break;
        }
        return battleMonsterIndexList.Select(i => new BattleMonsterIndexDataSet() {
            isSkillTarget = true,
            battleMonsterIndex = i,
        }).ToList();
    }

    private bool ExecuteProbability(SkillEffectMI skillEffect, BattleMonsterIndex doBattleMonsterIndex, BattleMonsterIndex beDoneBattleMonsterIndex) {
        var beDoneBattleMonster = GetBattleMonster(beDoneBattleMonsterIndex);
        if (skillEffect.type == SkillType.ConditionAdd) {
            var addedBattleCondition = battleConditionList.First(m => m.id == skillEffect.battleConditionId);
            var battleConditionResist = beDoneBattleMonster.battleConditionList.Concat(beDoneBattleMonster.baseBattleConditionList).Where(i => {
                var possessedBattleCondition = battleConditionList.First(m => m.id == i.battleConditionId);
                switch (possessedBattleCondition.battleConditionType) {
                    case BattleConditionType.BuffTypeResist:
                        return possessedBattleCondition.targetBuffType == addedBattleCondition.buffType;

                    case BattleConditionType.BattleConditionResist:
                        return possessedBattleCondition.targetBattleConditionId == addedBattleCondition.id;

                    default:
                        return false;
                }
            }).Sum(i => i.grantorSkillEffect.value);
            var statusResist = addedBattleCondition.buffType == BuffType.Buff ? beDoneBattleMonster.buffResistRate() : beDoneBattleMonster.debuffResistRate();
            var random = UnityEngine.Random.Range(1, 101);
            return random <= skillEffect.activateProbability - battleConditionResist - statusResist;
        } else {
            var random = UnityEngine.Random.Range(1, 101);
            return random <= skillEffect.activateProbability;
        }
    }

    private bool ExecuteProbability(int activateProbability) {
        var random = UnityEngine.Random.Range(1, 101);
        return random <= activateProbability;
    }

    private bool IsValidActivateCondition(BattleMonsterIndex battleMonsterIndex, ActivateConditionType activateConditionType, int activateConditionValue, long battleConditionId) {
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        return IsValidActivateCondition(battleMonster, activateConditionType, activateConditionValue, battleConditionId);
    }

    /// <summary>
    /// 発動条件の判定を行う
    /// </summary>
    /// <param name="battleConditionId">HaveMyselfBattleConditionNum用</param>
    private bool IsValidActivateCondition(BattleMonsterInfo battleMonster, ActivateConditionType activateConditionType, int activateConditionValue, long battleConditionId = 0) {
        switch (activateConditionType) {
            case ActivateConditionType.UnderPercentHP:
                // HPがn%未満ならOK
                return !battleMonster.isDead && battleMonster.currentHp < battleMonster.maxHp * (activateConditionValue / 100.0f);

            case ActivateConditionType.Alive:
                // HPが0より多ければOK
                return !battleMonster.isDead && battleMonster.currentHp > 0;

            case ActivateConditionType.Dead:
                // 戦闘不能ならOK
                return battleMonster.isDead;

            case ActivateConditionType.Healable:
                // 回復可能ならOK
                return !battleMonster.isDead && battleMonster.currentHp < battleMonster.maxHp;

            case ActivateConditionType.HaveBattleCondition:
                // 特定状態異常が付与されていればOK
                return battleMonster.battleConditionList.Any(c => c.battleConditionId == activateConditionValue);

            case ActivateConditionType.EnableEnergyUp:
                // エネルギー上昇可能であればOK
                return battleMonster.currentEnergy < battleMonster.maxEnergy;

            case ActivateConditionType.EnableEnergyDown:
                // エネルギー減少可能であればOK
                return battleMonster.currentEnergy > 0;

            case ActivateConditionType.HaveMyselfBattleConditionNum:
                // 当該状態異常が特定個数付与されていればOK(アクション状態異常用)
                return battleMonster.battleConditionList.Where(c => c.battleConditionId == battleConditionId).Count() == activateConditionValue;

            case ActivateConditionType.None:
            default:
                return false;
        }
    }

    private bool IsFront(BattleMonsterIndex battleMonsterIndex) {
        return ConstManager.Battle.FRONT_INDEX_LIST.Contains(battleMonsterIndex.index);
    }

    private bool IsBack(BattleMonsterIndex battleMonsterIndex) {
        return ConstManager.Battle.BACK_INDEX_LIST.Contains(battleMonsterIndex.index);
    }

    private void SetPlayerBattleMonsterList(List<UserMonsterInfo> userMonsterList) {
        userMonsterList.ForEach((userMonster, index) => {
            if (userMonster != null) {
                var monster = monsterList.First(m => m.id == userMonster.monsterId);
                var normalSkill = GetBattleMonsterNormalSkill(monster.id, userMonster.customData.level);
                var ultimateSkill = GetBattleMonsterUltimateSkill(monster.id, userMonster.customData.level);
                var passiveSkill = GetBattleMonsterPassiveSkill(monster.id, userMonster.customData.level);
                var battleMonster = BattleUtil.GetBattleMonster(monster, userMonster.customData.level, true, index, normalSkill, ultimateSkill, passiveSkill);
                playerBattleMonsterList.Add(battleMonster);
            }
        });
    }

    private void RefreshEnemyBattleMonsterList(int waveCount) {
        var waveIndex = waveCount - 1;
        var questMonsterList = quest.questMonsterListByWave[waveIndex];

        enemyBattleMonsterList.Clear();
        questMonsterList.ForEach((questMonster, index) => {
            var monster = monsterList.FirstOrDefault(m => m.id == questMonster.monsterId);
            if (monster != null) {
                var normalSkill = GetBattleMonsterNormalSkill(monster.id, questMonster.level);
                var ultimateSkill = GetBattleMonsterUltimateSkill(monster.id, questMonster.level);
                var passiveSkill = GetBattleMonsterPassiveSkill(monster.id, questMonster.level);
                var battleMonster = BattleUtil.GetBattleMonster(monster, questMonster.level, false, index, normalSkill, ultimateSkill, passiveSkill, waveCount);
                enemyBattleMonsterList.Add(battleMonster);
            }
        });

        // TODO: テスト用
        if (isTest) TestSetEnemyBattleMonsterList(waveCount);
    }

    private NormalSkillMB GetBattleMonsterNormalSkill(long monsterId, int monsterLevel) {
        var normalSkillId = ClientMonsterUtil.GetNormalSkillId(monsterId, monsterLevel);
        var normalSkill = normalSkillList.First(m => m.id == normalSkillId);
        return normalSkill;
    }

    private UltimateSkillMB GetBattleMonsterUltimateSkill(long monsterId, int monsterLevel) {
        var ultimateSkillId = ClientMonsterUtil.GetUltimateSkillId(monsterId, monsterLevel);
        var ultimateSkill = ultimateSkillList.First(m => m.id == ultimateSkillId);
        return ultimateSkill;
    }

    private PassiveSkillMB GetBattleMonsterPassiveSkill(long monsterId, int monsterLevel) {
        var passiveSkillId = ClientMonsterUtil.GetPassiveSkillId(monsterId, monsterLevel);
        var passiveSkill = passiveSkillList.FirstOrDefault(m => m.id == passiveSkillId);
        if (passiveSkill == null) {
            passiveSkill = new PassiveSkillMB() {
                name = "",
                effectList = new List<PassiveSkillEffectMI>(),
            };
        }
        return passiveSkill;
    }
}
