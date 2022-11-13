using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class RouletteDialogFactory
{
    public static IObservable<RouletteDialogResponse> Create(RouletteDialogRequest request)
    {
        return Observable.Create<RouletteDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("itemList", request.itemList);
            param.Add("electedIndex", request.electedIndex);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new RouletteDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<RouletteDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}