using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class GachaWindowFactory
{
    public static IObservable<GachaWindowResponse> Create(GachaWindowRequest request)
    {
        return Observable.Create<GachaWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new GachaWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<GachaWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

