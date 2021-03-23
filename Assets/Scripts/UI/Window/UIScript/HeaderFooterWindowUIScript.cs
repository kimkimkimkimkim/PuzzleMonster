using System;
using System.Collections.Generic;
using Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-HeaderFooter")]
public class HeaderFooterWindowUIScript : WindowBase
{
    [SerializeField] protected Toggle _homeToggle;
    [SerializeField] protected Toggle _monsterToggle;
    [SerializeField] protected Toggle _gachaToggle;
    [SerializeField] protected Toggle _shopToggle;

    public override void Init(WindowInfo info)
    {
        _homeToggle.OnValueChangedAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is HomeWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    HomeWindowFactory.Create(new HomeWindowRequest()).Take(1).Subscribe();
                }
            })
            .Subscribe();

        _monsterToggle.OnValueChangedAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                UIManager.Instance.CloseAllWindow(true);
            })
            .Subscribe();

        _gachaToggle.OnValueChangedAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                HomeWindowFactory.Create(new HomeWindowRequest()).Take(1).Subscribe();
            })
            .Subscribe();

        _shopToggle.OnValueChangedAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                //HomeWindowFactory.Create(new HomeWindowRequest()).Take(1).Subscribe();
            })
            .Subscribe();

        Observable.Timer(TimeSpan.FromSeconds(2)).Do(_ => UIManager.Instance.TryHideFullScreenLoadingView()).Take(1).Subscribe();
        _homeToggle.isOn = true;
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
    }
}
