using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class MonsterLevelUpDialogFactory
{
    public static IObservable<MonsterLevelUpDialogResponse> Create(MonsterLevelUpDialogRequest request)
    {
        return Observable.Create<MonsterLevelUpDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("userMonster", request.userMonster);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new MonsterLevelUpDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterLevelUpDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}