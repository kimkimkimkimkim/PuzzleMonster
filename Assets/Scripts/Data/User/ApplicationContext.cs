using System;
using PM.Enum.Item;
using PlayFab.ClientModels;
using UniRx;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using PM.Enum.Data;
using System.Linq;
using Newtonsoft.Json;

public static class ApplicationContext
{
    public static PlayerProfileModel playerProfile { get; set; } = new PlayerProfileModel();
    public static UserVirtualCurrencyInfo userVirtualCurrency { get; set; } = new UserVirtualCurrencyInfo();
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
                playerProfile = res.PlayerProfile;
            }))
            .SelectMany(_ => ApiConnection.GetUserInventory().Do(res =>
            {
                // ユーザーインベントリ情報の更新
                UpdateUserInventory(res);
            }))
            .SelectMany(_ => ApiConnection.GetUserData()) // ユーザーデータの更新
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
            if (virtualCurrency.Key == VirtualCurrencyType.OB.ToString()) userVirtualCurrency.orb = virtualCurrency.Value;
            if (virtualCurrency.Key == VirtualCurrencyType.CN.ToString()) userVirtualCurrency.coin = virtualCurrency.Value;
        }
    }

    /// <summary>
    /// ユーザーデータ情報キャッシュを更新する
    /// </summary>
    public static void UpdateUserData(UserDataInfo userData)
    {
        var dict = new Dictionary<string, string>();
        var properties = typeof(UserDataInfo).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (PropertyInfo property in properties)
        {
            var value = property.GetValue(userData);
            if(value != null)property.SetValue(ApplicationContext.userData, property.GetValue(userData));
        }
    }

    /// <summary>
    /// サーバーから取得したユーザーデータ情報でキャッシュを更新する
    /// </summary>
    public static void UpdateUserData(Dictionary<string, UserDataRecord> dict)
    {
        var strDict = dict.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);
        userData = GetUserData(strDict);
    }

    /// <summary>
    /// 指定したキーの値だけを更新する
    /// </summary>
    /// <param name="dict">更新したい値のキーとその値を保持した辞書</param>
    public static void UpdateUserData(Dictionary<UserDataKey, object> dict)
    {
        var strDict = dict.ToDictionary(kvp => kvp.Key.ToString(), kvp => JsonConvert.SerializeObject(kvp.Value));
        userData = GetUserData(strDict);
    }

    /// <summary>
    /// パラム名とその値のJsonからユーザーデータを返します
    /// </summary>
    /// <param name="dict">Dict.</param>
    private static UserDataInfo GetUserData(Dictionary<string,string> dict)
    {
        var userData = new UserDataInfo();

        foreach(var kvp in dict)
        {
            if(kvp.Key == UserDataKey.userMonsterList.ToString()) {
                userData.userMonsterList = JsonConvert.DeserializeObject<List<UserMonsterInfo>>(kvp.Value);
            }
        }

        return userData;
    }
}
