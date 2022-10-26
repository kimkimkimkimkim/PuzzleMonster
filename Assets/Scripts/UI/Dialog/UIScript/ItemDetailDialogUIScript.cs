using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;

[ResourcePath("UI/Dialog/Dialog-ItemDetail")]
public class ItemDetailDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected IconItem _iconItem;
    [SerializeField] protected Text _contentText;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var itemDescription = (ItemDescriptionMB)info.param["itemDescription"];

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

        SetInfo(itemDescription);
    }

    private void SetInfo(ItemDescriptionMB itemDescription) {
        _iconItem.SetIcon(itemDescription.itemType, itemDescription.itemId);
        _contentText.text = itemDescription.description;
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
