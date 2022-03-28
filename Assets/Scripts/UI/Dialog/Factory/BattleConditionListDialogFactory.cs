using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class BattleConditionListDialogFactory
{
    public static IObservable<BattleConditionListDialogResponse> Create(BattleConditionListDialogRequest request)
    {
        return Observable.Create<BattleConditionListDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("battleMonster", request.battleMonster);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new BattleConditionListDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<BattleConditionListDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}