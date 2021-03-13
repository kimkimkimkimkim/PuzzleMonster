using UniRx;
using GameBase;
using System;
using System.Collections.Generic;

public class UserNameRegistrationDialogFactory
{
    public static IObservable<UserNameRegistrationDialogResponse> Create(UserNameRegistrationDialogRequest request)
    {
        return Observable.Create<UserNameRegistrationDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new UserNameRegistrationDialogResponse() { });
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<UserNameRegistrationDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}
