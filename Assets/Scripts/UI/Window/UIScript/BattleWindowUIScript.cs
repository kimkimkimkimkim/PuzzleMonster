using GameBase;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected TextMeshProUGUI _turnText;
    [SerializeField] protected TextMeshProUGUI _waveText;
    [SerializeField] protected List<BattleMonsterBase> _playerMonsterBaseList;
    [SerializeField] protected List<BattleMonsterBase> _enemyMonsterBaseList;
    [SerializeField] protected Transform _fxParent;

    private UserMonsterPartyInfo userMonsterParty;
    private QuestMB quest;

    public void Init(string userMonsterPartyId, long questId)
    {
        userMonsterParty = ApplicationContext.userData.userMonsterPartyList.First(u => u.id == userMonsterPartyId);
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        SetPlayerMonsterImage();
        SetTurnText(1);
        SetWaveText(1, quest.questWaveIdList.Count);
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
            foreach (Transform child in monsterBase.transform)
            {
                Destroy(child.gameObject);
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

    public void SetTurnText(int turnCount)
    {
        _turnText.text = $"Turn {turnCount}";
    }

    public void SetWaveText(int waveCount, int maxWaveCount)
    {
        _waveText.text = $"Wave {waveCount}/{maxWaveCount}";
    }

    public IObservable<Unit> PlayStartAttackAnimationObservable(BattleMonsterIndex doBattleMonsterIndex)
    {
        var isPlayer = doBattleMonsterIndex.isPlayer;
        var doMonsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var doMonsterRT = doMonsterBaseList[doBattleMonsterIndex.index].battleMonsterItem.GetComponent<RectTransform>();
        return VisualFxManager.Instance.PlayStartAttackFxObservable(doMonsterRT, isPlayer);
    }

    public IObservable<Unit> PlayAttackAnimationObservable(BattleMonsterIndex doBattleMonsterIndex, List<BattleMonsterIndex> beDoneBattleMonsterIndexList)
    {
        var isPlayer = doBattleMonsterIndex.isPlayer;
        var doMonsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var beDoneMonsterBaseList = isPlayer ? _enemyMonsterBaseList : _playerMonsterBaseList;

        var doMonsterRT = doMonsterBaseList[doBattleMonsterIndex.index].battleMonsterItem.GetComponent<RectTransform>();
        var beDoneMonsterBaseTransformList = beDoneMonsterBaseList
            .Where((battleMonsterBase, index) => beDoneBattleMonsterIndexList.Select(battleMonsterIndex => battleMonsterIndex.index).Contains(index))
            .Select(battleMonsterBase => battleMonsterBase.transform)
            .ToList();

        return VisualFxManager.Instance.PlayNormalAttackFxObservable(doMonsterRT, beDoneMonsterBaseTransformList, isPlayer);
    }

    public IObservable<Unit> PlayTakeDamageAnimationObservable(BattleMonsterIndex beDoneBattleMonsterIndex,long skillFxId, int damage, int currentHp)
    {
        var isPlayer = beDoneBattleMonsterIndex.isPlayer;
        var monsterBaseList = isPlayer ? _playerMonsterBaseList : _enemyMonsterBaseList;
        var monsterBase = monsterBaseList[beDoneBattleMonsterIndex.index];
        var slider = monsterBase.battleMonsterItem.GetComponent<BattleMonsterItem>().hpSlider;

        return VisualFxManager.Instance.PlayTakeDamageFxObservable(slider, monsterBase.transform,skillFxId, damage, currentHp);
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
        SetEnemyMonsterImage(currentWaveCount);
        SetWaveText(currentWaveCount, maxWaveCount);
        return VisualFxManager.Instance.PlayWaveTitleFxObservable(_fxParent, currentWaveCount, maxWaveCount);
    }

    public IObservable<Unit> PlayTurnFxObservable(int currentTurn)
    {
        SetTurnText(currentTurn);
        return Observable.ReturnUnit();
    }

    public IObservable<Unit> PlayWinAnimationObservable()
    {
        return VisualFxManager.Instance.PlayWinBattleFxObservable(_fxParent).Delay(TimeSpan.FromSeconds(1));
    }

    public IObservable<Unit> PlayLoseAnimationObservable()
    {
        return VisualFxManager.Instance.PlayLoseBattleFxObservable(_fxParent).Delay(TimeSpan.FromSeconds(1)); ;
    }
}
