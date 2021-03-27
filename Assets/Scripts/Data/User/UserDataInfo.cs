using PlayFab.ClientModels;
/// <summary>
/// ユーザーデータ情報
/// </summary>
public class UserDataInfo
{
    /// <summary>
    /// ユーザープロフィール情報
    /// </summary>
    public PlayerProfileModel playerProfile { get; set; } = new PlayerProfileModel();

    /// <summary>
    /// ユーザー仮想通貨情報
    /// </summary>
    public UserVirtualCurrencyInfo userVirtualCurrency { get; set; } = new UserVirtualCurrencyInfo();
}
