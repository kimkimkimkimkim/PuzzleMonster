using System.Collections.Generic;
using PlayFab.ClientModels;
/// <summary>
/// ユーザーデータ情報
/// </summary>
public class UserDataInfo
{
    /// <summary>
    /// ユーザーモンスター情報
    /// </summary>
    public List<UserMonsterInfo> userMonsterList { get; set; } = new List<UserMonsterInfo>();
}
