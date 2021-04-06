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
    /// PlayFabで設定しているドロップテーブルID
    /// </summary>
    public string dropTableId { get; set; }

    /// <summary>
    /// 必要アイテムリスト
    /// </summary>
    public List<ItemMI> requiredItemList { get; set; }

    /// <summary>
    /// 排出するモンスターの数
    /// </summary>
    public int dropNum { get; set; }
}
