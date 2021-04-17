using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class MonsterDetailDialogFactory
{
    public static IObservable<MonsterDetailDialogResponse> Create(MonsterDetailDialogRequest request)
    {
        return Observable.Create<MonsterDetailDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("userMonster", request.userMonster);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new MonsterDetailDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterDetailDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}