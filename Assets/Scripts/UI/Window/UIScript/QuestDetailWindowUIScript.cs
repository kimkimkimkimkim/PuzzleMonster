using GameBase;
using PM.Enum.Battle;
using PM.Enum.Item;
using PM.Enum.UI;
using System;
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

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                var userData = ApplicationContext.userData;
                var lastCalculatedStaminaDateTime = userData.lastCalculatedStaminaDateTime;
                var stamina = userData.stamina;
                var maxStamina = MasterRecord.GetMasterOf<StaminaMB>().GetAll().First(m => m.rank == userData.rank).stamina;
                var currentStamina = UserDataUtil.GetCurrentStaminaAndLastCalculatedStaminaDateTime(lastCalculatedStaminaDateTime, stamina, maxStamina).currentStamina;
                var enoughStamina = currentStamina >= quest.consumeStamina;
                var enoughMaxStamina = maxStamina >= quest.consumeStamina;

                if (!enoughMaxStamina)
                {
                    // 最大スタミナが足りない時
                    var rank = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.stamina >= quest.consumeStamina)?.rank ?? 0;
                    return CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        content = $"このクエストはランク{rank}以上で挑戦することができます",
                    }).AsUnitObservable();
                }
                else if (!enoughStamina)
                {
                    // 現在のスタミナが足りない時
                    return CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        content = "挑戦するためのスタミナが足りません",
                    }).AsUnitObservable();
                }
                else
                {
                    // クエスト実行可能な時
                    return QuestSelectPartyWindowFactory.Create(new QuestSelectPartyWindowRequest()
                    {
                        questId = quest.id,
                    }).AsUnitObservable();
                }
            })
            .Subscribe();

        _questNameText.text = quest.name;
        _consumeStaminaText.text = $"消費スタミナ: {quest.consumeStamina}";
        isCleared = ApplicationContext.userData.userBattleList.Any(u => u.questId == quest.id && u.winOrLose == WinOrLose.Win && u.completedDate > DateTimeUtil.Epoch);

        RefreshMonsterInfiniteScroll();
        RefreshNormalRewardInfiniteScroll();
        RefreshDropRewardInfiniteScroll();
        RefreshFirstRewardInfiniteScroll();
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

        scrollItem.SetIcon(monsterItem, monsterItem.itemType != ItemType.Monster);
    }

    private void RefreshNormalRewardInfiniteScroll()
    {
        _normalRewardInfiniteScroll.Clear();

        // ドロップ率100%のものに絞る
        normalRewardItemList = quest.dropItemList.Where(p => p.percent >= 100).Select(i => (ItemMI)i).ToList();

        _normalRewardInfiniteScroll.Init(normalRewardItemList.Count, OnUpdateNormalRewardItem);
    }

    private void OnUpdateNormalRewardItem(int index, GameObject item)
    {
        if ((normalRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var normalRewardItem = normalRewardItemList[index];

        scrollItem.SetIcon(normalRewardItem, normalRewardItem.itemType != ItemType.Monster);
    }

    private void RefreshDropRewardInfiniteScroll()
    {
        _dropRewardInfiniteScroll.Clear();

        // ドロップ率100%以外のものに絞る
        dropRewardItemList = quest.dropItemList.Where(p => p.percent < 100).Select(i => (ItemMI)i).ToList();

        _dropRewardInfiniteScroll.Init(dropRewardItemList.Count, OnUpdateDropRewardItem);
    }

    private void OnUpdateDropRewardItem(int index, GameObject item)
    {
        if ((dropRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var dropRewardItem = dropRewardItemList[index];

        scrollItem.SetIcon(dropRewardItem, dropRewardItem.itemType != ItemType.Monster);
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

        scrollItem.SetIcon(firstRewardItem, firstRewardItem.itemType != ItemType.Monster);
        scrollItem.ShowGrayoutPanel(isCleared);
        scrollItem.ShowCheckImage(isCleared);
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