using GameBase;
using UnityEngine;

[ResourcePath("UI/Parts/Parts-QuestCategoryScrollItem")]
public class QuestCategoryScrollItem : ScrollItem
{
    [SerializeField] protected GameObject _grayOutPanel;

    public void ShowGrayOutPanel(bool isShow)
    {
        _grayOutPanel.SetActive(isShow);
    }
}