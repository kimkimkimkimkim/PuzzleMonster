using System.Collections.Generic;

public class UserMonsterPartyInfo
{
    /// <summary>
    /// ID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// パーティーID（1 ~ MAX_PARTY_NUM）
    /// </summary>
    public int partyId { get; set; }

    /// <summary>
    /// ユーザーモンスターIDリスト
    /// </summary>
    public List<string> userMonsterIdList { get; set; }
}
