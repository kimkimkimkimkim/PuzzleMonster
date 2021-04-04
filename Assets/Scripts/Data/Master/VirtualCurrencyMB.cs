using System.ComponentModel;

/// <summary>
///  仮想通貨マスタ
/// </summary>
[Description("VirtualCurrencyMB")]
public class VirtualCurrencyMB : MasterBookBase
{
    /// <summary>
    /// 名前
    /// </summary>
    public string name { get; set; }

    /// <summary>
    /// PlayFabで指定しているコード
    /// </summary>
    public string playFabCode { get; set; }
}
