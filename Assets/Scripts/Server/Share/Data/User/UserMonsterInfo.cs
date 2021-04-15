/// <summary>
/// ユーザーのモンスター情報
/// </summary>
public class UserMonsterInfo
{
    /// <summary>
    /// PlayFabのItemInstanceID
    /// </summary>
    public string id { get; set; }

    /// <summary>
    /// モンスターID
    /// </summary>
    public long monsterId { get; set; }

    /// <summary>
    /// カスタムデータ
    /// </summary>
    public UserMonsterCustomData customData { get; set; }
}
