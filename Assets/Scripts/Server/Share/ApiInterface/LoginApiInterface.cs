public class LoginApiInterface
{
    public static string functionName = "Login";
}

public class LoginApiRequest : PMApiRequestBase
{
}

public class LoginApiResponse : PMApiResponseBase
{
    /// <summary>
    /// スタミナ
    /// </summary>
    public int stamina;
}
