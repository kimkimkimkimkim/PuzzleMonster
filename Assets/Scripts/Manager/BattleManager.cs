using System;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using UniRx;
using GameBase;
using UnityEngine;

public class BattleManager: SingletonMonoBehaviour<BattleManager>
{
    [HideInInspector] public BattleBoardPieceItem[,] board = new BattleBoardPieceItem[ConstManager.Battle.BOARD_HEIGHT, ConstManager.Battle.BOARD_WIDTH];

    private BattleWindowUIScript battleWindow;
    private int moveCountPerTurn = 0;
    private int turnCount = 0;

    public void BattleStart()
    {
        // 初期化
        moveCountPerTurn = 0;
        turnCount = 0;

        // ゲーム画面に遷移
        FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(_ =>
            {
                battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
                battleWindow.Init();
                HeaderFooterManager.Instance.Show(false);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0))
            .Subscribe();
    }

    /// <summary>
    /// ピースがハマった時の処理
    /// </summary>
    public void OnPieceFit(List<BoardIndex> fitBoardIndexList)
    {
        moveCountPerTurn++;
        fitBoardIndexList.ForEach(i => board[i.row, i.column].SetPieceStatus(PieceStatus.Normal));
        UpdateBoard();

        // 全てのピースをはめ終わったら再生成
        if(moveCountPerTurn == ConstManager.Battle.MAX_PARTY_MEMBER_NUM)
        {
            moveCountPerTurn = 0;
            turnCount++;
            for(var i = 0; i < ConstManager.Battle.MAX_PARTY_MEMBER_NUM;i++)
            {
                var pieceId = UnityEngine.Random.Range(1, 6);
                battleWindow.CreateDragablePiece(i, pieceId);
            }
        }
    }

    /// <summary>
    /// boardを元に盤面を更新
    /// </summary>
    public void UpdateBoard()
    {
        for(var i = 0; i < ConstManager.Battle.BOARD_HEIGHT; i++)
        {
            for(var j = 0; j < ConstManager.Battle.BOARD_WIDTH; j++)
            {
                var piece = board[i, j];
                var color = piece.GetPieceStatus() == PieceStatus.Normal ? PieceColor.LightBrown : PieceColor.DarkBrown;
                piece.SetColor(color);
            }
        }
    }
}

public class BoardIndex
{
    public int row { get; set; }
    public int column { get; set; }
    public BoardIndex(int row, int column)
    {
        this.row = row;
        this.column = column;
    }
}
