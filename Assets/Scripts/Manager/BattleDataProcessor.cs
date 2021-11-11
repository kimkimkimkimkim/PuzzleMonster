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
    private List<BattleMonsterInfo> playerBattleMonsterList = new List<BattleMonsterInfo>();
    private List<BattleMonsterInfo> enemyBattleMonsterList = new List<BattleMonsterInfo>();
    private BattleMonsterIndex doBattleMonsterIndex;
    private BattleMonsterIndex beDoneBattleMonsterIndex;
    private WinOrLose currentWinOrLose;
    
    public List<BattleLogInfo> GetBattleLogList(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        Init(userMonsterParty, quest);
        
        var battleLogList = new List<BattleLogInfo>();

        // 最初にWave1開始のログを追加する
        battleLogList.Add(GetCurrentBattleLogInfo(BattleLogType.MoveWave)); 

        while(currentWinOrLose == WinOrLose.Continue){
            battleLogList.AddRange(CalculateBattleLogList());
        }
        
        return battleLogList;
    }

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

    /// <summary>
    /// 現在のメンバ変数の状態からバトルログ情報を計算する
    /// </summary>
    private List<BattleLogInfo> CalculateBattleLogList(){
        currentWinOrLose = GetWinOrLose();
        if(currentWinOrLose != WinOrLose.Continue)
        {
            // 勝敗が決まっていたら終了
            return new List<BattleLogInfo>() { GetCurrentBattleLogInfo(BattleLogType.Result) };
        }

        // 勝敗が決まっていないかつ敵が全滅していたら次のウェーブへ
        if (enemyBattleMonsterList.All(m => m.currentHp <= 0))
        {
            MoveNextWave();
            return new List<BattleLogInfo>() { GetCurrentBattleLogInfo(BattleLogType.MoveWave) };
        }

        // 勝敗が決まっていないかつ敵が全滅していないかつ次行動できるモンスターがいない場合は次のターンへ
        var nextActIndex = GetNextActBattleMonsterIndexOrDefault();
        if(nextActIndex == null)
        {
            MoveNextTurn();
            return new List<BattleLogInfo>() { GetCurrentBattleLogInfo(BattleLogType.MoveTurn) };
        }

        // それ以外の場合は、どのモンスターかが行動する
        var actionLogList = new List<BattleLogInfo>();

        // 攻撃開始ログ
        doBattleMonsterIndex = nextActIndex;
        beDoneBattleMonsterIndex = GetAttackTargetBattleMonsterIndex(doBattleMonsterIndex); // TODO: 攻撃対象取得処理の実装
        var startAttackLog = GetCurrentBattleLogInfo(BattleLogType.StartAttack);
        actionLogList.Add(startAttackLog);

        // 被ダメージログ
        var takeDamageLog = GetCurrentBattleLogInfo(BattleLogType.TakeDamage);
        actionLogList.Add(takeDamageLog);

        // 死亡ログ
        var takeDamageBattleMonster = GetBattleMonster(beDoneBattleMonsterIndex);
        if(takeDamageBattleMonster.currentHp <= 0)
        {
            var dieLog = GetCurrentBattleLogInfo(BattleLogType.Die);
            actionLogList.Add(dieLog);
        }

        return actionLogList;
    }

    private BattleMonsterInfo GetBattleMonster(BattleMonsterIndex battleMonsterIndex)
    {
        var battleMonsterList = battleMonsterIndex.isPlayer ? playerBattleMonsterList : enemyBattleMonsterList;
        return battleMonsterList.First(m => m.index.index == battleMonsterIndex.index);
    }
    
    private BattleMonsterIndex GetAttackTargetBattleMonsterIndex(BattleMonsterIndex doBattleMonsterIndex)
    {
        var battleMonsterList = doBattleMonsterIndex.isPlayer ? enemyBattleMonsterList : playerBattleMonsterList;
        var targetBattleMonsterInfo = battleMonsterList.Where(m => m.currentHp > 0).Shuffle().FirstOrDefault();

        return targetBattleMonsterInfo != null ? targetBattleMonsterInfo.index : null;
    }

    /// <summary>
    /// 次のターンに進行する際の処理を実行
    /// </summary>
    private void MoveNextTurn()
    {
        currentTurnCount++;
        playerBattleMonsterList.ForEach(m => m.isActed = false);
        enemyBattleMonsterList.ForEach(m => m.isActed = false);
        doBattleMonsterIndex = null;
        beDoneBattleMonsterIndex = null;
    }

    /// <summary>
    /// 次のウェーブに進行する際の処理を実行
    /// </summary>
    private void MoveNextWave()
    {
        currentWaveCount++;
        RefreshEnemyBattleMonsterList(currentWaveCount);
    }

    /// <summary>
    /// 攻撃処理
    /// </summary>
    /// <returns>ダメージ</returns>
    private int Attack()
    {
        if (doBattleMonsterIndex == null || beDoneBattleMonsterIndex == null) return 0;

        var doBattleMonster = GetBattleMonster(doBattleMonsterIndex);
        var beDoneBattleMonster = GetBattleMonster(beDoneBattleMonsterIndex);
        var random = new Random();
        var coefficient = 1.0f - (((float)random.NextDouble() * 0.15f) - 0.075f);
        var damage = doBattleMonster.currentAttack * coefficient;
        beDoneBattleMonster.currentHp -= (int)damage;
        doBattleMonster.isActed = true;
        return (int)damage;
    }

    /// <summary>
    /// 指定したログタイプと現在のメンバ変数の値をもとにバトルログ情報を取得
    /// </summary>
    /// <returns></returns>
    private BattleLogInfo GetCurrentBattleLogInfo(BattleLogType type)
    {
        var log = "";
        var damage = 0;

        switch (type) {
            case BattleLogType.StartAttack:
                log = $"{{do}}の「こうげき」";
                break;
            case BattleLogType.TakeDamage:
                damage = Attack();
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

        return new BattleLogInfo()
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
    }

    /// <summary>
    /// 次行動するモンスターのバトルモンスターインデックスを取得する
    /// </summary>
    private BattleMonsterIndex GetNextActBattleMonsterIndexOrDefault()
    {
        var battleMonsterList = new List<BattleMonsterInfo>(playerBattleMonsterList);
        battleMonsterList.AddRange(enemyBattleMonsterList);

        var battleMonster = battleMonsterList.OrderByDescending(m => m.currentSpeed).Where(m => !m.isActed && m.currentHp > 0).FirstOrDefault();
        return battleMonster?.index;
    }

    /// <summary>
    /// 勝ち負けを取得する
    /// </summary>
    private WinOrLose GetWinOrLose()
    {
        if(enemyBattleMonsterList.All(m => m.currentHp <= 0))
        {
            if (quest.questWaveIdList.Count == currentWaveCount) return WinOrLose.Win;
        }

        if (playerBattleMonsterList.All(m => m.currentHp <= 0)) return WinOrLose.Lose;

        return WinOrLose.Continue;
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
                var battleMonster = BattleUtil.GetBattleMonster(monster,questMonster.level, false, index);
                enemyBattleMonsterList.Add(battleMonster);
            }
        });
    }

    
}
