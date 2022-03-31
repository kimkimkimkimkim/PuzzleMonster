using PM.Enum.Battle;
using PM.Enum.Monster;
using System.Linq;
/// <summary>
/// クライアント用のモンスター関係のUtil 
/// </summary>
public static class ClientMonsterUtil
{
    public static long GetNormalSkillId(long monsterId, int monsterLevel)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
        var skillLevelUpTable = MasterRecord.GetMasterOf<SkillLevelUpTableMB>().GetAll()
            .Where(m => m.battleActionType == BattleActionType.NormalSkill)
            .OrderBy(m => m.requiredMonsterLevel)
            .LastOrDefault(m => monsterLevel >= m.requiredMonsterLevel);
        switch (skillLevelUpTable?.skillLevel ?? 0)
        {
            case 1:
                return monster.level1NormalSkillId;
            case 2:
                return monster.level2NormalSkillId;
            case 3:
                return monster.level3NormalSkillId;
            default:
                return 0;
        }
    }

    public static long GetUltimateSkillId(long monsterId, int monsterLevel)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
        var skillLevelUpTable = MasterRecord.GetMasterOf<SkillLevelUpTableMB>().GetAll()
            .Where(m => m.battleActionType == BattleActionType.UltimateSkill)
            .OrderBy(m => m.requiredMonsterLevel)
            .LastOrDefault(m => monsterLevel >= m.requiredMonsterLevel);
        switch (skillLevelUpTable?.skillLevel ?? 0)
        {
            case 1:
                return monster.level1UltimateSkillId;
            case 2:
                return monster.level2UltimateSkillId;
            case 3:
                return monster.level3UltimateSkillId;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 0の可能性もある
    /// </summary>
    public static long GetPassiveSkillId(long monsterId, int monsterLevel)
    {
        var monster = MasterRecord.GetMasterOf<MonsterMB>().Get(monsterId);
        var skillLevelUpTable = MasterRecord.GetMasterOf<SkillLevelUpTableMB>().GetAll()
            .Where(m => m.battleActionType == BattleActionType.PassiveSkill)
            .OrderBy(m => m.requiredMonsterLevel)
            .LastOrDefault(m => monsterLevel >= m.requiredMonsterLevel);
        switch (skillLevelUpTable?.skillLevel ?? 0)
        {
            case 1:
                return monster.level1PassiveSkillId;
            case 2:
                return monster.level2PassiveSkillId;
            case 3:
                return monster.level3PassiveSkillId;
            default:
                return 0;
        }
    }

    public static int GetMaxMonsterLevel(MonsterRarity rarity, int grade)
    {
        var targetMaxMonsterLevel = MasterRecord.GetMasterOf<MaxMonsterLevelMB>().GetAll().FirstOrDefault(m => m.monsterRarity == rarity && m.monsterGrade == grade);
        return targetMaxMonsterLevel?.maxMonsterLevel ?? 0;
    }
}