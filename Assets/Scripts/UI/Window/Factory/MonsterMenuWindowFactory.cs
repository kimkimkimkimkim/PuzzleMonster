using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class MonsterMenuWindowFactory
{
    public static IObservable<MonsterMenuWindowResponse> Create(MonsterMenuWindowRequest request)
    {
        return Observable.Create<MonsterMenuWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new MonsterMenuWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<MonsterMenuWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

