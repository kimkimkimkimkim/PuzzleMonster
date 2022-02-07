using GameBase;
using PM.Enum.Item;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-QuestDetail")]
public class QuestDetailWindowUIScript : WindowBase
{
    [SerializeField] protected Button _okButton;
    [SerializeField] protected TextMeshProUGUI _questNameText;
    [SerializeField] protected InfiniteScroll _monsterInfiniteScroll;
    [SerializeField] protected InfiniteScroll _normalRewardInfiniteScroll;
    [SerializeField] protected InfiniteScroll _dropRewardInfiniteScroll;
    [SerializeField] protected InfiniteScroll _firstRewardInfiniteScroll;

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
    }

    private void RefreshMonsterInfiniteScroll()
    {
        _monsterInfiniteScroll.Clear();

        monsterItemList = quest.questWaveIdList
            .SelectMany(questWaveId =>
            {
                var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);
                var questMonsterList = questWave.questMonsterIdList
                    .Where(questMonsterId => questMonsterId != 0)
                    .Select(questMonsterId => MasterRecord.GetMasterOf<QuestMonsterMB>().Get(questMonsterId));
                var monsterIdList = questMonsterList.Select(m => m.monsterId);
                return monsterIdList;
            })
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

        if (monsterItemList.Any()) _monsterInfiniteScroll.Init(monsterItemList.Count, OnUpdateMonsterItem);
    }

    private void OnUpdateMonsterItem(int index, GameObject item)
    {
        if ((monsterItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItemHorizontal>();
        var monsterItem = monsterItemList[index];

        scrollItem.SetIcon(monsterItem);
    }

    private void RefreshNormalRewardInfiniteScroll()
    {
        _normalRewardInfiniteScroll.Clear();

        var bundle = MasterRecord.GetMasterOf<BundleMB>().Get(quest.dropBundleId);
        normalRewardItemList = bundle.itemList.Where(i => i.itemType != ItemType.DropTable).ToList();

        if (normalRewardItemList.Any()) _normalRewardInfiniteScroll.Init(normalRewardItemList.Count, OnUpdateNormalRewardItem);
    }

    private void OnUpdateNormalRewardItem(int index, GameObject item)
    {
        if ((normalRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItemHorizontal>();
        var normalRewardItem = normalRewardItemList[index];

        scrollItem.SetIcon(normalRewardItem);
    }

    private void RefreshDropRewardInfiniteScroll()
    {
        _dropRewardInfiniteScroll.Clear();

        var bundle = MasterRecord.GetMasterOf<BundleMB>().Get(quest.dropBundleId);
        dropRewardItemList = bundle.itemList
            .Where(i => i.itemType == ItemType.DropTable)
            .Select(i => MasterRecord.GetMasterOf<DropTableMB>().Get(i.itemId))
            .SelectMany(dropTable =>
            {
                return dropTable.itemList.Where(i => i.itemType != ItemType.None);
            })
            .Select(p =>
            {
                return new ItemMI()
                {
                    itemType = p.itemType,
                    itemId = p.itemId,
                    num = p.num,
                };
            })
            .ToList();

        if (dropRewardItemList.Any()) _dropRewardInfiniteScroll.Init(dropRewardItemList.Count, OnUpdateDropRewardItem);
    }

    private void OnUpdateDropRewardItem(int index, GameObject item)
    {
        if ((dropRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItemHorizontal>();
        var dropRewardItem = dropRewardItemList[index];

        scrollItem.SetIcon(dropRewardItem);
    }

    private void RefreshFirstRewardInfiniteScroll()
    {
        _firstRewardInfiniteScroll.Clear();

        var bundle = MasterRecord.GetMasterOf<BundleMB>().Get(quest.firstRewardBundleId);
        firstRewardItemList = bundle.itemList;

        if (firstRewardItemList.Any()) _firstRewardInfiniteScroll.Init(firstRewardItemList.Count, OnUpdateFirstRewardItem);
    }

    private void OnUpdateFirstRewardItem(int index, GameObject item)
    {
        if ((firstRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItemHorizontal>();
        var firstRewardItem = firstRewardItemList[index];

        scrollItem.SetIcon(firstRewardItem);
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
