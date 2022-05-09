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
    [SerializeField] protected Button _allReceiveButton;
    [SerializeField] protected GameObject _allReceiveButtonGrayoutPanel;
    [SerializeField] protected InfiniteScroll _infiniteScroll;

    private List<UserPresentInfo> userPresentList;

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

        _allReceiveButton.OnClickIntentAsObservable()
            .SelectMany(_ =>
            {
                var userPresentIdList = userPresentList.Select(u => u.id).ToList();
                return ApiConnection.ReceivePresent(userPresentIdList);
            })
            .SelectMany(res => CommonReceiveDialogFactory.Create(new CommonReceiveDialogRequest() { itemList = res.userPresentList.Select(u => u.item).ToList() }))
            .Do(_ => RefreshScroll())
            .Subscribe();

        RefreshScroll();
    }

    private void RefreshScroll()
    {
        _infiniteScroll.Clear();

        userPresentList = ApplicationContext.userData.userPresentList.Where(u => u.IsValid()).ToList();
        _allReceiveButtonGrayoutPanel.SetActive(!userPresentList.Any());

        _infiniteScroll.Init(userPresentList.Count, OnUpdateItem);
    }

    private void OnUpdateItem(int index, GameObject item)
    {
        if ((userPresentList.Count <= index) || (index < 0)) return;

        var scrollItem = item.GetComponent<PresentScrollItem>();
        var userPresent = userPresentList[index];

        scrollItem.SetNameText(userPresent.title);
        scrollItem.SetDescriptionText(userPresent.message);
        scrollItem.ShowGrayoutPanel(false);
        scrollItem.SetIcon(userPresent.item);
        scrollItem.SetOnClickAction(() =>
        {
            ApiConnection.ReceivePresent(new List<string>() { userPresent.id })
                .SelectMany(res => CommonReceiveDialogFactory.Create(new CommonReceiveDialogRequest() { itemList = res.userPresentList.Select(u => u.item).ToList() }))
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
