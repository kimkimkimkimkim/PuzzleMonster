using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class BattlePauseDialogFactory
{
    public static IObservable<BattlePauseDialogResponse> Create(BattlePauseDialogRequest request)
    {
        return Observable.Create<BattlePauseDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new BattlePauseDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<BattlePauseDialogUIScript>(param, true, DialogAnimationType.None);
            return Disposable.Empty;
        });
    }
}