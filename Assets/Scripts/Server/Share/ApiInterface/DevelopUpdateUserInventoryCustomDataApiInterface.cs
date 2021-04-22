using System.Collections.Generic;

public class DevelopUpdateUserInventoryCustomDataApiInterface
{
    public static string functionName = "DevelopUpdateUserInventoryCustomData";
}

// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class DevelopUpdateUserInventoryCustomDataApiRequest : PMApiRequestBase
{
    /// <summary>
    /// PlayFabのアイテムインスタンスID
    /// </summary>
    public string itemInstanceId;

    /// <summary>
    /// 更新するカスタムデータ
    /// </summary>
    public Dictionary<string, string> data;
}

public class DevelopUpdateUserInventoryCustomDataApiResponse : PMApiResponseBase
{

}