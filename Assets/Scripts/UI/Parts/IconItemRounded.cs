using GameBase;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using PM.Enum.Item;
using PM.Enum.UI;
using UniRx;
using System;

[ResourcePath("UI/Parts/Parts-IconItemRounded")]
public class IconItemRounded : MonoBehaviour
{
    [SerializeField] protected Image _frameImage;
    [SerializeField] protected Image _iconImage;
    [SerializeField] protected GameObject _notifyPanel;
    [SerializeField] protected GameObject _focusPanel;
    [SerializeField] protected GameObject _grayoutPanel;
    [SerializeField] protected GameObject _labelPanel;
    [SerializeField] protected TextMeshProUGUI _numText;
    [SerializeField] protected TextMeshProUGUI _grayoutText;
    [SerializeField] protected TextMeshProUGUI _labelText;
    [SerializeField] protected List<Sprite> _frameSpriteList;
    [SerializeField] protected List<Color> _backgroundColorList;
    [SerializeField] protected Toggle _toggle;
    [SerializeField] protected Button _button;
    [SerializeField] protected CanvasGroup _canvasGroup;

    public Toggle toggle { get { return _toggle; } private set { _toggle = value; } }

    private IDisposable onClickButtonObservable;

    public void SetIcon(ItemMI item)
    {
        var iconColorType = ClientItemUtil.GetIconColorType(item);
        var iconImageType = ClientItemUtil.GetIconImageType(item.itemType);
        var numText = item.num <= 1 ? "" : item.num.ToString();

        SetFrameImage(iconColorType);
        SetIconImage(iconImageType, item.itemId);
        SetNumText(numText);
    }

    private void SetFrameImage(IconColorType iconColor)
    {
        var index = (int)iconColor;
        _frameImage.sprite = _frameSpriteList[index];
    }

    private void SetIconImage(IconImageType iconImageType, long itemId)
    {
        PMAddressableAssetUtil.GetIconImageSpriteObservable(iconImageType, itemId)
            .Do(sprite =>
            {
                if (sprite != null) _iconImage.sprite = sprite;
            })
            .Subscribe()
            .AddTo(this);
    }

    private void SetNumText(string text)
    {
        _numText.text = text;
    }

    public void SetToggleGroup(ToggleGroup toggleGroup)
    {
        _toggle.group = toggleGroup;
    }

    public void SetOnClickAction(Action action)
    {
        if (action == null) return;

        if (onClickButtonObservable != null)
        {
            onClickButtonObservable.Dispose();
            onClickButtonObservable = null;
        }

        onClickButtonObservable = _button.OnClickIntentAsObservable()
            .Do(_ => action())
            .Subscribe();
    }

    public void SetRaycastTarget(bool isOn)
    {
        _canvasGroup.blocksRaycasts = isOn;
    }

    public void ShowGrayoutPanel(bool isShow, string text = "")
    {
        _grayoutPanel.SetActive(isShow);
        _grayoutText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        _grayoutText.text = text;
    }

    public void ShowLabel(bool isShow, string text = "")
    {
        _labelPanel.SetActive(isShow);
        _labelText.gameObject.SetActive(!string.IsNullOrWhiteSpace(text));
        _labelText.text = text;
    }
}