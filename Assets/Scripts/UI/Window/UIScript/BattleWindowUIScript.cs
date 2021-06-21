using System.Collections.Generic;
using GameBase;
using PM.Enum.Battle;
using UnityEngine;
using UnityEngine.UI;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    [SerializeField] protected RectTransform _boardPanelRT;
    [SerializeField] protected RectTransform _dragablePieceBaseRT;
    [SerializeField] protected GridLayoutGroup _boardGridLayoutGroup;

    const int ROW_NUM = 8;
    const int BOARD_PIECE_SPACE = 12;

    private float pieceWidth;


    public void Init()
    {
        // ボードのパラメータ設定
        SetBoard();

        for(var i = 0; i < ROW_NUM * ROW_NUM; i++)
        {
            var boardPiece = UIManager.Instance.CreateContent<BattleBoardPieceItem>(_boardPanelRT);
            boardPiece.SetColor(PieceColor.DarkBrown);
        }

        var dragablePiece = UIManager.Instance.CreateContent<BattleDragablePieceItem>(_dragablePieceBaseRT);
        dragablePiece.SetPiece(BOARD_PIECE_SPACE, pieceWidth, 2);
    }

    private void SetBoard()
    {
        var boardWidth = _boardPanelRT.sizeDelta.x;
        pieceWidth = (boardWidth - ((ROW_NUM + 1) * BOARD_PIECE_SPACE)) / ROW_NUM;

        _boardGridLayoutGroup.cellSize = new Vector2(pieceWidth, pieceWidth);
        _boardGridLayoutGroup.spacing = new Vector2(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);
        _boardGridLayoutGroup.padding = new RectOffset(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);
    }
}
