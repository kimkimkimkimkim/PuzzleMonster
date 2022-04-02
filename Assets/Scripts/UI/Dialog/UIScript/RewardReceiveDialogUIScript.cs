using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;

[ResourcePath("UI/Dialog/Dialog-RewardReceive")]
public class RewardReceiveDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<RewardItemMI> rewardItemList;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        rewardItemList = (List<RewardItemMI>)info.param["rewardItemList"];

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

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        _infiniteScroll.Init(rewardItemList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((rewardItemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<CommonReceiveScrollItem>();
        var rewardItem = rewardItemList[index];

        scrollItem.iconItem.ShowLabel(rewardItem.isFirstClearReward, "èââÒ");
        scrollItem.SetIcon(rewardItem);
        scrollItem.SetNameText(ClientItemUtil.GetName(rewardItem));
        scrollItem.SetNumText(rewardItem.num);
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
}
