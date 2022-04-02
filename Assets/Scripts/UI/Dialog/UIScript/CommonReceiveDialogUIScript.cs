using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;

[ResourcePath("UI/Dialog/Dialog-CommonReceive")]
public class CommonReceiveDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<ItemMI> itemList;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        itemList = (List<ItemMI>)info.param["itemList"];

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

        _infiniteScroll.Init(itemList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((itemList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<CommonReceiveScrollItem>();
        var itemMI = itemList[index];

        scrollItem.SetIcon(itemMI);
        scrollItem.SetNameText(ClientItemUtil.GetName(itemMI));
        scrollItem.SetNumText(itemMI.num);
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
