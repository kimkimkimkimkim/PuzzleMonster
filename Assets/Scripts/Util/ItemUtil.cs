using System;
using System.Collections.Generic;
using System.Linq;
using PlayFab.ClientModels;
using PM.Enum.Item;
using PM.Enum.UI;

public static class ItemUtil
{
    /// <summary>
    /// モンスターマスタからユーザーモンスターデータを作成する
    /// </summary>
    public static List<UserMonsterInfo> GetUserMonsterList(List<MonsterMB> monsterList)
    {
        return monsterList.Select(m => GetUserMonster(m)).ToList();
    }

    /// <summary>
    /// モンスターマスタからユーザーモンスターデータを作成する
    /// </summary>
    public static UserMonsterInfo GetUserMonster(MonsterMB monster)
    {
        return new UserMonsterInfo()
        {
            id = Guid.NewGuid().ToString(),
            monsterId = monster.id,
            grade = monster.initialGrade,
        };
    }

    /// <summary>
    /// ItemInstanceからItemMIを返す
    /// </summary>
    public static List<ItemMI> GetItemMI(List<ItemInstance> itemInstanceList)
    {
        return itemInstanceList.Select(i =>
        {

            return new ItemMI()
            {
                itemType = GetItemType(i),
                itemId = GetItemId(i),
                num = 1,
            };
        }).ToList();
    }

    /// <summary>
    /// ItemInstanceからIdを返す
    /// </summary>
    public static long GetItemId(ItemInstance itemInstance)
    {
        // ItemInstanceのIdは「ItemType名+ID値」となっている
        var itemTypeWordCount = itemInstance.ItemClass.Length;
        var itemInstanceId = itemInstance.ItemId;
        var id = itemInstanceId.Substring(itemTypeWordCount);
        return long.Parse(id);
    }

    /// <summary>
    /// ItemInstanceからItemTypeを返す
    /// </summary>
    public static ItemType GetItemType(ItemInstance itemInstance)
    {
        // ItemInstanceのClassはItemTypeと等しい
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            if (itemInstance.ItemClass == itemType.ToString()) return itemType;
        }

        return ItemType.None;
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
            default:
                return IconImageType.None;
        }
    }
}
