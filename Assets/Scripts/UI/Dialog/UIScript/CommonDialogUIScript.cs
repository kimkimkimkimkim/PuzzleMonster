using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using Enum.UI;

[ResourcePath("UI/Dialog/Dialog-Common")]
public class CommonDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Text _contentText;
    [SerializeField] protected GameObject _closeButtonBase;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var onClickOk = (Action)info.param["onClickOk"];
        var commonDialogType = (CommonDialogType)info.param["commonDialogType"];
        var title = (string)info.param["title"];
        var content = (string)info.param["content"];

        // タイプ毎にUIを変更
        switch (commonDialogType)
        {
            case CommonDialogType.NoAndYes:
                break;
            case CommonDialogType.YesOnly:
                _closeButtonBase.SetActive(false);
                break;
        }

        _titleText.text = title;
        _contentText.text = content;

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

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickOk != null)
                {
                    onClickOk();
                    onClickOk = null;
                }
            })
            .Subscribe();
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
