using GameBase;
using PM.Enum.Battle;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattlePieceItem")]
public class BattlePieceItem : MonoBehaviour
{
    [SerializeField] protected RectTransform _rectTransform;
    [SerializeField] protected Color _darkBrown;
    [SerializeField] protected Color _lightBrown;
    [SerializeField] protected Image _image;

    private PieceColor pieceColor;

    public PieceColor GetPieceColor()
    {
        return pieceColor;
    }

    public void SetColor(PieceColor pieceColor)
    {
        this.pieceColor = pieceColor;
        var color = new Color();
        switch (pieceColor)
        {
            case PieceColor.LightBrown:
                color = _lightBrown;
                break;
            case PieceColor.DarkBrown:
                color = _darkBrown;
                break;
            case PieceColor.TransParent:
            default:
                color = new Color(0, 0, 0, 0);
                break;
        }

        _image.color = color;
    }

    public RectTransform GetRectTransform()
    {
        return _rectTransform;
    }
}