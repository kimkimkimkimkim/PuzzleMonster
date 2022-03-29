using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;

[ResourcePath("UI/Dialog/Dialog-Present")]
public class PresentDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<UserContainerInfo> userContainerList;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];

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

        var m = MasterRecord.GetMasterOf<ContainerMB>().GetAll();
        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        userContainerList = ApplicationContext.userInventory.userContainerList;

        _infiniteScroll.Init(userContainerList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userContainerList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<PresentScrollItem>();
        var userContainer = userContainerList[index];
        var container = MasterRecord.GetMasterOf<ContainerMB>().Get(userContainer.containerId);
        var firstItem = container.itemList.First();

        scrollItem.SetNameText(container.name);
        scrollItem.SetDescriptionText(container.description);
        scrollItem.ShowGrayoutPanel(false);
        scrollItem.SetIcon(firstItem);
        scrollItem.SetOnClickAction(() =>
        {
            ApiConnection.UnlockContainer(userContainer.id)
                .SelectMany(res => CommonReceiveDialogFactory.Create(new CommonReceiveDialogRequest() { itemList = res.itemList }))
                .Do(_ => RefreshScroll())
                .Subscribe();
        });
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
