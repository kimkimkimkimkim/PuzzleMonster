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
    [SerializeField] protected GameObject _okButtonGrayoutPanel;
    [SerializeField] protected TabAnimationController _tabAnimationController;
    [SerializeField] protected List<ToggleWithValue> _tabList;
    [SerializeField] protected List<PartyMonsterIconItem> _partyMonsterIconList;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected ToggleGroup _toggleGroup;

    private int currentPartyIndex = 0;
    private int selectedPartyMonsterIndex = -1;
    private string selectedUserMonsterId = null;
    private List<UserMonsterInfo> userMonsterList;
    private UserMonsterPartyInfo _currentUserMonsterParty;
    // TODO: サーバーからnullが入った状態で返ってくるようになったらこのプロパティは削除
    private UserMonsterPartyInfo currentUserMonsterParty
    {
        get
        {
            // パーティ情報が無ければダミーデータを作成
            if (_currentUserMonsterParty == null)
            {
                _currentUserMonsterParty = new UserMonsterPartyInfo()
                {
                    id = null, 
                    partyIndex = currentPartyIndex,
                    userMonsterIdList = new List<string>(),
                };
            };

            // パーティメンバー数に達していなければダミーデータを追加
            while(_currentUserMonsterParty.userMonsterIdList.Count < ConstManager.Battle.MAX_PARTY_MEMBER_NUM)
            {
                _currentUserMonsterParty.userMonsterIdList.Add(null);
            }
            return _currentUserMonsterParty;
        }
        set
        {
            _currentUserMonsterParty = value;
        }
    }

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        var questId = (long)info.param["questId"];
        var quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        currentUserMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex)?.Clone();
        userMonsterList = ApplicationContext.userInventory.userMonsterList;

        _titleText.text = quest.name;

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.id == currentUserMonsterParty.id);
                if(userMonsterParty != null && userMonsterParty.IsSame(currentUserMonsterParty))
                {
                    // 現在選択中のパーティ情報が存在し何も変更がなければそのまま進む
                    return Observable.ReturnUnit();
                }
                else
                {
                    // 変更が加えられていた場合はユーザーデータを更新する
                    return ApiConnection.UpdateUserMosnterFormation(currentPartyIndex, currentUserMonsterParty.userMonsterIdList)
                        .Do(res => currentUserMonsterParty = res.userMonsterParty)
                        .AsUnitObservable();
                }
            })
            .SelectMany(_ =>
            {
                return BattleManager.Instance.BattleStartObservable(questId, currentUserMonsterParty.id);
            })
            .Subscribe();

        _tabAnimationController.SetTabChangeAnimation();
        SetTabChangeAction();
        RefreshPartyUI();
        RefreshScroll();
        RefreshGrayoutPanel();
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
                    currentUserMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex)?.Clone();
                    _toggleGroup.SetAllTogglesOff();
                    RefreshPartyUI();
                    RefreshScroll();
                    RefreshGrayoutPanel();
                })
                .Subscribe();
        });
    }

    private void RefreshPartyUI()
    {
        if(currentUserMonsterParty == null)
        {
            _partyMonsterIconList.ForEach(i => i.ShowIconItem(false));
            return;
        }

        _partyMonsterIconList.ForEach((monsterIcon, index) =>
        {
            var isOutOfIndex = index >= currentUserMonsterParty.userMonsterIdList.Count;
            var userMonsterId = isOutOfIndex ? null : currentUserMonsterParty.userMonsterIdList[index];
            var userMonster = userMonsterList.FirstOrDefault(u => u.id == userMonsterId);

            if (userMonster == null)
            {
                monsterIcon.ShowIconItem(false);
            }
            else
            {
                var itemMI = ItemUtil.GetItemMI(userMonster);
                monsterIcon.ShowIconItem(true);
                monsterIcon.iconItem.SetIcon(itemMI);
            }

            monsterIcon.SetOnClickAction(() =>
            {
                if(selectedPartyMonsterIndex != -1 && selectedPartyMonsterIndex != index && (selectedUserMonsterId != null || userMonsterId != null))
                {
                    // 選択中のモンスターがパーティ編成されている自分以外のモンスターの場合
                    currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;
                    currentUserMonsterParty.userMonsterIdList[index] = selectedUserMonsterId;

                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    RefreshScroll(); // TODO: 初期化せずに表示中のアイテムだけ更新したい
                    RefreshGrayoutPanel();
                }
                else if(selectedUserMonsterId != null)
                {
                    // 選択中のモンスターがパーティに編成されていない場合
                    currentUserMonsterParty.userMonsterIdList[index] = selectedUserMonsterId;

                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    RefreshScroll(); // TODO: 初期化せずに表示中のアイテムだけ更新したい
                    RefreshGrayoutPanel();
                }
                else
                {
                    // 選択中のモンスターが存在しないあるいは自分の場合
                    monsterIcon.toggle.isOn = !monsterIcon.toggle.isOn;
                    if (monsterIcon.toggle.isOn)
                    {
                        selectedPartyMonsterIndex = index;
                        selectedUserMonsterId = userMonsterId;
                    }
                    else
                    {
                        // 非選択にする場合は選択中ユーザーモンスターIDをnullに、Indexを-1に
                        selectedPartyMonsterIndex = -1;
                        selectedUserMonsterId = null;
                    }
                }
            });
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
        var isIncludedParty = currentUserMonsterParty.userMonsterIdList.Contains(userMonster.id);

        scrollItem.SetIcon(itemMI);
        scrollItem.SetToggleGroup(_toggleGroup);
        scrollItem.ShowGrayoutPanel(isIncludedParty, "編成中");
        scrollItem.SetOnClickAction(() =>
        { 
            if(selectedPartyMonsterIndex != -1)
            {
                // 編成中のモンスターを選択中
                currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonster.id;

                _toggleGroup.SetAllTogglesOff();
                selectedPartyMonsterIndex = -1;
                selectedUserMonsterId = null;
                RefreshPartyUI();
                RefreshScroll(); // TODO: 初期化せずに表示中のアイテムだけ更新したい
                RefreshGrayoutPanel();
            }
            else
            {
                // 通常通り選択
                scrollItem.toggle.isOn = !scrollItem.toggle.isOn;
                if (scrollItem.toggle.isOn)
                {
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = userMonster.id;
                }
                else
                {
                    // 非選択にする場合は選択中ユーザーモンスターIDをnullに、Indexを-1に
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                }
            }
        });
    }

    private void RefreshGrayoutPanel()
    {
        var existsMonster = currentUserMonsterParty.userMonsterIdList.Any(id => id != null);
        _okButtonGrayoutPanel.SetActive(!existsMonster);
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
