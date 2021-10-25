using GameBase;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;

public class BattleDataProcessor
{
    private QuestMB quest;
    private List<BattleMonsterInfo> playerBattleMonsterList;
    private List<BattleMonsterInfo> enemyBattleMonsterList;
    private bool isBattleFinished = false;

    public void Init(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        this.quest = quest;

        SetPlayerBattleMonsterList(userMonsterParty);
        RefreshEnemyBattleMonsterList(1); // 初期化時はwave=1
    }
    
    public List<BattleLogInfo> GetBattleLogList(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        this.quest = quest;

        SetPlayerBattleMonsterList(userMonsterParty);
        RefreshEnemyBattleMonsterList(1); // 初期化時はwave=1
        
        var battleLogList = new List<BattleLogInfo>();
        while(!isBattleFinished){
            var battleLog = CalculateBattleLog();
            battleLogList.Add(battleLog);
        }
        
        return battleLogList;
    }
    
    public BattleLogInfo CalculateBattleLog(){
        var random = UnityEngine.Random.Range(0, 10);
        isBattleFinished = random == 0;
        return new BattleLogInfo();
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
                var battleMonster = BattleUtil.GetBattleMonster(userMonster);
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
                var battleMonster = BattleUtil.GetBattleMonster(questMonster);
                enemyBattleMonsterList.Add(battleMonster);
            }
            else
            {
                enemyBattleMonsterList.Add(null);
            }
        });
    }

    
}
