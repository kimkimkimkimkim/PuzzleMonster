using PM.Enum.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using GameBase;

public partial class BattleDataProcessor
{

    private int currentWaveCount;
    private int currentTurnCount;
    private QuestMB quest;
    private List<BattleLogInfo> battleLogList = new List<BattleLogInfo>();
    private List<BattleMonsterInfo> playerBattleMonsterList = new List<BattleMonsterInfo>(); // nullは許容しない（もともと表示されないモンスター用のデータは排除されている）
    private List<BattleMonsterInfo> enemyBattleMonsterList = new List<BattleMonsterInfo>(); // nullは許容しない（もともと表示されないモンスター用のデータは排除されている）
    private List<List<BattleMonsterInfo>> enemyBattleMonsterListByWave = new List<List<BattleMonsterInfo>>();
    private List<BattleChainParticipantInfo> battleChainParticipantList = new List<BattleChainParticipantInfo>();
    private WinOrLose currentWinOrLose;

    private void Init(List<UserMonsterInfo> userMonsterList, QuestMB quest)
    {
        this.quest = quest;

        currentWaveCount = 0;
        currentTurnCount = 0;
        currentWinOrLose = WinOrLose.Continue;

        SetPlayerBattleMonsterList(userMonsterList);
    }

    public List<BattleLogInfo> GetBattleLogList(List<UserMonsterInfo> userMonsterList, QuestMB quest)
    {
        Init(userMonsterList, quest);

        // バトル処理を開始する
        while (currentWinOrLose == WinOrLose.Continue)
        {
            PlayLoop();
        }

        return battleLogList;
    }

    private void PlayLoop()
    {
        // バトルを開始する
        StartBattleIfNeeded();

        // ウェーブを進行する
        var isWaveMove = MoveWaveIfNeeded();

        // ターンを進行する
        MoveTurnIfNeeded(isWaveMove);

        // アクション実行者を取得する
        var actionMonsterIndex = GetNormalActioner();

        // アクションストリームを開始する
        if (actionMonsterIndex != null)
        {
            var actionType = GetNormalActionerActionType(actionMonsterIndex);

            // 状態異常を確認して行動できるかチェック
            var canAction = CanAction(actionMonsterIndex, actionType);

            if (canAction)
            {
                // アクション開始
                var skillEffectList = GetSkillEffectList(actionMonsterIndex, actionType);
                var battleChainParticipant = new BattleChainParticipantInfo()
                {
                    battleMonsterIndex = actionMonsterIndex,
                    battleActionType = actionType,
                };
                StartActionStream(actionMonsterIndex, actionType, skillEffectList, battleChainParticipant);
                battleChainParticipantList.Clear();
            }
            else
            {
                // アクション失敗
                ActionFailed(actionMonsterIndex);

                // アクション失敗してもアクション終了時トリガースキルを発動する
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeActionEnd, actionMonsterIndex);
                battleChainParticipantList.Clear();
            }

            // 状態異常のターンを経過させる
            ProgressBattleConditionTurnIfNeeded(actionMonsterIndex);
        }

        // ターンを終了する
        var isTurnEnd = EndTurnIfNeeded();

        // ウェーブを終了する
        EndWaveIfNeeded();

        // バトルを終了する
        EndBattleIfNeeded(isTurnEnd);
    }

    private void StartBattleIfNeeded()
    {
        // ウェーブが0じゃなければスキップ
        if (currentWaveCount > 0) return;

        // バトル開始ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.StartBattle,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            log = "バトルを開始します",
        };
        battleLogList.Add(battleLog);

        // バトル開始時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnBattleStart, GetAllMonsterList().Select(m => m.index).ToList());
        battleChainParticipantList.Clear();
    }

    // 通常アクション実行者を取得
    // いなければnullを返す
    private BattleMonsterIndex GetNormalActioner()
    {
        // プレイヤーと敵のモンスターを合成したリストを取得
        var allMonsterList = GetAllMonsterList();

        // 次のアクション実行者を取得
        var actioner = allMonsterList.Where(b => !b.isActed && !b.isDead).OrderByDescending(b => b.currentSpeed()).ThenBy(_ => Guid.NewGuid()).FirstOrDefault();

        // アクション実行者を設定
        return actioner?.index;
    }

    // 通常アクション実行者のアクションタイプを取得
    private BattleActionType GetNormalActionerActionType(BattleMonsterIndex monsterIndex)
    {
        var battleMonster = GetBattleMonster(monsterIndex);
        return battleMonster.currentEnergy >= ConstManager.Battle.MAX_ENERGY_VALUE ? BattleActionType.UltimateSkill : BattleActionType.NormalSkill;
    }

    // アクション実行者とアクション内容を受け取りアクションを実行する
    private void StartActionStream(BattleMonsterIndex actionMonsterIndex, BattleActionType actionType, List<SkillEffectMI> skillEffectList, BattleChainParticipantInfo battleChainParticipant)
    {
        // 状態異常付与以外のスキル効果はこのタイミングで発動確率判定を行う
        skillEffectList = skillEffectList.Where(effect => effect.type == SkillType.ConditionAdd || ExecuteProbability(effect.activateProbability)).ToList();
        if (!skillEffectList.Any()) return;

        // チェーン参加者リストに追加
        battleChainParticipantList.Add(battleChainParticipant);

        // アクションを開始する
        StartAction(actionMonsterIndex, actionType);

        // アクション開始時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeActionStart, actionMonsterIndex);

        // アクションアニメーションを開始する
        var isCounter = actionType == BattleActionType.PassiveSkill && skillEffectList.Any(effect => effect.type == SkillType.Attack); // パッシブかつ攻撃のスキルは反撃と判定
        if(actionType == BattleActionType.NormalSkill || actionType == BattleActionType.UltimateSkill || isCounter) StartActionAnimation(actionMonsterIndex, actionType);

        // 各効果の実行
        var currentBeDoneMonsterIndexList = new List<BattleMonsterIndex>();
        skillEffectList.ForEach(skillEffect => {
            // アクションの対象を選択する
            currentBeDoneMonsterIndexList = GetBeDoneMonsterIndexList(actionMonsterIndex, currentBeDoneMonsterIndexList, skillEffect);

            // アクション処理を実行する
            ExecuteAction(actionMonsterIndex, actionType, currentBeDoneMonsterIndexList, skillEffect);
        });

        // パッシブスキル発動回数を計上
        if(actionType == BattleActionType.PassiveSkill)
        {
            var actionMonster = GetBattleMonster(actionMonsterIndex);
            actionMonster.passiveSkillExecuteCount++;
        }

        if (actionType == BattleActionType.NormalSkill)
        {
            // 通常攻撃後トリガースキルを発動する
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeNormalSkillEnd, actionMonsterIndex);
        }
        else if (actionType == BattleActionType.UltimateSkill)
        {
            // ウルト攻撃後トリガースキルを発動する
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeUltimateSkillEnd, actionMonsterIndex);
        }

        // 被アクション後トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeTakeActionAfter, currentBeDoneMonsterIndexList);

        // 死亡処理を実行
        ExecuteDieIfNeeded();

        // アクションを終了する
        EndAction(actionMonsterIndex, actionType);

        // アクション終了時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeActionEnd, actionMonsterIndex);
    }
    
    private void ActionFailed(BattleMonsterIndex actionMonsterIndex){
        var battleMonster = GetBattleMonster(actionMonsterIndex);
        var possess = actionMonsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

        // 行動済みフラグは立てる
        battleMonster.isActed = true;
        
        // アクション失敗ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.ActionFailed,
            doBattleMonsterIndex = actionMonsterIndex,
            log = $"{possess}{monster.name}は動けない",
        };
        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常のターンを経過させる
    /// </summary>
    private void ProgressBattleConditionTurnIfNeeded(BattleMonsterIndex battleMonsterIndex)
    {
        var isRemoved = false;
        var isProgress = false;
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        battleMonster.battleConditionList.ForEach(battleCondition =>
        {
            // 継続ターンがあるものに関しては残りターンをデクリメント
            if (battleCondition.remainingTurnNum > 0)
            {
                battleCondition.remainingTurnNum--;
                isProgress = true;
                if(battleCondition.remainingTurnNum == 0)
                {
                    // 解除出来たら解除時状態異常効果を発動
                    isRemoved = true;
                }
            }
        });
        
        // 状態異常のターンが一つも進行しなければ何もしない
        if(!isProgress) return;

        var beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>() { new BeDoneBattleMonsterData() { battleMonsterIndex = battleMonsterIndex } };
        
        // 状態異常ターン進行ログの差し込み
        var progressBattleConditionTurnBattleLog = new BattleLogInfo()
        {
            type = BattleLogType.ProgressBattleConditionTurn,
            beDoneBattleMonsterDataList = beDoneBattleMonsterDataList,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            log = "状態異常のターンを進行しました",
        };
        battleLogList.Add(progressBattleConditionTurnBattleLog);

        // 何も解除されなかったら何もしない
        if (!isRemoved) return;

        // ターンが切れている状態異常を削除する
        battleMonster.battleConditionList = battleMonster.battleConditionList.Where(battleCondition => battleCondition.remainingTurnNum != 0).ToList();

        // 状態異常解除ログの差し込み
        var takeBattleConditionRemoveBattleLog = new BattleLogInfo()
        {
            type = BattleLogType.TakeBattleConditionRemove,
            beDoneBattleMonsterDataList = beDoneBattleMonsterDataList,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            log = "状態異常を解除しました",
        };
        battleLogList.Add(takeBattleConditionRemoveBattleLog);
    }

    /// <summary>
    /// ウェーブ進行が必要ならウェーブを進行させる
    /// ウェーブ進行したか否かを返す
    /// </summary>
    private bool MoveWaveIfNeeded()
    {
        // 敵が全滅していたら実行、残っていたらスキップ
        if (enemyBattleMonsterList.Any(m => !m.isDead)) return false;

        // 現在が最終ウェーブであればスキップ
        if (currentWaveCount >= quest.questMonsterListByWave.Count) return false;

        // ウェーブ数をインクリメント
        currentWaveCount++;

        // 敵モンスターデータを更新
        RefreshEnemyBattleMonsterList(currentWaveCount);

        // ウェーブ進行ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.MoveWave,
            waveCount = currentWaveCount,
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            log = $"ウェーブ{currentWaveCount}を開始します",
        };
        battleLogList.Add(battleLog);

        // ウェーブ開始時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnWaveStart, GetAllMonsterList().Select(m => m.index).ToList());
        battleChainParticipantList.Clear();

        return true;
    }

    private void MoveTurnIfNeeded(bool isForce)
    {
        // すべてのモンスターが行動済みかつ0ターン目でなければ実行そうでなければスキップ
        if (((playerBattleMonsterList.Any(b => !b.isActed && !b.isDead) || enemyBattleMonsterList.Any(b => !b.isActed && !b.isDead)) && currentTurnCount > 0) && !isForce) return;

        // ターン数をインクリメント
        currentTurnCount++;

        // すべてのモンスターの行動済みフラグをもどす
        var allMonsterList = GetAllMonsterList();
        allMonsterList.ForEach(m => m.isActed = false);

        // ターン進行ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.MoveTurn,
            turnCount = currentTurnCount,
            log = $"ターン{currentTurnCount}を開始します",
        };
        battleLogList.Add(battleLog);

        // ターン開始時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnTurnStart, GetAllMonsterList().Select(m => m.index).ToList());
        battleChainParticipantList.Clear();
    }

    private void StartAction(BattleMonsterIndex monsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(monsterIndex);
        var possess = monsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        var skillName = GetSkillName(battleMonster, actionType);

        // アクション開始ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.StartAction,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            doBattleMonsterIndex = monsterIndex,
            actionType = actionType,
            log = $"{possess}{monster.name}が{skillName}を発動",
        };
        battleLogList.Add(battleLog);
    }
    
    private void StartActionAnimation(BattleMonsterIndex monsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(monsterIndex);
        var possess = monsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        var skillName = GetSkillName(battleMonster, actionType);

        // アクション開始ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.StartActionAnimation,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            doBattleMonsterIndex = monsterIndex,
            actionType = actionType,
            log = $"{possess}{monster.name}が{skillName}を実行",
        };
        battleLogList.Add(battleLog);
    }

    private void ExecuteAction(BattleMonsterIndex doMonsterIndex,BattleActionType actionType, List<BattleMonsterIndex> beDoneMonsterIndexList, SkillEffectMI skillEffect)
    {
        // 対象モンスターが存在しない場合はなにもしない
        if (!beDoneMonsterIndexList.Any()) return;

        var allMonsterList = GetAllMonsterList();
        var beDoneMonsterList = allMonsterList.Where(m => beDoneMonsterIndexList.Any(index => index.isPlayer == m.index.isPlayer && index.index == m.index.index)).ToList();

        var skillType = skillEffect.type;
        switch (skillType) {
            case SkillType.Attack:
                ExecuteAttack(doMonsterIndex,actionType, beDoneMonsterList, skillEffect);
                break;
            case SkillType.Heal:
                ExecuteHeal(doMonsterIndex, beDoneMonsterList, skillEffect);
                break;
            case SkillType.ConditionAdd:
                ExecuteBattleConditionAdd(doMonsterIndex, beDoneMonsterList, skillEffect);
                break;
            case SkillType.ConditionRemove:
                ExecuteBattleConditionRemove(doMonsterIndex, beDoneMonsterList, skillEffect);
                break;
            case SkillType.Revive:
                ExecuteRevive(doMonsterIndex, beDoneMonsterList, skillEffect);
                break; 
            default:
                break;
        }
    }

    private void ExecuteAttack(BattleMonsterIndex doMonsterIndex,BattleActionType actionType, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect)
    {
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            var actionValue = GetActionValue(doMonsterIndex, m.index, skillEffect);

            // 攻撃してきたモンスターの更新
            if (actionType == BattleActionType.NormalSkill || actionType == BattleActionType.UltimateSkill) m.currentBeDoneAttackedMonsterIndex = doMonsterIndex;

            // 効果量を反映
            // 攻撃でも回復でも加算
            var effectValue = m.ChangeHp(actionValue.value);
            
            // スコア計算
            AddScore(doMonsterIndex, m.index, SkillType.Attack, effectValue);

            // エネルギーを上昇させる
            if(actionType != BattleActionType.BattleCondition) m.ChangeEnergy(ConstManager.Battle.ENERGY_RISE_VALUE_ON_TAKE_DAMAGE);

            return new BeDoneBattleMonsterData()
            {
                battleMonsterIndex = m.index,
                hpChanges = actionValue.value,
                isCritical = actionValue.isCritical,
                isBlocked = actionValue.isBlocked,
            };
        }).ToList();
        var logList = beDoneMonsterDataList.Select(d => {
            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

            return $"{possess}{monster.name}に{Math.Abs(d.hpChanges)}ダメージ";
        }).ToList();
        var log = string.Join("\n", logList);

        // アクション実行ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.TakeDamage,
            doBattleMonsterIndex = doMonsterIndex,
            beDoneBattleMonsterDataList = beDoneMonsterDataList,
            playerBattleMonsterList = this.playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = this.enemyBattleMonsterList.Clone(),
            skillFxId = skillEffect.skillFxId,
            log = log,
        };
        battleLogList.Add(battleLog);

        // トリガースキルを発動する
        var beDoneBattleMonsterIndexList = beDoneMonsterDataList.Select(d => d.battleMonsterIndex).ToList();
        var allBattleMonsterList = GetAllMonsterList();
        var playerBattleMonsterIndexList = allBattleMonsterList.Where(m => m.index.isPlayer).Select(m => m.index).ToList();
        var enemyBattleMonsterIndexList = allBattleMonsterList.Where(m => !m.index.isPlayer).Select(m => m.index).ToList();
        var existsPlayer = beDoneBattleMonsterIndexList.Any(i => i.isPlayer);
        var existsEnemy = beDoneBattleMonsterIndexList.Any(i => !i.isPlayer);

        // 自身がダメージを与えたとき
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeExecuteDamageAfter, doMonsterIndex);

        // 自身がクリティカルを発動した時
        if (beDoneMonsterDataList.Any(d => d.isCritical)) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeExecuteCriticcalAfter, doMonsterIndex);

        // ブロックされた時
        if (beDoneMonsterDataList.Any(d => d.isBlocked)) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeBlocked, doMonsterIndex);

        beDoneMonsterDataList.Where(d => d.isBlocked).ToList().ForEach(d =>
        {
            // 自身がブロックした時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBlocked, d.battleMonsterIndex);

            // 味方がブロックした時
            if (d.battleMonsterIndex.isPlayer) 
            {
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyBlocked, playerBattleMonsterIndexList);
            }
            else
            {
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyBlocked, enemyBattleMonsterIndexList);
            }

            // 敵がブロックした時
            if (d.battleMonsterIndex.isPlayer)
            {
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBlocked, enemyBattleMonsterIndexList);
            }
            else
            {
                ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBlocked, playerBattleMonsterIndexList);
            }
        });

        // 通常攻撃またはウルトを発動したとき
        if (actionType == BattleActionType.NormalSkill || actionType == BattleActionType.UltimateSkill) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeExecuteNormalOrUltimateSkill, doMonsterIndex);

        // 通常攻撃またはウルトを受けたとき
        if (actionType == BattleActionType.NormalSkill || actionType == BattleActionType.UltimateSkill)
        {
            // 反撃系はトリガー発動の要因も渡す
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeExecutedNormalOrUltimateSkill, beDoneBattleMonsterIndexList, 0, doMonsterIndex, actionType, 0);
        }

        // ダメージを受けたとき
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeTakeDamageEnd, beDoneBattleMonsterIndexList);
    }

    private void ExecuteHeal(BattleMonsterIndex doMonsterIndex, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect)
    {
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            var actionValue = GetActionValue(doMonsterIndex, m.index, skillEffect);

            // 効果量を反映
            // 攻撃でも回復でも加算
            var effectValue = m.ChangeHp(actionValue.value);
            
            // スコア計算
            AddScore(doMonsterIndex, m.index, SkillType.Heal, effectValue);

            return new BeDoneBattleMonsterData()
            {
                battleMonsterIndex = m.index,
                hpChanges = actionValue.value,
            };
        }).ToList();
        var logList = beDoneMonsterDataList.Select(d => {
            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

            return $"{possess}{monster.name}の体力を{Math.Abs(d.hpChanges)}回復";
        }).ToList();
        var log = string.Join("\n", logList);

        // アクション実行ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.TakeHeal,
            doBattleMonsterIndex = doMonsterIndex,
            beDoneBattleMonsterDataList = beDoneMonsterDataList,
            playerBattleMonsterList = this.playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = this.enemyBattleMonsterList.Clone(),
            skillFxId = skillEffect.skillFxId,
            log = log,
        };
        battleLogList.Add(battleLog);
    }

    private void ExecuteBattleConditionAdd(BattleMonsterIndex doMonsterIndex, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect)
    {
        var battleConditionMB = MasterRecord.GetMasterOf<BattleConditionMB>().Get(skillEffect.battleConditionId);

        var beDoneMonsterDataList = beDoneMonsterList.Select(battleMonster =>
        {
            var battleConditionResist = battleMonster.battleConditionList
                .Where(c => {
                    var isTargetBattleConditionResist = c.battleCondition.battleConditionType == BattleConditionType.BattleConditionResist && c.battleCondition.targetBattleConditionId == battleConditionMB.id;
                    var isTargetBuffTypeResist = c.battleCondition.battleConditionType == BattleConditionType.BuffTypeResist && c.battleCondition.targetBuffType == battleConditionMB.buffType;
                    return isTargetBattleConditionResist || isTargetBuffTypeResist;
                })
                .Sum(c => c.skillEffect.value);
            var isSucceeded = ExecuteProbability(skillEffect.activateProbability - battleConditionResist);
            if (isSucceeded)
            {
                // 状態異常を付与
                AddBattleCondition(doMonsterIndex, battleMonster.index, skillEffect, battleConditionMB);
            }

            return new BeDoneBattleMonsterData()
            {
                battleMonsterIndex = battleMonster.index,
                isMissed = !isSucceeded,
            };
        }).ToList();
        var logList = beDoneMonsterDataList.Select(d => {
             
            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

            return d.isMissed ?
                $"{possess}{monster.name}への{battleConditionMB.name}の付与が失敗" :
                $"{possess}{monster.name}に{battleConditionMB.name}を付与";
        }).ToList();
        var log = string.Join("\n", logList);

        // アクション実行ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.TakeBattleConditionAdd,
            doBattleMonsterIndex = doMonsterIndex,
            beDoneBattleMonsterDataList = beDoneMonsterDataList,
            playerBattleMonsterList = this.playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = this.enemyBattleMonsterList.Clone(),
            skillFxId = skillEffect.skillFxId,
            log = log,
        };
        battleLogList.Add(battleLog);

        // 状態異常付与時トリガースキルを発動する
        var beAddedBattleMonsterIndexList = beDoneMonsterDataList.Where(d => !d.isMissed).Select(d => d.battleMonsterIndex).ToList();
        var allBattleMonsterList = GetAllMonsterList();
        var playerBattleMonsterIndexList = allBattleMonsterList.Where(m => m.index.isPlayer).Select(m => m.index).ToList();
        var enemyBattleMonsterIndexList = allBattleMonsterList.Where(m => !m.index.isPlayer).Select(m => m.index).ToList();

        beAddedBattleMonsterIndexList.ForEach(battleMonsterIndex =>
        {
            // 自身が付与された時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeBeAddedBattleCondition, battleMonsterIndex, (int)battleConditionMB.id);

            // 味方が付与された時
            if (battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyBeAddedBattleCondition, playerBattleMonsterIndexList, (int)battleConditionMB.id);
            if (!battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyBeAddedBattleCondition, enemyBattleMonsterIndexList, (int)battleConditionMB.id);

            // 敵が付与された時
            if (battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBeAddedBattleCondition, enemyBattleMonsterIndexList, (int)battleConditionMB.id);
            if (!battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyBeAddedBattleCondition, playerBattleMonsterIndexList, (int)battleConditionMB.id);
        });
    }

    private void ExecuteBattleConditionRemove(BattleMonsterIndex doMonsterIndex, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect)
    {

    }

    private void ExecuteRevive(BattleMonsterIndex doMonsterIndex, List<BattleMonsterInfo> beDoneMonsterList, SkillEffectMI skillEffect)
    {

    }

    private void ExecuteDieIfNeeded()
    {
        var allBattleMonsterList = GetAllMonsterList();
        var dieBattleMonsterList = allBattleMonsterList.Where(m => !m.isDead && m.currentHp <= 0).ToList();

        // 倒れたモンスターがいなければ何もしない
        if (!dieBattleMonsterList.Any()) return;

        // 死亡判定フラグを立てる
        dieBattleMonsterList.ForEach(m => m.isDead = true);

        // ログに渡す用のリストを作成
        var beDoneBattleMonsterDataList = dieBattleMonsterList.Clone().Select(m => new BeDoneBattleMonsterData() { battleMonsterIndex = m.index }).ToList();

        var logList = dieBattleMonsterList.Select(m => {
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
            var possess = m.index.isPlayer ? "味方の" : "敵の";
            return $"{possess}{monster.name}が倒れた";
        }).ToList();
        var log = string.Join("\n", logList);

        // 死亡ログを差し込む
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.Die,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            beDoneBattleMonsterDataList = beDoneBattleMonsterDataList,
            log = log,
        };
        battleLogList.Add(battleLog);

        // 戦闘不能時トリガースキルを発動する
        var existsPlayer = dieBattleMonsterList.Any(m => m.index.isPlayer);
        var existsEnemy = dieBattleMonsterList.Any(m => !m.index.isPlayer);
        var playerBattleMonsterIndexList = allBattleMonsterList.Where(m => m.index.isPlayer).Select(m => m.index).ToList();
        var enemyBattleMonsterIndexList = allBattleMonsterList.Where(m => !m.index.isPlayer).Select(m => m.index).ToList();

        dieBattleMonsterList.Select(m => m.index).ToList().ForEach(battleMonsterIndex =>
        {
            // 自分が戦闘不能時
            ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnMeDeadEnd, battleMonsterIndex);

            // 味方が戦闘不能時
            if (battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyDead, playerBattleMonsterIndexList);
            if (!battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnAllyDead, enemyBattleMonsterIndexList);

            // 敵が戦闘不能時
            if (battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyDead, enemyBattleMonsterIndexList);
            if (!battleMonsterIndex.isPlayer) ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnEnemyDead, playerBattleMonsterIndexList);
        });
    }

    private void EndAction(BattleMonsterIndex doMonsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(doMonsterIndex);
        var possess = doMonsterIndex.isPlayer ? "味方の" : "敵の";
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

        // 行動済みフラグを立てる
        battleMonster.isActed = true;

        // エネルギー計算処理を行う
        switch (actionType)
        {
            case BattleActionType.NormalSkill:
                battleMonster.ChangeEnergy(ConstManager.Battle.ENERGY_RISE_VALUE_ON_ACT);
                break;
            case BattleActionType.UltimateSkill:
                battleMonster.currentEnergy = 0;
                break;
            default:
                break;
        }

        // アクション終了ログを差し込む
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.EndAction,
            doBattleMonsterIndex = doMonsterIndex,
            playerBattleMonsterList = this.playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = this.enemyBattleMonsterList.Clone(),
            log = $"{possess}{monster.name}のアクションが終了しました",
        };
        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 現在のターンが終了すればターン終了時処理を実行
    /// ターンが終了するか否かを返す
    /// </summary>
    private bool EndTurnIfNeeded()
    {
        // 一体でも未行動のモンスターが存在すれば実行しない
        var isNotEnd = GetAllMonsterList().Any(m => !m.isActed && !m.isDead);
        if (isNotEnd) return false;;

        // ターン終了ログを差し込む
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.None,
            log = $"ターン{currentTurnCount}が終了しました",
        };
        battleLogList.Add(battleLog);

        // ターン終了時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnTurnEnd, GetAllMonsterList().Select(m => m.index).ToList());
        battleChainParticipantList.Clear();
        
        return true;
    }

    private void EndWaveIfNeeded()
    {
        // 敵に戦えるモンスターが一体でもいれば何もしない
        var existsEnemy = enemyBattleMonsterList.Any(m => !m.isDead);
        if (existsEnemy) return;

        // ウェーブ終了ログを差し込む
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.None,
            log = $"ウェーブ{currentWaveCount}が終了しました",
        };
        battleLogList.Add(battleLog);
        
        // Wave毎の敵情報リストの更新
        enemyBattleMonsterListByWave.Add(enemyBattleMonsterList);

        // ウェーブ終了時トリガースキルを発動する
        ExecuteTriggerSkillIfNeeded(SkillTriggerType.OnWaveEnd, GetAllMonsterList().Select(m => m.index).ToList());
        battleChainParticipantList.Clear();
    }
    
    private void EndBattleIfNeeded(bool isTurnEnd)
    {
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
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.Result,
            winOrLose = currentWinOrLose,
            log = currentWinOrLose == WinOrLose.Win ? "バトルに勝利しました" : "バトルに敗北しました",
            playerBattleMonsterList = playerBattleMonsterList,
            enemyBattleMonsterListByWave = enemyBattleMonsterListByWave,
        };
        battleLogList.Add(battleLog);
    }

    /// <summary>
    /// 状態異常を確認して行動できるかをチェック
    /// </summary>
    private bool CanAction(BattleMonsterIndex battleMonsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        switch (actionType) {
            case BattleActionType.NormalSkill:
                return !battleMonster.battleConditionList.Any(c => c.battleCondition.battleConditionType == BattleConditionType.NormalAndUltimateAndPassiveSkillUnavailable || c.battleCondition.battleConditionType == BattleConditionType.NormalSkillUnavailable);
            case BattleActionType.UltimateSkill:
                return !battleMonster.battleConditionList.Any(c => c.battleCondition.battleConditionType == BattleConditionType.NormalAndUltimateAndPassiveSkillUnavailable || c.battleCondition.battleConditionType == BattleConditionType.UltimateSkillUnavailable);
            case BattleActionType.PassiveSkill:
                return !battleMonster.battleConditionList.Any(c => c.battleCondition.battleConditionType == BattleConditionType.NormalAndUltimateAndPassiveSkillUnavailable || c.battleCondition.battleConditionType == BattleConditionType.PassiveSkillUnavailable);
            case BattleActionType.BattleCondition:
            default:
                return true;
        }
    }
    
    /// <summary>
    /// 状態異常情報を付与する
    /// </summary>
    private void AddBattleCondition(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect, BattleConditionMB battleConditionMB)
    {
        var beDoneBattleMonster = GetBattleMonster(beDoneMonsterIndex);
        var battleCondition = GetBattleCondition(doMonsterIndex, beDoneMonsterIndex, skillEffect, battleConditionMB, beDoneBattleMonster.battleConditionCount);
        
        // 状態異常を付与しカウントをインクリメント
        beDoneBattleMonster.battleConditionList.Add(battleCondition.Clone());
        beDoneBattleMonster.battleConditionCount++;
    }

    /// <summary>
    /// 状態異常情報を作成して返す
    /// </summary>
    private BattleConditionInfo GetBattleCondition(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect, BattleConditionMB battleConditionMB, int order)
    {

        var calculatedValue = battleConditionMB.battleConditionType == BattleConditionType.Action ? GetActionValue(doMonsterIndex, beDoneMonsterIndex, skillEffect).value : 0;
        var shieldValue = battleConditionMB.battleConditionType == BattleConditionType.Shield ? skillEffect.value : 0;

        // アクション状態異常の場合はスキル効果を修正する
        if(battleConditionMB.battleConditionType == BattleConditionType.Action)
        {
            skillEffect = skillEffect.Clone();
            skillEffect.type = battleConditionMB.skillType;
            skillEffect.skillTargetType = SkillTargetType.Myself;
        }

        var battleCondition = new BattleConditionInfo()
        {
            battleCondition = battleConditionMB,
            skillEffect = skillEffect,
            remainingTurnNum = skillEffect.durationTurnNum,
            actionValue = calculatedValue,
            shieldValue = shieldValue,
            order = order,
        };
        return battleCondition.Clone();
    }

    private List<BattleMonsterInfo> GetAllMonsterList()
    {
        var allMonsterList = new List<BattleMonsterInfo>();
        allMonsterList.AddRange(playerBattleMonsterList);
        allMonsterList.AddRange(enemyBattleMonsterList);
        return allMonsterList;
    }

    private string GetSkillName(BattleMonsterInfo battleMonster, BattleActionType actionType)
    {
        switch (actionType)
        {
            case BattleActionType.NormalSkill:
                var normalSkillId = ClientMonsterUtil.GetNormalSkillId(battleMonster.monsterId, battleMonster.level);
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(normalSkillId);
                return normalSkill.name;
            case BattleActionType.UltimateSkill:
                var ultimateSkillId = ClientMonsterUtil.GetUltimateSkillId(battleMonster.monsterId, battleMonster.level);
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(ultimateSkillId);
                return ultimateSkill.name;
            case BattleActionType.PassiveSkill:
                var passiveSkillId = ClientMonsterUtil.GetPassiveSkillId(battleMonster.monsterId, battleMonster.level);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(passiveSkillId);
                return passiveSkill?.name ?? "-";
            default:
                return "";
        }
    }

    private List<SkillEffectMI> GetSkillEffectList(BattleMonsterIndex monsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(monsterIndex);
        switch (actionType)
        {
            case BattleActionType.NormalSkill:
                var normalSkillId = ClientMonsterUtil.GetNormalSkillId(battleMonster.monsterId, battleMonster.level);
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(normalSkillId);
                return normalSkill.effectList.Select(m => (SkillEffectMI)m).ToList();
            case BattleActionType.UltimateSkill:
                var ultimateSkillId = ClientMonsterUtil.GetUltimateSkillId(battleMonster.monsterId, battleMonster.level);
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(ultimateSkillId);
                return ultimateSkill.effectList.Select(m => (SkillEffectMI)m).ToList();
            case BattleActionType.PassiveSkill:
                var passiveSkillId = ClientMonsterUtil.GetPassiveSkillId(battleMonster.monsterId, battleMonster.level);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(passiveSkillId);
                return passiveSkill?.effectList.Select(m => (SkillEffectMI)m).ToList() ?? new List<SkillEffectMI>();
            default:
                return new List<SkillEffectMI>();
        }
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex monsterIndex)
    {
        if (monsterIndex.isPlayer)
        {
            return playerBattleMonsterList.First(battleMonster => battleMonster.index.IsSame(monsterIndex));
        }
        else
        {
            return enemyBattleMonsterList.First(battleMonster => battleMonster.index.IsSame(monsterIndex));
        }
    }

    private List<BattleMonsterIndex> GetBeDoneMonsterIndexList(BattleMonsterIndex doMonsterIndex,List<BattleMonsterIndex> currentBeDoneMonsterIndexList , SkillEffectMI skillEffect)
    {
        var isDoMonsterPlayer = doMonsterIndex.isPlayer;
        var allyBattleMonsterList = isDoMonsterPlayer ? this.playerBattleMonsterList : this.enemyBattleMonsterList;
        var enemyBattleMonsterList = isDoMonsterPlayer ? this.enemyBattleMonsterList : this.playerBattleMonsterList;
        allyBattleMonsterList = allyBattleMonsterList.Where(b => IsValidActivateCondition(b, skillEffect.activateConditionType, skillEffect.activateConditionValue)).ToList();
        enemyBattleMonsterList = enemyBattleMonsterList.Where(b => IsValidActivateCondition(b, skillEffect.activateConditionType, skillEffect.activateConditionValue)).ToList();

        var battleMonsterIndexList = new List<BattleMonsterIndex>();
        switch (skillEffect.skillTargetType)
        {
            case SkillTargetType.Myself:
                battleMonsterIndexList = allyBattleMonsterList.Where(m => m.index.isPlayer == doMonsterIndex.isPlayer && m.index.index == doMonsterIndex.index).Select(b => b.index).ToList();
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
                var isBeAttackedValid = IsValidActivateCondition(doMonster.currentBeDoneAttackedMonsterIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue);
                battleMonsterIndexList = isBeAttackedValid ? new List<BattleMonsterIndex>() { doMonster.currentBeDoneAttackedMonsterIndex } : new List<BattleMonsterIndex>();
                break;
            case SkillTargetType.BeAttacked:
                battleMonsterIndexList = currentBeDoneMonsterIndexList.Where(battleIndex => IsValidActivateCondition(battleIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue)).Select(battleIndex => battleIndex).ToList();
                break;
            case SkillTargetType.AllyFrontAll:
                {
                    var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    battleMonsterIndexList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    break;
                }
            case SkillTargetType.AllyBackAll:
                {
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    battleMonsterIndexList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    break;
                }
            case SkillTargetType.EnemyFrontAll:
                {
                    var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    battleMonsterIndexList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackAll:
                {
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
            case SkillTargetType.JustBeforeElementTarget:
                battleMonsterIndexList = currentBeDoneMonsterIndexList.Where(battleIndex => IsValidActivateCondition(battleIndex, skillEffect.activateConditionType, skillEffect.activateConditionValue)).ToList();
                break;
            case SkillTargetType.AllyFrontRandom1:
                {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.AllyFrontRandom2:
                {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.AllyBackRandom1:
                {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.AllyBackRandom2:
                {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.AllyBackRandom3:
                {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(3).ToList();
                    break;
                }
            case SkillTargetType.EnemyFrontRandom1:
                {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.EnemyFrontRandom2:
                {
                    // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                    var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackRandom1:
                {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(1).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackRandom2:
                {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(2).ToList();
                    break;
                }
            case SkillTargetType.EnemyBackRandom3:
                {
                    // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                    var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                    var targetList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                    battleMonsterIndexList = targetList.Shuffle().Take(3).ToList();
                    break;
                }
            case SkillTargetType.None:
            default:
                battleMonsterIndexList = new List<BattleMonsterIndex>();
                break;
        }
        return battleMonsterIndexList;
    }

    private bool ExecuteProbability(int activateProbability)
    {
        var random = UnityEngine.Random.Range(1, 101);
        return random <= activateProbability;
    }

    private bool IsValidActivateCondition(BattleMonsterIndex battleMonsterIndex, ActivateConditionType activateConditionType, int activateConditionValue)
    {
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        return IsValidActivateCondition(battleMonster, activateConditionType, activateConditionValue);
    }

    private bool IsValidActivateCondition(BattleMonsterInfo battleMonster, ActivateConditionType activateConditionType, int activateConditionValue)
    {
        switch (activateConditionType)
        {
            case ActivateConditionType.UnderPercentHP:
                // HPが50%未満ならOK
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
            case ActivateConditionType.None:
            default:
                return false;
        }
    }

    private bool IsFront(BattleMonsterIndex battleMonsterIndex)
    {
        return ConstManager.Battle.FRONT_INDEX_LIST.Contains(battleMonsterIndex.index);
    }

    private bool IsBack(BattleMonsterIndex battleMonsterIndex)
    {
        return ConstManager.Battle.BACK_INDEX_LIST.Contains(battleMonsterIndex.index);
    }

    private void SetPlayerBattleMonsterList(List<UserMonsterInfo> userMonsterList) {
        userMonsterList.ForEach((userMonster, index) => {
            if (userMonster != null) {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
                var battleMonster = BattleUtil.GetBattleMonster(monster, userMonster.customData.level, true, index);
                playerBattleMonsterList.Add(battleMonster);
            }
        });
    }

    private void RefreshEnemyBattleMonsterList(int waveCount)
    {
        var waveIndex = waveCount - 1;
        var questMonsterList = quest.questMonsterListByWave[waveIndex];

        enemyBattleMonsterList.Clear();
        questMonsterList.ForEach((questMonster, index) =>
        {
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(questMonster.monsterId);
            if (monster != null)
            {
                var battleMonster = BattleUtil.GetBattleMonster(monster, questMonster.level, false, index);
                enemyBattleMonsterList.Add(battleMonster);
            }
        });
    }
}
