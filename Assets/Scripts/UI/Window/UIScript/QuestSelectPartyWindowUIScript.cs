using GameBase;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using System.Linq;

[ResourcePath("UI/Window/Window-QuestSelectParty")]
public class QuestSelectPartyWindowUIScript : WindowBase
{
    [SerializeField] protected TextMeshProUGUI _titleText;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected TabAnimationController _tabAnimationController;
    [SerializeField] protected List<ToggleWithValue> _tabList;
    [SerializeField] protected List<IconItem> _partyMonsterIconList;
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private int currentPartyIndex = 0;
    private UserMonsterPartyInfo userMonsterParty;
    private List<UserMonsterInfo> userMonsterList;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        var questId = (long)info.param["questId"];
        var quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex);
        userMonsterList = ApplicationContext.userInventory.userMonsterList;

        _titleText.text = quest.name;

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                return BattleManager.Instance.BattleStartObservable(questId, userMonsterParty.id);
            })
            .Subscribe();

        _tabAnimationController.SetTabChangeAnimation();
        SetTabChangeAction();
        RefreshPartyUI();
        RefreshScroll();
    }

    private void SetTabChangeAction()
    {
        _tabList.ForEach(tab =>
        {
            tab.OnValueChangedIntentAsObservable()
                .Where(isOn => isOn)
                .Do(_ =>
                {
                    var partyIndex = tab.value; // ここではタブの値がパーティインデックスに対応する
                    currentPartyIndex = partyIndex;
                    userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex);
                    RefreshPartyUI();
                    RefreshScroll();
                })
                .Subscribe();
        });
    }

    private void RefreshPartyUI()
    {
        if(userMonsterParty == null)
        {
            _partyMonsterIconList.ForEach(i => i.gameObject.SetActive(false));
            return;
        }

        userMonsterParty.userMonsterIdList.ForEach((userMonsterId,index) =>
        {
            var monsterIcon = _partyMonsterIconList[index];
            var userMonster = userMonsterList.FirstOrDefault(u => u.id == userMonsterId);
            if(userMonster != null)
            {
                var itemMI = ItemUtil.GetItemMI(userMonster);
                monsterIcon.gameObject.SetActive(true);
                monsterIcon.SetIcon(itemMI);
            }else
            {
                monsterIcon.gameObject.SetActive(false);
            }
        });
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        if (userMonsterList.Any()) _infiniteScroll.Init(userMonsterList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var userMonster = userMonsterList[index];
        var itemMI = ItemUtil.GetItemMI(userMonster);

        scrollItem.SetIcon(itemMI);
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
