using System.Collections.Generic;

public class GrantItemsToUserApiInterface
{
    public static string functionName = "GrantItemsToUser";
}

public class GrantItemsToUserApiRequest
{
    /// <summary>
    /// 付与するアイテムのアイテムIDリスト
    /// </summary>
    public List<string> itemIdList { get; set; }
}
