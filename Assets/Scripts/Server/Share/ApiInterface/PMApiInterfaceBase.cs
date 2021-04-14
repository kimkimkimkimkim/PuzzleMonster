// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class PMApiRequestBase
{

}

public class PMApiResponseBase
{
    public PMErrorCode errorCode;
    public string message;
}

public enum PMErrorCode
{
    Success = 0,
    Unknown = 1,
}