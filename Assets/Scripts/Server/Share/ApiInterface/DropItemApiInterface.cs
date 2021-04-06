public class DropItemApiInterface
{
    public static string functionName = "DropItem";
}

// APIリクエストの変数はプロパティにしてはいけない！(シリアライズの関係)
public class DropItemApiRequest
{
    /// <summary>
    /// ドロップテーブル名
    /// </summary>
    public string dropTableName;
}
