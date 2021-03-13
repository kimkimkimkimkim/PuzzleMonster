using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class TitleWindowFactory
{
    public static IObservable<TitleWindowResponse> Create(TitleWindowRequest request)
    {
        return Observable.Create<TitleWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            UIManager.Instance.OpenWindow<TitleWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}
