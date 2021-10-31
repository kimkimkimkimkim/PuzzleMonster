using GameBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected List<GameObject> _playerMonsterBaseList;
    [SerializeField] protected List<GameObject> _enemyMonsterBaseList;
    [SerializeField] protected Transform _waveFxParent;

    private UserMonsterPartyInfo userMonsterParty;
    private QuestMB quest;

    public void Init(string userMonsterPartyId, long questId)
    {
        userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        SetPlayerMonsterImage();
        SetEnemyMonsterImage(1);
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
    
    public IObservable<Unit> PlayWaveTitleFxObservable(int currentWaveCount, int maxWaveCount){
        return VisualFxManager.Instance.PlayWaveTitleFxObservable(_waveFxParent, currentWaveCount, maxWaveCount);
    }
}
