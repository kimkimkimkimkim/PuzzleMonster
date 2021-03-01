using UniRx;
using GameBase;
using System;
using System.Collections.Generic;

public class SampleGameBaseDialogFactory
{
    public static IObservable<SampleGameBaseDialogResponse> Create(SampleGameBaseDialogRequest request)
    {
        return Observable.Create<SampleGameBaseDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new SampleGameBaseDialogResponse() { });
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<SampleGameBaseDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}