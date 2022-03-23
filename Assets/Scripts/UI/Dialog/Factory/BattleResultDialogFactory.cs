using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class BattleResultDialogFactory
{
    public static IObservable<BattleResultDialogResponse> Create(BattleResultDialogRequest request)
    {
        return Observable.Create<BattleResultDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("winOrLose", request.winOrLose);
            param.Add("playerBattleMonsterList", request.playerBattleMonsterList);
            param.Add("enemyBattleMonsterListByWave", request.enemyBattleMonsterListByWave);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new BattleResultDialogResponse());
                observer.OnCompleted();
            }));
            param.Add("onClickOk", new Action(() => {
                observer.OnNext(new BattleResultDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<BattleResultDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}