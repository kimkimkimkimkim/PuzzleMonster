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
            param.Add("onClose", new Action(() => {
                observer.OnNext(new TitleWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<TitleWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}
