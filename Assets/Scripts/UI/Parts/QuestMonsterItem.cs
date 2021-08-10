using System;
using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-QuestMonsterItem")]
public class QuestMonsterItem : MonoBehaviour
{
    [SerializeField] protected Image _monsterImage;
    [SerializeField] protected CanvasGroup _canvasGroup;

    public CanvasGroup GetCanvasGroup()
    {
        return _canvasGroup;
    }

    public IObservable<Unit> SetMonsterImageObservable(long monsterId)
    {
        return PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .Do(res => {
                if(res != null) _monsterImage.sprite = res;
            })
            .AsUnitObservable();
    }
}