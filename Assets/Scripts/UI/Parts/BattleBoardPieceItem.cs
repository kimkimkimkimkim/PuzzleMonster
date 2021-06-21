using System;
using GameBase;
using PM.Enum.Battle;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[ResourcePath("UI/Parts/Parts-BattleBoardPieceItem")]
public class BattleBoardPieceItem : MonoBehaviour
{
    [SerializeField] protected Color _darkBrown;
    [SerializeField] protected Color _lightBrown;
    [SerializeField] protected Image _image;

    private const float ANIMATION_TIME = 0.5f;

    private BattleBoardPieceItem _targetPosPiece;
    private Vector3 _initialPos;

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

    public void SetInitialPos()
    {
        _initialPos = transform.position;
    }

    public void SetTargetPosPiece(BattleBoardPieceItem piece)
    {
        _targetPosPiece = piece;
    }

    public IObservable<Unit> PlayMoveToTargetPosAnimationObservable()
    {
        var moveAnimation = transform.DOMove(_targetPosPiece.transform.position, ANIMATION_TIME);

        var sequence = DOTween.Sequence()
            .Join(moveAnimation);
        return sequence.PlayAsObservable().AsUnitObservable();
    }

    public IObservable<Unit> PlayMoveToInitialPosAnimationObservable()
    {
        var moveAnimation = transform.DOMove(_initialPos, ANIMATION_TIME);

        var sequence = DOTween.Sequence()
            .Join(moveAnimation);
        return sequence.PlayAsObservable().AsUnitObservable();
    }
}