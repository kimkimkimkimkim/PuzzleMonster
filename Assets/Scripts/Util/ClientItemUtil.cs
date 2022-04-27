using PM.Enum.Item;
using PM.Enum.Monster;
using PM.Enum.UI;
using System.Linq;

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

    /// <summary>
    /// アイテム名を取得する
    /// </summary>
    public static string GetName(ItemMI item)
    {
        switch (item.itemType) 
        {
            case ItemType.VirtualCurrency:
                var virtualCurrency = (VirtualCurrencyType)item.itemId;
                return virtualCurrency.Name();
            case ItemType.Monster:
                var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(item.itemId);
                return monster.name;
            case ItemType.Property:
                var property = MasterRecord.GetMasterOf<PropertyMB>().Get(item.itemId);
                return property.name;
            default:
                return "";
        }
    }

    /// <summary>
    /// 指定したアイテムの所持数を取得する
    /// </summary>
    public static int GetPossessedNum(ItemType itemType, long itemId)
    {
        switch (itemType)
        {
            case ItemType.VirtualCurrency:
                var userVirtualCurrency = ApplicationContext.userVirtualCurrency.virtualCurrencyNumList.FirstOrDefault(c => c.virtualCurrencyId == itemId);
                return userVirtualCurrency != null ? userVirtualCurrency.num : 0;
            case ItemType.Monster:
                var userMonster = ApplicationContext.userData.userMonsterList.FirstOrDefault(u => u.monsterId == itemId);
                return userMonster != null ? userMonster.num : 0;
            case ItemType.Property:
                return ApplicationContext.userData.userPropertyList.GetNum((PropertyType)itemId);
            default:
                return 0;
        }

    }
}
