using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class RewardReceiveDialogFactory
{
    public static IObservable<RewardReceiveDialogResponse> Create(RewardReceiveDialogRequest request)
    {
        return Observable.Create<RewardReceiveDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("rewardItemList", request.rewardItemList);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new RewardReceiveDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<RewardReceiveDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}