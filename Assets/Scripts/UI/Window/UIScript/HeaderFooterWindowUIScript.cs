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
    [SerializeField] protected TextMeshProUGUI _userRankText;
    [SerializeField] protected TextMeshProUGUI _userNameText;
    [SerializeField] protected TextMeshProUGUI _userExpText;
    [SerializeField] protected Slider _userExpSlider;
    [SerializeField] protected Slider _staminaSlider;
    [SerializeField] protected Toggle _homeToggle;
    [SerializeField] protected Toggle _monsterToggle;
    [SerializeField] protected Toggle _gachaToggle;
    [SerializeField] protected Toggle _shopToggle;
    [SerializeField] protected GameObject _headerPanel;
    [SerializeField] protected GameObject _footerPanel;
    [SerializeField] protected GameObject _homeTabBadge;
    [SerializeField] protected Button _staminaButton;

    public GameObject headerPanel { get { return _headerPanel; } }
    public GameObject footerPanel { get { return _footerPanel; } }

    private IDisposable staminaCountDownObservable;
    private int initialStamina;
    private double elapsedTimeMilliSeconds;
    private bool isCountdown;

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

        _staminaButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                isCountdown = !isCountdown;
                _staminaText.gameObject.SetActive(!isCountdown);
                _staminaCountdownText.gameObject.SetActive(isCountdown);
            })
            .Subscribe();

        Observable.Timer(TimeSpan.FromSeconds(2)).Do(_ => UIManager.Instance.TryHideFullScreenLoadingView()).Take(1).Subscribe();
        _homeToggle.isOn = true;
        UpdateVirtualCurrencyText();
        SetStaminaUI();
        UpdateUserDataUI();
        ActivateBadge();
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
    public void SetStaminaUI()
    {
        initialStamina = ApplicationContext.userData.stamina;
        var maxStamina = MasterRecord.GetMasterOf<StaminaMB>().GetAll().FirstOrDefault(m => m.rank == ApplicationContext.userData.rank)?.stamina;
        if (maxStamina == null) return;

        if(staminaCountDownObservable != null)
        {
            staminaCountDownObservable.Dispose();
            staminaCountDownObservable = null;
        }

        var span = DateTimeUtil.Now - ApplicationContext.userData.lastCalculatedStaminaDateTime;
        elapsedTimeMilliSeconds = span.TotalMilliseconds;

        var initialMinutes = (int)((ConstManager.User.millSecondsPerStamina - elapsedTimeMilliSeconds) / 60);
        var initialSeconds = (int)((ConstManager.User.millSecondsPerStamina - elapsedTimeMilliSeconds) % 60);

        _staminaText.text = $"{initialStamina}/{maxStamina}";
        _staminaCountdownText.text = initialStamina == maxStamina 
            ? "00:00"
            : $"あと {TextUtil.GetZeroPaddingText2Digits(initialMinutes)}:{TextUtil.GetZeroPaddingText2Digits(initialSeconds)}";

        _staminaSlider.maxValue = (float)maxStamina;
        _staminaSlider.value = (float)initialStamina;

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
                    _staminaSlider.value = (float)stamina;
                }
                else
                {
                    _staminaCountdownText.text = $"00:00";
                    _staminaText.text = $"{maxStamina}/{maxStamina}";
                    _staminaSlider.value = (float)maxStamina;
                }
            })
            .Subscribe();
    }

    public void UpdateUserDataUI()
    {
        var playerProfile = ApplicationContext.playerProfile;
        var userData = ApplicationContext.userData;
        var currentRankUpTable = MasterRecord.GetMasterOf<UserRankUpTableMB>().GetAll().First(m => m.rank == userData.rank);
        var nextRankUpTable = MasterRecord.GetMasterOf<UserRankUpTableMB>().GetAll().FirstOrDefault(m => m.rank == userData.rank + 1);

        _userNameText.text = playerProfile.DisplayName;
        _userRankText.text = userData.rank.ToString();

        if(nextRankUpTable == null)
        {
            // 最大ランクの時
            _userExpText.text = "-";
            _userExpSlider.maxValue = currentRankUpTable.totalRequiredExp;
            _userExpSlider.value = currentRankUpTable.totalRequiredExp;
        }
        else
        {
            var requiredExp = nextRankUpTable.requiredExp;
            var currentExp = ApplicationContext.userVirtualCurrency.playerExp - currentRankUpTable.totalRequiredExp;
            _userExpText.text = $"{currentExp}/{requiredExp}";
            _userExpSlider.maxValue = requiredExp;
            _userExpSlider.value = currentExp;
        }
    }

    public void ActivateBadge()
    {
        // HomeTab
        var isShowPresentIconBadge = ApplicationContext.userInventory.userContainerList.Any();
        var isShowMissionIconBadge = MasterRecord.GetMasterOf<MissionMB>().GetAll()
            .Where(m =>
            {
                // 表示条件を満たしているミッションに絞る
                return ConditionUtil.IsValid(ApplicationContext.userData, m.displayConditionList);
            })
            .Where(m =>
            {
                // クリア条件を満たしているか否か
                var canClear = ConditionUtil.IsValid(ApplicationContext.userData, m.canClearConditionList);
                // すでにクリアしているか否か
                var isCleared = ApplicationContext.userData.userMissionList
                    .Where(u => u.missionId == m.id)
                    .Where(u => u.completedDate > DateTimeUtil.Epoch)
                    .Where(u => (u.startExpirationDate <= DateTimeUtil.Epoch && u.endExpirationDate <= DateTimeUtil.Epoch) || (u.startExpirationDate > DateTimeUtil.Epoch && u.endExpirationDate > DateTimeUtil.Epoch && u.startExpirationDate <= DateTimeUtil.Now && DateTimeUtil.Now < u.endExpirationDate))
                    .Any();

                // クリア可能 && 未クリアならバッチを表示
                return canClear && !isCleared;
            })
            .Any();
        var isShowHomeTabBadge = isShowPresentIconBadge || isShowMissionIconBadge;
        _homeTabBadge.SetActive(isShowHomeTabBadge);
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
