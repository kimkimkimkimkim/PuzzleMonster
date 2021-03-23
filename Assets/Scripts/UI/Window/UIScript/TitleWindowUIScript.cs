﻿using System;
using System.Collections;
using System.Collections.Generic;
using Enum.UI;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Title")]
public class TitleWindowUIScript : WindowBase
{
    [SerializeField] protected Button _tapToStartButton;

    public override void Init(WindowInfo info)
    {
        var onClose = (Action)info.param["onClose"];

        _tapToStartButton.OnClickIntentAsObservable()
            .SelectMany(_ => ApiConnection.LoginWithCustomID())
            .SelectMany(_ => ApiConnection.GetPlayerProfile())
            .SelectMany(res =>
            {
                if(string.IsNullOrWhiteSpace(res.PlayerProfile.DisplayName))
                {
                    // 名前が未設定なので名前登録ダイアログを開く
                    return UserNameRegistrationDialogFactory.Create(new UserNameRegistrationDialogRequest())
                        .SelectMany(resp =>
                        {
                            switch (resp.dialogResponseType)
                            {
                                case DialogResponseType.Yes:
                                    return ApiConnection.UpdateUserTitleDisplayName(resp.userName).Select(_ => true);
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
            .SelectMany(isOk =>
            {
                if (isOk)
                {
                    UIManager.Instance.ShowFullScreenLoadingView();
                    return Observable.ReturnUnit()
                        .Do(_ => UIManager.Instance.CloseWindow(forceCloseParent: true))
                        .SelectMany(_ => Observable.NextFrame())
                        .Do(_ =>
                        {
                            if (onClose != null)
                            {
                                onClose();
                                onClose = null;
                            }
                        });
                }
                else
                {
                    return CommonDialogFactory.Create(new CommonDialogRequest()
                    {
                        commonDialogType = CommonDialogType.YesOnly,
                        title = "お知らせ",
                        content = $"ログイン{(isOk ? "成功" : "失敗")}"
                    }).AsUnitObservable();
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
