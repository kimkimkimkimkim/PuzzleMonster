using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using PlayFab.ClientModels;
using PM.Enum.Data;

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
    /// パラム名とその値のJsonからユーザーデータを返します
    /// </summary>
    /// string : "userMonsterList"
    /// string : "[{"id":1,"name":"ヒコザル"}]"
    public static UserDataInfo GetUserData(Dictionary<string, string> dict)
    {
        var userData = new UserDataInfo();

        foreach (var kvp in dict)
        {
            if (kvp.Key == UserDataKey.userMonsterList.ToString())
            {
                userData.userMonsterList = JsonConvert.DeserializeObject<List<UserMonsterInfo>>(kvp.Value);
            }
        }

        return userData;
    }

    /// <summary>
    /// GetUserDataApiのレスポンスからユーザーデータを作成します
    /// </summary>
    public static UserDataInfo GetUserData(Dictionary<string, UserDataRecord> dict)
    {
        var userDataDict = dict.ToDictionary(kvp => kvp.Key, kvp => JsonConvert.DeserializeObject<object>(kvp.Value.Value));
        var userDataDictJson = JsonConvert.SerializeObject(userDataDict);
        var userData = JsonConvert.DeserializeObject<UserDataInfo>(userDataDictJson);
        return userData;
    }
}
