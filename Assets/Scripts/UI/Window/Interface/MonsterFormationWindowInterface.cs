using System.Collections.Generic;

public class MonsterFormationWindowRequest
{
    /// <summary>
    /// パーティID
    /// </summary>
    /// <value>The party identifier.</value>
    public int partyId { get; set; }

    /// <summary>
    /// ユーザーモンスターリスト
    /// </summary>
    public List<UserMonsterInfo> userMontserList { get; set; }

    /// <summary>
    /// 選択したパーティーのユーザーモンスターリスト
    /// </summary>
    public List<UserMonsterInfo> initialUserMonsterList { get; set; }
}

public class MonsterFormationWindowResponse
{
}