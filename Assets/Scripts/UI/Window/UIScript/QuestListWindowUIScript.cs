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

        // ï\é¶èåèÇñûÇΩÇµÇΩÇ‡ÇÃÇæÇØï\é¶Ç∑ÇÈ
        var questCategoryId = (long)info.param["questCategoryId"];
        questList = MasterRecord.GetMasterOf<QuestMB>().GetAll()
            .Where(m => m.questCategoryId == questCategoryId)
            .Where(m => ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList))
            .ToList();

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
        var canExecute = ConditionUtil.IsValid(ApplicationContext.userData, quest.canExecuteConditionList);

        scrollItem.SetText(quest.name);
        scrollItem.ShowGrayOutPanel(!canExecute);
        scrollItem.SetOnClickAction(() =>
        {
            QuestDetailWindowFactory.Create(new QuestDetailWindowRequest() { questId = quest.id })
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
