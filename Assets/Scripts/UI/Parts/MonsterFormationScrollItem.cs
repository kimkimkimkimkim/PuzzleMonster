using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-MonsterFormationScrollItem")]
public class MonsterFormationScrollItem : ScrollItem { 

    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected GameObject _backgroundPanel;
    [SerializeField] protected GameObject _numberPanel;
    [SerializeField] protected Text _numberText;

    public void SetGradeImage(int grade)
    {
        _monsterGradeParts.SetGradeImage(grade);
    }

    public void SetMonsterImage(long monsterId)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .Where(res => res != null)
            .Do(res => _monsterImage.sprite = res)
            .Subscribe();
    }

    public void SetSelectionState(int number)
    {
        if(number <= 0)
        {
            if(_backgroundPanel != null)_backgroundPanel.SetActive(false);
            _numberPanel.SetActive(false);
        }
        else
        {
            _numberText.text = number.ToString();
            if(_backgroundPanel != null)_backgroundPanel.SetActive(true);
            _numberPanel.SetActive(true);
        }
    }
}