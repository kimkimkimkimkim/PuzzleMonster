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
            var skillEffectList = GetSkillEffectList(actionMonsterIndex, actionType);
            StartActionStream(actionMonsterIndex, actionType, skillEffectList);
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
    private void StartActionStream(BattleMonsterIndex actionMonsterIndex, BattleActionType actionType, List<SkillEffectMI> skillEffectList)
    {
        // チェーン参加者リストに追加
        if (actionType == BattleActionType.PassiveSkill) chainParticipantMonsterIndexList.Add(actionMonsterIndex);

        // アクションを開始する
        StartAction(actionMonsterIndex, actionType);

        // アクション開始時パッシブスキルを発動する
        ExecutePassiveIfNeeded(SkillTriggerType.OnMeActionStart);

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
            playerBattleMonsterList = playerBattleMonsterList,
            enemyBattleMonsterList = enemyBattleMonsterList,
            doBattleMonsterIndex = monsterIndex,
            actionType = actionType,
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
            m.ChangeHp(actionValue);

            // エネルギーを上昇させる
            m.ChangeEnergy(ConstManager.Battle.ENERGY_RISE_VALUE_ON_TAKE_DAMAGE);

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
                var skillEffectList = passiveSkill.effectList.Where(effect => effect.triggerType == SkillTriggerType.OnMeActionEnd).Select(effect => (SkillEffectMI)effect).ToList();
                if (skillEffectList.Any()) StartActionStream(actionMonsterIndex, BattleActionType.PassiveSkill, skillEffectList);
                break;
            default:
                break;
        }
    }

    private void ExecuteOnBattleStartPassiveIfNeeded()
    {
        GetAllMonsterList()
            .ForEach(m => {
                if (chainParticipantMonsterIndexList.Contains(m.index)) return;
                if (m.isDead) return;

                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(m.monsterId);
                var passiveSkill = MasterRecord.GetMasterOf<PassiveSkillMB>().Get(monster.passiveSkillId);
                var effectList = passiveSkill.effectList.Where(effect => effect.triggerType == SkillTriggerType.OnBattleStart).Select(effect => (SkillEffectMI)effect).ToList();
                if (!effectList.Any()) return;

                StartActionStream(m.index, BattleActionType.PassiveSkill, effectList);
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
                return battleMonster.currentAttack();
            case ValueTargetType.MyCurrentDefense:
                return battleMonster.currentAttack();
            case ValueTargetType.MyCurrentHeal:
                return battleMonster.currentHeal();
            case ValueTargetType.MyCurrentSpeed:
                return battleMonster.currentSpeed();
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