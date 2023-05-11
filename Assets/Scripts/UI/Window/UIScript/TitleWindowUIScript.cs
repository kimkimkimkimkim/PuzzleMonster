using System;
using GameBase;
using PM.Enum.Loading;
using PM.Enum.Sound;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Title")]
public class TitleWindowUIScript : WindowBase
{
    [SerializeField] protected GameObject _tapToStartButtonBase;
    [SerializeField] protected Button _tapToStartButton;
    [SerializeField] protected Slider _loadingBar;

    private IDisposable _onClickButtonObservable;

    public override void Init(WindowInfo info)
    {
        base.Init(info);

        var onClose = (Action)info.param["onClose"];

        _tapToStartButton.OnClickIntentAsObservable()
            .Do(_ =>
            {
                // メインシーンへ遷移
                SceneLoadManager.ChangeScene(SceneType.Main);
                SoundManager.Instance.bgm.Play(BGM.Main);
            })
            .Subscribe();

        ShowTapToStartButton(false);
        SetLoadingBarValue(0);
        ShowLoadingBar(true);

        // アプリケーション起動時の処理
        ApplicationContext.EstablishSession(SetLoadingBarValue)
            .SelectMany(_ =>
            {
                if (string.IsNullOrWhiteSpace(ApplicationContext.playerProfile.DisplayName))
                {
                    // 名前が未設定なので名前登録ダイアログを開く
                    return UserNameRegistrationDialogFactory.Create(new UserNameRegistrationDialogRequest())
                        .SelectMany(resp =>
                        {
                            switch (resp.dialogResponseType)
                            {
                                case DialogResponseType.Yes:
                                    return ApiConnection.UpdateUserTitleDisplayName(resp.userName).Select(res => true);

                                case DialogResponseType.No:
                                default:
                                    return Observable.Return<bool>(false);
                            }
                        });
                }
                else
                {
                    // 通常ログイン
                    return Observable.Return<bool>(true);
                }
            })
            .Where(isOk => isOk)
            .SelectMany(_ =>
            {
                // ここで事前ダウンロードを行う
                return Addressables.InitializeAsync().AsObservable().Do(res => SetLoadingBarValue(TitleLoadingPhase.InitializeAddressables));
            })
            .SelectMany(_ =>
            {
                // ローディングバーを100%にしてから少し待って非表示に
                SetLoadingBarValue(TitleLoadingPhase.End);
                return Observable.Timer(TimeSpan.FromSeconds(0.1f));
            })
            .Do(_ =>
            {
                // スタートボタンを表示
                ShowTapToStartButton(true);

                // ローディングバーを非表示
                ShowLoadingBar(false);

                // デバッグボタンの作成
                CreateDebugButtonIfNeeded();

                // スタッカブルダイアログの積みなおし
                PMStackableDialogManager.Instance.Restack();
            })
            .Subscribe();
    }

    private void ShowTapToStartButton(bool isShow)
    {
        _tapToStartButtonBase.SetActive(isShow);
    }

    private void ShowLoadingBar(bool isShow)
    {
        _loadingBar.gameObject.SetActive(isShow);
    }

    private void SetLoadingBarValue(TitleLoadingPhase phase)
    {
        var value = (int)phase;
        _loadingBar.value = value;
    }

    private void CreateDebugButtonIfNeeded()
    {
        if (!ApplicationSettingsManager.Instance.isDebugMode) return;

        // デバッグボタンの作成
        var debugItem = UIManager.Instance.CreateContent<DebugItem>(UIManager.Instance.debugParent);
        debugItem.SetOnClickAction(() =>
        {
            TestActionDialogFactory.Create(new TestActionDialogRequest()).Subscribe();
        });
    }

    public override void Open(WindowInfo info)
    {
    }

    public override void Back(WindowInfo info)
    {
    }

    public override void Close(WindowInfo info)
    {
        base.Close(info);
    }
}