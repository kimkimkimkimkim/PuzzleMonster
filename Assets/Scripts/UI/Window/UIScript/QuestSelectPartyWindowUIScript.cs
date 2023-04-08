using GameBase;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.UI;
using PM.Enum.Quest;

[ResourcePath("UI/Window/Window-QuestSelectParty")]
public class QuestSelectPartyWindowUIScript : WindowBase
{
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected List<ToggleWithValue> _tabList;
    [SerializeField] protected List<PartyMonsterIconItem> _partyMonsterIconList;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected ToggleGroup _toggleGroup;

    private QuestMB quest;
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
            while (_currentUserMonsterParty.userMonsterIdList.Count < ConstManager.Battle.MAX_PARTY_MEMBER_NUM)
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
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        currentUserMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex)?.Clone();
        userMonsterList = ApplicationContext.userData.userMonsterList.OrderBy(u => u.monsterId).ToList();

        // はずすアイコンようにnullデータを先頭に追加
        userMonsterList.Insert(0, null);

        _titleText.text = quest.name;

        _okButton.OnClickIntentAsObservable()
            .SelectMany(res =>
            {
                var isValidDisplayCondition = ConditionUtil.IsValid(ApplicationContext.userData, quest.displayConditionList);
                var isValidExecuteCondition = ConditionUtil.IsValid(ApplicationContext.userData, quest.canExecuteConditionList);
                var questCategory = MasterRecord.GetMasterOf<QuestCategoryMB>().Get(quest.questCategoryId);
                var isEventQuest = questCategory.questType == QuestType.Event;
                var isHolding = MasterRecord.GetMasterOf<EventQuestScheduleMB>().GetAll().Any(m =>
                {
                    return m.questCategoryId == questCategory.id && DateTimeUtil.GetDateFromMasterString(m.startDate) <= DateTimeUtil.Now && DateTimeUtil.Now < DateTimeUtil.GetDateFromMasterString(m.endDate);
                });

                if (!currentUserMonsterParty.userMonsterIdList.Any(id => id != null))
                {
                    return CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        content = "モンスターを1体以上選択してください",
                    }).AsUnitObservable();
                }
                else if (isValidDisplayCondition && isValidExecuteCondition && (!isEventQuest || (isEventQuest && isHolding)))
                {
                    // バトルを実行
                    return Observable.ReturnUnit()
                        .SelectMany(_ =>
                        {
                            var userMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.id == currentUserMonsterParty.id);
                            if (userMonsterParty != null && userMonsterParty.IsSame(currentUserMonsterParty))
                            {
                                // 現在選択中のパーティ情報が存在し何も変更がなければそのまま進む
                                return Observable.ReturnUnit();
                            }
                            else
                            {
                                // 変更が加えられていた場合はユーザーデータを更新する
                                return ApiConnection.UpdateUserMosnterFormation(currentPartyIndex, currentUserMonsterParty.userMonsterIdList)
                                    .Do(resp => currentUserMonsterParty = resp.userMonsterParty)
                                    .AsUnitObservable();
                            }
                        })
                        .SelectMany(_ => BattleManager.Instance.StartBattleObservable(questId, currentUserMonsterParty.id));
                }
                else
                {
                    // クエスト実行条件を満たしていない場合はホーム画面に戻る
                    return CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        title = "確認",
                        content = "クエスト実行条件を満たしていません\nホーム画面に戻ります",
                        commonDialogType = CommonDialogType.YesOnly,
                    })
                        .Do(_ =>
                        {
                            UIManager.Instance.CloseAllWindow(true);
                            HomeWindowFactory.Create(new HomeWindowRequest()).Subscribe();
                        })
                        .AsUnitObservable();
                }
            })
            .Subscribe();

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
                    currentUserMonsterParty = ApplicationContext.userData.userMonsterPartyList.FirstOrDefault(u => u.partyIndex == currentPartyIndex)?.Clone();
                    _toggleGroup.SetAllTogglesOff();
                    RefreshPartyUI();
                    RefreshScroll();
                })
                .Subscribe();
        });
    }

    private void RefreshPartyUI()
    {
        if (currentUserMonsterParty == null)
        {
            _partyMonsterIconList.ForEach(i => i.ShowIconItem(false));
            return;
        }

        _partyMonsterIconList.ForEach((monsterIcon, index) =>
        {
            var isOutOfIndex = index >= currentUserMonsterParty.userMonsterIdList.Count;
            var userMonsterId = isOutOfIndex ? null : currentUserMonsterParty.userMonsterIdList[index];
            var userMonster = userMonsterList.FirstOrDefault(u => u?.id == userMonsterId);
            var isSelected = selectedPartyMonsterIndex == index;

            if (userMonster == null)
            {
                monsterIcon.ShowIconItem(false);
                monsterIcon.toggle.isOn = false;
            }
            else
            {
                var itemMI = ItemUtil.GetItemMI(userMonster);
                monsterIcon.ShowIconItem(true);
                monsterIcon.iconItem.SetIcon(itemMI);
                monsterIcon.toggle.isOn = isSelected;
            }

            monsterIcon.SetOnClickAction(() =>
            {
                if (isSelected)
                {
                    // 選択中なら非選択状態に
                    selectedPartyMonsterIndex = -1;
                    RefreshPartyUI();
                }
                else
                {
                    if (selectedUserMonsterId == null && selectedPartyMonsterIndex < 0)
                    {
                        // スクロールモンスターを選択中じゃないかつパーティモンスターも選択中じゃないなら選択状態に
                        monsterIcon.toggle.isOn = true;
                        selectedPartyMonsterIndex = index;
                    }
                    else if (selectedUserMonsterId == null && selectedPartyMonsterIndex >= 0)
                    {
                        // スクロールモンスターを選択中じゃないかつパーティモンスターを選択中なら選択中のパーティモンスターと交換
                        currentUserMonsterParty.userMonsterIdList[index] = currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex];
                        currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;
                        _toggleGroup.SetAllTogglesOff();
                        selectedPartyMonsterIndex = -1;
                        selectedUserMonsterId = null;
                        RefreshPartyUI();
                        ReloadScroll();
                    }
                    else
                    {
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
    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        _infiniteScroll.Init(userMonsterList.Count, OnUpdateItem);
    }

    /// <summary>
    /// 初期化せず表示のみ変更する
    /// </summary>
    private void ReloadScroll()
    {
        _infiniteScroll.ChangeMaxDataCount(userMonsterList.Count);
        _infiniteScroll.UpdateCurrentDisplayItems();
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var userMonster = userMonsterList[index];
        var userMonsterId = userMonster?.id;

        scrollItem.SetToggleGroup(_toggleGroup);
        if (userMonster == null)
        {
            // はずすアイコン
            scrollItem.ShowIcon(false);
            scrollItem.ShowText(true, "はずす");
            scrollItem.SetOnClickAction(() =>
            {
                if (selectedPartyMonsterIndex > 0)
                {
                    // パーティモンスター選択中なら外す
                    currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = null;
                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    ReloadScroll();
                }
                else
                {
                    // それ以外なら選択状態をリセット
                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    ReloadScroll();
                }
            });
        }
        else
        {
            // モンスターアイコン
            var itemMI = ItemUtil.GetItemMI(userMonster);
            var isIncludedParty = currentUserMonsterParty.userMonsterIdList.Contains(userMonster.id);

            scrollItem.ShowText(true);
            scrollItem.SetIcon(itemMI);
            scrollItem.ShowText(false);
            scrollItem.ShowGrayoutPanel(isIncludedParty, "編成中");
            scrollItem.SetOnClickAction(() =>
            {
                if (scrollItem.toggle.isOn)
                {
                    // このアイテムを選択中なら非選択に
                    scrollItem.toggle.isOn = false;
                    selectedUserMonsterId = null;
                }
                else
                {
                    if (selectedPartyMonsterIndex < 0)
                    {
                        // 選択中じゃないかつパーティモンスター選択中じゃないなら選択状態に
                        scrollItem.toggle.isOn = true;
                        selectedUserMonsterId = userMonsterId;
                    }
                    else
                    {
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

    private enum GrayoutReason
    {
        None,
        NotExistsMonster,
        NotEnoughStamina,
        NotEnoughMaxStamina,
    }
}