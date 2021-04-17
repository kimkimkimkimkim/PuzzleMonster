using System.Collections.Generic;
using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-MonsterBoxScrollItem")]
public class MonsterBoxScrollItem : ScrollItem
{
    [SerializeField] protected MonsterGradeParts _monsterGradeParts;
    [SerializeField] protected Image _monsterImage;

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
}