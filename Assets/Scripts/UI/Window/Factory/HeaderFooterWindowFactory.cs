using System;
using System.Collections.Generic;
using GameBase;
using UniRx;

public class HeaderFooterWindowFactory
{
    public static IObservable<HeaderFooterWindowResponse> Create(HeaderFooterWindowRequest request)
    {
        return Observable.Create<HeaderFooterWindowResponse>(observer =>
        {
            var param = new Dictionary<string, object>();

            var windowInfo = new WindowInfo();
            windowInfo.param = param;

            var headerFooterWindow = UIManager.Instance.CreateContent<HeaderFooterWindowUIScript>(UIManager.Instance.headerFooterParent);
            HeaderManager.Instance.SetHeaderFooterWindowUIScript(headerFooterWindow);
            headerFooterWindow.Init(windowInfo);
            headerFooterWindow.Open(windowInfo);

            return Disposable.Empty;
        });
    }
}

