using UniRx;
using GameBase;
using System;
using System.Collections.Generic;

public class GachaEmissionRateDialogFactory
{
    public static IObservable<GachaEmissionRateDialogResponse> Create(GachaEmissionRateDialogRequest request)
    {
        return Observable.Create<GachaEmissionRateDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("gachaBoxId", request.gachaBoxId);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new GachaEmissionRateDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<GachaEmissionRateDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}