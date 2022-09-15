using GameBase;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaExecuteButton")]
public class GachaExecuteButton : ScrollItem
{
    [SerializeField] protected Text _costText;
    [SerializeField] protected Image _propertyImage;
    [SerializeField] protected GameObject _grayoutPanel;

    public void SetCostText(string cost)
    {
        _costText.text = cost;
    }

    public void SetPropertyImage(long propertyId)
    {

    }

    public void ShowGrayoutPanel(bool isShow)
    {
        _grayoutPanel.SetActive(isShow);
    }
}