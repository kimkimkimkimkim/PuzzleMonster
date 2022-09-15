using GameBase;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-GachaBoxScrollItem")]
public class GachaBoxScrollItem : MonoBehaviour
{
    [SerializeField] public Text _titleText;
    [SerializeField] public Transform _executeButtonBase;

    public void SetText(string text)
    {
        _titleText.text = text;
    }
}