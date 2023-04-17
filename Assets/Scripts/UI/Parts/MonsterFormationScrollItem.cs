using GameBase;
using PM.Enum.Item;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-MonsterFormationScrollItem")]
public class MonsterFormationScrollItem : ScrollItem
{
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected IconItem _monsterIconItem;
    [SerializeField] protected GameObject _backgroundPanel;
    [SerializeField] protected GameObject _numberPanel;
    [SerializeField] protected Text _numberText;
    [SerializeField] protected Sprite _emptySprite;

    public void SetGradeImage(int grade)
    {
        _monsterGradeParts.SetGradeImage(grade);
    }

    public void SetMonsterImage(long monsterId)
    {
        if (monsterId <= 0) _monsterIconItem.SetSprite(_emptySprite);

        _monsterIconItem.SetIcon(ItemType.Monster, monsterId);
    }

    public void SetSelectionState(int number)
    {
        if (number <= 0)
        {
            if (_backgroundPanel != null) _backgroundPanel.SetActive(false);
            _numberPanel.SetActive(false);
        }
        else
        {
            _numberText.text = number.ToString();
            if (_backgroundPanel != null) _backgroundPanel.SetActive(true);
            _numberPanel.SetActive(true);
        }
    }
}