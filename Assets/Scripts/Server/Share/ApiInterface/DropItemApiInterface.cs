using System.Collections.Generic;

public class DropItemApiInterface
{
    public static string functionName = "DropItem";
}

// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class DropItemApiRequest : PMApiRequestBase
{
    /// <summary>
    /// ドロップテーブル名
    /// </summary>
    public string dropTableName;
}

public class DropItemApiResponse : PMApiResponseBase
{
    /// <summary>
    /// 取得したアイテムリスト
    /// </summary>
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN
    public List<PlayFab.ClientModels.ItemInstance> itemInstanceList { get; set; }
#else
    public List<PlayFab.ServerModels.GrantedItemInstance> itemInstanceList { get; set; }
#endif
}
