using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class TestActionDialogFactory
{
    public static IObservable<TestActionDialogResponse> Create(TestActionDialogRequest request)
    {
        return Observable.Create<TestActionDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new TestActionDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<TestActionDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}