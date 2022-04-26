using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class LoginBonusDialogFactory: IStackableDialogFactory
{

    private static StackableDialogPriority priority = StackableDialogPriority.LoginBonus;
    private static string type(string userLoginBonusId) => $"{typeof(LoginBonusDialogUIScript).Name}_{userLoginBonusId}";

    public static IObservable<LoginBonusDialogResponse> Create(LoginBonusDialogRequest request)
    {
        return Observable.Create<LoginBonusDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("userLoginBonus", request.userLoginBonus);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new LoginBonusDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<LoginBonusDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }

    public static IObservable<LoginBonusDialogResponse> Push(LoginBonusDialogRequest request)
    {
        return StackableDialogManager.Instance.Push(Create(request), (int)priority, type(request.userLoginBonus.id));
    }

    public static bool IsStacked(string userLoginBonusId)
    {
        return StackableDialogManager.Instance.IsStacked(type(userLoginBonusId));
    }
}