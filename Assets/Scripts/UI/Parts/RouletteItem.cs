using GameBase;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-RouletteItem")]
public class RouletteItem : MonoBehaviour
{
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected Text _numText;
    [SerializeField] protected GameObject _focusBase;

    public IObservable<Unit> SetUIObservable(ItemMI item)
    {
        var iconImageType = ClientItemUtil.GetIconImageType(item.itemType);
        return PMAddressableAssetUtil.GetIconImageSpriteObservable(iconImageType, item.itemId)
            .Do(sprite =>
            {
                _iconImage.sprite = sprite;
                _numText.text = item.num.ToString();
            })
            .AsUnitObservable();
    }

    public void Focus(bool isFocus)
    {
        _focusBase.SetActive(isFocus);
    }
}