using GameBase;
using PM.Enum.Battle;
using PM.Enum.Item;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-QuestDetail")]
public class QuestDetailWindowUIScript : WindowBase
{
    [SerializeField] protected Button _okButton;
    [SerializeField] protected Text _questNameText;
    [SerializeField] protected Text _consumeStaminaText;
    [SerializeField] protected InfiniteScroll _monsterInfiniteScroll;
    [SerializeField] protected InfiniteScroll _normalRewardInfiniteScroll;
    [SerializeField] protected InfiniteScroll _dropRewardInfiniteScroll;
    [SerializeField] protected InfiniteScroll _firstRewardInfiniteScroll;
    [SerializeField] protected GameObject _okButtonGrayoutPanel;

    private bool isCleared;
    private QuestMB quest;
    private List<ItemMI> monsterItemList;
    private List<ItemMI> normalRewardItemList;
    private List<ItemMI> dropRewardItemList;
    private List<ItemMI> firstRewardItemList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        var questId = (long)info.param["questId"];
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);

        _questNameText.text = quest.name;
        _consumeStaminaText.text = $"����X�^�~�i: {quest.consumeStamina}";
        isCleared = ApplicationContext.userData.userBattleList.Any(u => u.questId == quest.id && u.winOrLose == WinOrLose.Win && u.completedDate > DateTimeUtil.Epoch);

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                return QuestSelectPartyWindowFactory.Create(new QuestSelectPartyWindowRequest()
                {
                    questId = questId,
                });
            })
            .Subscribe();

        RefreshMonsterInfiniteScroll();
        RefreshNormalRewardInfiniteScroll();
        RefreshDropRewardInfiniteScroll();
        RefreshFirstRewardInfiniteScroll();
        RefreshGrayoutPanel();
    }

    private void RefreshMonsterInfiniteScroll()
    {
        _monsterInfiniteScroll.Clear();

        monsterItemList = quest.questMonsterListByWave
            .SelectMany(questMonsterList => questMonsterList.Select(questMonster => questMonster.monsterId))
            .Where(monsterId => monsterId > 0)
            .Distinct()
            .Select(monsterId =>
            {
                return new ItemMI()
                {
                    itemType = ItemType.Monster,
                    itemId = monsterId,
                };
            })
            .ToList();

        _monsterInfiniteScroll.Init(monsterItemList.Count, OnUpdateMonsterItem);
    }

    private void OnUpdateMonsterItem(int index, GameObject item)
    {
        if ((monsterItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var monsterItem = monsterItemList[index];

        scrollItem.SetIcon(monsterItem);
    }

    private void RefreshNormalRewardInfiniteScroll()
    {
        _normalRewardInfiniteScroll.Clear();

        // �h���b�v��100%�̂��̂ɍi��
        normalRewardItemList = quest.dropItemList.Where(p => p.percent >= 100).Select(i => (ItemMI)i).ToList();

        _normalRewardInfiniteScroll.Init(normalRewardItemList.Count, OnUpdateNormalRewardItem);
    }

    private void OnUpdateNormalRewardItem(int index, GameObject item)
    {
        if ((normalRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var normalRewardItem = normalRewardItemList[index];

        scrollItem.SetIcon(normalRewardItem);
    }

    private void RefreshDropRewardInfiniteScroll()
    {
        _dropRewardInfiniteScroll.Clear();

        // �h���b�v��100%�ȊO�̂��̂ɍi��
        dropRewardItemList = quest.dropItemList.Where(p => p.percent < 100).Select(i => (ItemMI)i).ToList();

        _dropRewardInfiniteScroll.Init(dropRewardItemList.Count, OnUpdateDropRewardItem);
    }

    private void OnUpdateDropRewardItem(int index, GameObject item)
    {
        if ((dropRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var dropRewardItem = dropRewardItemList[index];

        scrollItem.SetIcon(dropRewardItem);
    }

    private void RefreshFirstRewardInfiniteScroll()
    {
        _firstRewardInfiniteScroll.Clear();

        firstRewardItemList = quest.firstRewardItemList;

        _firstRewardInfiniteScroll.Init(firstRewardItemList.Count, OnUpdateFirstRewardItem);
    }

    private void OnUpdateFirstRewardItem(int index, GameObject item)
    {
        if ((firstRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var firstRewardItem = firstRewardItemList[index];

        scrollItem.SetIcon(firstRewardItem);
        scrollItem.ShowGrayoutPanel(isCleared);
        scrollItem.ShowCheckImage(isCleared);
    }

    private void RefreshGrayoutPanel()
    {
        var userData = ApplicationContext.userData;
        var lastCalculatedStaminaDateTime = userData.lastCalculatedStaminaDateTime;
        var stamina = userData.stamina;
        var maxStamina = MasterRecord.GetMasterOf<StaminaMB>().GetAll().First(m => m.rank == userData.rank).stamina;
        var currentStamina = UserDataUtil.GetCurrentStaminaAndLastCalculatedStaminaDateTime(lastCalculatedStaminaDateTime, stamina, maxStamina).currentStamina;
        var enoughStamina = userData.stamina >= quest.consumeStamina;
        _okButtonGrayoutPanel.SetActive(!enoughStamina);
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
