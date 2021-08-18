using System;
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

    [HideInInspector] public int index { get; private set; }

    private const float ANIMATION_TIME = 0.25f;
    private const float MIN_PIECE_SPAN = 0.23f;

    private PieceMB piece;
    private List<PieceData> pieceDataList = new List<PieceData>();
    #region list
    private List<PieceMB> pieceList = new List<PieceMB>()
    {
        new PieceMB()
        {
            id = 1,
            name = "正方形1",
            horizontalConstraint = 1,
            pieceList = new List<bool>(){ true },
        },
        new PieceMB()
        {
            id = 2,
            name = "左上三角3",
            horizontalConstraint = 3,
            pieceList = new List<bool>(){ true, true, true, true, false, false, true, false, false },
        },
        new PieceMB()
        {
            id = 3,
            name = "縦線2",
            horizontalConstraint = 1,
            pieceList = new List<bool>(){ true, true },
        },
        new PieceMB()
        {
            id = 4,
            name = "横線5",
            horizontalConstraint = 5,
            pieceList = new List<bool>(){ true, true, true, true, true },
        },
        new PieceMB()
        {
            id = 5,
            name = "右下三角2",
            horizontalConstraint = 2,
            pieceList = new List<bool>(){ false, true, true, true},
        },
    };
    #endregion

    public void OnPointerDown(PointerEventData data)
    {
        // ドラッガブル内のピースを形に応じた位置に移動させるアニメーション
        VisualFxManager.Instance.PlayOnPointerDownAtDragablePieceFxObservable(pieceDataList).Subscribe();
    }

    public void OnDrag(PointerEventData data)
    {
        var targetPos = Camera.main.ScreenToWorldPoint(data.position);
        targetPos.z = 0;
        transform.position = targetPos;
    }

    public void OnPointerUp(PointerEventData data) {
        var fitBoardIndexList = GetFitBoardIndexList(out BoardIndex boardIndex);
        if (fitBoardIndexList.Any())
        {
            // ピースがハマる場合
            VisualFxManager.Instance.PlayOnDragablePieceFitFxObservable(boardIndex, piece, pieceDataList)
                .Do(_ => {
                    BattleManager.Instance.OnPieceFit(index, fitBoardIndexList);
                    Destroy(gameObject);
                })
                .Subscribe();
        }
        else
        {
            // どこにもハマらないなら初期位置に戻す
            VisualFxManager.Instance.PlayDragablePieceMoveInitialPositionFxObservable(transform, pieceDataList).Subscribe();
        }
    }

    /// <summary>
    /// Gets the fit board index list.
    /// </summary>
    public List<BoardIndex> GetFitBoardIndexList(BoardIndex boardIndex)
    {
        // 左上のピースがはまるボードピースを元に全体のピースが盤面に含まれているかを判定
        var isInclude = IsInclude(boardIndex);
        if (!isInclude) return new List<BoardIndex>();

        var truePieceIndexList = new List<int>();
        for (var i = 0; i < piece.pieceList.Count; i++)
        {
            if (piece.pieceList[i]) truePieceIndexList.Add(i);
        }
        truePieceIndexList = truePieceIndexList
            .Select(index => {
                var row = index / piece.horizontalConstraint;
                var column = index % piece.horizontalConstraint;
                return column + (row * ConstManager.Battle.BOARD_WIDTH);
            })
            .Select(index =>
            {
                var nearestBoardPieceIndex = boardIndex.row * ConstManager.Battle.BOARD_WIDTH + boardIndex.column;
                return index + nearestBoardPieceIndex;
            }).ToList();
        var fitBoardIndexList = truePieceIndexList.Select(index => GetBoardIndex(index)).ToList();

        var isOverlaped = fitBoardIndexList.Any(i => BattleManager.Instance.board[i.row, i.column].GetPieceStatus() != PieceStatus.Free);
        if (isOverlaped) return new List<BoardIndex>();

        return truePieceIndexList.Select(index => GetBoardIndex(index)).ToList();
    }

    private List<BoardIndex> GetFitBoardIndexList(out BoardIndex boardIndex)
    {
        // 一番左上のピース
        var leftUpperPiece = pieceDataList.First().pieceItem;
        // 左上のピースに一番近いボードピース
        BattleBoardPieceItem nearestBoardPiece = null;
        var minDistance = float.MaxValue;
        // TODO : おそらく基本的には距離が短くなり続けて最近距離になってからは遠くなり続けるので、その時点で検索を終了してもいいかも
        for (var i = 0; i < ConstManager.Battle.BOARD_HEIGHT; i++)
        {
            for (var j = 0; j < ConstManager.Battle.BOARD_WIDTH; j++)
            {
                var p = BattleManager.Instance.board[i, j];
                var dist = Get2DDistance(leftUpperPiece.transform.position, p.transform.position);
                if (dist > minDistance) continue;

                minDistance = dist;
                nearestBoardPiece = p;
            }
        }

        // 左上のピースと最近距離ボードピースとの距離が範囲外だったらその時点で終了
        if (minDistance > MIN_PIECE_SPAN)
        {
            boardIndex = null;
            return new List<BoardIndex>();
        }

        boardIndex = nearestBoardPiece.boardIndex;
        return GetFitBoardIndexList(nearestBoardPiece.boardIndex);
    }

    private BoardIndex GetBoardIndex(int listIndex)
    {
        var row = listIndex / ConstManager.Battle.BOARD_WIDTH;
        var column = listIndex % ConstManager.Battle.BOARD_WIDTH;
        return new BoardIndex(row, column);
    }

    // ピースが盤面に収まっているかどうかを返す
    private bool IsInclude(BoardIndex baseBoardIndex)
    {
        var width = piece.horizontalConstraint;
        var height = piece.pieceList.Count / width;
        var lowerRightPieceBoardIndex = new BoardIndex(baseBoardIndex.row + height - 1, baseBoardIndex.column + width - 1);
        return lowerRightPieceBoardIndex.row < ConstManager.Battle.BOARD_HEIGHT && lowerRightPieceBoardIndex.column < ConstManager.Battle.BOARD_WIDTH;
    }

    private float Get2DDistance(Vector3 a, Vector3 b)
    {
        var newA = new Vector3(a.x, a.y, 0);
        var newB = new Vector3(b.x, b.y, 0);
        return Vector3.Distance(newA, newB);
    }

    public void SetPiece(int index, int boardSpace,float pieceWidth, long pieceId)
    {
        this.index = index;
        piece = pieceList.First(m => m.id == pieceId);
        var startAxis = GridLayoutGroup.Axis.Horizontal;
        var constraint = piece.horizontalConstraint != 0 ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
        var contraintCount = piece.horizontalConstraint;

        _pieceBaseGridLayoutGroup.startAxis = startAxis;
        _pieceBaseGridLayoutGroup.constraint = constraint;
        _pieceBaseGridLayoutGroup.constraintCount = contraintCount;

        _boardPieceBaseGridLayoutGroup.padding = new RectOffset(boardSpace, boardSpace, boardSpace, boardSpace);
        _boardPieceBaseGridLayoutGroup.spacing = new Vector2(boardSpace, boardSpace);
        _boardPieceBaseGridLayoutGroup.cellSize = new Vector2(pieceWidth, pieceWidth);
        _boardPieceBaseGridLayoutGroup.startAxis = startAxis;
        _boardPieceBaseGridLayoutGroup.constraint = constraint;
        _boardPieceBaseGridLayoutGroup.constraintCount = contraintCount;

        // ピース生成
        piece.pieceList.ForEach(b =>
        {
            var p = UIManager.Instance.CreateContent<BattlePieceItem>(_pieceBaseRT);
            var pieceColor = b ? PieceColor.LightBrown : PieceColor.TransParent;
            p.SetColor(pieceColor);

            var boardP = UIManager.Instance.CreateContent<BattlePieceItem>(_boardPieceBaseRT);
            boardP.SetColor(PieceColor.TransParent);

            var pieceData = new PieceData()
            {
                pieceItem = p,
                targetPieceItem = boardP,
            };
            pieceDataList.Add(pieceData);
        });

        // GridLayoutGroupの関係上1フレーム遅らせて初期位置を指定
        UIManager.Instance.ShowTapBlocker();
        Observable.NextFrame()
            .Do(_ => pieceDataList.ForEach(p => p.initialPos = p.pieceItem.transform.localPosition))
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .Subscribe();
    }
}

public class PieceData
{
    public BattlePieceItem pieceItem { get; set; }
    public BattlePieceItem targetPieceItem { get; set; }
    public Vector3 initialPos { get; set; }
}
