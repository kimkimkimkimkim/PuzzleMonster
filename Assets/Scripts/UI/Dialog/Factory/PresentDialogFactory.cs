using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class PresentDialogFactory
{
    public static IObservable<PresentDialogResponse> Create(PresentDialogRequest request)
    {
        return Observable.Create<PresentDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new PresentDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<PresentDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}