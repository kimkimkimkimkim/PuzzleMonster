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
    private IObserver<Unit> battleTurnObserver;
    private IObserver<Unit> pieceMoveObserver;
    private BattleWindowUIScript battleWindow;
    private long questId;
    private QuestMB quest;
    private int moveCountPerTurn;
    private int turnCount;
    private int waveCount;
    private int enemyHp;
    private int playerHp;
    private WinOrLose wol;
    private List<BattleEnemyMonsterInfo> battleEnemyMonsterList;
    private BattlePlayerInfo battlePlayer;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init(long questId){
        this.questId = questId;
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        moveCountPerTurn = 0;
        turnCount = 1;
        turnCountInWave = 1;
        waveCount = 0;
        maxWaveCount = 0;
        playerHp = 100;
        enemyHp = 100;
        wol = WinOrLose.Continue;
        battleEnemyMonsterList = new List<BattleEnemyMonsterInfo>();
        battlePlayer = null;
    }

    /// <summary>
    /// バトルを開始する
    /// </summary>
    public IObservable<BattleResult> BattleStartObservable(long questId)
    {
        // 初期化
        Init(questId);

        return Observable.Create<BattleResult>(battleObserver => {
            this.battleObserver = battleObserver;

            Observable.ReturnUnit()
                .SelectMany(_ => FadeInObservable())
                .Do(_ => Debug.Log("バトル開始"))
                .SelectMany(_ => (
                    // バトルのターン進行開始
                    Observable.Create<Unit>(battleTurnObserver => {
                        this.battleTurnObserver = battleTurnObserver;
                        StartTurnProgress();
                        return Disposable.Empty;
                    })
                ))
                .Do(_ => Debug.Log("バトル終了"))
                .Do(_ => Debug.Log(wol == WinOrLose.Win ? "勝利" : "敗北"))
                .SelectMany(_ => FadeOutObservable())
                .Subscribe();

            return Disposable.Empty;
        });
    }

    /// <summary>
    /// 画面遷移（フェードイン）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeInObservable()
    {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(res =>
            {
                battleWindow = UIManager.Instance.CreateDummyWindow<BattleWindowUIScript>();
                battleWindow.Init();
                HeaderFooterManager.Instance.Show(false);
            })
            .SelectMany(res => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }

    /// <summary>
    /// 画面遷移（フェードアウト）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeOutObservable()
    {
        return CommonDialogFactory.Create(new CommonDialogRequest()
        {
            title = "確認",
            content = "これ以上動かせません",
            commonDialogType = CommonDialogType.YesOnly,
        })
            .SelectMany(res => FadeManager.Instance.PlayFadeAnimationObservable(1))
            .Do(res =>
            {
                Destroy(battleWindow.gameObject);
            })
            .SelectMany(res => FadeManager.Instance.PlayFadeAnimationObservable(0))
            .Do(res =>
            {
                var result = new BattleResult();

                if (battleObserver != null)
                {
                    battleObserver.OnNext(result);
                    battleObserver.OnCompleted();
                    battleObserver = null;
                }
            });
    }

    /// <summary>
    /// バトルのターン進行を開始する
    /// </summary>
    private void StartTurnProgress()
    {
        Observable.ReturnUnit()
            .Do(_ => Debug.Log($"ターン{turnCount}開始"))
            .SelectMany(_ => MoveNextWaveObservable())
            .SelectMany(_ => CreateEnemyObservable())
            .SelectMany(_ => CreateDragablePieceObservable())
            .SelectMany(_ => StartPieceMovingTimeObservable())
            .SelectMany(_ => StartPlayerAttackObservable())
            .SelectMany(_ => StartEnemyAttackObservable())
            .SelectMany(_ => JudgeContinueBattleObservable())
            .Where(isContinue => isContinue)
            .RepeatSafe()
            .Subscribe();
    }
    
    /// <summary>
    /// 次のウェーブに移動する
    /// </summary>
    private IObservable<Unit> MoveNextWaveObservable()
    {
        // 勝敗がついていれば何もしない
        if(wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
        var 
        
        return Observable.ReturnUnit();
    }

    /// <summary>
    /// 敵を生成する
    /// </summary>
    private IObservable<Unit> CreateEnemyObservable()
    {
        // 勝敗がついていれば何もしない
        if(wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
        Debug.Log($"自分のHP:{playerHp}, 敵のHP:{enemyHp}");
        
        return battleWindow.CreateEnemyObservable(questId);
    }

    /// <summary>
    /// ドラッガブルピースを生成する
    /// </summary>
    private IObservable<Unit> CreateDragablePieceObservable()
    {
        // 勝敗がついていれば何もしない
        if(wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
        for (var i = 0; i < ConstManager.Battle.MAX_PARTY_MEMBER_NUM; i++)
        {
            var pieceId = UnityEngine.Random.Range(1, 6);
            battleWindow.CreateDragablePiece(i, pieceId);
        }
        return Observable.ReturnUnit();
    }

    /// <summary>
    /// ピース移動タイムを開始する
    /// </summary>
    private IObservable<Unit> StartPieceMovingTimeObservable()
    {
        // 勝敗がついていれば何もしない
        if(wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
        return Observable.Create<Unit>(pieceMoveObserver =>
        {
            this.pieceMoveObserver = pieceMoveObserver;
            return Disposable.Empty;
        });
    }

    /// <summary>
    /// プレイヤーの攻撃フェイズを開始する
    /// </summary>
    private IObservable<Unit> StartPlayerAttackObservable()
    {
        // 勝敗がついていれば何もしない
        if(wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
        var damage = UnityEngine.Random.Range(1,25);
        enemyHp -= damage;
        JudgeWinOrLoseObservable();
        
        Debug.Log($"プレイヤーの攻撃！　{damage}のダメージ.　敵のHP:{enemyHp}");
        
        return Observable.ReturnUnit();
    }

    /// <summary>
    /// 敵の攻撃フェイズを開始する
    /// </summary>
    private IObservable<Unit> StartEnemyAttackObservable()
    {
        // 勝敗がついていれば何もしない
        if(wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
        var damage = UnityEngine.Random.Range(1,25);
        playerHp -= damage;
        JudgeWinOrLoseObservable();
        
        Debug.Log($"敵の攻撃！　{damage}のダメージ.　プレイヤーのHP:{playerHp}");
        
        return Observable.ReturnUnit();
    }

    /// <summary>
    /// バトルを続行するか（勝敗がついたか）否かを判定する
    /// </summary>
    private IObservable<bool> JudgeContinueBattleObservable()
    {
        if (wol == WinOrLose.Win || wol == WinOrLose.Lose)
        {
            // 勝敗がついたのでバトルを終了
            if (battleTurnObserver != null)
            {
                battleTurnObserver.OnNext(Unit.Default);
                battleTurnObserver.OnCompleted();
                battleTurnObserver = null;
            }
            return Observable.ReturnUnit().DelayFrame(1).Select(_ => false);
        }
        else
        {
            // まだ続行
            return Observable.ReturnUnit().DelayFrame(1).Select(_ => true);
        }
    }

    /// <summary>
    /// 勝敗を判定する
    /// </summary>
    private void JudgeWinOrLoseObservable()
    {
        if(enemyHp <= 0){
            // 相手のHPが0なら勝利
            wol = WinOrLose.Win;
        }else if(playerHp <= 0){
            // 自分のHPが0なら敗北
            wol = WinOrLose.Lose;
        }else if(!IsRemainPieceCanFit()){
            // ピースを置く場所が無ければ敗北
            wol = WinOrLose.Lose;
        }else{
            // それ以外なら続行
            wol = WinOrLose.Continue;
        }
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
        
        JudgeWinOrLoseObservable();
        
        // 勝敗がついていれば次の処理へ
        if(wol != WinOrLose.Continue) {
            pieceMoveObserver.OnNext(Unit.Default);
            pieceMoveObserver.OnCompleted();
            pieceMoveObserver = null;
            return;
        }
        

        // 全てのピースをはめ終わったら次の処理へ
        if (moveCountPerTurn == ConstManager.Battle.MAX_PARTY_MEMBER_NUM)
        {
            moveCountPerTurn = 0;
            turnCount++;
            pieceMoveObserver.OnNext(Unit.Default);
            pieceMoveObserver.OnCompleted();
            pieceMoveObserver = null;
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

        // ピースを全てはめ終わっていればtrueを返す
        if (dragablePieceList.All(p => p == null)) return true;

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
