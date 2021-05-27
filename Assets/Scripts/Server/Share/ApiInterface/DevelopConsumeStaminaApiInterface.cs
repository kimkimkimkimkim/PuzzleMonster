public class DevelopConsumeStaminaApiInterface
{
    public static string functionName = "DevelopConsumeStamina";
}

// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class DevelopConsumeStaminaApiRequest : PMApiRequestBase
{
    /// <summary>
    /// 消費するスタミナ量
    /// </summary>
    public int consumeStamina;
}

public class DevelopConsumeStaminaApiResponse : PMApiResponseBase
{
    /// <summary>
    /// 消費後のスタミナ値
    /// </summary>
    public int stamina;
}