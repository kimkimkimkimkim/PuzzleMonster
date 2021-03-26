using System;
using Enum.UI;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.SharedModels;
using UniRx;
using UnityEngine;

public partial class ApiConnection
{
    /// <summary>
    /// カスタムIDでのログイン処理
    /// </summary>
    public static IObservable<LoginResult> LoginWithCustomID()
    {
        return SendRequest<LoginWithCustomIDRequest, LoginResult>(ApiType.LoginWithCustomID, new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = SaveDataUtil.System.GetCustomId(),
            CreateAccount = true,
        });
    }

    /// <summary>
    /// プレイヤープロフィールを取得
    /// </summary>
    public static IObservable<GetPlayerProfileResult> GetPlayerProfile()
    {
        return SendRequest<GetPlayerProfileRequest, GetPlayerProfileResult > (ApiType.GetPlayerProfile, new GetPlayerProfileRequest()
        {
            PlayFabId = PlayFabSettings.staticPlayer.PlayFabId,
        });
    }

    /// <summary>
    /// ユーザー名の更新
    /// </summary>
    public static IObservable<UpdateUserTitleDisplayNameResult> UpdateUserTitleDisplayName(string userName)
    {
        return SendRequest<UpdateUserTitleDisplayNameRequest, UpdateUserTitleDisplayNameResult>(ApiType.UpdateUserTitleDisplayName, new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = userName
        });
    }

    /// <summary>
    /// ユーザー名の更新
    /// </summary>
    public static IObservable<GetUserInventoryResult> GetUserInventory(string userName)
    {
        return SendRequest<GetUserInventoryRequest, GetUserInventoryResult>(ApiType.UpdateUserTitleDisplayName, new GetUserInventoryRequest());
    }
}
