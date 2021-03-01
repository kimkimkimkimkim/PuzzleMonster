using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class SampleGameBaseWindowFactory
{
    public static IObservable<SampleGameBaseWindowResponse> Create(SampleGameBaseWindowRequest request)
    {
        return Observable.Create<SampleGameBaseWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            UIManager.Instance.OpenWindow<SampleGameBaseWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}