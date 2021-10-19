using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleMonsterItem")]
public class BattleMonsterItem : MonoBehaviour
{
    [SerializeField] protected Image _monsterImage;

    public void SetMonsterImage(long monsterId)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .Do(sprite =>
            {
                if (sprite != null) _monsterImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);
    }
}