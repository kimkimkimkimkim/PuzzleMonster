using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class MonsterFormationWindowFactory
{
    public static IObservable<MonsterFormationWindowResponse> Create(MonsterFormationWindowRequest request)
    {
        return Observable.Create<MonsterFormationWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("partyId", request.partyId);
            param.Add("userMonsterList", request.userMontserList);
            param.Add("initialUserMonsterList", request.initialUserMonsterList);
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new MonsterFormationWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<MonsterFormationWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

