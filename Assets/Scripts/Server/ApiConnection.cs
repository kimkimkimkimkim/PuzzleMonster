﻿using System;
using System.Collections;
using System.Collections.Generic;
using PM.Enum.UI;
using GameBase;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;
using UniRx;
using UnityEngine;

public partial class ApiConnection
{
    private static IObservable<TResp> SendRequest<TReq, TResp>(ApiType apiType, TReq request) where TReq : PlayFabRequestCommon where TResp : PlayFabResultCommon
    {
        return Observable.Create<TResp>(o =>
        {
            var callback = new Action<TResp>(res =>
            {
                UIManager.Instance.TryHideTapBlocker();
                o.OnNext(res);
                o.OnCompleted();
            });
            var onErrorAction = new Action<PlayFabError>(error =>
            {
                UIManager.Instance.TryHideTapBlocker();
                OnErrorAction(error);
                o.OnCompleted();
            });

            UIManager.Instance.ShowTapBlocker();
            ExecuteApi<TReq, TResp>(apiType, request, callback, onErrorAction);
            return Disposable.Empty;
        });
    }

    #region ExecuteApi
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
            default:
                break;
        }
    }
    #endregion

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

    private enum ApiType
    {
        LoginWithCustomID,
        GetPlayerProfile,
        UpdateUserTitleDisplayName,
        GetUserInventory,
        AddUserVirtualCurrency,
        GetTitleData,
    }
}
