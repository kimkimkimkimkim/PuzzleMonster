using System.Collections.Generic;
using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-QuestList")]
public class QuestListWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<QuestMB> questList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        var questCategoryId = (long)info.param["questCategoryId"];
        questList = MasterRecord.GetMasterOf<QuestMB>().GetAll().Where(m => m.questCategoryId == questCategoryId).ToList();

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        if (questList.Any()) _infiniteScroll.Init(questList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((questList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<QuestCategoryScrollItem>();
        var quest = questList[index];

        scrollItem.SetText(quest.name);
        scrollItem.SetOnClickAction(() =>
        {
            BattleManager.Instance.BattleStartObservable(quest.id)
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
