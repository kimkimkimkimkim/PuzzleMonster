using GameBase;
using PM.Enum.Sound;
using PM.Enum.UI;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class TitleSceneLoader : ISceneLoadable
{
    private TitleWindowUIScript titleWindowUIScript;

    public override IObservable<Unit> Activate(Dictionary<string, object> param)
    {
        return Observable.ReturnUnit();
    }

    public override IObservable<Unit> Deactivate(Dictionary<string, object> param)
    {
        return Observable.ReturnUnit();
    }

    public override void OnPause(bool pause)
    {
    }

    public override void OnLoadComplete()
    {
        var titleWindowUIScriptObject = GameObject.FindGameObjectWithTag("titleUIScript");
        titleWindowUIScript = titleWindowUIScriptObject.GetComponent<TitleWindowUIScript>();
        titleWindowUIScript.ShowTapToStartButton(false);
        titleWindowUIScript.SetOnClickAction(() => StartGame());

        // アプリケーション起動時の処理
        ApplicationContext.EstablishSession()
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
            .Do(_ =>
            {
                // スタートボタンを表示
                titleWindowUIScript.ShowTapToStartButton(true);

                // デバッグボタンの作成
                var debugItem = UIManager.Instance.CreateContent<DebugItem>(UIManager.Instance.debugParent);
                debugItem.SetOnClickAction(() =>
                {
                    TestActionDialogFactory.Create(new TestActionDialogRequest()).Subscribe();
                });

                // スタッカブルダイアログの積みなおし
                PMStackableDialogManager.Instance.Restack();
            })
            .Subscribe();
    }

    private void StartGame()
    {
        // メインシーンへ遷移
        SceneLoadManager.ChangeScene(SceneType.Main);
        SoundManager.Instance.bgm.Play(BGM.Main);
    }
}