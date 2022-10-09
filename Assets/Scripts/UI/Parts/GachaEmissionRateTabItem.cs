using GameBase;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaEmissionRateTabItem")]
public class GachaEmissionRateTabItem : MonoBehaviour
{
    [SerializeField] protected Transform _focusLineBase;
    [SerializeField] protected Transform _focusLine;
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Toggle _toggle;

    public Toggle toggle { get { return _toggle; } }

    private Color offColor = new Color(0.76f, 0.76f, 0.76f);
    private Color onColor = new Color(0.9647059f, 0.8823529f, 0.6117647f);
    private bool isOn;

    public void Init(string title,ToggleGroup toggleGroup,  bool isOn, bool isSingle)
    {
        _titleText.text = title;
        _toggle.group = toggleGroup;
        this.isOn = isOn;
        _focusLineBase.gameObject.SetActive(!isSingle);
        _focusLine.gameObject.SetActive(isOn);
    }

    public void OnValueChenged(bool isOn)
    {
        this.isOn = isOn;
        _focusLine.gameObject.SetActive(isOn);
        _titleText.color = isOn ? onColor : offColor;
    }
}