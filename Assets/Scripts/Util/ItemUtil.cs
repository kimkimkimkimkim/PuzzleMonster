using System;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Item;
using PM.Enum.UI;

public static class ItemUtil
{
    /// <summary>
    /// モンスターマスタからユーザーモンスターデータを作成する
    /// </summary>
    public static List<UserMonsterInfo> GetUserMonsterList(List<MonsterMB> monsterList)
    {
        return monsterList.Select(m =>
        {
            return new UserMonsterInfo()
            {
                id = Guid.NewGuid().ToString(),
                monsterId = m.id,
                grade = m.initialGrade,
            };
        }).ToList();
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
