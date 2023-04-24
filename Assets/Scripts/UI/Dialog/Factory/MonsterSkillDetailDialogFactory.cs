using UniRx;
using GameBase;
using System;
using System.Collections.Generic;

public class MonsterSkillDetailDialogFactory {
    public static IObservable<MonsterSkillDetailDialogResponse> Create(MonsterSkillDetailDialogRequest request) {
        return Observable.Create<MonsterSkillDetailDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("userMonster", request.userMonster);
            param.Add("battleActionType", request.battleActionType);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new MonsterSkillDetailDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<MonsterSkillDetailDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}
