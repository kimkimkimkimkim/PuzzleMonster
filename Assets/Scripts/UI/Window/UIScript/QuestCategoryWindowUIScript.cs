using System.Collections.Generic;
using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-QuestCategory")]
public class QuestCategoryWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<QuestCategoryMB> questCategoryList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        questCategoryList = MasterRecord.GetMasterOf<QuestCategoryMB>().GetAll().ToList();

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        if (questCategoryList.Any()) _infiniteScroll.Init(questCategoryList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((questCategoryList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<QuestCategoryScrollItem>();
        var questCategory = questCategoryList[index];

        scrollItem.SetText(questCategory.name);
        scrollItem.SetOnClickAction(() =>
        {

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
