// バトル関係のUtil
using System.Collections.Generic;

public static class BattleUtil
{
    public static BattleMonsterInfo GetBattleMonster(UserMonsterInfo userMonster)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(userMonster.monsterId);
        var status = MonsterUtil.GetMonsterStatus(monster, userMonster.customData.level);

        return new BattleMonsterInfo()
        {
            level = userMonster.customData.level,
            maxHp = status.hp,
            currentHp = status.hp,
            baseAttack = status.attack,
            currentAttack = status.attack,
            baseDefense = status.defense,
            currentDefense = status.defense,
            baseSpeed = status.speed,
            currentSpeed = status.speed,
            baseHeal = status.heal,
            currentHeal = status.heal,
            currentCt = 0,
            battleConditionList = new List<BattleConditionInfo>(),
        };
    }

    public static BattleMonsterInfo GetBattleMonster(QuestMonsterMB questMonster)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(questMonster.monsterId);
        var status = MonsterUtil.GetMonsterStatus(monster, questMonster.level);

        return new BattleMonsterInfo()
        {
            level = questMonster.level,
            maxHp = status.hp,
            currentHp = status.hp,
            baseAttack = status.attack,
            currentAttack = status.attack,
            baseDefense = status.defense,
            currentDefense = status.defense,
            baseSpeed = status.speed,
            currentSpeed = status.speed,
            baseHeal = status.heal,
            currentHeal = status.heal,
            currentCt = 0,
            battleConditionList = new List<BattleConditionInfo>(),
        };
    }
}