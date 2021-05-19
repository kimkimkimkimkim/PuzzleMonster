using GameBase;
using PM.Enum.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-HomeMonsterItem")]
public class HomeMonsterItem : MonoBehaviour
{
    [SerializeField] protected Image _monsterImage;

    public void SetMonsterImage(long monsterId)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(IconImageType.Monster, monsterId)
            .Where(res => res != null)
            .Do(res => _monsterImage.sprite = res)
            .Subscribe();
    }
}