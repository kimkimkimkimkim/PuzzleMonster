using GameBase;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected List<BattleMonsterBase> _playerMonsterBaseList;
    [SerializeField] protected List<BattleMonsterBase> _enemyMonsterBaseList;
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

                parent.SetBattleMonsterItem(item);
                item.Init(userMonster.monsterId, userMonster.customData.level);
            }
        });
    }

    public void SetEnemyMonsterImage(int waveCount)
    {
        _enemyMonsterBaseList.ForEach(monsterBase =>
        {
            foreach (GameObject child in monsterBase.transform)
            {
                Destroy(child);
            }
        });

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

                parent.SetBattleMonsterItem(item);
                item.Init(questMonster.monsterId, questMonster.level);
            }
        });
    }

    public IObservable<Unit> PlayAttackAnimationObservable(BattleMonsterIndex doBattleMonsterIndex, BattleMonsterIndex beDoneBattleMonsterIndex)
    {
        var isPlayer = doBattleMonsterIndex.isPlayer;
        var doMonsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var beDoneMonsterBaseList = isPlayer ? _enemyMonsterBaseList : _playerMonsterBaseList;
        var doMonsterRT = doMonsterBaseList[doBattleMonsterIndex.index].battleMonsterItem.GetComponent<RectTransform>();
        var beDoneMonsterBase = beDoneMonsterBaseList[beDoneBattleMonsterIndex.index];

        return VisualFxManager.Instance.PlayNormalAttackFxObservable(doMonsterRT, beDoneMonsterBase.transform, isPlayer);
    }

    public IObservable<Unit> PlayTakeDamageAnimationObservable(BattleMonsterIndex beDoneBattleMonsterIndex, int damage, int currentHp)
    {
        var isPlayer = beDoneBattleMonsterIndex.isPlayer;
        var monsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var monsterBase = monsterBaseList[beDoneBattleMonsterIndex.index];
        var slider = monsterBase.battleMonsterItem.GetComponent<BattleMonsterItem>().hpSlider;

        return VisualFxManager.Instance.PlayTakeDamageFxObservable(slider, monsterBase.transform, damage, currentHp);
    }

    public IObservable<Unit> PlayDieAnimationObservable(BattleMonsterIndex battleMonsterIndex)
    {
        var isPlayer = battleMonsterIndex.isPlayer;
        var monsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var monster = monsterBaseList[battleMonsterIndex.index].battleMonsterItem.gameObject;
        monster.SetActive(false);
        return Observable.ReturnUnit();
    }
    
    public IObservable<Unit> PlayWaveTitleFxObservable(int currentWaveCount, int maxWaveCount){
        return VisualFxManager.Instance.PlayWaveTitleFxObservable(_waveFxParent, currentWaveCount, maxWaveCount);
    }
}
