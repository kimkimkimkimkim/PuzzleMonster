using GameBase;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using UniRx;

[ResourcePath("UI/Parts/Parts-IconItem")]
public class IconItem : MonoBehaviour
{
    [SerializeField] protected Image _backgroundImage;
    [SerializeField] protected Image _frameImage;
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected GameObject _notifyPanel;
    [SerializeField] protected GameObject _focusPanel;
    [SerializeField] protected TextMeshProUGUI _numText;
    [SerializeField] protected List<Sprite> _frameSpriteList;
    [SerializeField] protected List<Color> _backgroundColorList;

    public void SetIcon(ItemMI item)
    {
        var iconColorType = ItemUtil.GetIconColorType(item);
        var iconImageType = ItemUtil.GetIconImageType(item.itemType);
        var numText = item.num <= 1 ? "" : item.num.ToString();

        SetFrameImage(iconColorType);
        SetIconImage(iconImageType, item.itemId);
        SetNumText(numText);
    }

    public void SetFrameImage(IconColorType iconColor)
    {
        var index = (int)iconColor;
        _frameImage.sprite = _frameSpriteList[index];
        _backgroundImage.color = _backgroundColorList[index];
    }

    public void SetIconImage(IconImageType iconImageType, long itemId)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(iconImageType, itemId)
            .Do(sprite =>
            {
                if(sprite != null)_iconImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);
    }

    public void SetNumText(string text)
    {
        _numText.text = text;
    }

    public void ShowFocusImage(bool isShow)
    {
        _focusPanel.SetActive(isShow);
    }
}