using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;
using PM.Enum.Battle;
using TMPro;

[ResourcePath("UI/Dialog/Dialog-BattleResult")]
public class BattleResultDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected TextMeshProUGUI _questNameText;
    [SerializeField] protected InfiniteScroll _infiniteScroll;
    [SerializeField] protected GameObject _winPanel;
    [SerializeField] protected GameObject _losePanel;

    private Action onClickClose;
    private UserBattleInfo userBattle;
    private List<BattleRewardItemMI> battleRewardItemList;

    public override void Init(DialogInfo info)
    {
        onClickClose = (Action)info.param["onClickClose"];
        var userBattleId = (string)info.param["userBattleId"];

        userBattle = ApplicationContext.userData.userBattleList.First(u => u.id == userBattleId);

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        RefreshPanel(userBattle.winOrLose);
    }

    private void RefreshPanel(WinOrLose winOrLose)
    {
        if (winOrLose == WinOrLose.Win)
        {
            // 勝った時のUI更新処理
            _winPanel.SetActive(true);
            _losePanel.SetActive(false);

            SetQuestNameText();
            RefreshScroll();
        }
        else
        {
            // 負けたときのUI更新処理
            _winPanel.SetActive(false);
            _losePanel.SetActive(true);

            const float DELAY_TIME = 2.5f;
            Observable.Timer(TimeSpan.FromSeconds(DELAY_TIME))
                .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
                .Do(_ => {
                    if (onClickClose != null)
                    {
                        onClickClose();
                        onClickClose = null;
                    }
                })
                .Subscribe();
        }
    }

    private void SetQuestNameText()
    {
        var quest = MasterRecord.GetMasterOf<QuestMB>().Get(userBattle.questId);
        _questNameText.text = quest.name;
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        battleRewardItemList = userBattle.rewardItemList.Select(i => new BattleRewardItemMI() { item = i, isFirstClear = false }).ToList();
        battleRewardItemList.AddRange(userBattle.firstClearRewardItemList.Select(i => new BattleRewardItemMI() { item = i, isFirstClear = true }).ToList());

        if (battleRewardItemList.Any()) _infiniteScroll.Init(battleRewardItemList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((battleRewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<IconItem>();
        var battleRewardItem = battleRewardItemList[index];

        scrollItem.SetIcon(battleRewardItem.item);
        scrollItem.ShowGrayoutPanel(false);
        scrollItem.ShowLabel(battleRewardItem.isFirstClear, "初回");
    }

    public override void Back(DialogInfo info)
    {
    }
    public override void Close(DialogInfo info)
    {
    }
    public override void Open(DialogInfo info)
    {
    }

    public class BattleRewardItemMI
    {
        public ItemMI item { get; set; }
        public bool isFirstClear { get; set; }
    }
}
