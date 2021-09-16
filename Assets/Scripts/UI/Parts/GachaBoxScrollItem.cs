using GameBase;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[ResourcePath("UI/Parts/Parts-GachaBoxScrollItem")]
public class GachaBoxScrollItem : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI _titleText;
    [SerializeField] public Transform _executeButtonBase;

    public void SetText(string text)
    {
        _titleText.text = text;
    }
}