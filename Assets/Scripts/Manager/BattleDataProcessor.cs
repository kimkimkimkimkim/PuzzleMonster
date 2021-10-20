using GameBase;
using System.Collections.Generic;
using System.Linq;

public class BattleDataProcessor
{
    private QuestMB quest;
    private List<BattleMonsterInfo> playerBattleMonsterList;
    private List<BattleMonsterInfo> enemyBattleMonsterList;

    public void Init(UserMonsterPartyInfo userMonsterParty, QuestMB quest)
    {
        this.quest = quest;

        SetPlayerBattleMonsterList(userMonsterParty);
        RefreshEnemyBattleMonsterList(1); // 初期化時はwave=1
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
