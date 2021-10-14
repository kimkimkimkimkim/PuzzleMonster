using GameBase;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-PartyMonsterIconItem")]
public class PartyMonsterIconItem : IconItem
{
    [SerializeField] protected Text _titleText;
    [SerializeField] protected IconItem _iconItem;

    public IconItem iconItem { get { return _iconItem; } private set { _iconItem = value; } }

    public void SetTitleText(string text)
    {
        _titleText.text = text;
    }

    public void ShowTitleText(bool isShow)
    {
        _titleText.gameObject.SetActive(isShow);
    }

    public void ShowIconItem(bool isShow)
    {
        _iconItem.gameObject.SetActive(isShow);
    }
}