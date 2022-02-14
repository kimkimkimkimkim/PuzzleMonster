using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class MissionDialogFactory
{
    public static IObservable<MissionDialogResponse> Create(MissionDialogRequest request)
    {
        return Observable.Create<MissionDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new MissionDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MissionDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}