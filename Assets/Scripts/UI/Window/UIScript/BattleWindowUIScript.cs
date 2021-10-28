using GameBase;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected List<GameObject> _playerMonsterBaseList;
    [SerializeField] protected List<GameObject> _enemyMonsterBaseList;

    private UserMonsterPartyInfo userMonsterParty;
    private QuestMB quest;

    public void Init(string userMonsterPartyId, long questId)
    {
        userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        SetPlayerMonsterImage();
        SetEnemyMonsterImage(1);
    }
    
    public static string GetLogText(BattleLogInfo battleLog){
        var log = battleLog.log;
        
        if(battleLog.log.Contains("{do}")){
            if(battleLog.doBattleMonsterIndex == null) return log;
            
            var battleMonsterList = battleLog.doBattleMonsterIndex.isPlayer ? battleLog.playerBattleMonsterList : battleLog.enemyBattleMonsterList;
            var doBattleMonster = battleMonsterList.First(m => m.index.index == battleLog.doBattleMonsterIndex.index);
            // TODO: 実際のモンスターを取得
            var monsterName = $"{(battleLog.doBattleMonsterIndex.isPlayer ? "" : "あいての")}モンスター{battleLog.doBattleMonsterIndex.index + 1}";
            log = log.Replace("{do}", monsterName);
        }
        
        if(battleLog.log.Contains("{beDone}")){
            if(battleLog.beDoneBattleMonsterIndex == null) return log;
            
            var battleMonsterList = battleLog.beDoneBattleMonsterIndex.isPlayer ? battleLog.playerBattleMonsterList : battleLog.enemyBattleMonsterList;
            var beDoneBattleMonster = battleMonsterList.First(m => m.index.index == battleLog.beDoneBattleMonsterIndex.index);
            // TODO: 実際のモンスターを取得
            var monsterName = $"{(battleLog.beDoneBattleMonsterIndex.isPlayer ? "" : "あいての")}モンスター{battleLog.beDoneBattleMonsterIndex.index + 1}";
            log = log.Replace("{beDone}", monsterName);
        }
        
        return log;
    }

    private void SetPlayerMonsterImage()
    {
        userMonsterParty.userMonsterIdList.ForEach((userMonsterId, index) =>
        {
            var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            if (userMonster != null) {
                var parent = _playerMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);

                item.Init(userMonster.monsterId, userMonster.customData.level);
            }
        });
    }

    public void SetEnemyMonsterImage(int waveCount)
    {
        var waveIndex = waveCount - 1;
        var questWaveId = quest.questWaveIdList[waveIndex];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);

        questWave.questMonsterIdList.ForEach((questMonsterId, index) =>
        {
            var questMonster = MasterRecord.GetMasterOf<QuestMonsterMB>().GetAll().FirstOrDefault(m => m.id == questMonsterId);
            if(questMonster != null)
            {
                var parent = _enemyMonsterBaseList[index];
                var item = UIManager.Instance.CreateContent<BattleMonsterItem>(parent.transform);

                item.Init(questMonster.monsterId, questMonster.level);
            }
        });
    }
}
