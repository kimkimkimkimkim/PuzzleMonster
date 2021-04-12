using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PM.Enum.Item;

/// <summary>
/// ユーザーデータに関するUtil
/// </summary>
public static class UserDataUtil
{
    /// <summary>
    /// ユーザーデータのIdを作成して返す
    /// </summary>
    public static string CreateUserDataId()
    {
        return Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 辞書で保持したデータを指定のクラスでデシリアライズして返す
    /// Valueをobjectにデシリアライズしたあと全体をシリアリズして最後に指定のクラスでデシリアライズ
    /// </summary>
    /// <param name="dict">key:変数名, value:その変数の値を文字列にしたもの</param>
    public static T GetDeserializedObject<T>(Dictionary<string,string> dict)
    {
        if (dict == null) dict = new Dictionary<string, string>();

        var userDataDict = dict.ToDictionary(kvp => kvp.Key, kvp => JsonConvert.DeserializeObject<object>(kvp.Value));
        var userDataDictJson = JsonConvert.SerializeObject(userDataDict);
        return JsonConvert.DeserializeObject<T>(userDataDictJson);
    }

    /// <summary>
    /// GetUserDataApiのレスポンスからユーザーデータを作成します
    /// </summary>
#if UNITY_EDITOR
    public static UserDataInfo GetUserData(Dictionary<string, PlayFab.ClientModels.UserDataRecord> dict)
#else
    public static UserDataInfo GetUserData(Dictionary<string, PlayFab.ServerModels.UserDataRecord> dict)
#endif
    {
        var userDataDict = dict.ToDictionary(kvp => kvp.Key, kvp => JsonConvert.DeserializeObject<object>(kvp.Value.Value));
        var userDataDictJson = JsonConvert.SerializeObject(userDataDict);
        var userData = JsonConvert.DeserializeObject<UserDataInfo>(userDataDictJson);
        return userData;
    }

    /// <summary>
    /// GetUserInventoryApiのレスポンスからユーザー仮想通貨情報を作成します
    /// </summary>
#if UNITY_EDITOR
    public static UserVirtualCurrencyInfo GetUserVirutalCurrency(PlayFab.ClientModels.GetUserInventoryResult result)
#else
    public static UserVirtualCurrencyInfo GetUserVirutalCurrency(PlayFab.ServerModels.GetUserInventoryResult result)
#endif
    {
        // 仮想通貨情報の更新
        var userVirtualCurrency = new UserVirtualCurrencyInfo();
        foreach (var virtualCurrency in result.VirtualCurrency)
        {
            if (virtualCurrency.Key == VirtualCurrencyType.OB.ToString()) userVirtualCurrency.orb = virtualCurrency.Value;
            if (virtualCurrency.Key == VirtualCurrencyType.CN.ToString()) userVirtualCurrency.coin = virtualCurrency.Value;
        }
        return userVirtualCurrency;
    }

    /// <summary>
    /// GetUserInventoryApiのレスポンスからユーザーインベントリを作成します
    /// </summary>
#if UNITY_EDITOR
    public static UserInventoryInfo GetUserInventory(PlayFab.ClientModels.GetUserInventoryResult result)
#else
    public static UserInventoryInfo GetUserInventory(PlayFab.ServerModels.GetUserInventoryResult result)
#endif
    {
        var userInventory = new UserInventoryInfo();
        var itemInstanceList = result.Inventory;
        itemInstanceList.ForEach(i =>
        {
            var itemType = ItemUtil.GetItemType(i);
            switch (itemType)
            {
                case ItemType.Property:
                    var userProperty = new UserPropertyInfo()
                    {
                        id = i.ItemInstanceId,
                        propertyId = ItemUtil.GetItemId(i),
                        num = i.RemainingUses ?? 0,
                    };
                    userInventory.userPropertyList.Add(userProperty);
                    break;
                case ItemType.Monster:
                    var userMonsterCustomData = GetDeserializedObject<UserMonsterCustomData>(i.CustomData);
                    var userMonster = new UserMonsterInfo()
                    {
                        id = i.ItemInstanceId,
                        monsterId = ItemUtil.GetItemId(i),
                        customData = userMonsterCustomData,
                    };
                    userInventory.userMonsterList.Add(userMonster);
                    break;
                default:
                    break;
            }
        });

        return userInventory;
    }
}
