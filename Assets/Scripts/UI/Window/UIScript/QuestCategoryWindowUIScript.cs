using System.Collections.Generic;
using System.Linq;
using GameBase;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-QuestCategory")]
public class QuestCategoryWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<QuestCategoryMB> questCategoryList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        // カテゴリに含まれるクエストのうち一つでも表示条件を満たしているものがあれば表示する
        questCategoryList = MasterRecord.GetMasterOf<QuestCategoryMB>().GetAll()
            .Where(questCategory =>
            {
                var questList = MasterRecord.GetMasterOf<QuestMB>().GetAll().Where(quest => quest.questCategoryId == questCategory.id).ToList();
                return questList.Any(quest => ConditionUtil.IsValid(ApplicationContext.userData,quest.displayConditionList));
            })
            .ToList();

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
            QuestListWindowFactory.Create(new QuestListWindowRequest() { questCategoryId = questCategory.id }).Subscribe();
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
