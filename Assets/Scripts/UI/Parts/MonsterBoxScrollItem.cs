using GameBase;
using PM.Enum.Monster;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-MonsterBoxScrollItem")]
public class MonsterBoxScrollItem : ScrollItem
{
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected Image _monsterRarityImage;
    [SerializeField] protected Image _monsterAttributeImage;
    [SerializeField] protected Text _monsterLevelText;
    [SerializeField] protected Text _monsterStackNumText;

    public void SetGradeImage(int grade)
    {
        _monsterGradeParts.SetGradeImage(grade);
    }

    public void SetUI(long monsterId, MonsterRarity monsterRarity, MonsterAttribute monsterAttribute, int monsterLevel, int stackNum)
    {
        _monsterLevelText.text = $"Lv {monsterLevel}";
        _monsterStackNumText.text = stackNum.ToString();
        Observable.WhenAll(
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId).Do(sprite => _monsterImage.sprite = sprite),
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterRarity, (int)monsterRarity).Do(sprite => _monsterRarityImage.sprite = sprite),
            PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.MonsterAttribute, (int)monsterAttribute).Do(sprite => _monsterAttributeImage.sprite = sprite)
        )
            .Subscribe();
    }
}