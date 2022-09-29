using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using GameBase;

[ResourcePath("UI/Dialog/Dialog-BattlePause")]
public class BattlePauseDialogUIScript : DialogBase
{
    [SerializeField] protected Button _closeButton;
    [SerializeField] protected Button _interruptionButton;

    public override void Init(DialogInfo info)
    {
        var onClickClose = (Action)info.param["onClickClose"];
        var onClickInterruption = (Action)info.param["onClickInterruption"];

        _closeButton.OnClickIntentAsObservable()
            .Do(_ => 
            {
                // TimeScale = 0なのでObservableではない方でダイアログを閉じる
                UIManager.Instance.CloseDialog();
            })
            .Do(_ => 
            {
                if (onClickClose != null)
                {
                    onClickClose();
                    onClickClose = null;
                }
            })
            .Subscribe();

        _interruptionButton.OnClickIntentAsObservable()
            .Do(_ => 
            {
                // TimeScale = 0なのでObservableではない方でダイアログを閉じる
                UIManager.Instance.CloseDialog();
            })
            .Do(_ => 
            {
                if (onClickInterruption != null)
                {
                    onClickInterruption();
                    onClickInterruption = null;
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
