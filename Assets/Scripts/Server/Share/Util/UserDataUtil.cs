using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
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
    /// GetUserDataApiのレスポンスからユーザーデータを作成します
    /// クライアント用
    /// </summary>
    public static UserDataInfo GetUserData(Dictionary<string, PlayFab.ClientModels.UserDataRecord> dict)
    {
        var userDataDict = dict.ToDictionary(kvp => kvp.Key, kvp => JsonConvert.DeserializeObject<object>(kvp.Value.Value));
        var userDataDictJson = JsonConvert.SerializeObject(userDataDict);
        var userData = JsonConvert.DeserializeObject<UserDataInfo>(userDataDictJson);
        return userData;
    }

    /// <summary>
    /// GetUserDataApiのレスポンスからユーザーデータを作成します
    /// サーバー用
    /// </summary>
#if !UNITY_EDITOR
    public static UserDataInfo GetUserData(Dictionary<string, PlayFab.ServerModels.UserDataRecord> dict)
    {
        var userDataDict = dict.ToDictionary(kvp => kvp.Key, kvp => JsonConvert.DeserializeObject<object>(kvp.Value.Value));
        var userDataDictJson = JsonConvert.SerializeObject(userDataDict);
        var userData = JsonConvert.DeserializeObject<UserDataInfo>(userDataDictJson);
        return userData;
    }
#endif
}
