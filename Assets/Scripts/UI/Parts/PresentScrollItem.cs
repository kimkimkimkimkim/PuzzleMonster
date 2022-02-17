using GameBase;
using TMPro;
using UnityEngine;

[ResourcePath("UI/Parts/Parts-PresentScrollItem")]
public class PresentScrollItem : ScrollItem
{
    [SerializeField] protected TextMeshProUGUI _nameText;
    [SerializeField] protected TextMeshProUGUI _descriptionText;
    [SerializeField] protected GameObject _grayoutPanel;
    [SerializeField] protected IconItem _iconItem;

    public void SetNameText(string name)
    {
        _nameText.text = name;
    }

    public void SetDescriptionText(string description)
    {
        _descriptionText.text = description;
    }

    public void ShowGrayoutPanel(bool isShow)
    {
        _grayoutPanel.SetActive(isShow);
    }

    public void SetIcon(ItemMI item)
    {
        _iconItem.SetIcon(item, true);
    }
}