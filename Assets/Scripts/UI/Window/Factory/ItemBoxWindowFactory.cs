using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class ItemBoxWindowFactory
{
    public static IObservable<ItemBoxWindowResponse> Create(ItemBoxWindowRequest request)
    {
        return Observable.Create<ItemBoxWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new ItemBoxWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<ItemBoxWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

