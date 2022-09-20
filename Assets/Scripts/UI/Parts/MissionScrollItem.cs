using GameBase;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-MissionScrollItem")]
public class MissionScrollItem : ScrollItem
{
    [SerializeField] protected Text _nameText;
    [SerializeField] protected GameObject _grayoutPanel;
    [SerializeField] protected GameObject _clearedPanel;
    [SerializeField] protected IconItem _iconItem;

    public void SetNameText(string name)
    {
        _nameText.text = name;
    }

    public void ShowGrayoutPanel(bool isShow)
    {
        _grayoutPanel.SetActive(isShow);
    }

    public void SetIcon(ItemMI item)
    {
        _iconItem.SetIcon(item, true);
    }

    public void ShowClearedPanel(bool isShow)
    {
        _clearedPanel.SetActive(isShow);
        if (isShow) ShowGrayoutPanel(false);
    }
}