using GameBase;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using System;

public class BattleDataProcessor
{
    private int currentWaveCount;
    private int currentTurnCount;
    private QuestMB quest;
    private List<BattleLogInfo> battleLogList = new List<BattleLogInfo>();
    private List<BattleMonsterInfo> playerBattleMonsterList = new List<BattleMonsterInfo>();
    private List<BattleMonsterInfo> enemyBattleMonsterList = new List<BattleMonsterInfo>();
    private BattleMonsterIndex doBattleMonsterIndex;
    private BattleMonsterIndex beDoneBattleMonsterIndex;
    private BattleMonsterActType doBattleMonsterActType;
    private WinOrLose currentWinOrLose;
    private BattlePhase currentBattlePhase;

    private void Init(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        this.quest = quest;

        currentWaveCount = 1;
        currentTurnCount = 1;
        currentWinOrLose = WinOrLose.Continue;
        doBattleMonsterIndex = null;
        beDoneBattleMonsterIndex = null;

        SetPlayerBattleMonsterList(userMonsterParty);
        RefreshEnemyBattleMonsterList(currentWaveCount);
    }

    public List<BattleLogInfo> GetBattleLogList(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        Init(userMonsterParty, quest);

        // 最初にウェーブ移動とターン移動のログを入れておく
        AddBattleLog(BattleLogType.MoveWave);
        AddBattleLog(BattleLogType.MoveTurn);

        // 勝敗が決まるまで続ける
        while (currentWinOrLose == WinOrLose.Continue)
        {
            ExecuteBattlePhase();
        }

        return battleLogList;
    }

    private void ExecuteBattlePhase()
    {
        switch (currentBattlePhase)
        {
            case BattlePhase.SetDoMonster:
                ExecuteSetDoMonsterPhase();
                break;
            case BattlePhase.MoveNextWave:
                ExecuteMoveNextWavePhase();
                break;
            case BattlePhase.MoveNextTurn:
                ExecuteMoveNextTurnPhase();
                break;
            case BattlePhase.SetDoSkill:
                ExecuteSetDoSkillPhase();
                break;
            case BattlePhase.SetBeDoneMonster:
                ExecuteSetBeDoneMonsterPhase();
                break;
            case BattlePhase.ActivateSkill:
                ExecuteActivateSkillPhase();
                break;
            case BattlePhase.ReflectSkill:
                ExecuteReflectSkillPhase();
                break;
            case BattlePhase.JudgeBattleEnd:
                ExecuteJudgeBattleEndPhase();
                break;
        }

        currentBattlePhase = GetNextBattlePhase(currentBattlePhase);
    }

    /// <summary>
    /// 次行動するモンスターのモンスターインデックスを設定する
    /// nullの場合もある
    /// </summary>
    private void ExecuteSetDoMonsterPhase()
    {
        // 敵味方含めたバトルモンスターリストを取得
        var battleMonsterList = new List<BattleMonsterInfo>(playerBattleMonsterList);
        battleMonsterList.AddRange(enemyBattleMonsterList);

        // まだ行動していないかつスピードが一番早いモンスターを取得
        var battleMonster = battleMonsterList.Where(m => !m.isActed && m.currentHp > 0).OrderByDescending(m => m.currentSpeed).FirstOrDefault();
        doBattleMonsterIndex = battleMonster?.index;
    }

    /// <summary>
    /// 必要であれば次のウェーブに進む
    /// </summary>
    private void ExecuteMoveNextWavePhase()
    {
        // 敵が全滅していたら次のウェーブへ
        if (enemyBattleMonsterList.All(m => m.currentHp <= 0))
        {
            currentWaveCount++;
            RefreshEnemyBattleMonsterList(currentWaveCount);

            // ログ追加
            AddBattleLog(BattleLogType.MoveWave);
        }
    }

    /// <summary>
    /// 必要であれば次のターンに進む
    /// </summary>
    private void ExecuteMoveNextTurnPhase()
    {
        // 次行動するモンスターがいなければ次のターンへ
        if (doBattleMonsterIndex == null)
        {
            currentTurnCount++;
            playerBattleMonsterList.ForEach(m => m.isActed = false);
            enemyBattleMonsterList.ForEach(m => m.isActed = false);
            doBattleMonsterIndex = null;
            beDoneBattleMonsterIndex = null;

            // ログ追加
            AddBattleLog(BattleLogType.MoveTurn);
        }
    }

    /// <summary>
    /// 次行動するモンスターの使用するスキルを決める
    /// </summary>
    private void ExecuteSetDoSkillPhase()
    {
        // CTが最大まで溜まっていればアルティメットスキルそうでなければ通常スキルを設定
        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
        doBattleMonsterActType = doBattleMonster.currentCt >= ConstManager.Battle.MAX_CT_VALUE ? BattleMonsterActType.UltimateSkill : BattleMonsterActType.NormalSkill;
    }

    /// <summary>
    /// 使用するスキルの対象を設定する
    /// </summary>
    private void ExecuteSetBeDoneMonsterPhase()
    {
        var battleMonsterList = doBattleMonsterIndex.isPlayer ? enemyBattleMonsterList : playerBattleMonsterList;
        var targetBattleMonsterInfo = battleMonsterList.Where(m => m.currentHp > 0).Shuffle().FirstOrDefault();

        beDoneBattleMonsterIndex = targetBattleMonsterInfo != null ? targetBattleMonsterInfo.index : null;
    }

    /// <summary>
    /// スキルを発動する
    /// </summary>
    private void ExecuteActivateSkillPhase()
    {
        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
        doBattleMonster.isActed = true;

        // ログを追加
        AddBattleLog(BattleLogType.StartAttack);
    }

    /// <summary>
    /// スキル効果を反映する
    /// </summary>
    private void ExecuteReflectSkillPhase()
    {
        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
        var beDoneBattleMonster = GetBattleMonster(beDoneBattleMonsterIndex);
        var random = new Random();
        var coefficient = 1.0f - (((float)random.NextDouble() * 0.15f) - 0.075f);
        var damage = (int)(doBattleMonster.currentAttack * coefficient);
        beDoneBattleMonster.currentHp -= damage;

        // ログを追加
        AddBattleLog(BattleLogType.TakeDamage, damage);
    }

    /// <summary>
    /// 勝敗を判定する
    /// </summary>
    private void ExecuteJudgeBattleEndPhase()
    {
        if (enemyBattleMonsterList.All(m => m.currentHp <= 0))
        {
            // 敵が全滅かつ最後のウェーブであれば勝利
            if (quest.questWaveIdList.Count == currentWaveCount)
            {
                currentWinOrLose = WinOrLose.Win;
                AddBattleLog(BattleLogType.Result);
                return;
            }
        }

        // 味方が全滅していたら負け
        if (playerBattleMonsterList.All(m => m.currentHp <= 0))
        {
            currentWinOrLose = WinOrLose.Lose;
            AddBattleLog(BattleLogType.Result);
            return;
        }

        // それ以外であれば続行
        currentWinOrLose = WinOrLose.Continue;
    }

    private void AddBattleLog(BattleLogType type, int damage = 0)
    {
        var log = "";

        switch (type)
        {
            case BattleLogType.StartAttack:
                log = $"{{do}}の「こうげき」";
                break;
            case BattleLogType.TakeDamage:
                log = $"{{beDone}}に{damage}のダメージ";
                break;
            case BattleLogType.Die:
                log = $"{{beDone}}はたおれた";
                break;
            case BattleLogType.MoveWave:
                log = $"Wave{currentWaveCount} スタート";
                break;
            case BattleLogType.MoveTurn:
                log = $"Turn{currentTurnCount} スタート";
                break;
            case BattleLogType.Result:
                log = currentWinOrLose == WinOrLose.Win ? "WIN" : "LOSE";
                break;
            default:
                break;
        }

        var battleLog = new BattleLogInfo()
        {
            type = type,
            playerBattleMonsterList = playerBattleMonsterList.Clone(),
            enemyBattleMonsterList = enemyBattleMonsterList.Clone(),
            doBattleMonsterIndex = doBattleMonsterIndex != null ? new BattleMonsterIndex(doBattleMonsterIndex) : null,
            beDoneBattleMonsterIndex = beDoneBattleMonsterIndex != null ? new BattleMonsterIndex(beDoneBattleMonsterIndex) : null,
            waveCount = currentWaveCount,
            turnCount = currentTurnCount,
            winOrLose = currentWinOrLose,
            log = log,
            damage = damage,
        };
        battleLogList.Add(battleLog);
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
        var questWaveId = quest.questWaveIdList[waveIndex];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);

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

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex battleMonsterIndex)
    {
        var battleMonsterList = battleMonsterIndex.isPlayer ? playerBattleMonsterList : enemyBattleMonsterList;
        return battleMonsterList[battleMonsterIndex.index];
    }

    private BattlePhase GetNextBattlePhase(BattlePhase currentBattlePhase)
    {
        var enumCount = Enum.GetNames(typeof(BattlePhase)).Length;
        var currentNum = (int)currentBattlePhase;
        var nextNum = currentNum == enumCount - 1 ? 0 : currentNum + 1;
        return (BattlePhase)nextNum;
    }

    private enum BattlePhase
    {
        /// <summary>
        /// 行動するモンスターを決める
        /// </summary>
        SetDoMonster = 0,

        /// <summary>
        /// 必要であれば次のウェーブに進む
        /// </summary>
        MoveNextWave,

        /// <summary>
        /// 必要であれば次のターンに進む
        /// </summary>
        MoveNextTurn,

        /// <summary>
        /// 行動するモンスターが使用するスキルを決める
        /// </summary>
        SetDoSkill,

        /// <summary>
        /// スキル対象のモンスターを決める
        /// </summary>
        SetBeDoneMonster,

        /// <summary>
        /// 行動を実行する
        /// </summary>
        ActivateSkill,

        /// <summary>
        /// スキルの効果を反映させる
        /// </summary>
        ReflectSkill,

        /// <summary>
        /// ゲームの勝敗判定を行う
        /// </summary>
        JudgeBattleEnd,
    }

    private enum BattleMonsterActType
    {
        PassiveSkill,
        NormalSkill,
        UltimateSkill,
    }
}
