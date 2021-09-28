using System.Collections.Generic;
using System.ComponentModel;
using PM.Enum.Gacha;

[Description("GachaBoxDetailMB")]
public class GachaBoxDetailMB : MasterBookBase
{
    /// <summary>
    /// ガチャボックスID
    /// </summary>
    public long gachaBoxId { get; set; }

    /// <summary>
    /// タイトル
    /// </summary>
    public string title { get; set; }

    /// <summary>
    /// PlayFabで設定しているバンドルID
    /// </summary>
    public long bundleId { get; set; }

    /// <summary>
    /// 必要アイテムリスト
    /// </summary>
    public List<ItemMI> requiredItemList { get; set; }
}
