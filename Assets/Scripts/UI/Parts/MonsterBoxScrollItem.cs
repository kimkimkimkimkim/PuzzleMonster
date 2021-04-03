using System.Collections.Generic;
using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-MonsterBoxScrollItem")]
public class MonsterBoxScrollItem : ScrollItem
{
    [SerializeField] protected List<GameObject> _gradeStarOnImageBaseList;
    [SerializeField] protected Image _monsterImage;

    public void SetGradeImage(int grade) {
        if (grade < 0 || _gradeStarOnImageBaseList.Count < grade) return;
        _gradeStarOnImageBaseList.ForEach((b, index) => {
            b.SetActive(index <= grade - 1);
        });
    }

    public void SetMonsterImage(long monsterId)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .Where(res => res != null)
            .Do(res => _monsterImage.sprite = res)
            .Subscribe();
    }
}