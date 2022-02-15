using System.Collections.Generic;

public class MonsterFormationWindowRequest
{
    /// <summary>
    /// パーティID
    /// </summary>
    /// <value>The party identifier.</value>
    public int partyId { get; set; }

    /// <summary>
    /// 選択したパーティーのユーザーモンスターリスト
    /// </summary>
    public List<UserMonsterInfo> initialUserMonsterList { get; set; }
}

public class MonsterFormationWindowResponse
{
}