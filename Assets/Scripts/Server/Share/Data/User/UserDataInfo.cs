using System.Collections.Generic;

/// <summary>
/// ユーザーデータ情報
/// </summary>
public class UserDataInfo
{
    /// <summary>
    /// ユーザーモンスター情報
    /// </summary>
    public List<UserMonsterInfo> userMonsterList { get; set; } = new List<UserMonsterInfo>();

    /// <summary>
    /// ユーザー資産リスト
    /// </summary>
    public List<UserPropertyInfo> userPropertyList { get; set; } = new List<UserPropertyInfo>();
}
