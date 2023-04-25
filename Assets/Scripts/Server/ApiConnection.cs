using System;
using PM.Enum.UI;
using GameBase;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;
using UniRx;
using UnityEngine;
using PlayFab.CloudScriptModels;
using System.Linq;

public partial class ApiConnection
{
    #region ClientApi

    private static IObservable<TResp> SendRequest<TReq, TResp>(ApiType apiType, TReq request) where TReq : PlayFabRequestCommon where TResp : PlayFabResultCommon
    {
        if (!CheckSendRequestAction())
        {
            return Observable.Create<TResp>(o =>
            {
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        return Observable.Create<TResp>(o =>
        {
            var callback = new Action<TResp>(res =>
            {
                UIManager.Instance.TryHideLoadingView();
                o.OnNext(res);
                o.OnCompleted();
            });
            var onErrorAction = new Action<PlayFabError>(error =>
            {
                UIManager.Instance.TryHideLoadingView();
                OnErrorAction(error);
                o.OnCompleted();
            });

            UIManager.Instance.ShowLoadingView();
            ExecuteApi<TReq, TResp>(apiType, request, callback, onErrorAction);
            return Disposable.Empty;
        });
    }

    private static void ExecuteApi<TReq, TResp>(ApiType apiType, TReq request, Action<TResp> callback, Action<PlayFabError> onErrorAction) where TResp : PlayFabResultCommon where TReq : PlayFabRequestCommon
    {
        switch (apiType)
        {
            case ApiType.LoginWithCustomID:
                PlayFabClientAPI.LoginWithCustomID(request as LoginWithCustomIDRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.GetPlayerProfile:
                PlayFabClientAPI.GetPlayerProfile(request as GetPlayerProfileRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.UpdateUserTitleDisplayName:
                PlayFabClientAPI.UpdateUserTitleDisplayName(request as UpdateUserTitleDisplayNameRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.GetUserInventory:
                PlayFabClientAPI.GetUserInventory(request as GetUserInventoryRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.AddUserVirtualCurrency:
                PlayFabClientAPI.AddUserVirtualCurrency(request as AddUserVirtualCurrencyRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.GetTitleData:
                PlayFabClientAPI.GetTitleData(request as GetTitleDataRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.GetUserData:
                PlayFabClientAPI.GetUserData(request as GetUserDataRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.UpdateUserData:
                PlayFabClientAPI.UpdateUserData(request as UpdateUserDataRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            case ApiType.GrantCharacterToUser:
                PlayFabClientAPI.GrantCharacterToUser(request as GrantCharacterToUserRequest, res => callback(res as TResp), error => onErrorAction(error));
                break;

            default:
                break;
        }
    }

    private enum ApiType
    {
        LoginWithCustomID,
        GetPlayerProfile,
        UpdateUserTitleDisplayName,
        GetUserInventory,
        AddUserVirtualCurrency,
        GetTitleData,
        GetUserData,
        UpdateUserData,
        GrantCharacterToUser,
    }

    #endregion ClientApi

    #region CloudFunction

    private static IObservable<TResp> SendRequest<TReq, TResp>(string functionName, TReq request) where TResp : PMApiResponseBase
    {
        if (!CheckSendRequestAction())
        {
            return Observable.Create<TResp>(o =>
            {
                o.OnCompleted();
                return Disposable.Empty;
            });
        }

        return Observable.Create<TResp>(o =>
        {
            var callback = new Action<ExecuteFunctionResult>((ExecuteFunctionResult result) =>
            {
                var response = DataProcessUtil.GetResponse<TResp>(result.FunctionResult.ToString());

                if (response.status == PMApiStatus.OK)
                {
                    // サーバーAPIを実行したら確定でユーザーデータを更新している
                    // TODO : いずれは適切に差分更新とかするべきかも
                    ApplicationContext.UpdateUserDataObservable()
                        .Do(_ =>
                        {
                            // UIの更新
                            HeaderFooterManager.Instance.UpdatePropertyPanelText();
                            HeaderFooterManager.Instance.UpdateUserDataUI();
                            HeaderFooterManager.Instance.SetStaminaText();
                            HeaderFooterManager.Instance.ActivateBadge();

                            // 通知の制御
                            response.userNotificationList.ForEach(userNotification =>
                            {
                                NotificationManager.Instance.ExecuteNotification(userNotification);
                            });

                            // ロッカブルの更新
                            UIManager.Instance.RefreshLockableUI();

                            // タップブロッカーを非表示に
                            UIManager.Instance.TryHideLoadingView();

                            o.OnNext(response);
                            o.OnCompleted();
                        })
                        .Subscribe();
                }
                else
                {
                    // エラーが返ってきた
                    UIManager.Instance.TryHideLoadingView();
                    OnErrorAction(response.exception);
                    o.OnCompleted();
                }
            });
            var onErrorAction = new Action<PlayFabError>(error =>
            {
                UIManager.Instance.TryHideLoadingView();
                OnErrorAction(error);
                o.OnCompleted();
            });

            UIManager.Instance.ShowLoadingView();
            ExecuteCloudFunction<TReq>(functionName, request, callback, onErrorAction);
            return Disposable.Empty;
        });
    }

    private static void ExecuteCloudFunction<TReq>(string functionName, TReq request, Action<ExecuteFunctionResult> callback, Action<PlayFabError> onErrorAction)
    {
        PlayFabCloudScriptAPI.ExecuteFunction(new ExecuteFunctionRequest()
        {
            Entity = new PlayFab.CloudScriptModels.EntityKey()
            {
                Id = PlayFabSettings.staticPlayer.EntityId,
                Type = PlayFabSettings.staticPlayer.EntityType
            },
            FunctionName = functionName,
            FunctionParameter = DataProcessUtil.GetRequest<TReq>(request),
            GeneratePlayStreamEvent = false,
        }, res => callback(res), error => onErrorAction(error));
    }

    #endregion CloudFunction

    /// <summary>
    /// API実行時のチェック処理
    /// </summary>
    /// <returns>API実行可ならtrue, 不可ならfalse</returns>
    private static bool CheckSendRequestAction()
    {
        var maintenanceCachedRecords = MasterRecord.GetMasterOf<MaintenanceMB>();
        if (maintenanceCachedRecords != null)
        {
            var maintenance = MasterRecord.GetMasterOf<MaintenanceMB>().GetAll().FirstOrDefault(m => DateTimeUtil.GetDateFromMasterString(m.startDate) <= DateTimeUtil.Now && DateTimeUtil.Now < DateTimeUtil.GetDateFromMasterString(m.endDate));
            if (maintenance != null)
            {
                var content = SceneLoadManager.activateSceneType == SceneType.Title ?
                    $"ただいまメンテナンス中です\nご迷惑をおかけしてしまい申し訳ございません\nメンテナンス終了予定時間: {DateTimeUtil.GetDateFromMasterString(maintenance.endDate).ToString("yyyy/MM/dd hh:mm:ss")}" :
                    "ただいまメンテナンス中のため\nタイトルに戻ります";
                CommonDialogFactory.Create(new CommonDialogRequest()
                {
                    commonDialogType = CommonDialogType.YesOnly,
                    title = "お知らせ",
                    content = content,
                })
                    .Do(_ => SceneLoadManager.ChangeScene(SceneType.Splash))
                    .Subscribe();

                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 共通エラーアクション
    /// </summary>
    private static void OnErrorAction(PlayFabError error)
    {
        Debug.LogError(error.ToString());
        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            commonDialogType = CommonDialogType.YesOnly,
            title = "お知らせ",
            content = "エラーが発生しました"
        }).Subscribe();
    }

    /// <summary>
    /// CloudScriptでのエラーアクション
    /// </summary>
    private static void OnErrorAction(PMApiException exception)
    {
        Debug.LogError(exception.message);
        Debug.LogError(exception.StackTrace);
        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            commonDialogType = CommonDialogType.YesOnly,
            title = "お知らせ",
            content = "エラーが発生しました"
        }).Subscribe();
    }
}