public class PlayerRankUpDialogRequest
{
    /// <summary>
    /// ランクアップ前ランク
    /// </summary>
    public int beforeRank { get; set; }

    /// <summary>
    /// ランクアップ後ランク
    /// </summary>
    public int afterRank { get; set; }

    /// <summary>
    /// ランクアップ前スタミナ最大値
    /// </summary>
    public int beforeMaxStamina { get; set; }

    /// <summary>
    /// ランクアップ後スタミナ最大値
    /// </summary>
    public int afterMaxStamina { get; set; }
}

public class PlayerRankUpDialogResponse
{
}