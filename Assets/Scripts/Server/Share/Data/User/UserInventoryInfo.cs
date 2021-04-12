using System.Collections.Generic;

/// <summary>
/// ユーザーのインベントリ情報
/// モンスター以外のインベントリ情報を扱う
/// </summary>
public class UserInventoryInfo
{
    /// <summary>
    /// ユーザー資産情報
    /// </summary>
    public List<UserPropertyInfo> userPropertyList { get; set; } = new List<UserPropertyInfo>();

    /// <summary>
    /// ユーザーモンスター情報
    /// </summary>
    public List<UserMonsterInfo> userMonsterList { get; set; } = new List<UserMonsterInfo>();
}
