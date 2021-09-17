using System.Collections.Generic;

public class UserMonsterPartyInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// パーティーインデックス（0 ~ MAX_PARTY_NUM - 1）
    /// </summary>
    public int partyIndex { get; set; }

    /// <summary>
    /// ユーザーモンスターIDリスト（空の場合はnullが入っている）
    /// </summary>
    public List<string> userMonsterIdList { get; set; }
}
