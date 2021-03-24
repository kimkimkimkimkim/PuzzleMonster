using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class ShopWindowFactory
{
    public static IObservable<ShopWindowResponse> Create(ShopWindowRequest request)
    {
        return Observable.Create<ShopWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new ShopWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<ShopWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

