﻿using System;
using PM.Enum.Item;
using PlayFab.ClientModels;
using UniRx;

public static class ApplicationContext
{
    public static UserDataInfo userData { get; private set; } = new UserDataInfo();

    /// <summary>
    /// アプリ起動時処理
    /// </summary>
    public static IObservable<Unit> EstablishSession()
    {
        return ApiConnection.LoginWithCustomID()
            .SelectMany(_ => ApiConnection.GetPlayerProfile().Do(res =>
            {
                // ユーザープロフィールの同期
                userData.playerProfile = res.PlayerProfile;
            }))
            .SelectMany(_ => ApiConnection.GetUserInventory().Do(res =>
            {
                // ユーザーインベントリ情報の更新
                UpdateUserInventory(res);
            }))
            .AsUnitObservable();
    }

    /// <summary>
    /// ユーザーインベントリ情報の更新
    /// </summary>
    public static void UpdateUserInventory(GetUserInventoryResult result)
    {
        // 仮想通貨情報の更新
        foreach (var virtualCurrency in result.VirtualCurrency)
        {
            if (virtualCurrency.Key == VirtualCurrencyType.OB.ToString()) userData.userVirtualCurrency.orb = virtualCurrency.Value;
            if (virtualCurrency.Key == VirtualCurrencyType.CN.ToString()) userData.userVirtualCurrency.coin = virtualCurrency.Value;
        }
    }
}