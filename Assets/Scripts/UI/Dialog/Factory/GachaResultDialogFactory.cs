using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class GachaResultDialogFactory
{
    public static IObservable<GachaResultDialogResponse> Create(GachaResultDialogRequest request)
    {
        return Observable.Create<GachaResultDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("itemList", request.itemList);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new GachaResultDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<GachaResultDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}