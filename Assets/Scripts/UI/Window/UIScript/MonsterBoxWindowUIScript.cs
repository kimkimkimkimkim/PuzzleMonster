using System.Collections.Generic;
using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-MonsterBox")]
public class MonsterBoxWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<UserMonsterInfo> userMonsterList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        userMonsterList = (List<UserMonsterInfo>)info.param["userMonsterList"];

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        _infiniteScroll.Init(userMonsterList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<MonsterBoxScrollItem>();
        var userMonster = userMonsterList[index];

        scrollItem.SetGradeImage(userMonster.customData.grade);
        scrollItem.SetMonsterImage(userMonster.monsterId);
        scrollItem.SetOnClickAction(() =>
        {
            MonsterDetailDialogFactory.Create(new MonsterDetailDialogRequest(){ userMonster = userMonster })
                .Where(res => res.isNeedRefresh)
                .Do(_ =>
                {
                    userMonsterList = ApplicationContext.userData.userMonsterList;
                    RefreshScroll();
                })
                .Subscribe();
        });
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
