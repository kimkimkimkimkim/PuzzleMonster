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
    [SerializeField] protected Button _grayoutButton;
    [SerializeField] protected GameObject _okButtonGrayoutPanel;
    [SerializeField] protected List<ToggleWithValue> _tabList;
    [SerializeField] protected List<PartyMonsterIconItem> _partyMonsterIconList;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected ToggleGroup _toggleGroup;

    private GrayoutReason grayoutReason;
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
                var isHolding = MasterRecord.GetMasterOf<EventQuestScheduleMB>().GetAll().Any(m => {
                    return m.questCategoryId == questCategory.id && DateTimeUtil.GetDateFromMasterString(m.startDate) <= DateTimeUtil.Now && DateTimeUtil.Now < DateTimeUtil.GetDateFromMasterString(m.endDate);
                });

                if (isValidDisplayCondition && isValidExecuteCondition && (!isEventQuest || (isEventQuest && isHolding)))
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

        _grayoutButton.OnClickAsObservable()
            .SelectMany(_ =>
            {
                switch (grayoutReason)
                {
                    case GrayoutReason.NotExistsMonster:
                        return CommonDialogFactory.Create(new CommonDialogRequest()
                        {
                            commonDialogType = CommonDialogType.YesOnly,
                            content = "モンスターを1体以上選択してください",
                        }).AsUnitObservable();
                    case GrayoutReason.NotEnoughStamina:
                        return CommonDialogFactory.Create(new CommonDialogRequest()
                        {
                            commonDialogType = CommonDialogType.YesOnly,
                            content = "挑戦するためのスタミナが足りません",
                        }).AsUnitObservable();
                    case GrayoutReason.NotEnoughMaxStamina:
                        var rank = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.stamina >= quest.consumeStamina)?.rank ?? 0;
                        return CommonDialogFactory.Create(new CommonDialogRequest()
                        {
                            commonDialogType = CommonDialogType.YesOnly,
                            content = $"このクエストはランク{rank}以上で挑戦することができます",
                        }).AsUnitObservable();
                    default:
                        return Observable.ReturnUnit();
                }
            })
            .Subscribe();

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
            var userMonster = userMonsterList.FirstOrDefault(u => u?.id == userMonsterId);

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
                    ReloadScroll();
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
                    ReloadScroll();
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
    private void ReloadScroll() {
        _infiniteScroll.ChangeMaxDataCount(userMonsterList.Count);
        _infiniteScroll.UpdateCurrentDisplayItems();
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var userMonster = userMonsterList[index];
        var userMonsterId = userMonster?.id;

        if(userMonster == null)
        {
            // はずすアイコン
            scrollItem.ShowIcon(false);
            scrollItem.ShowText(true, "はずす");
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
        }

        scrollItem.SetToggleGroup(_toggleGroup);
        scrollItem.SetOnClickAction(() =>
        { 
            if(selectedPartyMonsterIndex != -1)
            {
                // 編成中のモンスターを選択中
                currentUserMonsterParty.userMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;

                _toggleGroup.SetAllTogglesOff();
                selectedPartyMonsterIndex = -1;
                selectedUserMonsterId = null;
                RefreshPartyUI();
                ReloadScroll();
                RefreshGrayoutPanel();
            }
            else
            {
                // 通常通り選択
                scrollItem.toggle.isOn = !scrollItem.toggle.isOn;
                if (scrollItem.toggle.isOn)
                {
                    selectedPartyMonsterIndex = -1;
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
    }

    private void RefreshGrayoutPanel()
    {
        var existsMonster = currentUserMonsterParty.userMonsterIdList.Any(id => id != null);
        var enoughStamina = ApplicationContext.userData.stamina >= quest.consumeStamina;
        var rank = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.rank == ApplicationContext.userData.rank);
        var maxStamina = rank?.stamina ?? 0;
        var enoughMaxStamina = maxStamina >= quest.consumeStamina;

        grayoutReason = 
            !existsMonster ? GrayoutReason.NotExistsMonster
            : !enoughStamina ? GrayoutReason.NotEnoughStamina
            : !enoughMaxStamina ? GrayoutReason.NotEnoughMaxStamina
            : GrayoutReason.None;

        _okButtonGrayoutPanel.SetActive(grayoutReason != GrayoutReason.None);
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
