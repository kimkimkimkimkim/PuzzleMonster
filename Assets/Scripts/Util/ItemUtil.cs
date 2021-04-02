using System;
using System.Collections.Generic;
using System.Linq;

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
}
