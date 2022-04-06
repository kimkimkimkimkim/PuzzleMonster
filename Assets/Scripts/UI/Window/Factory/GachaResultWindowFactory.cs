using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class GachaResultWindowFactory
{
    public static IObservable<GachaResultWindowResponse> Create(GachaResultWindowRequest request)
    {
        return Observable.Create<GachaResultWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("itemList", request.itemList);
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new GachaResultWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<GachaResultWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

