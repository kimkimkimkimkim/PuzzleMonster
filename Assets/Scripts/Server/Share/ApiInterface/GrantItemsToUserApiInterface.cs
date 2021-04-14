using System.Collections.Generic;
using PlayFab.ClientModels;

public class GrantItemsToUserApiInterface
{
    public static string functionName = "GrantItemsToUser";
}

public class GrantItemsToUserApiRequest : PMApiRequestBase
{
    /// <summary>
    /// 付与するアイテムのアイテムIDリスト
    /// </summary>
    public List<string> itemIdList;
}

public class GrantItemsToUserApiResponse : PMApiResponseBase
{    
    /// <summary>
    /// 取得したアイテムリスト
    /// </summary>
    public List<ItemInstance> itemInstanceList { get; set; }
}
