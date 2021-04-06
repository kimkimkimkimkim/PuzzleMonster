using PM.Enum.Item;

/// <summary>
/// 全てのアイテムを統一して扱うためのクラス
/// </summary>
public class ItemMI
{
    /// <summary>
    /// アイテムタイプ
    /// </summary>
    public ItemType itemType { get; set; }

    /// <summary>
    /// アイテムID
    /// </summary>
    public long itemId { get; set; }

    /// <summary>
    /// 個数
    /// </summary>
    public int num { get; set; }
}
