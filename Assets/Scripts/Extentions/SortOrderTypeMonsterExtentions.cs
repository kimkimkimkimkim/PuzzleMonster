using PM.Enum.Monster;
using PM.Enum.SortOrder;

public static class SortOrderTypeMonsterExtends
{
    public static string Name(this SortOrderTypeMonster type)
    {
        switch (type)
        {
            case SortOrderTypeMonster.Id:
                return "ID順";
            case SortOrderTypeMonster.Attribute:
                return "属性順";
            case SortOrderTypeMonster.Rarity:
                return "レアリティ順";
            case SortOrderTypeMonster.Level:
                return "レベル順";
            case SortOrderTypeMonster.Grade:
                return "グレード順";
            case SortOrderTypeMonster.Luck:
                return "ラック順";
            case SortOrderTypeMonster.Hp:
                return "HP順";
            case SortOrderTypeMonster.Attack:
                return "攻撃力順";
            case SortOrderTypeMonster.Defense:
                return "防御力順";
            case SortOrderTypeMonster.Speed:
                return "スピード順";
            default:
                return "";
        }
    }
}