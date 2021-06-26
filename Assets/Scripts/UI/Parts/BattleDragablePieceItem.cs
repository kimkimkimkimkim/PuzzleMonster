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
    #endregion

    public void OnPointerDown(PointerEventData data)
    {
        // ドラッガブル内のピースを形に応じた位置に移動させるアニメーション
        pieceDataList.ForEach(pieceData =>
        {
            var targetPosition = pieceData.targetPieceItem.transform.localPosition + new Vector3(0, 250, 0);
            var targetPieceSize = pieceData.targetPieceItem.GetRectTransform().sizeDelta;
            var pieceSize = pieceData.pieceItem.GetRectTransform().sizeDelta;
            var scale = targetPieceSize.x / pieceSize.x; // 現状ピースは正方形なのでxの値だけで指定

            var moveAnimation = pieceData.pieceItem.transform.DOLocalMove(targetPosition, ANIMATION_TIME);
            var scaleAnimation = pieceData.pieceItem.transform.DOScale(scale, ANIMATION_TIME);

            var sequence = DOTween.Sequence()
                .Join(moveAnimation)
                .Join(scaleAnimation);

            UIManager.Instance.ShowTapBlocker();
            sequence.OnCompleteAsObservable()
                .Do(_ => UIManager.Instance.TryHideTapBlocker())
                .Subscribe();
        });
    }

    public void OnDrag(PointerEventData data)
    {
        var targetPos = Camera.main.ScreenToWorldPoint(data.position);
        targetPos.z = 0;
        transform.position = targetPos;
    }

    public void OnPointerUp(PointerEventData data) {
        var fitBoardIndexList = GetFitBoardIndexList();
        Debug.Log(string.Join(", ", fitBoardIndexList.Select(i => $"({i.row},{i.column})")));

        // ドラッガブル内のピースの位置元に戻すアニメーション
        pieceDataList.ForEach(pieceData =>
        {
            var moveAnimation = pieceData.pieceItem.transform.DOLocalMove(pieceData.initialPos, ANIMATION_TIME);
            var scaleAnimation = pieceData.pieceItem.transform.DOScale(1, ANIMATION_TIME);

            var sequence = DOTween.Sequence()
                .Join(moveAnimation)
                .Join(scaleAnimation);

            UIManager.Instance.ShowTapBlocker();
            sequence.OnCompleteAsObservable()
                .Do(_ => UIManager.Instance.TryHideTapBlocker())
                .AsUnitObservable();
        });

        // ドラッガブルピースを元の位置に戻すアニメーション
        UIManager.Instance.ShowTapBlocker();
        DOTween.Sequence()
            .Join(transform.DOLocalMove(new Vector3(0, 0, 0), ANIMATION_TIME))
            .OnCompleteAsObservable()
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable()
            .Subscribe();
    }

    private List<BoardIndex> GetFitBoardIndexList()
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
        if (minDistance > MIN_PIECE_SPAN) return new List<BoardIndex>();

        // 左上のピースがはまるボードピースを元に全体のピースが盤面に含まれているかを判定
        var isInclude = IsInclude(nearestBoardPiece.boardIndex);
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
                var nearestBoardPieceIndex = nearestBoardPiece.boardIndex.row * ConstManager.Battle.BOARD_WIDTH + nearestBoardPiece.boardIndex.column;
                return index + nearestBoardPieceIndex;
            }).ToList();
        var fitBoardIndexList = truePieceIndexList.Select(index => GetBoardIndex(index)).ToList();

        var isOverlaped = fitBoardIndexList.Any(i => BattleManager.Instance.board[i.row, i.column].GetPieceStatus() != PieceStatus.Free);
        if (isOverlaped) return new List<BoardIndex>();

        return truePieceIndexList.Select(index => GetBoardIndex(index)).ToList();
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

    public void SetPiece(int boardSpace,float pieceWidth, long pieceId)
    {
        piece = pieceList.First(m => m.id == pieceId);
        var startAxis = piece.isHorizontal ? GridLayoutGroup.Axis.Horizontal : GridLayoutGroup.Axis.Vertical;
        var constraint = piece.horizontalConstraint != 0 ? GridLayoutGroup.Constraint.FixedColumnCount : GridLayoutGroup.Constraint.FixedRowCount;
        var contraintCount = piece.horizontalConstraint != 0 ? piece.horizontalConstraint : piece.verticalConstraint;

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

    private class PieceData
    {
        public BattlePieceItem pieceItem { get; set; }
        public BattlePieceItem targetPieceItem { get; set; }
        public Vector3 initialPos { get; set; }
    }
}