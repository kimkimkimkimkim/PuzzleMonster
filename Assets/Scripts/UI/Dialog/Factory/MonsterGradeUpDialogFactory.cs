using UniRx;
using GameBase;
using System;
using System.Collections.Generic;

public class MonsterGradeUpDialogFactory
{
    public static IObservable<MonsterGradeUpDialogResponse> Create(MonsterGradeUpDialogRequest request)
    {
        return Observable.Create<MonsterGradeUpDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("userMonster", request.userMonster);
            param.Add("onClickClose", new Action<bool>((isNeedRefresh) => {
                observer.OnNext(new MonsterGradeUpDialogResponse() { isNeedRefresh = isNeedRefresh});
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterGradeUpDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}