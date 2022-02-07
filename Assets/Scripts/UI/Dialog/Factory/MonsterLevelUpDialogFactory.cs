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
            param.Add("onClickClose", new Action<bool>((isNeedRefresh) => {
                observer.OnNext(new MonsterLevelUpDialogResponse() { isNeedRefresh = isNeedRefresh });
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterLevelUpDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}