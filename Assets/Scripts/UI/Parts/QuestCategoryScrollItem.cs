using GameBase;
using UnityEngine;

[ResourcePath("UI/Parts/Parts-QuestCategoryScrollItem")]
public class QuestCategoryScrollItem : ScrollItem
{
    [SerializeField] protected GameObject _grayOutPanel;
    [SerializeField] protected GameObject _completeImagePanel;
    [SerializeField] protected GameObject _clearImagePanel;
    [SerializeField] protected GameObject _newImagePanel;

    public void ShowGrayOutPanel(bool isShow)
    {
        _grayOutPanel.SetActive(isShow);
    }

    public void ShowCompleteImage(bool isShow)
    {
        _completeImagePanel.SetActive(isShow);
    }

    public void ShowClearImagePanel(bool isShow)
    {
        _clearImagePanel.SetActive(isShow);
    }

    public void ShowNewImagePanel(bool isShow)
    {
        _newImagePanel.SetActive(isShow);
    }
}