using GameBase;
using PM.Enum.Item;
using UnityEngine;

[ResourcePath("UI/Parts/Parts-MonsterBoxScrollItem")]
public class MonsterBoxScrollItem : ScrollItem
{
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected IconItem _monsterIconItem;

    public void SetGradeImage(int grade)
    {
        _monsterGradeParts.SetGradeImage(grade);
    }

    public void SetMonsterImage(long monsterId)
    {
        _monsterIconItem.SetIcon(ItemType.Monster, monsterId);
    }
}