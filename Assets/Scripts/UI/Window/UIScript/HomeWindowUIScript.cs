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
    [SerializeField] protected Button _missionButton;
    [SerializeField] protected Button _presentButton;
    [SerializeField] protected Transform _monsterAreaParentBase;

    private List<Transform> monsterAreaParentList = new List<Transform>();

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _questButton.OnClickIntentAsObservable()
            .SelectMany(_ => QuestCategoryWindowFactory.Create(new QuestCategoryWindowRequest()))
            .Subscribe();

        _missionButton.OnClickIntentAsObservable()
            .SelectMany(_ => MissionDialogFactory.Create(new MissionDialogRequest()))
            .Subscribe();

        _presentButton.OnClickIntentAsObservable()
            .SelectMany(_ => PresentDialogFactory.Create(new PresentDialogRequest()))
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
                var userMonster = ApplicationContext.userInventory.userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
                if (userMonster != null)
                {
                    var homeMonsterItem = UIManager.Instance.CreateContent<HomeMonsterItem>(parent);
                    var rectTransform = homeMonsterItem.GetComponent<RectTransform>();
                    homeMonsterItem.SetMonsterImage(userMonster.monsterId);
                }
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
