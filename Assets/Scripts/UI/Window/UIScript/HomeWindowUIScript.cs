using System;
using System.Collections;
using System.Collections.Generic;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

[ResourcePath("UI/Window/Window-Home")]
public class HomeWindowUIScript : WindowBase
{
    [SerializeField] protected Button _questButton;
    [SerializeField] protected Transform _monsterAreaParentBase;

    private QuestMB quest;
    private List<Transform> monsterAreaParentList = new List<Transform>();

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        quest = MasterRecord.GetMasterOf<QuestMB>().Get(2); // TODO: 挑戦するクエストの指定処理の追加

        _questButton.OnClickIntentAsObservable()
            .SelectMany(_ => BattleManager.Instance.BattleStartObservable(quest.id, 1)) // TODO: 実際のuserMonsterPartyIdを指定するように
            .Subscribe();

        SetMonsterImage();
    }

    private void SetMonsterImage()
    {
        var userMonsterParty = ApplicationContext.userData.userMonsterPartyList?.FirstOrDefault();
        if(userMonsterParty != null)
        {
            monsterAreaParentList.Clear();
            foreach(Transform child in _monsterAreaParentBase)
            {
                monsterAreaParentList.Add(child);
            }
            monsterAreaParentList = monsterAreaParentList.Shuffle().ToList();

            userMonsterParty.userMonsterIdList.ForEach((userMonsterId, index) =>
            {
                var parent = monsterAreaParentList[index];
                var userMonster = ApplicationContext.userInventory.userMonsterList.First(u => u.id == userMonsterId);
                var homeMonsterItem = UIManager.Instance.CreateContent<HomeMonsterItem>(parent);
                var rectTransform = homeMonsterItem.GetComponent<RectTransform>();
                homeMonsterItem.SetMonsterImage(userMonster.monsterId);
            });
        }
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
