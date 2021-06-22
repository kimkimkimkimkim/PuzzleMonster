using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GameBase;
using PM.Enum.Battle;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ResourcePath("UI/Parts/Parts-BattleDragablePieceItem")]
public class BattleDragablePieceItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] protected RectTransform _pieceBaseRT;
    [SerializeField] protected RectTransform _boardPieceBaseRT;
    [SerializeField] protected GridLayoutGroup _pieceBaseGridLayoutGroup;
    [SerializeField] protected GridLayoutGroup _boardPieceBaseGridLayoutGroup;

    private List<BattleBoardPieceItem> pieceItemList = new List<BattleBoardPieceItem>();
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
        pieceItemList.ForEach(piece => piece.PlayMoveToTargetPosAnimationObservable().Subscribe());
    }

    public void OnDrag(PointerEventData data)
    {
        var targetPos = Camera.main.ScreenToWorldPoint(data.position);
        targetPos.z = 0;
        transform.position = targetPos;
    }

    public void OnPointerUp(PointerEventData data) {
        pieceItemList.ForEach(piece => piece.PlayMoveToInitialPosAnimationObservable().Subscribe());

        var moveAnimation = transform.DOLocalMove(new Vector3(0,0,0), 0.2f);

        var sequence = DOTween.Sequence()
            .Join(moveAnimation);

        UIManager.Instance.ShowTapBlocker();
        sequence.OnCompleteAsObservable()
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable()
            .Subscribe();
    }

    public void SetPiece(int boardSpace,float pieceWidth, long pieceId)
    {
        var piece = pieceList.First(m => m.id == pieceId);
        var startAxis = piece.isHorizontal ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;
        var constraint = piece.horizontalConstraint != 0 ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
        var contraintCount = piece.horizontalConstraint != 0 ? piece.horizontalConstraint : piece.verticalConstraint;

        _pieceBaseGridLayoutGroup.startAxis = startAxis;
        _pieceBaseGridLayoutGroup.constraint = constraint;
        _pieceBaseGridLayoutGroup.constraintCount = contraintCount;

        _boardPieceBaseGridLayoutGroup.padding = new RectOffset(boardSpace, boardSpace, boardSpace, boardSpace);
        _boardPieceBaseGridLayoutGroup.cellSize = new Vector2(pieceWidth, pieceWidth);
        _boardPieceBaseGridLayoutGroup.startAxis = startAxis;
        _boardPieceBaseGridLayoutGroup.constraint = constraint;
        _boardPieceBaseGridLayoutGroup.constraintCount = contraintCount;

        piece.pieceList.ForEach(b =>
        {
            var p = UIManager.Instance.CreateContent<BattleBoardPieceItem>(_pieceBaseRT);
            var pieceColor = b ? PieceColor.LightBrown : PieceColor.TransParent;
            p.SetColor(pieceColor);

            var boardP = UIManager.Instance.CreateContent<BattleBoardPieceItem>(_boardPieceBaseRT);
            boardP.SetColor(PieceColor.TransParent);

            p.SetTargetPosPiece(boardP);
            pieceItemList.Add(p);
        });

        UIManager.Instance.ShowTapBlocker();
        Observable.NextFrame()
            .Do(_ => pieceItemList.ForEach(p => p.SetInitialPos()))
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .Subscribe();
    }
}