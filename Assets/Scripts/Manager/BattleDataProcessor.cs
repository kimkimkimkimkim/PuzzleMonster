using PM.Enum.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using GameBase;

public class BattleDataProcessor
{

    private int currentWaveCount;
    private int currentTurnCount;
    private List<QuestWaveMB> questWaveList;
    private List<BattleLogInfo> battleLogList = new List<BattleLogInfo>();
    private List<BattleMonsterInfo> playerBattleMonsterList = new List<BattleMonsterInfo>();
    private List<BattleMonsterInfo> enemyBattleMonsterList = new List<BattleMonsterInfo>();
    private List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList;
    private List<BattleMonsterIndex> chainParticipantMonsterIndexList = new List<BattleMonsterIndex>();
    private WinOrLose currentWinOrLose;

    private void Init(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        questWaveList = quest.questWaveIdList.Select(id => MasterRecord.GetMasterOf<QuestWaveMB>().Get(id)).ToList();

        currentWaveCount = 0;
        currentTurnCount = 0;
        currentWinOrLose = WinOrLose.Continue;
        beDoneBattleMonsterDataList = new List<BeDoneBattleMonsterData>();

        SetPlayerBattleMonsterList(userMonsterParty);
    }

    int loopCount = 0;
    public List<BattleLogInfo> GetBattleLogList(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        Init(userMonsterParty, quest);

        // バトル処理を開始する
        while (currentWinOrLose == WinOrLose.Continue && loopCount < 1000)
        {
            loopCount++;
            PlayLoop();
        }

        // TODO
        if(battleLogList.FirstOrDefault(l => l.type == BattleLogType.Result) == null)
        {
            var battleLog = new BattleLogInfo()
            {
                type = BattleLogType.Result,
                winOrLose = WinOrLose.Lose,
                log = "バトルに敗北しました",
            };
            battleLogList.Add(battleLog);
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
            StartActionStream(actionMonsterIndex, actionType);
            chainParticipantMonsterIndexList.Clear();
        }

        // ターンを終了する
        EndTurnIfNeeded();

        // ウェーブを終了する
        EndWaveIfNeeded();

        // バトルを終了する
        EndBattleIfNeeded();
    }

    private void StartBattleIfNeeded()
    {
        // ウェーブが0じゃなければスキップ
        if (currentWaveCount > 0) return;

        // バトル開始ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.StartBattle,
            playerBattleMonsterList = playerBattleMonsterList,
            log = "バトルを開始します",
        };
        battleLogList.Add(battleLog);

        // バトル開始時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnBattleStart);
        chainParticipantMonsterIndexList.Clear();
    }

    // 通常アクション実行者を取得
    // いなければnullを返す
    private BattleMonsterIndex GetNormalActioner()
    {
        // プレイヤーと敵のモンスターを合成したリストを取得
        var allMonsterList = GetAllMonsterList();

        // 次のアクション実行者を取得
        var actioner = allMonsterList.Where(b => !b.isActed && !b.isDead).OrderByDescending(b => b.currentSpeed).ThenBy(_ => Guid.NewGuid()).FirstOrDefault();

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
    private void StartActionStream(BattleMonsterIndex actionMonsterIndex, BattleActionType actionType)
    {
        // チェーン参加者リストに追加
        if (actionType == BattleActionType.PassiveSkill) chainParticipantMonsterIndexList.Add(actionMonsterIndex);

        // アクションを開始する
        StartAction(actionMonsterIndex, actionType);

        // アクション開始時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnMeActionStart);

        // 実行するアクションの効果リストを取得する
        var skillEffectList = GetSkillEffectList(actionMonsterIndex, actionType);

        // 被アクション前パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnMeTakeActionBefore);

        // 各効果の実行
        skillEffectList.ForEach(skillEffect => {
            // アクションの対象を選択する
            var beDoneActionMonsterIndexList = GetBeDoneMonsterIndexList(actionMonsterIndex, skillEffect);

            // アクション処理を実行する
            ExecuteAction(actionMonsterIndex, beDoneActionMonsterIndexList, skillEffect);
        });

        // 被アクション後パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnMeTakeActionAfter);

        // 死亡処理を実行
        ExecuteDieIfNeeded();

        // アクションを終了する
        EndAction(actionMonsterIndex, actionType);

        // アクション終了時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnMeActionEnd, actionMonsterIndex);
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
        if (currentWaveCount >= questWaveList.Count) return false;

        // ウェーブ数をインクリメント
        currentWaveCount++;

        // 敵モンスターデータを更新
        RefreshEnemyBattleMonsterList(currentWaveCount);

        // ウェーブ進行ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.MoveWave,
            waveCount = currentWaveCount,
            log = $"ウェーブ{currentWaveCount}を開始します",
        };
        battleLogList.Add(battleLog);

        // ウェーブ開始時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnWaveStart);
        chainParticipantMonsterIndexList.Clear();

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

        // ターン開始時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnTurnStart);
        chainParticipantMonsterIndexList.Clear();
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
            doBattleMonsterIndex = monsterIndex,
            log = $"{possess}{monster.name}が{skillName}を発動",
        };
        battleLogList.Add(battleLog);
    }

    private void ExecuteAction(BattleMonsterIndex doMonsterIndex, List<BattleMonsterIndex> beDoneMonsterIndex, SkillEffectMI skillEffect)
    {
        var allMonsterList = GetAllMonsterList();
        var beDoneMonsterList = allMonsterList.Where(m => beDoneMonsterIndex.Any(index => index.isPlayer == m.index.isPlayer && index.index == m.index.index)).ToList();

        // 対象モンスターが存在しない場合はなにもしない
        if (!beDoneMonsterList.Any()) return;

        var skillType = skillEffect.type;
        var beDoneMonsterDataList = beDoneMonsterList.Select(m => {
            var actionValue = GetActionValue(doMonsterIndex, m.index, skillEffect);

            // 効果量を反映
            // 攻撃でも回復でも加算
            m.currentHp += actionValue;

            // エネルギーを上昇させる
            m.currentEnergy += ConstManager.Battle.ENERGY_RISE_VALUE_ON_TAKE_DAMAGE;

            return new BeDoneBattleMonsterData()
            {
                battleMonsterIndex = m.index,
                hpChanges = actionValue,
            };
        }).ToList();
        var logList = beDoneMonsterDataList.Select(d => {
            var battleMonster = GetBattleMonster(d.battleMonsterIndex);
            var possess = d.battleMonsterIndex.isPlayer ? "味方の" : "敵の";
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);

            switch (skillEffect.type)
            {
                case SkillType.Damage:
                    return $"{possess}{monster.name}に{Math.Abs(d.hpChanges)}ダメージ";
                case SkillType.Heal:
                    return $"{possess}{monster.name}の体力を{Math.Abs(d.hpChanges)}回復";
                default:
                    return "";
            }
        }).ToList();
        var log = string.Join("\n", logList);

        // アクション実行ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.TakeAction,
            doBattleMonsterIndex = doMonsterIndex,
            beDoneBattleMonsterDataList = beDoneMonsterDataList,
            playerBattleMonsterList = this.playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = this.enemyBattleMonsterList.Clone(),
            skillFxId = skillEffect.skillFxId,
            log = log,
        };
        battleLogList.Add(battleLog);
    }

    private void ExecuteDieIfNeeded()
    {
        var allMonsterList = GetAllMonsterList();
        var dieMonsterList = allMonsterList.Where(m => !m.isDead && m.currentHp <= 0).ToList();

        // 死亡判定フラグを立てる
        dieMonsterList.ForEach(m => m.isDead = true);

        // ログに渡す用のリストを作成
        var beDoneBattleMonsterDataList = dieMonsterList.Clone().Select(m => new BeDoneBattleMonsterData() { battleMonsterIndex = m.index }).ToList();

        var logList = dieMonsterList.Select(m => {
            var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
            var possess = m.index.isPlayer ? "味方の" : "敵の";
            return $"{possess}{monster.name}が倒れた";
        }).ToList();
        var log = string.Join("\n", logList);

        // 死亡ログを差し込む
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.Die,
            beDoneBattleMonsterDataList = beDoneBattleMonsterDataList,
            log = log,
        };
        battleLogList.Add(battleLog);
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
                battleMonster.currentEnergy += ConstManager.Battle.ENERGY_RISE_VALUE_ON_ACT;
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
            type = BattleLogType.None,
            log = $"{possess}{monster.name}のアクションが終了しました",
        };
        battleLogList.Add(battleLog);
    }

    private void EndTurnIfNeeded()
    {
        // 一体でも未行動のモンスターが存在すれば実行しない
        var isNotEnd = GetAllMonsterList().Any(m => !m.isActed && !m.isDead);
        if (isNotEnd) return;

        // ターン終了ログを差し込む
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.None,
            log = $"ターン{currentTurnCount}が終了しました",
        };
        battleLogList.Add(battleLog);

        // ターン終了時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnTurnEnd);
        chainParticipantMonsterIndexList.Clear();
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

        // ウェーブ終了時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnWaveEnd);
        chainParticipantMonsterIndexList.Clear();
    }

    private void EndBattleIfNeeded()
    {
        // 最終ウェーブでなければ何もしない
        if (currentWaveCount < questWaveList.Count) return;

        // 敵味方ともに戦えるモンスターが一体でもいれば何もしない
        var existsAlly = playerBattleMonsterList.Any(m => !m.isDead);
        var existsEnemy = enemyBattleMonsterList.Any(m => !m.isDead);
        if (existsAlly && existsEnemy) return;

        // 味方が残っていれば勝利
        var winOrLose = existsAlly ? WinOrLose.Win : WinOrLose.Lose;
        currentWinOrLose = winOrLose;

        // バトル終了ログの差し込み
        var battleLog = new BattleLogInfo()
        {
            type = BattleLogType.Result,
            winOrLose = winOrLose,
            log = winOrLose == WinOrLose.Win ? "バトルに勝利しました" : "バトルに敗北しました",
        };
        battleLogList.Add(battleLog);
    }

    private void ExecutePassiveIfNeeded(SkillTriggerType triggerType, BattleMonsterIndex actionMonsterIndex = null, List<BattleMonsterIndex> beDoneActionMonsterIndexList = null)
    {
        switch (triggerType)
        {
            case SkillTriggerType.OnBattleStart:
                ExecuteOnBattleStartPassiveIfNeeded();
                break;
            case SkillTriggerType.OnMeActionEnd:
                if (actionMonsterIndex == null || chainParticipantMonsterIndexList.Contains(actionMonsterIndex)) return;

                var actionBattleMonster = GetBattleMonster(actionMonsterIndex);
                if (actionBattleMonster.isDead) return;

                var actionMonster = MasterRecord.GetMasterOf<MonsterMB>().Get(actionBattleMonster.monsterId);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(actionMonster.passiveSkillId);
                if (passiveSkill.triggerType == SkillTriggerType.OnMeActionEnd) StartActionStream(actionMonsterIndex, BattleActionType.PassiveSkill);
                break;
            default:
                break;
        }
    }

    private void ExecuteOnBattleStartPassiveIfNeeded()
    {
        GetAllMonsterList()
            .Where(m => {
                if (chainParticipantMonsterIndexList.Contains(m.index)) return false;
                if (m.isDead) return false;

                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                return passiveSkill.triggerType == SkillTriggerType.OnBattleStart;
            })
            .ToList()
            .ForEach(m => {
                StartActionStream(m.index, BattleActionType.PassiveSkill);
                chainParticipantMonsterIndexList.Clear();
            });
    }

    /// <summary>
    /// スキルの効果量を返す
    /// </summary>
    private int GetActionValue(BattleMonsterIndex doMonsterIndex, BattleMonsterIndex beDoneMonsterIndex, SkillEffectMI skillEffect)
    {
        // TODO: 攻撃受ける側の防御力計算
        var baseValue = GetTargetValue(doMonsterIndex, skillEffect);
        var random = UnityEngine.Random.Range(0.85f, 1.0f);
        var rate = (float)skillEffect.value / 100;
        var skillType = skillEffect.type;
        var skillTypeCoefficient =
            skillType == SkillType.Damage ? -1 :
            skillType == SkillType.Heal ? 1 :
            0;

        return (int)(baseValue * rate * random) * skillTypeCoefficient;
    }

    /// <summary>
    /// スキルの効果量の対象ステータス値を返す
    /// </summary>
    private float GetTargetValue(BattleMonsterIndex doMonsterIndex, SkillEffectMI skillEffect)
    {
        var battleMonster = GetBattleMonster(doMonsterIndex);
        switch (skillEffect.valueTargetType)
        {
            case ValueTargetType.MyCurrentAttack:
                return battleMonster.currentAttack;
            case ValueTargetType.MyCurrentDefense:
                return battleMonster.currentAttack;
            case ValueTargetType.MyCurrentHeal:
                return battleMonster.currentHeal;
            case ValueTargetType.MyCurrentSpeed:
                return battleMonster.currentSpeed;
            case ValueTargetType.MyCurrentHP:
                return battleMonster.currentHp;
            case ValueTargetType.MyMaxHp:
                return battleMonster.maxHp;
            default:
                return 0.0f;
        }
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
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        switch (actionType)
        {
            case BattleActionType.NormalSkill:
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.normalSkillId);
                return normalSkill.name;
            case BattleActionType.UltimateSkill:
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.ultimateSkillId);
                return ultimateSkill.name;
            case BattleActionType.PassiveSkill:
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                return passiveSkill.name;
            default:
                return "";
        }
    }

    private List<SkillEffectMI> GetSkillEffectList(BattleMonsterIndex monsterIndex, BattleActionType actionType)
    {
        var battleMonster = GetBattleMonster(monsterIndex);
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(battleMonster.monsterId);
        switch (actionType)
        {
            case BattleActionType.NormalSkill:
                var normalSkill = MasterRecord.GetMasterOf<NormalSkillMB>().Get(monster.normalSkillId);
                return normalSkill.effectList.Select(m => (SkillEffectMI)m).ToList();
            case BattleActionType.UltimateSkill:
                var ultimateSkill = MasterRecord.GetMasterOf<UltimateSkillMB>().Get(monster.ultimateSkillId);
                return ultimateSkill.effectList.Select(m => (SkillEffectMI)m).ToList();
            case BattleActionType.PassiveSkill:
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                return passiveSkill.effectList.Select(m => (SkillEffectMI)m).ToList();
            default:
                return new List<SkillEffectMI>();
        }
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex monsterIndex)
    {
        if (monsterIndex.isPlayer)
        {
            return playerBattleMonsterList[monsterIndex.index];
        }
        else
        {
            return enemyBattleMonsterList[monsterIndex.index];
        }
    }

    private List<BattleMonsterIndex> GetBeDoneMonsterIndexList(BattleMonsterIndex doMonsterIndex, SkillEffectMI skillEffect)
    {
        var isDoMonsterPlayer = doMonsterIndex.isPlayer;
        var allyBattleMonsterList = isDoMonsterPlayer ? this.playerBattleMonsterList : this.enemyBattleMonsterList;
        var enemyBattleMonsterList = isDoMonsterPlayer ? this.enemyBattleMonsterList : this.playerBattleMonsterList;
        allyBattleMonsterList = allyBattleMonsterList.Where(b => IsValid(b, skillEffect.activateConditionType)).ToList();
        enemyBattleMonsterList = enemyBattleMonsterList.Where(b => IsValid(b, skillEffect.activateConditionType)).ToList();

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
                battleMonsterIndexList = new List<BattleMonsterIndex>() { doMonsterIndex };
                break;
            case SkillTargetType.BeAttacked:
                battleMonsterIndexList = beDoneBattleMonsterDataList.Where(d => IsValid(d.battleMonsterIndex, skillEffect.activateConditionType)).Select(d => d.battleMonsterIndex).ToList();
                break;
            case SkillTargetType.AllyFrontAll:
                var allyFrontAll = allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                battleMonsterIndexList = allyFrontAll.Any() ? allyFrontAll : allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                break;
            case SkillTargetType.AllyBackAll:
                var allyBackAll = allyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                battleMonsterIndexList = allyBackAll.Any() ? allyBackAll : allyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyFrontAll:
                var enemyFrontAll = enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                // 前衛のモンスターが1体もいない場合は後衛全体を対象とする
                battleMonsterIndexList = enemyFrontAll.Any() ? enemyFrontAll : enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                break;
            case SkillTargetType.EnemyBackAll:
                var enemyBackAll = enemyBattleMonsterList.Where(b => IsBack(b.index)).Select(b => b.index).ToList();
                // 後衛のモンスターが1体もいない場合は前衛全体を対象とする
                battleMonsterIndexList = enemyBackAll.Any() ? enemyBackAll : enemyBattleMonsterList.Where(b => IsFront(b.index)).Select(b => b.index).ToList();
                break;
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
            case SkillTargetType.Target:
                battleMonsterIndexList = beDoneBattleMonsterDataList.Where(d => IsValid(d.battleMonsterIndex, skillEffect.activateConditionType)).Select(d => d.battleMonsterIndex).ToList();
                break;
            case SkillTargetType.None:
            default:
                battleMonsterIndexList = new List<BattleMonsterIndex>();
                break;
        }
        return battleMonsterIndexList;
    }

    private bool IsValid(BattleMonsterIndex battleMonsterIndex, ActivateConditionType activateConditionType)
    {
        var battleMonster = GetBattleMonster(battleMonsterIndex);
        return IsValid(battleMonster, activateConditionType);
    }

    private bool IsValid(BattleMonsterInfo battleMonster, ActivateConditionType activateConditionType)
    {
        switch (activateConditionType)
        {
            case ActivateConditionType.Under50PercentMyHP:
                // HPが50%未満ならOK
                return battleMonster.currentHp < battleMonster.maxHp / 2;
            case ActivateConditionType.Alive:
                // HPが0より多ければOK
                return battleMonster.currentHp > 0;
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

    private void SetPlayerBattleMonsterList(UserMonsterPartyInfo userMonsterParty)
    {
        playerBattleMonsterList.Clear();
        userMonsterParty.userMonsterIdList.ForEach((userMonsterId, index) =>
        {
            var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            if (userMonster != null)
            {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
                var battleMonster = BattleUtil.GetBattleMonster(monster, userMonster.customData.level, true, index);
                playerBattleMonsterList.Add(battleMonster);
            }
        });
    }

    private void RefreshEnemyBattleMonsterList(int waveCount)
    {
        var waveIndex = waveCount - 1;
        var questWave = questWaveList[waveIndex];

        enemyBattleMonsterList.Clear();
        questWave.questMonsterIdList.ForEach((questMonsterId, index) =>
        {
            var questMonster = MasterRecord.GetMasterOf<QuestMonsterMB>().GetAll().FirstOrDefault(m => m.id == questMonsterId);
            if (questMonster != null)
            {
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(questMonster.monsterId);
                var battleMonster = BattleUtil.GetBattleMonster(monster, questMonster.level, false, index);
                enemyBattleMonsterList.Add(battleMonster);
            }
        });
    }
}

    #region Setting
/*
    public static class MasterRecord
    {
        private static List<MonsterMB> monsterList = new List<MonsterMB>() {
            new MonsterMB() {
                id = 1,
                name = "モンスター1",
                attribute = MonsterAttribute.Red,
                passiveSkillId = 2,
                normalSkillId = 1,
                ultimateSkillId = 1,
            },
            new MonsterMB() {
                id = 2,
                name = "モンスター2",
                attribute = MonsterAttribute.Blue,
                passiveSkillId = 1,
                normalSkillId = 1,
                ultimateSkillId = 1,
            },
            new MonsterMB() {
                id = 3,
                name = "モンスター3",
                attribute = MonsterAttribute.Green,
                passiveSkillId = 3,
                normalSkillId = 1,
                ultimateSkillId = 1,
            },
        };
        private static List<PassiveSkillMB> passiveSkillList = new List<PassiveSkillMB>() {
            new PassiveSkillMB() {
                id = 1,
                name = "虚無",
                description = "何もしない",
                effectList = new List<PassiveSkillEffectMI>(),
                triggerType = SkillTriggerType.None,
            },
            new PassiveSkillMB() {
                id = 2,
                name = "毎ターンヒール",
                description = "自分のターン終了後に自身のHPを「自身の攻撃力の120%」分回復する。",
                effectList = new List<PassiveSkillEffectMI>() {
                    new PassiveSkillEffectMI() {
                        activateConditionType = ActivateConditionType.Alive,
                        skillTargetType = SkillTargetType.Myself,
                        valueTargetType = ValueTargetType.MyCurrentAttack,
                        type = SkillType.Heal,
                        value = 120,
                        battleConditionType = BattleConditionType.None,
                        durationTurnNum = 0,
                    },
                },
                triggerType = SkillTriggerType.OnMeActionEnd,
            },
            new PassiveSkillMB() {
                id = 3,
                name = "背水の陣",
                description = "バトル開始時に「自身の最大HPの90パーセント」のダメージを受ける",
                effectList = new List<PassiveSkillEffectMI>() {
                    new PassiveSkillEffectMI() {
                        activateConditionType = ActivateConditionType.Alive,
                        skillTargetType = SkillTargetType.Myself,
                        valueTargetType = ValueTargetType.MyMaxHp,
                        type = SkillType.Damage,
                        value = 90,
                        battleConditionType = BattleConditionType.None,
                        durationTurnNum = 0,
                    },
                },
                triggerType = SkillTriggerType.OnBattleStart,
            },
        };
        private static List<NormalSkillMB> normalSkillList = new List<NormalSkillMB>() {
            new NormalSkillMB() {
                id = 1,
                name = "一番前の敵に「自身の攻撃力の100%」のダメージ攻撃",
                description = "一番前の敵に「自身の攻撃力の100%」のダメージ攻撃",
                effectList = new List<NormalSkillEffectMI>() {
                    new NormalSkillEffectMI() {
                        skillTargetType = SkillTargetType.EnemyMostFront,
                        activateConditionType = ActivateConditionType.Alive,
                        valueTargetType = ValueTargetType.MyCurrentAttack,
                        type = SkillType.Damage,
                        value = 100,
                        battleConditionType = BattleConditionType.None,
                        durationTurnNum = 0,
                    },
                },
            },
        };
        private static List<UltimateSkillMB> ultimateSkillList = new List<UltimateSkillMB>() {
            new UltimateSkillMB() {
                id = 1,
                name = "敵全体に「自身の攻撃力の300%」のダメージ攻撃",
                description = "敵全体に「自身の攻撃力の300%」のダメージ攻撃",
                effectList = new List<UltimateSkillEffectMI>() {
                    new UltimateSkillEffectMI() {
                        skillTargetType = SkillTargetType.EnemyAll,
                        activateConditionType = ActivateConditionType.Alive,
                        valueTargetType = ValueTargetType.MyCurrentAttack,
                        type = SkillType.Damage,
                        value = 300,
                        battleConditionType = BattleConditionType.None,
                        durationTurnNum = 0,
                    },
                },
            },
        };

        public static Record<T> GetMasterOf<T>() where T : MasterBookBase
        {
            return new Record<T>();
        }

        public class Record<T> where T : MasterBookBase
        {
            private List<T> GetList()
            {
                switch (typeof(T).ToString())
                {
                    case "Battle.MonsterMB":
                        return (List<T>)(dynamic)monsterList;
                    case "Battle.NormalSkillMB":
                        return (List<T>)(dynamic)normalSkillList;
                    case "Battle.PassiveSkillMB":
                        return (List<T>)(dynamic)passiveSkillList;
                    case "Battle.UltimateSkillMB":
                        return (List<T>)(dynamic)ultimateSkillList;
                    default:
                        return null;
                }
            }

            public List<T> GetAll()
            {
                return GetList();
            }

            public T Get(long id)
            {
                return GetList().FirstOrDefault(m => {
                    return m.id == id;
                });
            }
        }
    }

    public class MasterBookBase
    {
        public long id { get; set; }
    }

    public class BattleDefaultData
    {
        public static List<BattleMonsterInfo> playerBattleMonsterList = new List<BattleMonsterInfo>() {
        new BattleMonsterInfo() {
            level = 100,
            maxHp = 1000,
            currentHp = 1000,
            baseAttack = 100,
            currentAttack = 100,
            baseDefense = 100,
            currentDefense = 100,
            baseSpeed = 100,
            currentSpeed = 100,
            maxEnergy = 100,
            currentEnergy = 0,
            battleConditionList = new List<BattleConditionInfo>(),
            isActed = false,
            index = new BattleMonsterIndex() {
                isPlayer = true,
                index = 0,
            },
            monsterId = 1,
        },
        new BattleMonsterInfo() {
            level = 100,
            maxHp = 1000,
            currentHp = 1000,
            baseAttack = 100,
            currentAttack = 100,
            baseDefense = 100,
            currentDefense = 100,
            baseSpeed = 100,
            currentSpeed = 100,
            maxEnergy = 100,
            currentEnergy = 0,
            battleConditionList = new List<BattleConditionInfo>(),
            isActed = false,
            index = new BattleMonsterIndex() {
                isPlayer = true,
                index = 1,
            },
            monsterId = 2,
        },
        new BattleMonsterInfo() {
            level = 100,
            maxHp = 1000,
            currentHp = 1000,
            baseAttack = 100,
            currentAttack = 100,
            baseDefense = 100,
            currentDefense = 100,
            baseSpeed = 100,
            currentSpeed = 100,
            maxEnergy = 100,
            currentEnergy = 0,
            battleConditionList = new List<BattleConditionInfo>(),
            isActed = false,
            index = new BattleMonsterIndex() {
                isPlayer = true,
                index = 2,
            },
            monsterId = 3,
        },
    };
        public static List<BattleMonsterInfo> enemyBattleMonsterList = new List<BattleMonsterInfo>() {
            new BattleMonsterInfo() {
                level = 100,
                maxHp = 1000,
                currentHp = 1000,
                baseAttack = 100,
                currentAttack = 100,
                baseDefense = 100,
                currentDefense = 100,
                baseSpeed = 100,
                currentSpeed = 100,
                maxEnergy = 100,
                currentEnergy = 0,
                battleConditionList = new List<BattleConditionInfo>(),
                isActed = false,
                index = new BattleMonsterIndex() {
                    isPlayer = false,
                    index = 0,
                },
                monsterId = 1,
            },
            new BattleMonsterInfo() {
                level = 100,
                maxHp = 1000,
                currentHp = 1000,
                baseAttack = 100,
                currentAttack = 100,
                baseDefense = 100,
                currentDefense = 100,
                baseSpeed = 100,
                currentSpeed = 100,
                maxEnergy = 100,
                currentEnergy = 0,
                battleConditionList = new List<BattleConditionInfo>(),
                isActed = false,
                index = new BattleMonsterIndex() {
                    isPlayer = false,
                    index = 1,
                },
                monsterId = 2,
            },
            new BattleMonsterInfo() {
                level = 100,
                maxHp = 1000,
                currentHp = 1000,
                baseAttack = 100,
                currentAttack = 100,
                baseDefense = 100,
                currentDefense = 100,
                baseSpeed = 100,
                currentSpeed = 100,
                maxEnergy = 100,
                currentEnergy = 0,
                battleConditionList = new List<BattleConditionInfo>(),
                isActed = false,
                index = new BattleMonsterIndex() {
                    isPlayer = false,
                    index = 2,
                },
                monsterId = 3,
            },
        };
    }

    public class BattleMonsterInfo
    {
        /// <summary>
        /// レベル
        /// </summary>
        public int level { get; set; }

        /// <summary>
        /// 最大体力
        /// </summary>
        public int maxHp { get; set; }

        /// <summary>
        /// 現在の体力
        /// </summary>
        public int currentHp { get; set; }

        /// <summary>
        /// 基準攻撃力
        /// </summary>
        public int baseAttack { get; set; }

        /// <summary>
        /// 現在の攻撃力
        /// </summary>
        public float currentAttack { get; set; }

        /// <summary>
        /// 基準防御力
        /// </summary>
        public int baseDefense { get; set; }

        /// <summary>
        /// 現在の防御力
        /// </summary>
        public float currentDefense { get; set; }

        /// <summary>
        /// 基準スピード
        /// </summary>
        public int baseSpeed { get; set; }

        /// <summary>
        /// 現在のスピード
        /// </summary>
        public float currentSpeed { get; set; }

        /// <summary>
        /// 基準回復力
        /// </summary>
        public int baseHeal { get; set; }

        /// <summary>
        /// 現在の回復力
        /// </summary>
        public float currentHeal { get; set; }

        /// <summary>
        /// 最大のエネルギー
        /// </summary>
        public int maxEnergy { get; set; }

        /// <summary>
        /// 現在のエネルギー
        /// </summary>
        public int currentEnergy { get; set; }

        /// <summary>
        /// 状態異常リスト
        /// </summary>
        public List<BattleConditionInfo> battleConditionList { get; set; }

        /// <summary>
        /// このターンすでに行動しているか否か
        /// </summary>
        public bool isActed { get; set; }

        /// <summary>
        /// 戦闘不能か否か
        /// </summary>
        public bool isDead { get; set; }

        /// <summary>
        /// バトルモンスターインデックス
        /// </summary>
        public BattleMonsterIndex index { get; set; }

        public long monsterId { get; set; }
    }

    public enum MonsterAttribute
    {
        Red = 1,
        Blue = 2,
        Green = 3,
        Yellow = 4,
        Purple = 5,
    }

    public class MonsterMB : MasterBookBase
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public MonsterAttribute attribute { get; set; }

        /// <summary>
        /// パッシブスキルID
        /// </summary>
        public long passiveSkillId { get; set; }

        /// <summary>
        /// 通常スキルID
        /// </summary>
        public long normalSkillId { get; set; }

        /// <summary>
        /// アルティメットスキルID
        /// </summary>
        public long ultimateSkillId { get; set; }
    }

    public class BattleMonsterIndex
    {
        /// <summary>
        /// プレイヤーのモンスターか否か
        /// </summary>
        public bool isPlayer { get; set; }

        /// <summary>
        /// インデックス
        /// </summary>
        public int index { get; set; }

        public BattleMonsterIndex() { }

        public BattleMonsterIndex(BattleMonsterIndex battleMonsterIndex)
        {
            this.isPlayer = battleMonsterIndex.isPlayer;
            this.index = battleMonsterIndex.index;
        }
    }

    public class BattleConditionInfo
    {
        /// <summary>
        /// 状態異常タイプ
        /// </summary>
        public BattleConditionType type;

        /// <summary>
        /// 残りターン数
        /// </summary>
        public int remainingTurnCount;
    }

    public class NormalSkillMB : MasterBookBase
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 説明
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// スキル効果リスト
        /// </summary>
        public List<NormalSkillEffectMI> effectList { get; set; }
    }

    public class UltimateSkillMB : MasterBookBase
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 説明
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// スキル効果リスト
        /// </summary>
        public List<UltimateSkillEffectMI> effectList { get; set; }
    }

    public class PassiveSkillMB : MasterBookBase
    {
        /// <summary>
        /// 名前
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// 説明
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// パッシブスキル効果リスト
        /// </summary>
        public List<PassiveSkillEffectMI> effectList { get; set; }

        /// <summary>
        /// スキルトリガータイプ
        /// </summary>
        public SkillTriggerType triggerType { get; set; }
    }

    public class SkillEffectMI
    {
        /// <summary>
        /// スキル対象タイプ
        /// </summary>
        public SkillTargetType skillTargetType { get; set; }

        /// <summary>
        /// 発動条件タイプ
        /// </summary>
        public ActivateConditionType activateConditionType { get; set; }

        /// <summary>
        /// 効果量の対象タイプ
        /// </summary>
        public ValueTargetType valueTargetType { get; set; }

        /// <summary>
        /// スキルタイプ
        /// </summary>
        public SkillType type { get; set; }

        /// <summary>
        /// 効果量（%）
        /// </summary>
        public int value { get; set; }

        /// <summary>
        /// 状態異常タイプ（状態異常用）
        /// </summary>
        public BattleConditionType battleConditionType { get; set; }

        /// <summary>
        /// 継続ターン数（状態異常用）
        /// </summary>
        public int durationTurnNum { get; set; }

        /// <summary>
        /// スキル演出ID
        /// </summary>
        public long skillFxId { get; set; }
    }

    /// <summary>
    /// 発動条件タイプ
    /// above: より大きい, upper: 以上
    /// under: 未満, lower: 以下
    /// </summary>
    public enum ActivateConditionType
    {
        None = 0,

        /// <summary>
        /// HPが50%未満の時
        /// </summary>
        Under50PercentMyHP = 1,

        /// <summary>
        /// 生きている時
        /// </summary>
        Alive = 2,
    }

    /// <summary>
    /// スキルタイプ
    /// </summary>
    public enum SkillType
    {
        None,

        /// <summary>
        /// 攻撃
        /// </summary>
        Damage,

        /// <summary>
        /// 回復
        /// </summary>
        Heal,

        /// <summary>
        /// 状態異常
        /// </summary>
        Condition,
    }

    /// <summary>
    /// 効果量ターゲットタイプ
    /// </summary>
    public enum ValueTargetType
    {
        None = 0,

        /// <summary>
        /// 自身の現在の体力
        /// </summary>
        MyCurrentHP = 1,

        /// <summary>
        /// 自身の現在の攻撃力
        /// </summary>
        MyCurrentAttack = 2,

        /// <summary>
        /// 自身の現在の防御力
        /// </summary>
        MyCurrentDefense = 3,

        /// <summary>
        /// 自身の現在の回復力
        /// </summary>
        MyCurrentHeal = 4,

        /// <summary>
        /// 自身の現在のスピード
        /// </summary>
        MyCurrentSpeed = 5,

        /// <summary>
        /// 自身の最大の体力
        /// </summary>
        MyMaxHp = 6,
    }

    /// <summary>
    /// 状態異常タイプ
    /// </summary>
    public enum BattleConditionType
    {
        None = 0,

        /// <summary>
        /// 攻撃力上昇
        /// </summary>
        AttackUp = 1,

        /// <summary>
        /// 攻撃力減少
        /// </summary>
        AttackDown = 2,

        /// <summary>
        /// 防御力上昇
        /// </summary>
        DefenseUp = 3,

        /// <summary>
        /// 防御力減少
        /// </summary>
        DefenseDown = 4,

        /// <summary>
        /// 回復力上昇
        /// </summary>
        HealUp = 5,

        /// <summary>
        /// 回復力減少
        /// </summary>
        HealDown = 6,

        /// <summary>
        /// スピード上昇
        /// </summary>
        SpeedUp = 7,

        /// <summary>
        /// スピード減少
        /// </summary>
        SpeedDown = 8,

        /// <summary>
        /// リジェネ状態
        /// </summary>
        Regeneration = 9,

        /// <summary>
        /// 継続ダメージ状態
        /// </summary>
        DamageOverTime = 10,
    }

    /// <summary>
    /// スキル対象タイプ
    /// </summary>
    public enum SkillTargetType
    {
        None = 0,

        /// <summary>
        /// 自分
        /// </summary>
        Myself = 1,

        /// <summary>
        /// 味方全体
        /// </summary>
        AllyAll = 2,

        /// <summary>
        /// 敵全体
        /// </summary>
        EnemyAll = 3,

        /// <summary>
        /// 味方全体の中からランダムで1体
        /// </summary>
        AllyAllRandom1 = 4,

        /// <summary>
        /// 味方全体の中からランダムで2体
        /// </summary>
        AllyAllRandom2 = 5,

        /// <summary>
        /// 味方全体の中からランダムで3体
        /// </summary>
        AllyAllRandom3 = 6,

        /// <summary>
        /// 味方全体の中からランダムで4体
        /// </summary>
        AllyAllRandom4 = 7,

        /// <summary>
        /// 味方全体の中からランダムで5体
        /// </summary>
        AllyAllRandom5 = 8,

        /// <summary>
        /// 敵全体の中からランダムで1体
        /// </summary>
        EnemyAllRandom1 = 9,

        /// <summary>
        /// 敵全体の中からランダムで2体
        /// </summary>
        EnemyAllRandom2 = 10,

        /// <summary>
        /// 敵全体の中からランダムで3体
        /// </summary>
        EnemyAllRandom3 = 11,

        /// <summary>
        /// 敵全体の中からランダムで4体
        /// </summary>
        EnemyAllRandom4 = 12,

        /// <summary>
        /// 敵全体の中からランダムで5体
        /// </summary>
        EnemyAllRandom5 = 13,

        /// <summary>
        /// 攻撃したモンスター
        /// </summary>
        DoAttack = 14,

        /// <summary>
        /// 攻撃されたモンスター
        /// </summary>
        BeAttacked = 15,

        /// <summary>
        /// 味方前衛全体
        /// </summary>
        AllyFrontAll = 16,

        /// <summary>
        /// 味方後衛全体
        /// </summary>
        AllyBackAll = 17,

        /// <summary>
        /// 敵前衛全体
        /// </summary>
        EnemyFrontAll = 18,

        /// <summary>
        /// 敵後衛全体
        /// </summary>
        EnemyBackAll = 19,

        /// <summary>
        /// 一番前の味方
        /// </summary>
        AllyMostFront = 20,

        /// <summary>
        /// 一番前の敵
        /// </summary>
        EnemyMostFront = 21,

        /// <summary>
        /// 味方全体の中からHPが低い順に1体
        /// </summary>
        AllyAllHPLowest1 = 22,

        /// <summary>
        /// 味方全体の中からHPが低い順に2体
        /// </summary>
        AllyAllHPLowest2 = 23,

        /// <summary>
        /// 味方全体の中からHPが低い順に3体
        /// </summary>
        AllyAllHPLowest3 = 24,

        /// <summary>
        /// 味方全体の中からHPが低い順に4体
        /// </summary>
        AllyAllHPLowest4 = 25,

        /// <summary>
        /// 敵全体の中からHPが低い順に1体
        /// </summary>
        EnemyAllHPLowest1 = 26,

        /// <summary>
        /// 敵全体の中からHPが低い順に2体
        /// </summary>
        EnemyAllHPLowest2 = 27,

        /// <summary>
        /// 敵全体の中からHPが低い順に3体
        /// </summary>
        EnemyAllHPLowest3 = 28,

        /// <summary>
        /// 敵全体の中からHPが低い順に4体
        /// </summary>
        EnemyAllHPLowest4 = 29,

        /// <summary>
        /// すでに対象にしたモンスター
        /// リストの最初の要素の対象
        /// </summary>
        Target = 30,
    }

    /// <summary>
    /// スキルトリガータイプ
    /// 時＋主語＋動作＋前後
    /// </summary>
    public enum SkillTriggerType
    {
        None = 0,

        /// <summary>
        /// 毎アクション毎
        /// </summary>
        EveryTimeEnd = 1,

        /// <summary>
        /// バトル開始時
        /// </summary>
        OnBattleStart = 2,

        /// <summary>
        /// 自分のターン終了時
        /// </summary>
        OnMeTurnEnd = 3,

        /// <summary>
        /// 自分の通常攻撃終了時
        /// </summary>
        OnMeNormalSkillEnd = 4,

        /// <summary>
        /// 自分のウルト終了時
        /// </summary>
        OnMeUltimateSkillEnd = 5,

        /// <summary>
        /// 自分がダメージを受けたとき
        /// </summary>
        OnMeTakeDamageEnd = 6,

        /// <summary>
        /// 自分が倒れたとき
        /// </summary>
        OnMeDeadEnd = 7,

        /// <summary>
        /// ウェーブ開始時
        /// </summary>
        OnWaveStart = 8,

        /// <summary>
        /// ターン開始時
        /// </summary>
        OnTurnStart = 8,

        /// <summary>
        /// 自分のアクション開始時
        /// </summary>
        OnMeActionStart = 9,

        /// <summary>
        /// 自分がアクション処理される前
        /// </summary>
        OnMeTakeActionBefore = 10,

        /// <summary>
        /// 自分がアクション処理された後
        /// </summary>
        OnMeTakeActionAfter = 11,

        /// <summary>
        /// 自分のアクション終了時
        /// </summary>
        OnMeActionEnd = 10,

        /// <summary>
        /// ターン終了時
        /// </summary>
        OnTurnEnd = 11,

        /// <summary>
        /// ウェーブ終了時
        /// </summary>
        OnWaveEnd = 12,
    }

    public class NormalSkillEffectMI : SkillEffectMI
    {

    }

    public class PassiveSkillEffectMI : SkillEffectMI
    {

    }

    public class UltimateSkillEffectMI : SkillEffectMI
    {

    }

    /// <summary>
    /// アクションタイプ
    /// </summary>
    public enum BattleActionType
    {
        None,
        NormalSkill,
        UltimateSkill,
        PassiveSkill,
    }

    /// <summary>
    /// ログタイプ
    /// </summary>
    public enum BattleLogType
    {
        /// <summary>
        /// ログ確認用
        /// </summary>
        None,

        /// <summary>
        /// ウェーブ進行ログ
        /// </summary>
        MoveWave,

        /// <summary>
        /// ターン進行ログ
        /// </summary>
        MoveTurn,

        /// <summary>
        /// アクション開始ログ
        /// </summary>
        StartAction,

        /// <summary>
        /// スキル効果発動ログ
        /// </summary>
        StartSkillEffect,

        /// <summary>
        /// 被アクションログ
        /// </summary>
        TakeAction,

        /// <summary>
        /// 死亡ログ
        /// </summary>
        Die,

        /// <summary>
        /// 被状態異常ログ
        /// </summary>
        TakeBattleCondition,

        /// <summary>
        /// バトル結果ログ
        /// </summary>
        Result,
    }

    /// <summary>
    /// バトルログ情報
    /// </summary>
    public class BattleLogInfo
    {
        /// <summary>
        /// バトルログタイプ
        /// </summary>
        public BattleLogType type { get; set; }

        /// <summary>
        /// プレイヤーバトルモンスターリスト
        /// </summary>
        public List<BattleMonsterInfo> playerBattleMonsterList { get; set; }

        /// <summary>
        /// 敵バトルモンスターリスト
        /// </summary>
        public List<BattleMonsterInfo> enemyBattleMonsterList { get; set; }

        /// <summary>
        /// する側のモンスターのインデックス
        /// </summary>
        public BattleMonsterIndex doBattleMonsterIndex { get; set; }

        /// <summary>
        /// 対象モンスターがされたことリスト
        /// </summary>
        public List<BeDoneBattleMonsterData> beDoneBattleMonsterDataList { get; set; }

        /// <summary>
        /// ウェーブ数
        /// </summary>
        public int waveCount { get; set; }

        /// <summary>
        /// ターン数
        /// </summary>
        public int turnCount { get; set; }

        /// <summary>
        /// 勝敗
        /// </summary>
        public WinOrLose winOrLose { get; set; }

        /// <summary>
        /// ログ文字列
        /// </summary>
        public string log { get; set; }

        /// <summary>
        /// スキル演出ID
        /// </summary>
        public long skillFxId { get; set; }
    }

    /// <summary>
    /// 勝敗判定
    /// </summary>
    public enum WinOrLose
    {
        None,
        Win,
        Lose,
        Continue,
    }

    /// <summary>
    /// スキル対象となったモンスターが何をされたのかを表す
    /// </summary>
    public class BeDoneBattleMonsterData
    {
        /// <summary>
        /// 対象のモンスターのインデックス
        /// </summary>
        public BattleMonsterIndex battleMonsterIndex { get; set; }

        /// <summary>
        /// HPの変化量
        /// </summary>
        public int hpChanges { get; set; }
    }

    public class BattleMonsterStatus
    {
        public float defaultHp { get; set; }
        public float defaultAttack { get; set; }
        public float currentMaxHp { get; set; }
        public float currentHp { get; set; }
        public float currentAttack { get; set; }
        public float currentMaxEnergy { get; set; }
        public float currentEnergy { get; set; }
    }

    public class BattleManagerTestActionDataSet : ITestAction
    {
        public List<ActionDataSet> GetActionDataSetList()
        {
            var actionDataSetList = new List<ActionDataSet>();

            actionDataSetList.Add(new ActionDataSet()
            {
                key = "バトル実行",
                action = new Action(() => {
                    var manager = new BattleManager();
                    manager.StartBattleObservable().Subscribe();
                }),
                assignKeyCode = UnityEngine.KeyCode.F1,
            });

            return actionDataSetList;
        }
    }

    public static class CloneExtensions
    {
        public static T Clone<T>(this T t)
        {
            var json = JsonConvert.SerializeObject(t);
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public static class IEnumerableExtensions
    {
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource, int> predicate)
        {
            var index = 0;
            foreach (TSource element in source)
            {
                predicate(element, index++);
            }
        }

        public static IEnumerable<TSource> Shuffle<TSource>(this IEnumerable<TSource> source)
        {
            return source.OrderBy(_ => Guid.NewGuid());
        }
    }

    public static class ConstManager
    {
        public static class Battle
        {
            public static float MAX_ENERGY_VALUE = 100.0f;

            /// <summary>
            /// 行動したときのエネルギー上昇量
            /// </summary>
            public static int ENERGY_RISE_VALUE_ON_ACT = 50;

            /// <summary>
            /// ダメージを受けたときのエネルギー上昇量
            /// </summary>
            public static int ENERGY_RISE_VALUE_ON_TAKE_DAMAGE = 25;

            /// <summary>
            /// 前衛のインデックスリスト
            /// </summary>
            public static List<int> FRONT_INDEX_LIST = new List<int>() { 0, 1 };

            /// <summary>
            /// 後衛のインデックスリスト
            /// </summary>
            public static List<int> BACK_INDEX_LIST = new List<int>() { 2, 3, 4 };
        }
    }
*/
    #endregion Setting