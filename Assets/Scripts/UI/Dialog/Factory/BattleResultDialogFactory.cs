using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class BattleResultDialogFactory
{
    public static IObservable<BattleResultDialogResponse> Create(BattleResultDialogRequest request)
    {
        return Observable.Create<BattleResultDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new BattleResultDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<BattleResultDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}