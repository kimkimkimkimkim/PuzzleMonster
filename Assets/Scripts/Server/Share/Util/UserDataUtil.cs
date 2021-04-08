using System;
using System.Collections.Generic;
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
}
