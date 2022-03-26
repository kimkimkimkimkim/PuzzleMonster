using GameBase;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-CommonReceiveScrollItem")]
public class CommonReceiveScrollItem : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI _nameText;
    [SerializeField] protected TextMeshProUGUI _numText;
    [SerializeField] protected IconItem _iconItem;

    public IconItem iconItem { get { return _iconItem; } }

    public void SetNameText(string name)
    {
        _nameText.text = name;
    }

    public void SetIcon(ItemMI item)
    {
        _iconItem.SetIcon(item, true);

        // �����ł͌��e�L�X�g�͏�ɔ�\��
        _iconItem.SetNumText("");
    }

    public void SetNumText(int num)
    {
        _numText.text = $"�~ {num}";
    }
}