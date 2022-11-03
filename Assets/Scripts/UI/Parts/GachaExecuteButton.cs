using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaExecuteButton")]
public class GachaExecuteButton : ScrollItem
{
    [SerializeField] protected Text _costText;
    [SerializeField] protected Image _propertyImage;
    [SerializeField] protected GameObject _grayoutPanel;

    private ItemMI item;

    public void SetCostText(string cost)
    {
        _costText.text = cost;
    }

    public void SetCostIcon(ItemMI item)
    {
        this.item = item;

        PMAddressableAssetUtil.GetIconImageSpriteObservable(item)
            .Where(_ => this.item.itemType == item.itemType && this.item.itemId == item.itemId)
            .Do(sprite => _propertyImage.sprite = sprite)
            .Subscribe();
    }

    public void ShowGrayoutPanel(bool isShow)
    {
        _grayoutPanel.SetActive(isShow);
    }
}