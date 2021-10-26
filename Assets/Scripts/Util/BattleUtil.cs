// バトル関係のUtil
using System.Collections.Generic;

public static class BattleUtil
{
    public static BattleMonsterInfo GetBattleMonster(UserMonsterInfo userMonster, bool isPlayer, int index)
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
            maxCt = ConstManager.Battle.MAX_CT_VALUE,
            currentCt = 0,
            battleConditionList = new List<BattleConditionInfo>(),
            isActed = false,
            index = new BattleMonsterIndex() { isPlayer = isPlayer, index = index},
        };
    }

    public static BattleMonsterInfo GetBattleMonster(QuestMonsterMB questMonster, bool isPlayer, int index)
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
            maxCt = ConstManager.Battle.MAX_CT_VALUE,
            currentCt = 0,
            battleConditionList = new List<BattleConditionInfo>(),
            isActed = false,
            index = new BattleMonsterIndex() { isPlayer = isPlayer, index = index },
        };
    }
}