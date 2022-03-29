using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class PlayerRankUpDialogFactory
{
    public static IObservable<PlayerRankUpDialogResponse> Create(PlayerRankUpDialogRequest request)
    {
        return Observable.Create<PlayerRankUpDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("beforeRank", request.beforeRank);
            param.Add("afterRank", request.afterRank);
            param.Add("beforeMaxStamina", request.beforeMaxStamina);
            param.Add("afterMaxStamina", request.afterMaxStamina);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new PlayerRankUpDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<PlayerRankUpDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}