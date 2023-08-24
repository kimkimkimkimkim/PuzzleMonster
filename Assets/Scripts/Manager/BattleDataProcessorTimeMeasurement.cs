using Newtonsoft.Json;
using PM.Enum.Battle;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;
using UnityEngine;
using UniRx;

public partial class BattleDataProcessor
{
    private Stopwatch totalSW = new Stopwatch();
    private Stopwatch InitSW = new Stopwatch();
    private Stopwatch PlayBattleTimeMeasurementSW = new Stopwatch();
    private Stopwatch AddStartBattleLogSW = new Stopwatch();
    private Stopwatch SkillTriggerTypeOnBattleStartSW = new Stopwatch();
    private Stopwatch AddEndBattleLogSW = new Stopwatch();
    private Stopwatch MoveWaveIfNeededSW = new Stopwatch();
    private Stopwatch MoveTurnIfNeededSW = new Stopwatch();
    private Stopwatch GetNormalActionerSW = new Stopwatch();
    private Stopwatch GetNormalActionerActionTypeSW = new Stopwatch();
    private Stopwatch AddStartTurnActionLogSW = new Stopwatch();
    private Stopwatch CanActionSW = new Stopwatch();
    private Stopwatch GetSkillEffectListSW = new Stopwatch();
    private Stopwatch StartActionStreamSW = new Stopwatch();
    private Stopwatch ActionFailedSW = new Stopwatch();
    private Stopwatch AddEndTurnActionLogSW = new Stopwatch();
    private Stopwatch SkillTriggerTypeOnMeTurnActionEndSW = new Stopwatch();
    private Stopwatch SkillTriggerTypeOnTargetBattleConditionAddedAndMeTurnActionEndSW = new Stopwatch();
    private Stopwatch ProgressBattleConditionTurnIfNeededSW = new Stopwatch();
    private Stopwatch EndTurnIfNeededSW = new Stopwatch();
    private Stopwatch EndWaveIfNeededSW = new Stopwatch();
    private Stopwatch EndBattleIfNeededSW = new Stopwatch();
    private Stopwatch SW = new Stopwatch();

    public IObservable<List<BattleLogInfo>> GetBattleLogListTimeMeasurementObservable(List<UserMonsterInfo> userMonsterList, QuestMB quest) {
        return Observable.ReturnUnit()
            .Select(_ => {
                totalSW.Start();

                InitSW.Start();
                Init(userMonsterList, quest);
                InitSW.Stop();

                // バトル処理を開始する
                PlayBattleTimeMeasurementSW.Start();
                PlayBattleTimeMeasurement();
                PlayBattleTimeMeasurementSW.Stop();

                totalSW.Stop();

                ExportTime();

                return battleLogList;
            });
    }

    private void ExportTime() {
        var swList = new List<(string name, Stopwatch sw)>() {
            ("totalSW", totalSW),
            ("InitSW", InitSW),
            ("PlayBattleTimeMeasurementSW", PlayBattleTimeMeasurementSW),
            ("AddStartBattleLogSW", AddStartBattleLogSW),
            ("SkillTriggerTypeOnBattleStartSW", SkillTriggerTypeOnBattleStartSW),
            ("AddEndBattleLogSW", AddEndBattleLogSW),
            ("MoveWaveIfNeededSW", MoveWaveIfNeededSW),
            ("MoveTurnIfNeededSW", MoveTurnIfNeededSW),
            ("GetNormalActionerSW", GetNormalActionerSW),
            ("GetNormalActionerActionTypeSW", GetNormalActionerActionTypeSW),
            ("AddStartTurnActionLogSW", AddStartTurnActionLogSW),
            ("CanActionSW", CanActionSW),
            ("GetSkillEffectListSW", GetSkillEffectListSW),
            ("StartActionStreamSW", StartActionStreamSW),
            ("ActionFailedSW", ActionFailedSW),
            ("AddEndTurnActionLogSW", AddEndTurnActionLogSW),
            ("SkillTriggerTypeOnMeTurnActionEndSW", SkillTriggerTypeOnMeTurnActionEndSW),
            ("SkillTriggerTypeOnTargetBattleConditionAddedAndMeTurnActionEndSW", SkillTriggerTypeOnTargetBattleConditionAddedAndMeTurnActionEndSW),
            ("ProgressBattleConditionTurnIfNeededSW", ProgressBattleConditionTurnIfNeededSW),
            ("EndTurnIfNeededSW", EndTurnIfNeededSW),
            ("EndWaveIfNeededSW", EndWaveIfNeededSW),
            ("EndBattleIfNeededSW", EndBattleIfNeededSW),
            ("SW", SW), 
        };

        var totalSeconds = totalSW.Elapsed.TotalSeconds;
        swList = swList.OrderByDescending(s => s.sw.Elapsed.TotalSeconds).ToList();
        UnityEngine.Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        swList.ForEach(s => {
            UnityEngine.Debug.Log($"{s.name}: {s.sw.Elapsed.TotalSeconds}[s] ({(float)s.sw.Elapsed.TotalSeconds*100/totalSeconds}%)");
        });
        UnityEngine.Debug.Log(">>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
    }

    private void PlayBattleTimeMeasurement() {
        // バトル開始ログの差し込み
        AddStartBattleLogSW.Start();
        AddStartBattleLog();
        AddStartBattleLogSW.Stop();

        // バトル開始時トリガースキルを発動する
        SkillTriggerTypeOnBattleStartSW.Start();
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnBattleStart, allBattleMonsterOrderBySpeedList.Select(m => m.index).ToList());
        SkillTriggerTypeOnBattleStartSW.Stop();

        // 勝敗が決まるまでバトルを続ける
        while (currentWinOrLose == WinOrLose.Continue) {
            PlayLoopTimeMeasurement();
        }

        // バトル終了ログの差し込み
        AddEndBattleLogSW.Start();
        AddEndBattleLog();
        AddEndBattleLogSW.Stop();
    }

    private void PlayLoopTimeMeasurement() {
        // ウェーブを進行する
        MoveWaveIfNeededSW.Start();
        var isWaveMove = MoveWaveIfNeeded();
        MoveWaveIfNeededSW.Stop();

        // ターンを進行する
        MoveTurnIfNeededSW.Start();
        MoveTurnIfNeeded(isWaveMove);
        MoveTurnIfNeededSW.Stop();

        // アクション実行者を取得する
        GetNormalActionerSW.Start();
        var actionMonsterIndex = GetNormalActioner();
        GetNormalActionerSW.Stop();

        // アクションストリームを開始する
        if (actionMonsterIndex != null) {
            GetNormalActionerActionTypeSW.Start();
            var actionType = GetNormalActionerActionType(actionMonsterIndex);
            GetNormalActionerActionTypeSW.Stop();

            // ターンアクション開始ログの追加
            AddStartTurnActionLogSW.Start();
            AddStartTurnActionLog(actionMonsterIndex);
            AddStartTurnActionLogSW.Stop();

            // 状態異常を確認して行動できるかチェック
            CanActionSW.Start();
            var canAction = CanAction(actionMonsterIndex, actionType);
            CanActionSW.Stop();

            if (canAction) {
                // アクション開始
                GetSkillEffectListSW.Start();
                var battleSkillEffectList = GetSkillEffectList(actionMonsterIndex, actionType).Select(m => new BattleSkillEffectMI() { isActive = true, skillEffect = m }).ToList();
                GetSkillEffectListSW.Stop();

                StartActionStreamSW.Start();
                StartActionStream(actionMonsterIndex, actionType, null, battleSkillEffectList, null);
                StartActionStreamSW.Stop();
            } else {
                // アクション失敗
                ActionFailedSW.Start();
                ActionFailed(actionMonsterIndex, actionType);
                ActionFailedSW.Stop();
            }

            // ターンアクション終了ログの追加
            AddEndTurnActionLogSW.Start();
            AddEndTurnActionLog(actionMonsterIndex);
            AddEndTurnActionLogSW.Stop();

            // ターンアクション終了時トリガースキルの発動
            SkillTriggerTypeOnMeTurnActionEndSW.Start();
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeTurnActionEnd, actionMonsterIndex);
            SkillTriggerTypeOnMeTurnActionEndSW.Stop();

            SkillTriggerTypeOnTargetBattleConditionAddedAndMeTurnActionEndSW.Start();
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnTargetBattleConditionAddedAndMeTurnActionEnd, actionMonsterIndex);
            SkillTriggerTypeOnTargetBattleConditionAddedAndMeTurnActionEndSW.Stop();

            // 状態異常のターンを経過させる
            ProgressBattleConditionTurnIfNeededSW.Start();
            ProgressBattleConditionTurnIfNeeded(actionMonsterIndex);
            ProgressBattleConditionTurnIfNeededSW.Stop();
        }

        // ターンを終了する
        EndTurnIfNeededSW.Start();
        var isTurnEnd = EndTurnIfNeeded();
        EndTurnIfNeededSW.Stop();

        // ウェーブを終了する
        EndWaveIfNeededSW.Start();
        EndWaveIfNeeded();
        EndWaveIfNeededSW.Stop();

        // バトルを終了する
        EndBattleIfNeededSW.Start();
        EndBattleIfNeeded(isTurnEnd);
        EndBattleIfNeededSW.Stop();
    }
}
