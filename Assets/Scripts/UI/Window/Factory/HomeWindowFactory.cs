using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class HomeWindowFactory
{
    public static IObservable<HomeWindowResponse> Create(HomeWindowRequest request)
    {
        return Observable.Create<HomeWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();
            param.Add("onClose", new Action(() =>
            {
                observer.OnNext(new HomeWindowResponse());
                observer.OnCompleted();
            }));

            // Home画面表示時には一度スタッカブルダイアログ表示不可にしておく
            MainSceneManager.Instance.SetIsReadyToShowStackableDialog(false);
            UIManager.Instance.OpenWindow<HomeWindowUIScript>(param, animationType: WindowAnimationType.None);
            return Disposable.Empty;
        });
    }
}

