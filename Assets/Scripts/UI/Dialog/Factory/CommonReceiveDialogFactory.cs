using UniRx;
using GameBase;
using System;
using System.Collections.Generic;
using PM.Enum.UI;

public class CommonReceiveDialogFactory {
    public static IObservable<CommonReceiveDialogResponse> Create(CommonReceiveDialogRequest request) {
        return Observable.Create<CommonReceiveDialogResponse>(observer => {
            var param = new Dictionary<string, object>();
            param.Add("title", request.title);
            param.Add("content", request.content);
            param.Add("itemList", request.itemList);
            param.Add("onClickClose", new Action(() => {
                observer.OnNext(new CommonReceiveDialogResponse());
                observer.OnCompleted();
            }));
            UIManager.Instance.OpenDialog<CommonReceiveDialogUIScript>(param, true);
            return Disposable.Empty;
        });
    }
}
