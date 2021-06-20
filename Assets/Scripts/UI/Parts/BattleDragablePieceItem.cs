using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Battle;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleDragablePieceItem")]
public class BattleDragablePieceItem : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] protected RectTransform _pieceBaseRT;
    [SerializeField] protected RectTransform _boardPieceBaseRT;
    [SerializeField] protected GridLayoutGroup _pieceBaseGridLayoutGroup;
    [SerializeField] protected GridLayoutGroup _boardPieceBaseGridLayoutGroup;

    private List<PieceMB> pieceList = new List<PieceMB>()
    {
        new PieceMB()
        {
            id = 1,
            name = "正方形1",
            isHorizontal = true,
            horizontalConstraint = 1,
            verticalConstraint = 0,
            pieceList = new List<bool>(){ true },
        },
        new PieceMB()
        {
            id = 2,
            name = "左上三角3",
            isHorizontal = true,
            horizontalConstraint = 3,
            verticalConstraint = 0,
            pieceList = new List<bool>(){ true, true, true, true, false, false, true, false, false },
        },
        new PieceMB()
        {
            id = 3,
            name = "縦線2",
            isHorizontal = false,
            horizontalConstraint = 1,
            verticalConstraint = 0,
            pieceList = new List<bool>(){ true, true },
        },
        new PieceMB()
        {
            id = 4,
            name = "横線5",
            isHorizontal = true,
            horizontalConstraint = 5,
            verticalConstraint = 0,
            pieceList = new List<bool>(){ true, true, true, true, true },
        },
        new PieceMB()
        {
            id = 5,
            name = "右下三角2",
            isHorizontal = true,
            horizontalConstraint = 2,
            verticalConstraint = 0,
            pieceList = new List<bool>(){ false, true, true, true},
        },
    };

    public void OnPointerDown(PointerEventData data)
    {
        Debug.Log("touch");
    }

    public void OnDrag(PointerEventData data)
    {
        var targetPos = Camera.main.ScreenToWorldPoint(data.position);
        targetPos.z = 0;
        transform.position = targetPos;
    }

    public void SetPiece(float boardSpace,float pieceWidth, long pieceId)
    {
        var piece = pieceList.First(m => m.id == pieceId);
        var contraintCount = piece.horizontalConstraint != 0 ? piece.horizontalConstraint : piece.verticalConstraint;

        _pieceBaseGridLayoutGroup.startAxis = piece.isHorizontal ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;
        _pieceBaseGridLayoutGroup.constraint = piece.horizontalConstraint != 0 ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
        _pieceBaseGridLayoutGroup.constraintCount = contraintCount;

        piece.pieceList.ForEach(b =>
        {
            var p = UIManager.Instance.CreateContent<BattleBoardPieceItem>(_pieceBaseRT);
            var pieceColor = b ? PieceColor.LightBrown : PieceColor.TransParent;
            p.SetColor(pieceColor);
        });
    }
}