using GameBase;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;

public class BattleDataProcessor
{
    private int currentWaveCount;
    private int currentTurnCount;
    private QuestMB quest;
    private List<BattleMonsterInfo> playerBattleMonsterList;
    private List<BattleMonsterInfo> enemyBattleMonsterList;
    private BattleMonsterIndex doBattleMonsterIndex;
    private BattleMonsterIndex beDoneBattleMonsterIndex;
    private WinOrLose currentWinOrLose;
    
    public List<BattleLogInfo> GetBattleLogList(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        Init(userMonsterParty, quest);
        
        var battleLogList = new List<BattleLogInfo>();
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

        var nextActIndex = GetNextActBattleMonsterIndexOrDefault();
        if(nextActIndex == null)
        {
            if (enemyBattleMonsterList.Any(m => m.currentHp > 0))
            {
                // 次のターンへ
                MoveNextTurn();
                return new List<BattleLogInfo>() { GetCurrentBattleLogInfo(BattleLogType.MoveTurn) };
            }
            else
            {
                // 次のウェーブへ
                MoveNextWave();
                return new List<BattleLogInfo>() { GetCurrentBattleLogInfo(BattleLogType.MoveWave) };
            }
        }
        else
        {
            // 攻撃
            doBattleMonsterIndex = nextActIndex;

        }

        return new List<BattleLogInfo>();
    }

    /// <summary>
    /// 次のログタイプを計算する
    /// </summary>
    private BattleLogType CalculateBattleLogType()
    {
        currentWinOrLose = GetWinOrLose();

        // 勝敗が決まっていたら結果ログ
        if (currentWinOrLose != WinOrLose.Continue) return BattleLogType.Result;

        // 敵が全滅していたら次のウェーブ
        if (enemyBattleMonsterList.All(m => m.currentHp <= 0)) return BattleLogType.MoveWave;

        // それ以外は攻撃
        return BattleLogType.Attack;
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
    /// 指定したログタイプと現在のメンバ変数の値をもとにバトルログ情報を取得
    /// </summary>
    /// <returns></returns>
    private BattleLogInfo GetCurrentBattleLogInfo(BattleLogType type)
    {
        return new BattleLogInfo()
        {
            type = type,
            playerBattleMonsterList = playerBattleMonsterList,
            enemyBattleMonsterList = enemyBattleMonsterList,
            doBattleMonsterIndex = doBattleMonsterIndex,
            beDoneBattleMonsterIndex = beDoneBattleMonsterIndex,
            waveCount = currentWaveCount,
            turnCount = currentTurnCount,
            winOrLose = currentWinOrLose,
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
                var battleMonster = BattleUtil.GetBattleMonster(userMonster, true, index);
                playerBattleMonsterList.Add(battleMonster);
            }
            else
            {
                playerBattleMonsterList.Add(null);
            }
        });
    }

    private void RefreshEnemyBattleMonsterList(int waveCount)
    {
        var waveIndex = waveCount - 1;
        var questWaveId = quest.questWaveIdList[waveIndex];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);

        questWave.questMonsterIdList.ForEach((questMonsterId, index) =>
        {
            var questMonster = MasterRecord.GetMasterOf<QuestMonsterMB>().GetAll().FirstOrDefault(m => m.id == questMonsterId);
            if (questMonster != null)
            {
                var battleMonster = BattleUtil.GetBattleMonster(questMonster, false, index);
                enemyBattleMonsterList.Add(battleMonster);
            }
            else
            {
                enemyBattleMonsterList.Add(null);
            }
        });
    }

    
}
