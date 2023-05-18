using GameBase;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-DevelopInputBattleSimulationInfo")]
public class DevelopInputBattleSimulationInfoWindowUIScript : WindowBase
{
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Button _prevButton;
    [SerializeField] protected Button _nextButton;
    [SerializeField] protected Button _startBattleButton;
    [SerializeField] protected GameObject _prevButtonGrayoutPanel;
    [SerializeField] protected GameObject _nextButtonGrayoutPanel;
    [SerializeField] protected GameObject _startBattleButtonGrayoutPanel;
    [SerializeField] protected List<PartyMonsterIconItem> _partyMonsterIconList;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected ToggleGroup _toggleGroup;

    private int phaseNumber = 0;
    private int selectedPartyMonsterIndex = -1;
    private string selectedUserMonsterId = null;
    private List<UserMonsterInfo> userMonsterListForScroll;
    private List<UserMonsterInfo> userMonsterListForBattle;
    private List<string> partyUserMonsterIdList = new List<string>();
    private List<List<QuestMonsterMI>> questMonsterListByWave = new List<List<QuestMonsterMI>>();

    private QuestMB quest = new QuestMB()
    {
        id = 0,
        name = "バトルシミュレーション",
        questCategoryId = 0,
        firstRewardItemList = new List<ItemMI>(),
        dropItemList = new List<ProbabilityItemMI>(),
        questMonsterListByWave = new List<List<QuestMonsterMI>>(),
        displayConditionList = new List<ConditionMI>(),
        canExecuteConditionList = new List<ConditionMI>(),
        consumeStamina = 0,
        limitTurnNum = 99,
        isLastWaveBoss = true,
    };

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        _prevButton.OnClickIntentAsObservable()
            .Do(_ => OnClickPrevButtonAction())
            .Subscribe();

        _nextButton.OnClickIntentAsObservable()
            .Do(_ => OnClickNextButtonAction())
            .Subscribe();

        _startBattleButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                quest.questMonsterListByWave = questMonsterListByWave;
                return BattleManager.Instance.StartBattleSimulationObservable(userMonsterListForBattle, quest);
            })
            .Subscribe();

        ResetUI();
    }

    private void RefreshPartyUI()
    {
        _partyMonsterIconList.ForEach((monsterIcon, index) =>
        {
            var isOutOfIndex = index >= partyUserMonsterIdList.Count;
            var userMonsterId = isOutOfIndex ? null : partyUserMonsterIdList[index];
            var userMonster = userMonsterListForScroll.FirstOrDefault(u => u?.id == userMonsterId);

            if (userMonster == null)
            {
                monsterIcon.ShowIconItem(false);
            }
            else
            {
                var itemMI = ItemUtil.GetItemMI(userMonster);
                monsterIcon.ShowIconItem(true);
                monsterIcon.SetIcon(itemMI);
            }

            monsterIcon.SetOnClickAction(() =>
            {
                if (selectedPartyMonsterIndex != -1 && selectedPartyMonsterIndex != index && (selectedUserMonsterId != null || userMonsterId != null))
                {
                    // 選択中のモンスターがパーティ編成されている自分以外のモンスターの場合
                    partyUserMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;
                    partyUserMonsterIdList[index] = selectedUserMonsterId;

                    _toggleGroup.SetAllTogglesOff();
                    selectedPartyMonsterIndex = -1;
                    selectedUserMonsterId = null;
                    RefreshPartyUI();
                    ReloadScroll();
                    RefreshGrayoutPanel();
                }
                else if (selectedUserMonsterId != null)
                {
                    // 選択中のモンスターがパーティに編成されていない場合
                    partyUserMonsterIdList[index] = selectedUserMonsterId;

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

        userMonsterListForScroll = MasterRecord.GetMasterOf<MonsterMB>().GetAll()
            .Select(m => new UserMonsterInfo()
            {
                id = m.id.ToString(),
                monsterId = m.id,
                num = 1,
                customData = new UserMonsterCustomData()
                {
                    level = 60,
                    exp = 0,
                    grade = 3,
                    luck = 0,
                },
            })
            .ToList();

        // はずすアイコンようにnullデータを先頭に追加
        userMonsterListForScroll.Insert(0, null);

        _infiniteScroll.Init(userMonsterListForScroll.Count, OnUpdateItem);
    }

    /// <summary>
    /// 初期化せず表示のみ変更する
    /// </summary>
    private void ReloadScroll()
    {
        _infiniteScroll.ChangeMaxDataCount(userMonsterListForScroll.Count);
        _infiniteScroll.UpdateCurrentDisplayItems();
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userMonsterListForScroll.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var userMonster = userMonsterListForScroll[index];
        var userMonsterId = userMonster?.id;

        if (userMonster == null)
        {
            // はずすアイコン
            scrollItem.ShowIcon(false);
            scrollItem.ShowText(true, "はずす");
        }
        else
        {
            // モンスターアイコン
            var itemMI = ItemUtil.GetItemMI(userMonster);
            var isIncludedParty = partyUserMonsterIdList.Contains(userMonster.id);

            scrollItem.ShowText(true);
            scrollItem.SetIcon(itemMI);
            scrollItem.ShowText(false);
            scrollItem.ShowGrayoutPanel(isIncludedParty, "編成中");
        }

        scrollItem.SetToggleGroup(_toggleGroup);
        scrollItem.SetOnClickAction(() =>
        {
            if (selectedPartyMonsterIndex != -1)
            {
                // 編成中のモンスターを選択中
                partyUserMonsterIdList[selectedPartyMonsterIndex] = userMonsterId;

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

    private void OnClickPrevButtonAction()
    {
        var index = phaseNumber - 1;
        var count = questMonsterListByWave.Count - index;
        if (count > 0) questMonsterListByWave.RemoveRange(index, count);

        phaseNumber--;
        ResetUI();
    }

    private void OnClickNextButtonAction()
    {
        if (phaseNumber == 0)
        {
            userMonsterListForBattle = partyUserMonsterIdList.Select(userMonsterId => userMonsterListForScroll.FirstOrDefault(u => u?.id == userMonsterId)).ToList();
        }
        else
        {
            var index = phaseNumber - 1;
            var questMonsterList = partyUserMonsterIdList
                .Select(userMonsterId =>
                {
                    var userMonster = userMonsterListForScroll.FirstOrDefault(u => u?.id == userMonsterId);
                    return new QuestMonsterMI()
                    {
                        monsterId = userMonster?.monsterId ?? 0,
                        level = userMonster?.customData?.level ?? 0,
                    };
                })
                .ToList();
            questMonsterListByWave.Add(questMonsterList);
        }

        phaseNumber++;
        ResetUI();
    }

    private void ResetUI()
    {
        selectedPartyMonsterIndex = -1;
        selectedUserMonsterId = null;
        partyUserMonsterIdList = Enumerable.Repeat<string>(null, ConstManager.Battle.MAX_PARTY_MEMBER_NUM).ToList();
        RefreshTitle();
        RefreshScroll();
        RefreshPartyUI();
        RefreshGrayoutPanel();
    }

    private void RefreshTitle()
    {
        if (phaseNumber == 0)
        {
            _titleText.text = "自分のパーティモンスターを選択してください";
        }
        else
        {
            _titleText.text = $"ウェーブ{phaseNumber}の相手のパーティモンスターを選択してください";
        }
    }

    private void RefreshGrayoutPanel()
    {
        var notSelectedPartyUserMonster = !partyUserMonsterIdList.Any() || partyUserMonsterIdList.All(userMonsterId => userMonsterId == null);
        _prevButtonGrayoutPanel.SetActive(phaseNumber == 0);
        _nextButtonGrayoutPanel.SetActive(notSelectedPartyUserMonster);
        _startBattleButtonGrayoutPanel.SetActive(phaseNumber < 2);
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