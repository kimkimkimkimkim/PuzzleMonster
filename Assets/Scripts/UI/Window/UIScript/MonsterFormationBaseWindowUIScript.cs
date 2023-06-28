using GameBase;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using System.Linq;

public class MonsterFormationBaseWindowUIScript : WindowBase {
    [SerializeField] protected List<ToggleWithValue> _tabList;
    [SerializeField] protected List<PartyMonsterIconItem> _partyMonsterIconList;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected ToggleGroup _toggleGroup;

    protected int currentPartyIndex = 0;
    protected int selectedPartyMonsterIndex = -1;
    protected string selectedUserMonsterId = null;
    protected List<UserMonsterInfo> userMonsterList;
    protected UserMonsterPartyInfo currentUserMonsterParty;

    public override void Init(WindowInfo info) {
        base.Init(info);

        currentPartyIndex = (int)info.param["partyIndex"];

        userMonsterList = ApplicationContext.userData.userMonsterList.OrderBy(u => u.monsterId).ToList();

        // はずすアイコンようにnullデータを先頭に追加
        userMonsterList.Insert(0, null);

        SetTabChangeAction();
        SetCurrentUserMonsterParty();
        RefreshPartyUI();
        RefreshScroll();
    }

    private void SetTabChangeAction() {
        _tabList.ForEach(tab => {
            int partyIndex = tab.value; // ここではタブの値がパーティインデックスに対応する
            tab.isOn = partyIndex == currentPartyIndex;
        });

        _tabList.ForEach(tab => {
            tab.OnValueChangedIntentAsObservable()
                .Where(isOn => isOn)
                .Do(_ => {
                    var partyIndex = tab.value; // ここではタブの値がパーティインデックスに対応する
                    currentPartyIndex = partyIndex;
                    _toggleGroup.SetAllTogglesOff();
                    SetCurrentUserMonsterParty();
                    RefreshPartyUI();
                    RefreshScroll();
                })
                .Subscribe();
        });
    }

    private void SetCurrentUserMonsterParty() {
        currentUserMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex)?.Clone();
        if (currentUserMonsterParty == null) {
            currentUserMonsterParty = new UserMonsterPartyInfo() {
                partyIndex = currentPartyIndex,
                userMonsterIdList = Enumerable.Repeat<string>(null, ConstManager.Battle.MAX_PARTY_MEMBER_NUM).ToList(),
            };
        }
    }

    private void RefreshPartyUI() {
        if (currentUserMonsterParty == null) {
            _partyMonsterIconList.ForEach(i => i.ShowIconItem(false));
            return;
        }

        _partyMonsterIconList.ForEach((monsterIcon, index) => {
            var isOutOfIndex = index >= currentUserMonsterParty.userMonsterIdList.Count;
            var userMonsterId = isOutOfIndex ? null : currentUserMonsterParty.userMonsterIdList[index];
            var userMonster = userMonsterList.FirstOrDefault(u => u?.id == userMonsterId);
            var isSelected = selectedPartyMonsterIndex == index;

            if (userMonster == null) {
                monsterIcon.ShowIconItem(false);
                monsterIcon.ShowRarityImage(false);
                monsterIcon.ShowLevelText(false);
                monsterIcon.SetShowMonsterDetailDialogAction(false);
                monsterIcon.toggle.isOn = false;
            } else {
                var itemMI = ItemUtil.GetItemMI(userMonster);
                monsterIcon.ShowIconItem(true);
                monsterIcon.ShowRarityImage(true);
                monsterIcon.ShowLevelText(true);
                monsterIcon.SetIcon(itemMI);
                monsterIcon.SetShowMonsterDetailDialogAction(true);
                monsterIcon.toggle.isOn = isSelected;
            }

            monsterIcon.SetOnClickAction(() => {
                if (isSelected) {
                    // 選択中なら非選択状態に
                    selectedPartyMonsterIndex = -1;
                    RefreshPartyUI();
                } else {
                    if (selectedUserMonsterId == null && selectedPartyMonsterIndex < 0) {
                        // スクロールモンスターを選択中じゃないかつパーティモンスターも選択中じゃないなら選択状態に
                        monsterIcon.toggle.isOn = true;
                        selectedPartyMonsterIndex = index;
                    } else if (selectedUserMonsterId == null && selectedPartyMonsterIndex >= 0) {
                        // スクロールモンスターを選択中じゃないかつパーティモンスターを選択中なら選択中のパーティモンスターと交換
                        currentUserMonsterParty.userMonsterIdList[index] = currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex];
                        currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;
                        _toggleGroup.SetAllTogglesOff();
                        selectedPartyMonsterIndex = -1;
                        selectedUserMonsterId = null;
                        RefreshPartyUI();
                        ReloadScroll();
                    } else {
                        // スクロールモンスターを選択中ならそのモンスターと交換
                        currentUserMonsterParty.userMonsterIdList[index] = selectedUserMonsterId;
                        _toggleGroup.SetAllTogglesOff();
                        selectedPartyMonsterIndex = -1;
                        selectedUserMonsterId = null;
                        RefreshPartyUI();
                        ReloadScroll();
                    }
                }
            });
        });
    }

    /// <summary>
    /// 初期化（スクロールアイテムの削除・生成）を行う
    /// </summary>
    private void RefreshScroll() {
        _infiniteScroll.Clear();

        _infiniteScroll.Init(userMonsterList.Count, OnUpdateItem);
    }

    /// <summary>
    /// 初期化せず表示のみ変更する
    /// </summary>
    private void ReloadScroll() {
        _infiniteScroll.ChangeMaxDataCount(userMonsterList.Count);
        _infiniteScroll.UpdateCurrentDisplayItems();
    }

    private void OnUpdateItem(int index, GameObject item) {
        if ((userMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var userMonster = userMonsterList[index];
        var userMonsterId = userMonster?.id;

        scrollItem.SetToggleGroup(_toggleGroup);
        if (userMonster == null) {
            // はずすアイコン
            scrollItem.ShowIcon(false);
            scrollItem.ShowRarityImage(false);
            scrollItem.ShowLevelText(false);
            scrollItem.ShowText(true, "はずす");
            scrollItem.SetShowMonsterDetailDialogAction(false);
            scrollItem.SetOnClickAction(() => {
                if (selectedPartyMonsterIndex >= 0) {
                    // パーティモンスター選択中なら外す
                    currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = null;
                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    ReloadScroll();
                } else {
                    // それ以外なら選択状態をリセット
                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    ReloadScroll();
                }
            });
        } else {
            // モンスターアイコン
            var itemMI = ItemUtil.GetItemMI(userMonster);
            var isIncludedParty = currentUserMonsterParty.userMonsterIdList.Contains(userMonster.id);

            scrollItem.ShowText(true);
            scrollItem.SetIcon(itemMI);
            scrollItem.ShowRarityImage(true);
            scrollItem.ShowLevelText(true);
            scrollItem.ShowText(false);
            scrollItem.ShowGrayoutPanel(isIncludedParty, "編成中");
            scrollItem.SetShowMonsterDetailDialogAction(true);
            scrollItem.SetOnClickAction(() => {
                if (scrollItem.toggle.isOn) {
                    // このアイテムを選択中なら非選択に
                    scrollItem.toggle.isOn = false;
                    selectedUserMonsterId = null;
                } else {
                    if (selectedPartyMonsterIndex < 0) {
                        // 選択中じゃないかつパーティモンスター選択中じゃないなら選択状態に
                        scrollItem.toggle.isOn = true;
                        selectedUserMonsterId = userMonsterId;
                    } else {
                        // パーティモンスター選択中ならそのモンスターと交代
                        currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;
                        _toggleGroup.SetAllTogglesOff();
                        selectedPartyMonsterIndex = -1;
                        selectedUserMonsterId = null;
                        RefreshPartyUI();
                        ReloadScroll();
                    }
                }
            });
        }
    }

    public override void Open(WindowInfo info) {
    }

    public override void Back(WindowInfo info) {
    }

    public override void Close(WindowInfo info) {
        base.Close(info);
    }
}
