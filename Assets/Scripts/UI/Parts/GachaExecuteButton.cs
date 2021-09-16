using GameBase;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ResourcePath("UI/Parts/Parts-GachaExecuteButton")]
public class GachaExecuteButton : ScrollItem
{
    [SerializeField] protected TextMeshProUGUI _costText;
    [SerializeField] protected Image _propertyImage;

    public void SetCostText(string cost)
    {
        _costText.text = cost;
    }

    public void SetPropertyImage(long propertyId)
    {

    }
}