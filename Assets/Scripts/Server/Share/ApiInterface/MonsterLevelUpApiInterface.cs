public class MonsterLevelUpApiInterface
{
    public static string functionName = "MonsterLevelUp";
}

// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class MonsterLevelUpApiRequest
{
    /// <summary>
    /// ユーザーモンスターID
    /// </summary>
    public string userMonsterId;

    /// <summary>
    /// 与える経験値量
    /// </summary>
    public int exp;
}

public class MonsterLevelUpApiResponse
{
    /// <summary>
    /// 経験値付与後のレベル
    /// </summary>
    public int level;
}
