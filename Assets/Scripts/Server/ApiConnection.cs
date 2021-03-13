using System;
using Enum.UI;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;
using UniRx;
using UnityEngine;

public class ApiConnection
{
    /// <summary>
    /// カスタムIDでのログイン処理
    /// </summary>
    public static IObservable<LoginResult> LoginWithCustomID()
    {
        return Observable.Create<LoginResult>(o =>
        {
            var callback = new Action<LoginResult>(res =>
            {
                o.OnNext(res);
                o.OnCompleted();
            });
            var onErrorAction = new Action<PlayFabError>(error => 
            {
                OnErrorAction(error);
                o.OnCompleted();
            });

            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = SaveDataUtil.System.GetCustomId(),
                CreateAccount = true,
            },res => callback(res),error => onErrorAction(error));
            return Disposable.Empty;
        });
    }

    /// <summary>
    /// プレイヤープロフィールを取得
    /// </summary>
    public static IObservable<GetPlayerProfileResult> GetPlayerProfile()
    {
        return Observable.Create<GetPlayerProfileResult>(o =>
        {
            var callback = new Action<GetPlayerProfileResult>(res =>
            {
                o.OnNext(res);
                o.OnCompleted();
            });
            var onErrorAction = new Action<PlayFabError>(error =>
            {
                OnErrorAction(error);
                o.OnCompleted();
            });

            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest()
            {
                PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
            }, res => callback(res), error => onErrorAction(error));
            return Disposable.Empty;
        });
    }

    /// <summary>
    /// ユーザー名の更新
    /// </summary>
    public static IObservable<UpdateUserTitleDisplayNameResult> UpdateUserTitleDisplayName(string userName)
    {
        return Observable.Create<UpdateUserTitleDisplayNameResult>(o =>
        {
            var callback = new Action<UpdateUserTitleDisplayNameResult>(res =>
            {
                o.OnNext(res);
                o.OnCompleted();
            });
            var onErrorAction = new Action<PlayFabError>(error =>
            {
                OnErrorAction(error);
                o.OnCompleted();
            });

            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
            {
                DisplayName = userName
            }, res => callback(res), error => onErrorAction(error));
            return Disposable.Empty;
        });
    }

    private static IObservable<TResp> SendRequest<TResp,TReq>(TReq request) where TResp : PlayFabResultCommon where TReq : PlayFabRequestCommon
    {
        return Observable.Create<TResp>(o =>
        {
            var callback = new Action<TResp>(res =>
            {
                o.OnNext(res);
                o.OnCompleted();
            });
            var onErrorAction = new Action<PlayFabError>(error =>
            {
                OnErrorAction(error);
                o.OnCompleted();
            });

            Api<TResp, TReq>(request,callback,onErrorAction);
            return Disposable.Empty;
        });
    }

    private static void Api<TResp, TReq>(TReq request,Action<TResp> callback, Action<PlayFabError> onErrorAction) where TResp : PlayFabResultCommon where TReq : PlayFabRequestCommon
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(request as UpdateUserTitleDisplayNameRequest, res => callback(res as TResp), error => onErrorAction(error));
    }

    /// <summary>
    /// 共通エラーアクション
    /// </summary>
    private static void OnErrorAction(PlayFabError error)
    {
        Debug.LogError(error.ErrorMessage);
        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            commonDialogType = CommonDialogType.YesOnly,
            title = "お知らせ",
            content = "エラーが発生しました"
        }).Subscribe();
    }
}
