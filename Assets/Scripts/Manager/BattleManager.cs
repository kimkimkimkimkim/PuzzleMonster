using System;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using UniRx;
using GameBase;
using UnityEngine;
using PM.Enum.UI;

public class BattleManager: SingletonMonoBehaviour<BattleManager>
{
    [HideInInspector] public BattleBoardPieceItem[,] board = new BattleBoardPieceItem[ConstManager.Battle.BOARD_HEIGHT, ConstManager.Battle.BOARD_WIDTH];
    [HideInInspector] public BattleDragablePieceItem[] dragablePieceList = new BattleDragablePieceItem[ConstManager.Battle.MAX_PARTY_MEMBER_NUM];

    private IObserver<BattleResult> battleObserver;
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
                CreateDragablePiece();
                HeaderFooterManager.Instance.Show(false);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0))
            .Subscribe();
    }

    /// <summary>
    /// バトルを開始する
    /// </summary>
    public IObservable<BattleResult> BattleStartObservable()
    {
        // 初期化
        moveCountPerTurn = 0;
        turnCount = 0;

        return Observable.Create<BattleResult>(observer => {
            battleObserver = observer;

            // ゲーム画面に遷移
            FadeManager.Instance.PlayFadeAnimationObservable(1)
                .Do(_ =>
                {
                    battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
                    battleWindow.Init();
                    CreateDragablePiece();
                    HeaderFooterManager.Instance.Show(false);
                })
                .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0))
                .Subscribe();

            return Disposable.Empty;
        });
    }

    /// <summary>
    /// バトルを終了する
    /// </summary>
    public void EndBattle(BattleResult result)
    {
        if (battleObserver == null) return;

        // 残りのピースをはめることができなければこの時点で終了
        CommonDialogFactory.Create(new CommonDialogRequest()
        {
            title = "確認",
            content = "これ以上動かせません",
            commonDialogType = CommonDialogType.YesOnly,
        })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(1))
            .Do(_ =>
            {
                Destroy(battleWindow.gameObject);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0))
            .Do(_ =>
            {
                battleObserver.OnNext(result);
                battleObserver.OnCompleted();
            })
            .Subscribe();

        battleObserver = null;
    }

    /// <summary>
    /// ピースがハマった時の処理
    /// </summary>
    public void OnPieceFit(int dragablePieceIndex,List<BoardIndex> fitBoardIndexList)
    {
        moveCountPerTurn++;
        dragablePieceList[dragablePieceIndex] = null;

        Fit(fitBoardIndexList);
        Crash();

        // 全てのピースをはめ終わったら再生成
        if (moveCountPerTurn == ConstManager.Battle.MAX_PARTY_MEMBER_NUM)
        {
            moveCountPerTurn = 0;
            turnCount++;
            CreateDragablePiece();
        }

        if (!IsRemainPieceCanFit())
        {
            // 残りのピースをはめることができなければこの時点で終了
            var result = new BattleResult() { isWin = false };
            EndBattle(result);
            return;
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

    private void CreateDragablePiece()
    {
        for (var i = 0; i < ConstManager.Battle.MAX_PARTY_MEMBER_NUM; i++)
        {
            var pieceId = UnityEngine.Random.Range(1, 6);
            battleWindow.CreateDragablePiece(i, pieceId);
        }
    }

    /// <summary>
    /// ピースをはめる
    /// </summary>
    private void Fit(List<BoardIndex> fitBoardIndexList)
    {
        fitBoardIndexList.ForEach(i => board[i.row, i.column].SetPieceStatus(PieceStatus.Normal));
        UpdateBoard();
    }

    /// <summary>
    /// 揃った列を壊す
    /// </summary>
    private void Crash()
    {
        var crashRowIndexList = new List<int>();
        var crashColumnIndexList = new List<int>();

        // 壊す行、列のインデックスを取得
        for(var i = 0; i < ConstManager.Battle.BOARD_HEIGHT; i++)
        {
            for(var j = 0; j < ConstManager.Battle.BOARD_WIDTH; j++)
            {
                var piece = board[i,j];
                if (piece.GetPieceStatus() != PieceStatus.Normal) break;
                if (j == ConstManager.Battle.BOARD_WIDTH - 1) crashRowIndexList.Add(i);
            }
        }
        for (var i = 0; i < ConstManager.Battle.BOARD_WIDTH; i++)
        {
            for (var j = 0; j < ConstManager.Battle.BOARD_HEIGHT; j++)
            {
                var piece = board[j, i];
                if (piece.GetPieceStatus() != PieceStatus.Normal) break;
                if (j == ConstManager.Battle.BOARD_HEIGHT - 1) crashColumnIndexList.Add(i);
            }
        }

        // リストを元にピースを壊す
        for (var i = 0; i < ConstManager.Battle.BOARD_HEIGHT; i++)
        {
            for (var j = 0; j < ConstManager.Battle.BOARD_WIDTH; j++)
            {
                if (crashRowIndexList.Contains(i) || crashColumnIndexList.Contains(j)) board[i, j].SetPieceStatus(PieceStatus.Free);
            }
        }

        // UIを更新
        UpdateBoard();
    }

    /// <summary>
    /// 残りのピースをはめることができるか否かを返す
    /// </summary>
    private bool IsRemainPieceCanFit()
    {
        var boardIndexList = new List<BoardIndex>();
        for(var i = 0; i < ConstManager.Battle.BOARD_HEIGHT; i++)
        {
            for(var j = 0; j < ConstManager.Battle.BOARD_WIDTH; j++)
            {
                boardIndexList.Add(new BoardIndex(i,j));
            }

        }
        return dragablePieceList.Any(piece =>
        {
            if (piece == null) return false;

            // 全盤面を順に見ていき１つでもはめられればOK
            return boardIndexList.Any(index =>
            {
                if (board[index.row, index.column].GetPieceStatus() != PieceStatus.Free) return false;
                return piece.GetFitBoardIndexList(index).Any();
            });
        });
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
