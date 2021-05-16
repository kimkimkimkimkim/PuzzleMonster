using System.Collections.Generic;

public class UpdateUserMonsterFormationApiInterface
{
    public static string functionName = "UpdateUserMonsterFormation";
}

// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class UpdateUserMonsterFormationApiRequest : PMApiRequestBase
{
    /// <summary>
    /// パーティID
    /// </summary>
    public int partyId;

    /// <summary>
    /// ユーザーモンスターIDリスト
    /// </summary>
    public List<string> userMonsterIdList;
}

public class UpdateUserMonsterFormationApiResponse : PMApiResponseBase
{
}
