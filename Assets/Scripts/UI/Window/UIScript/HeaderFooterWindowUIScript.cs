using System;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

[ResourcePath("UI/Window/Window-HeaderFooter")]
public class HeaderFooterWindowUIScript : WindowBase
{
    [SerializeField] protected TextMeshProUGUI _orbText;
    [SerializeField] protected TextMeshProUGUI _coinText;
    [SerializeField] protected TextMeshProUGUI _staminaText;
    [SerializeField] protected TextMeshProUGUI _staminaCountdownText;
    [SerializeField] protected Toggle _homeToggle;
    [SerializeField] protected Toggle _monsterToggle;
    [SerializeField] protected Toggle _gachaToggle;
    [SerializeField] protected Toggle _shopToggle;
    [SerializeField] protected GameObject _headerPanel;
    [SerializeField] protected GameObject _footerPanel;

    public GameObject headerPanel { get { return _headerPanel; } }
    public GameObject footerPanel { get { return _footerPanel; } }

    private IDisposable staminaCountDownObservable;
    private int initialStamina;
    private double elapsedTimeMilliSeconds;

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
        SetStaminaText();
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

    /// <summary>
    /// スタミナテキストの設定
    /// スタミナ値が変わるたびに実行する
    /// </summary>
    public void SetStaminaText()
    {
        initialStamina = ApplicationContext.userData.stamina;
        var maxStamina = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.rank == ApplicationContext.userData.rank)?.stamina;
        if (maxStamina == null) return;

        if(staminaCountDownObservable != null)
        {
            staminaCountDownObservable.Dispose();
            staminaCountDownObservable = null;
        }

        var span = DateTimeUtil.Now() - ApplicationContext.userData.lastCalculatedStaminaDateTime;
        elapsedTimeMilliSeconds = span.TotalMilliseconds;

        var initialMinutes = (int)((ConstManager.User.millSecondsPerStamina - elapsedTimeMilliSeconds) / 60);
        var initialSeconds = (int)((ConstManager.User.millSecondsPerStamina - elapsedTimeMilliSeconds) % 60);

        _staminaText.text = $"{initialStamina}/{maxStamina}";
        _staminaCountdownText.text = initialStamina == maxStamina 
            ? "00:00"
            : $"あと {TextUtil.GetZeroPaddingText2Digits(initialMinutes)}:{TextUtil.GetZeroPaddingText2Digits(initialSeconds)}";

        staminaCountDownObservable = Observable.EveryUpdate()
            .Do(_ =>
            {
                var delta = Time.deltaTime;
                elapsedTimeMilliSeconds += delta * 1000;

                var increaseStamina = elapsedTimeMilliSeconds / ConstManager.User.millSecondsPerStamina;
                var lastElapsedTimeMilliSeconds = (int)(elapsedTimeMilliSeconds % ConstManager.User.millSecondsPerStamina);

                var stamina = initialStamina + increaseStamina;
                var minutes = (ConstManager.User.millSecondsPerStamina - lastElapsedTimeMilliSeconds) / 1000 / 60;
                var seconds = ((ConstManager.User.millSecondsPerStamina - lastElapsedTimeMilliSeconds) / 1000) % 60;

                if(stamina < maxStamina)
                {
                    _staminaCountdownText.text = $"あと {TextUtil.GetZeroPaddingText2Digits(minutes)}:{TextUtil.GetZeroPaddingText2Digits(seconds)}";
                    _staminaText.text = $"{(int)stamina}/{maxStamina}";
                }
                else
                {
                    _staminaCountdownText.text = $"00:00";
                    _staminaText.text = $"{maxStamina}/{maxStamina}";
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
