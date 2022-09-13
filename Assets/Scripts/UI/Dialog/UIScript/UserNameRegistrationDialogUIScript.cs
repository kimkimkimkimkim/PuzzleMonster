using System;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;

[ResourcePath("UI/Dialog/Dialog-UserNameRegistration")]
public class UserNameRegistrationDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _registerButton;
    [SerializeField] protected InputField _userNameInputField;
    [SerializeField] protected GameObject _grayoutPanel;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var onClickOk = (Action<string>)info.param["onClickOk"];

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

        _registerButton.OnClickIntentAsObservable()
            .SelectMany(_ => UIManager.Instance.CloseDialogObservable())
            .Do(_ => {
                if (onClickOk != null)
                {
                    onClickOk(_userNameInputField.text);
                    onClickOk = null;
                }
            })
            .Subscribe();

        _userNameInputField.OnValueChangedAsObservable()
            .Do(_ => UpdateGrayoutPanel())
            .Subscribe();

        UpdateGrayoutPanel();
    }

    private void UpdateGrayoutPanel()
    {
        var isValid = IsValid();
        _grayoutPanel.SetActive(!isValid);
    }

    private bool IsValid()
    {
        var count = _userNameInputField.text.Length;
        return count >= ConstManager.System.userNameMinWordCount && count <= ConstManager.System.userNameMaxWordCount;
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
