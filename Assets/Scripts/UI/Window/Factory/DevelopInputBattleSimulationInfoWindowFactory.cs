using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class DevelopInputBattleSimulationInfoWindowFactory
{
    public static IObservable<DevelopInputBattleSimulationInfoWindowResponse> Create(DevelopInputBattleSimulationInfoWindowRequest request)
    {
        return Observable.Create<DevelopInputBattleSimulationInfoWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new DevelopInputBattleSimulationInfoWindowResponse());
                observer.OnCompleted();
            }));

            UIManager.Instance.OpenWindow<DevelopInputBattleSimulationInfoWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

