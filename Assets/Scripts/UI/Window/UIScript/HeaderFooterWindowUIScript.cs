using System;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-HeaderFooter")]
public class HeaderFooterWindowUIScript : WindowBase
{
    [SerializeField] protected Text _coinText;
    [SerializeField] protected Text _orbText;
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
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is MonsterMenuWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    MonsterMenuWindowFactory.Create(new MonsterMenuWindowRequest()).Take(1).Subscribe();
                }
            })
            .Subscribe();

        _gachaToggle.OnValueChangedAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is GachaWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    GachaWindowFactory.Create(new GachaWindowRequest()).Take(1).Subscribe();
                }
            })
            .Subscribe();

        _shopToggle.OnValueChangedAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is ShopWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    ShopWindowFactory.Create(new ShopWindowRequest()).Take(1).Subscribe();
                }
            })
            .Subscribe();

        Observable.Timer(TimeSpan.FromSeconds(2)).Do(_ => UIManager.Instance.TryHideFullScreenLoadingView()).Take(1).Subscribe();
        _homeToggle.isOn = true;
        UpdateVirtualCurrencyText();
    }

    /// <summary>
    /// 仮想通貨の所持数表示を更新
    /// </summary>
    public void UpdateVirtualCurrencyText()
    {
        ApiConnection.GetUserInventory()
            .Do(res =>
            {
                foreach (var virtualCurrency in res.VirtualCurrency)
                {
                    if (virtualCurrency.Key == VirtualCurrencyType.OB.ToString()) _orbText.text = virtualCurrency.Value.ToString();
                    if (virtualCurrency.Key == VirtualCurrencyType.CN.ToString()) _coinText.text = virtualCurrency.Value.ToString();
                }
            })
            .Subscribe();
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
