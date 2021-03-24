using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class MonsterBoxWindowFactory
{
    public static IObservable<MonsterBoxWindowResponse> Create(MonsterBoxWindowRequest request)
    {
        return Observable.Create<MonsterBoxWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new MonsterBoxWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<MonsterBoxWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

