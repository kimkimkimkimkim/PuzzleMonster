using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class CommonDialogFactory
{
    public static IObservable<CommonDialogResponse> Create(CommonDialogRequest request)
    {
        return Observable.Create<CommonDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("commonDialogType", request.commonDialogType);
            param.Add("title", request.title);
            param.Add("content", request.content);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new CommonDialogResponse()
                {
                    dialogResponseType = DialogResponseType.No,
                });
                observer.OnCompleted();
            }));
            param.Add("onClickOk", new Action(() => {
                observer.OnNext(new CommonDialogResponse()
                {
                    dialogResponseType = DialogResponseType.Yes,
                });
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<CommonDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}