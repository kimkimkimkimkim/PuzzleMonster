using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class MonsterGradeUpFxDialogFactory
{
    public static IObservable<MonsterGradeUpFxDialogResponse> Create(MonsterGradeUpFxDialogRequest request)
    {
        return Observable.Create<MonsterGradeUpFxDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("beforeGrade", request.beforeGrade);
            param.Add("afterGrade", request.afterGrade);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new MonsterGradeUpFxDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterGradeUpFxDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}