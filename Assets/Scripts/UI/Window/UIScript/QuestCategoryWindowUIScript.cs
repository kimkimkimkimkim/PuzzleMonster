using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Battle;
using PM.Enum.Quest;
using UniRx;
using UnityEngine;

[ResourcePath("UI/Window/Window-QuestCategory")]
public class QuestCategoryWindowUIScript : WindowBase
{
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected List<ToggleWithValue> _tabList;

    private const int NORMAL_QUEST_TAB_VALUE = 0;
    private const int EVENT_QUEST_TAB_VALUE = 1;
    private const int GUERRILLA_QUEST_TAB_VALUE = 2;

    private int currentTabValue = NORMAL_QUEST_TAB_VALUE;
    private List<QuestCategoryMB> targetQuestCategoryList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        SetTabChangeAction();
    }

    private void SetTabChangeAction()
    {
        _tabList.ForEach(tab =>
        {
            tab.OnValueChangedIntentAsObservable()
                .Where(isOn => isOn)
                .Do(_ =>
                {
                    currentTabValue = tab.value;
                    RefreshScroll();
                })
                .Subscribe();
        });
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        // カテゴリに含まれるクエストのうち一つでも表示条件を満たしているものがあれば表示する
        targetQuestCategoryList = MasterRecord.GetMasterOf<QuestCategoryMB>().GetAll()
            .Where(questCategory => questCategory.questType == GetQuestTypeFromTabValue(currentTabValue))
            .Where(questCategory =>
            {
                var questList = MasterRecord.GetMasterOf<QuestMB>().GetAll().Where(quest => quest.questCategoryId == questCategory.id).ToList();
                return questList.Any(quest => ConditionUtil.IsValid(ApplicationContext.userData, quest.displayConditionList));
            })
            .OrderBy(m => m.id)
            .ToList();

        _infiniteScroll.Init(targetQuestCategoryList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((targetQuestCategoryList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<QuestCategoryScrollItem>();
        var questCategory = targetQuestCategoryList[index];
        var questList = MasterRecord.GetMasterOf<QuestMB>().GetAll().Where(quest => quest.questCategoryId == questCategory.id).ToList();
        var userBattleList = ApplicationContext.userData.userBattleList;
        var canExecute = questList.Any(m => ConditionUtil.IsValid(ApplicationContext.userData, m.canExecuteConditionList));
        var isCompleted = questList.All(m => userBattleList.Any(u => u.questId == m.id && u.winOrLose == WinOrLose.Win && u.completedDate > DateTimeUtil.Epoch));

        scrollItem.SetText(questCategory.name);
        scrollItem.ShowGrayOutPanel(!canExecute);
        scrollItem.ShowCompleteImage(isCompleted);
        scrollItem.SetOnClickAction(() =>
        {
            QuestListWindowFactory.Create(new QuestListWindowRequest() { questCategoryId = questCategory.id }).Subscribe();
        });
    }

    private QuestType GetQuestTypeFromTabValue(int tabValue)
    {
        switch (tabValue) {
            case NORMAL_QUEST_TAB_VALUE:
                return QuestType.Normal;
            case EVENT_QUEST_TAB_VALUE:
                return QuestType.Event;
            case GUERRILLA_QUEST_TAB_VALUE:
                return QuestType.Guerrilla;
            default:
                return QuestType.Normal;
        }
    }

    public override void Open(WindowInfo info)
    {
        // 表示されるたびに更新したいのでここで実行する
        RefreshScroll();
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}
