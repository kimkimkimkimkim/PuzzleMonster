using System;
using PM.Enum.Item;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using PM.Enum.UI;
using System.Collections.Generic;

[ResourcePath("UI/Window/Window-HeaderFooter")]
public class HeaderFooterWindowUIScript : WindowBase
{
    [SerializeField] protected Text _orbText;
    [SerializeField] protected Text _coinText;
    [SerializeField] protected Text _gachaTicketText;
    [SerializeField] protected Text _ssrGachaTicketText;
    [SerializeField] protected Text _staminaText;
    [SerializeField] protected Text _staminaCountdownText;
    [SerializeField] protected Text _userRankText;
    [SerializeField] protected Text _userNameText;
    [SerializeField] protected Text _userExpText;
    [SerializeField] protected Slider _userExpSlider;
    [SerializeField] protected Slider _staminaSlider;
    [SerializeField] protected Toggle _homeToggle;
    [SerializeField] protected Toggle _monsterToggle;
    [SerializeField] protected Toggle _arenaToggle;
    [SerializeField] protected Toggle _gachaToggle;
    [SerializeField] protected Toggle _shopToggle;
    [SerializeField] protected GameObject _headerPanel;
    [SerializeField] protected GameObject _footerPanel;
    [SerializeField] protected GameObject _homeTabBadge;
    [SerializeField] protected List<GameObject> _propertyPanelBaseList;
    [SerializeField] protected Button _staminaButton;
    [SerializeField] protected Button _arenaBlockerButton;
    [SerializeField] protected long _rewardAdButtonRewardAdId;

    public GameObject headerPanel { get { return _headerPanel; } }
    public GameObject footerPanel { get { return _footerPanel; } }

    private IDisposable staminaCountDownObservable;
    private int initialStamina;
    private double elapsedTimeMilliSeconds;
    private bool isCountdown;

    public override void Init(WindowInfo info)
    {
        _homeToggle.OnValueChangedIntentAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is HomeWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    ShowPropertyPanel(new List<PropertyPanelType>() { PropertyPanelType.Coin, PropertyPanelType.Orb });
                    HomeWindowFactory.Create(new HomeWindowRequest()).Take(1).Subscribe();
                }
            })
            .Subscribe();

        _monsterToggle.OnValueChangedIntentAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is MonsterMenuWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    ShowPropertyPanel(new List<PropertyPanelType>() { PropertyPanelType.Coin, PropertyPanelType.Orb });
                    MonsterMenuWindowFactory.Create(new MonsterMenuWindowRequest()).Take(1).Subscribe();
                }
            })
            .Subscribe();

        _arenaBlockerButton.OnClickIntentAsObservable(isAnimation: false)
            .SelectMany(_ => {
                var title = "お知らせ";
                var content = "アリーナ機能は現在開発中です\nもうしばらくお待ちください";
                return CommonDialogFactory.Create(new CommonDialogRequest() {
                    commonDialogType = CommonDialogType.YesOnly,
                    title = title,
                    content = content,
                });
            })
            .Subscribe();

        _arenaToggle.OnValueChangedIntentAsObservable()
            .Where(isOn => isOn)
            .Subscribe();

        _gachaToggle.OnValueChangedIntentAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is GachaWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    ShowPropertyPanel(new List<PropertyPanelType>() { PropertyPanelType.Coin, PropertyPanelType.Orb, PropertyPanelType.GachaTicket, PropertyPanelType.SsrGachaTicket });
                    GachaWindowFactory.Create(new GachaWindowRequest()).Take(1).Subscribe();
                }
            })
            .Subscribe();

        _shopToggle.OnValueChangedIntentAsObservable()
            .Where(isOn => isOn)
            .Do(_ =>
            {
                if (UIManager.Instance.currentWindowInfo == null || !(UIManager.Instance.currentWindowInfo.component is ShopWindowUIScript))
                {
                    UIManager.Instance.CloseAllWindow(true);
                    ShowPropertyPanel(new List<PropertyPanelType>() { PropertyPanelType.Coin, PropertyPanelType.Orb });
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

        UpdatePropertyPanelText();
        ShowPropertyPanel(new List<PropertyPanelType>() { PropertyPanelType.Coin, PropertyPanelType.Orb });
        SetStaminaUI();
        UpdateUserDataUI();
        ActivateBadge();
    }

    /// <summary>
    /// プロパティパネルの表示を更新
    /// </summary>
    public void UpdatePropertyPanelText()
    {
        _orbText.text = ClientItemUtil.GetPossessedNum(ItemType.VirtualCurrency, (long)VirtualCurrencyType.OB).ToString();
        _coinText.text = ClientItemUtil.GetPossessedNum(ItemType.VirtualCurrency, (long)VirtualCurrencyType.CN).ToString();
        _gachaTicketText.text = ClientItemUtil.GetPossessedNum(ItemType.Property, (long)PropertyType.GachaTicket).ToString();
        _ssrGachaTicketText.text = ClientItemUtil.GetPossessedNum(ItemType.Property, (long)PropertyType.SsrGachaTicket).ToString();
    }

    /// <summary>
    /// 指定したプロパティパネルを指定した順序(右上から)で表示
    /// </summary>
    public void ShowPropertyPanel(List<PropertyPanelType> propertyPanelTypeList) {
        _propertyPanelBaseList.ForEach((p,i) => {
            var propertyPanelType = (PropertyPanelType)i;
            var index = propertyPanelTypeList.IndexOf(propertyPanelType);
            var siblingIndex = index >= 0 ? index : propertyPanelTypeList.Count;

            p.SetActive(index >= 0);
            p.transform.SetSiblingIndex(siblingIndex);
        });
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
                    _staminaText.text = $"{(int)stamina}/{maxStamina}";
                    _staminaSlider.value = (float)maxStamina;

                    // 現在のスタミナが最大値以上だったら更新処理を破棄する
                    staminaCountDownObservable.Dispose();
                    staminaCountDownObservable = null;
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
            var currentExp = ApplicationContext.userData.userPropertyList.GetNum(PropertyType.PlayerExp) - currentRankUpTable.totalRequiredExp;
            _userExpText.text = $"{currentExp}/{requiredExp}";
            _userExpSlider.maxValue = requiredExp;
            _userExpSlider.value = currentExp;
        }
    }

    public void ActivateBadge()
    {
        // HomeTab
        _homeTabBadge.SetActive(IsShowHomeTabBadge());
    }

    private bool IsShowHomeTabBadge()
    {
        // プレゼントボックスボタン
        var isShowPresentIconBadge = ApplicationContext.userData.userPresentList.Any(u => u.IsValid());
        if (isShowPresentIconBadge) return true;

        // ミッションボタン
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
        if (isShowMissionIconBadge) return true;

        // リワード広告
        var rewardAd = MasterRecord.GetMasterOf<RewardAdMB>().Get(_rewardAdButtonRewardAdId);
        var isShowRewardAdButton = DateTimeUtil.GetTermValidUserRewardAdList(rewardAd.termType, ApplicationContext.userData.userRewardAdList).Where(u => u.rewardAdId == rewardAd.id).Count() < rewardAd.limitNum;
        if (isShowRewardAdButton) return true;

        return false;
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
