using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class MonsterLevelUpFxDialogFactory
{
    public static IObservable<MonsterLevelUpFxDialogResponse> Create(MonsterLevelUpFxDialogRequest request)
    {
        return Observable.Create<MonsterLevelUpFxDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("beforeLevel", request.beforeLevel);
            param.Add("afterLevel", request.afterLevel);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new MonsterLevelUpFxDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterLevelUpFxDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}