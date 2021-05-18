using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class MonsterPartyListWindowFactory
{
    public static IObservable<MonsterPartyListWindowResponse> Create(MonsterPartyListWindowRequest request)
    {
        return Observable.Create<MonsterPartyListWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new MonsterPartyListWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<MonsterPartyListWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

