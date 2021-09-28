using System.Collections.Generic;

public class UpdateUserMonsterFormationApiInterface
{
    public static string functionName = "UpdateUserMonsterFormation";
}

// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class UpdateUserMonsterFormationApiRequest : PMApiRequestBase
{
    /// <summary>
    /// パーティインデックス
    /// </summary>
    public int partyIndex;

    /// <summary>
    /// ユーザーモンスターIDリスト
    /// </summary>
    public List<string> userMonsterIdList;
}

public class UpdateUserMonsterFormationApiResponse : PMApiResponseBase
{
}
