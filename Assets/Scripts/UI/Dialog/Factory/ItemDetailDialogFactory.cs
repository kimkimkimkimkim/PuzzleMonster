using UniRx;
using GameBase;
using System;
using System.Collections.Generic;

public class ItemDetailDialogFactory
{
    public static IObservable<ItemDetailDialogResponse> Create(ItemDetailDialogRequest request)
    {
        return Observable.Create<ItemDetailDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("itemDescription", request.itemDescription);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new ItemDetailDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<ItemDetailDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}