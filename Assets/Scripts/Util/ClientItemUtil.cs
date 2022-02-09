using PM.Enum.Item;
using PM.Enum.Monster;
using PM.Enum.UI;

public static class ClientItemUtil
{
    /// <summary>
    /// アイコン色タイプを取得する
    /// </summary>
    public static IconColorType GetIconColorType(ItemMI item)
    {
        switch (item.itemType)
        {
            case ItemType.Monster:
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(item.itemId);
                var iconColorType = ClientItemUtil.GetIconColorType(monster.attribute);
                return iconColorType;
            default:
                return IconColorType.None;
        }
    }

    /// <summary>
    /// アイコン色タイプを取得する
    /// </summary>
    public static IconColorType GetIconColorType(MonsterAttribute monsterAttribute)
    {
        switch (monsterAttribute)
        {
            case MonsterAttribute.Red:
                return IconColorType.Red;
            case MonsterAttribute.Blue:
                return IconColorType.Blue;
            case MonsterAttribute.Green:
                return IconColorType.Green;
            case MonsterAttribute.Yellow:
                return IconColorType.Yellow;
            case MonsterAttribute.Purple:
                return IconColorType.Purple;
            default:
                return IconColorType.None;
        }
    }

    /// <summary>
    /// アイコン画像タイプを取得する
    /// </summary>
    public static IconImageType GetIconImageType(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Monster:
                return IconImageType.Monster;
            case ItemType.Property:
                return IconImageType.Property;
            case ItemType.VirtualCurrency:
                return IconImageType.VirtualCurrency;
            default:
                return IconImageType.None;
        }
    }
}
