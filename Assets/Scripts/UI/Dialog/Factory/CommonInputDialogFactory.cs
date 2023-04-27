using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class CommonInputDialogFactory
{
    public static IObservable<CommonInputDialogResponse> Create(CommonInputDialogRequest request)
    {
        return Observable.Create<CommonInputDialogResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("contentText", request.contentText);
            param.Add("onClickClose", new Action(() =>
            {
                observer.OnNext(new CommonInputDialogResponse() { dialogResponseType = DialogResponseType.No, });
                observer.OnCompleted();
            }));
            param.Add("onClickOk", new Action<string>((inputText) =>
            {
                observer.OnNext(new CommonInputDialogResponse() { dialogResponseType = DialogResponseType.Yes, inputText = inputText });
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<CommonInputDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}