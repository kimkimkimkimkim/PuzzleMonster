using System.Collections.Generic;
using System.Linq;
using PM.Enum.Item;
/// <summary>
/// ユーザー資産情報
/// </summary>
public class UserPropertyInfo
{
    /// <summary>
    /// PlayFabのItemInstanceID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// 資産ID
    /// </summary>
    public long propertyId { get; set; }

    /// <summary>
    /// 数
    /// </summary>
    public int num { get; set; }
}

/// <summary>
/// ユーザー資産情報の拡張クラス
/// </summary>
public static class UserPropertyExtensions
{
    /// <summary>
    /// ユーザー資産リストから指定のタイプのユーザー資産情報を返す
    /// </summary>
    public static UserPropertyInfo GetOrDefault(this List<UserPropertyInfo> list, PropertyType type)
    {
        return list.FirstOrDefault(u => u.propertyId == (int)type);
    }

    /// <summary>
    /// 指定したユーザー資産の所持数を返す
    /// </summary>
    public static int GetNum(this List<UserPropertyInfo> list, PropertyType type)
    {
        var userProperty = list.GetOrDefault(type);
        return userProperty == null ? 0 : userProperty.num;
    }
}
