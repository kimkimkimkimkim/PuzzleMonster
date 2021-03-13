using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using Enum.UI;

public class UserNameRegistrationDialogFactory
{
    public static IObservable<UserNameRegistrationDialogResponse> Create(UserNameRegistrationDialogRequest request)
    {
        return Observable.Create<UserNameRegistrationDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new UserNameRegistrationDialogResponse()
                {
                    dialogResponseType = DialogResponseType.No,
                });
                observer.OnCompleted();
            }));
            param.Add("onClickOk", new Action<string>(userName => {
                observer.OnNext(new UserNameRegistrationDialogResponse() { 
                    dialogResponseType = DialogResponseType.Yes,
                    userName = userName,
                });
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<UserNameRegistrationDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}
