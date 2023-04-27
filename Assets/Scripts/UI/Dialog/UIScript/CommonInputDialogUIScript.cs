using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;
using PM.Enum.UI;

[ResourcePath("UI/Dialog/Dialog-CommonInput")]
public class CommonInputDialogUIScript : DialogBase
{
    [SerializeField] protected Text _contentText;
    [SerializeField] protected InputField _inputField;
    [SerializeField] protected Button _okButton;
    [SerializeField] protected Button _closeButton;

    public override void Init(DialogInfo info)
    {
        var contentText = (string)info.param["contentText"];
        var onClickOk = (Action<string>)info.param["onClickOk"];
        var onClickClose = (Action)info.param["onClickClose"];

        _contentText.text = contentText;

        _inputField.OnValueChangedAsObservable()
            .Subscribe();

        _okButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ =>
            {
                if (onClickOk != null)
                {
                    onClickOk(_inputField.text);
                    onClickOk = null;
                }
            })
            .Subscribe();

        _closeButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ =>
            {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
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