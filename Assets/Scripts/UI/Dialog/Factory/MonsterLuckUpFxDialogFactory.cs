using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class MonsterLuckUpFxDialogFactory
{
    public static IObservable<MonsterLuckUpFxDialogResponse> Create(MonsterLuckUpFxDialogRequest request)
    {
        return Observable.Create<MonsterLuckUpFxDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("beforeLuck", request.beforeLuck);
            param.Add("afterLuck", request.afterLuck);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new MonsterLuckUpFxDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterLuckUpFxDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}