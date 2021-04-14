using System.Collections.Generic;
using PlayFab.ClientModels;

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
    public List<ItemInstance> itemInstanceList { get; set; }
}
