using System.Collections.Generic;

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
#if UNITY_EDITOR
    public List<PlayFab.ClientModels.ItemInstance> itemInstanceList { get; set; }
#else
    public List<PlayFab.ServerModels.GrantedItemInstance> itemInstanceList { get; set; }
#endif
}
