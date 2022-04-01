using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class MonsterLuckUpDialogFactory
{
    public static IObservable<MonsterLuckUpDialogResponse> Create(MonsterLuckUpDialogRequest request)
    {
        return Observable.Create<MonsterLuckUpDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("userMonster", request.userMonster);
            param.Add("onClickClose", new Action<bool>(isNeedRefresh => {
                observer.OnNext(new MonsterLuckUpDialogResponse() { isNeedRefresh = isNeedRefresh});
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterLuckUpDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}