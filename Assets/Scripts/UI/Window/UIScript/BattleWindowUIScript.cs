using System.Collections.Generic;
using GameBase;
using PM.Enum.Battle;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected RectTransform _boardPanelRT;
    [SerializeField] protected GridLayoutGroup _boardGridLayoutGroup;
    [SerializeField] protected List<RectTransform> _dragablePieceBaseRTList;

    private const  int BOARD_PIECE_SPACE = 12;

    private int BOARD_HEIGHT = ConstManager.Battle.BOARD_HEIGHT;
    private int BOARD_WIDTH = ConstManager.Battle.BOARD_WIDTH;
    private float pieceWidth;

    public void Init()
    {
        // ボードのパラメータ設定
        SetBoard();

        for(var i=0; i< BOARD_HEIGHT; i++)
        {
            for(var j = 0; j < BOARD_WIDTH; j++)
            {
                var boardPiece = UIManager.Instance.CreateContent<BattleBoardPieceItem>(_boardPanelRT);
                boardPiece.SetPieceStatus(PieceStatus.Free);
                boardPiece.SetColor(PieceColor.DarkBrown);
                boardPiece.SetBoardIndex(new BoardIndex(i, j));
                BattleManager.Instance.board[i, j] = boardPiece;
            }
        }
    }

    public void CreateDragablePiece(int index,long id)
    {
        var dragablePieceBaseRT = _dragablePieceBaseRTList[index];
        var dragablePiece = UIManager.Instance.CreateContent<BattleDragablePieceItem>(dragablePieceBaseRT);
        dragablePiece.SetPiece(index, BOARD_PIECE_SPACE, pieceWidth, id);
        BattleManager.Instance.dragablePieceList[index] = dragablePiece;
    }

    private void SetBoard()
    {
        var boardWidth = _boardPanelRT.sizeDelta.x;
        pieceWidth = (boardWidth - ((BOARD_WIDTH + 1) * BOARD_PIECE_SPACE)) / BOARD_WIDTH;

        _boardGridLayoutGroup.cellSize = new Vector2(pieceWidth, pieceWidth);
        _boardGridLayoutGroup.spacing = new Vector2(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);
        _boardGridLayoutGroup.padding = new RectOffset(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);
    }
}