using GameBase;
using PM.Enum.Battle;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleBoardPieceItem")]
public class BattleBoardPieceItem : MonoBehaviour
{
    [SerializeField] protected Color _darkBrown;
    [SerializeField] protected Color _lightBrown;
    [SerializeField] protected Image _image;

    public void SetColor(PieceColor pieceColor)
    {
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
}