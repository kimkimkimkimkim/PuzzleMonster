using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class HomeWindowFactory
{
    public static IObservable<HomeWindowResponse> Create(HomeWindowRequest request)
    {
        return Observable.Create<HomeWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new HomeWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<HomeWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

