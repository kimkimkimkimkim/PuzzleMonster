using System;
using System.Collections.Generic;
using System.Linq;
using PM.Enum.Battle;
using UniRx;
using GameBase;
using UnityEngine;
using PM.Enum.UI;
using PM.Enum.Monster;

public class BattlePuzzleManager: SingletonMonoBehaviour<BattlePuzzleManager>
{
    [HideInInspector] public BattleBoardPieceItem[,] board = new BattleBoardPieceItem[ConstManager.Battle.BOARD_HEIGHT, ConstManager.Battle.BOARD_WIDTH];
    [HideInInspector] public BattleDragablePieceItem[] dragablePieceList = new BattleDragablePieceItem[ConstManager.Battle.MAX_PARTY_MEMBER_NUM];

    private IObserver<BattleResult> battleObserver;
    private IObserver<Unit> battleTurnObserver;
    private IObserver<Unit> pieceMoveObserver;
    private BattlePuzzleWindowUIScript battleWindow;
    private long questId;
    private QuestMB quest;
    private int currentMoveCountPerTurn;
    private int currentTurnCount;
    private int currentTurnCountInWave;
    private int currentWaveCount;
    private int maxWaveCount;
    private BattleResult battleResult;
    private List<QuestMonsterItem> enemyQuestMonsterItemList;
    private List<QuestMonsterItem> playerQuestMonsterItemList;
    private List<BattleEnemyMonsterInfo> battleEnemyMonsterList;
    private List<BattlePlayerMonsterInfo> battlePlayerMonsterList;
    private BattlePlayerInfo battlePlayer;
    
    /// <summary>
    /// 初期化処理
    /// </summary>
    private void Init(long questId, long userMonsterPartyId){
        this.questId = questId;
        quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        currentMoveCountPerTurn = 0;
        currentTurnCount = 0;
        currentTurnCountInWave = 0;
        currentWaveCount = 0;
        maxWaveCount = quest.questWaveIdList.Count;
        battleResult = new BattleResult() { wol = WinOrLose.Continue };
        battleEnemyMonsterList = new List<BattleEnemyMonsterInfo>();
        battlePlayerMonsterList = new List<BattlePlayerMonsterInfo>(){
            new BattlePlayerMonsterInfo(){ monsterId = 1, hp = 15, attack = 3 },
            new BattlePlayerMonsterInfo(){ monsterId = 2, hp = 15, attack = 4 },
            new BattlePlayerMonsterInfo(){ monsterId = 3, hp = 15, attack = 1 },
            new BattlePlayerMonsterInfo(){ monsterId = 4, hp = 15, attack = 3 },
            new BattlePlayerMonsterInfo(){ monsterId = 5, hp = 15, attack = 5 },
            new BattlePlayerMonsterInfo(){ monsterId = 6, hp = 15, attack = 7 },
        };
        var playerHp = battlePlayerMonsterList.Sum(m => m.hp);
        battlePlayer = new BattlePlayerInfo() { currentHp = playerHp};
    }

    /// <summary>
    /// バトルを開始する
    /// </summary>
    public IObservable<BattleResult> BattleStartObservable(long questId, long userMonsterPartyId)
    {
        // 初期化
        Init(questId, userMonsterPartyId);

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
                .Do(_ => Debug.Log(battleResult.wol))
                .SelectMany(_ => ShowResultDialogObservable())
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
            .SelectMany(res =>
            {
                battleWindow = UIManager.Instance.CreateDummyWindow<BattlePuzzleWindowUIScript>();
                battleWindow.Init();
                HeaderFooterManager.Instance.Show(false);
                return battleWindow.CreatePlayerMonsterObservable(battlePlayerMonsterList)
                    .Do(list => playerQuestMonsterItemList = list);
            })
            .SelectMany(_ => VisualFxManager.Instance.PlayQuestTitleFxObservable(quest.name))
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0));
    }
    
    /// <summary>
    /// バトル結果ダイアログを表示する
    /// </summary>
    private IObservable<Unit> ShowResultDialogObservable()
    {
        return VisualFxManager.Instance.PlayWinBattleFxObservable(battleWindow._windowFrameRT);
    }

    /// <summary>
    /// 画面遷移（フェードアウト）時処理を実行
    /// </summary>
    private IObservable<Unit> FadeOutObservable()
    {
        return FadeManager.Instance.PlayFadeAnimationObservable(1)
            .Do(_ =>
            {
                Destroy(battleWindow.gameObject);
                HeaderFooterManager.Instance.Show(true);
            })
            .SelectMany(_ => FadeManager.Instance.PlayFadeAnimationObservable(0))
            .Do(res =>
            {
                if (battleObserver != null)
                {
                    battleObserver.OnNext(battleResult);
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
            .Do(_ => Debug.Log($"ターン{currentTurnCount}開始"))
            .SelectMany(_ => CountUpTurnObservable())
            .SelectMany(isMoveNextWave => MoveNextWaveObservable(isMoveNextWave))
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
    /// 各種ターンの計算を行う
    /// </summary>
    private IObservable<bool> CountUpTurnObservable()
    {
        // 勝敗がついていれば何もしない
        if (battleResult.wol != WinOrLose.Continue) return Observable.Return(false);

        var isNoEnemy = battleEnemyMonsterList.All(m => m.currentHp <= 0);
        var isMaxWave = currentWaveCount == maxWaveCount;
        var isMoveNextWave = isNoEnemy && !isMaxWave;

        // 敵が全滅かつ最終Waveではないなら次のWaveに移動
        if (isMoveNextWave)
        {
            currentWaveCount++;
            currentTurnCountInWave = 0;
        }

        currentTurnCount++;
        currentTurnCountInWave++;

        return Observable.Return(isMoveNextWave);
    }

    /// <summary>
    /// 次のウェーブに移動する
    /// </summary>
    private IObservable<Unit> MoveNextWaveObservable(bool isMoveNextWave)
    {
        // 勝敗がついていれば何もしない
        if(battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        if(isMoveNextWave)
        {
            return VisualFxManager.Instance.PlayWaveTitleFxObservable(battleWindow._windowFrameRT, currentWaveCount, maxWaveCount);
        }

        return Observable.ReturnUnit();
    }

    /// <summary>
    /// 敵を生成する
    /// </summary>
    private IObservable<Unit> CreateEnemyObservable()
    {
        // 勝敗がついていれば何もしない
        if(battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        // Waveの最初のターンでなければ何もしない
        if (currentTurnCountInWave != 1) return Observable.ReturnUnit();

        var questWaveId = quest.questWaveIdList[currentWaveCount - 1];
        var questWave = MasterRecord.GetMasterOf<QuestWaveMB>().Get(questWaveId);
        var questMonsterIdList = new List<long>() { questWave.questMonsterId1 };
        var questMonsterList = questMonsterIdList.Select(id => MasterRecord.GetMasterOf<QuestMonsterMB>().Get(id)).ToList();
        battleEnemyMonsterList = questMonsterList.Select(m => new BattleEnemyMonsterInfo()
        {
            currentHp = 10,
        }).ToList();
        
        return battleWindow.CreateEnemyObservable(questId, currentWaveCount)
            .Do(list => enemyQuestMonsterItemList = list)
            .AsUnitObservable();
    }

    /// <summary>
    /// ドラッガブルピースを生成する
    /// </summary>
    private IObservable<Unit> CreateDragablePieceObservable()
    {
        // 勝敗がついていれば何もしない
        if(battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
        var observableList = new List<IObservable<Unit>>();
        for (var i = 0; i < ConstManager.Battle.MAX_PARTY_MEMBER_NUM; i++)
        {
            var pieceId = UnityEngine.Random.Range(1, 6);
            observableList.Add(battleWindow.CreateDragablePieceAndPlayAnimationObservable(i, pieceId));
        }
        
        return Observable.WhenAll(observableList).AsUnitObservable();
    }

    /// <summary>
    /// ピース移動タイムを開始する
    /// </summary>
    private IObservable<Unit> StartPieceMovingTimeObservable()
    {
        // 勝敗がついていれば何もしない
        if(battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();
        
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
        if(battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        var playerMonsterIndex = UnityEngine.Random.Range(0, ConstManager.Battle.MAX_PARTY_MEMBER_NUM);
        var damage = battlePlayerMonsterList.Sum(m => m.attack); // TODO
        var enemyMonsterIndex = battleEnemyMonsterList.FindIndex(m => m.currentHp > 0);
        var enemyMonsterItem = enemyQuestMonsterItemList[enemyMonsterIndex];
        var enemyMonster = battleEnemyMonsterList[enemyMonsterIndex];

        enemyMonster.currentHp -= damage;

        JudgeWinOrLose();

        var fromPosition = playerQuestMonsterItemList[playerMonsterIndex].transform.position;
        var toPosition = enemyQuestMonsterItemList[enemyMonsterIndex].transform.position;

        var attribute = (MonsterAttribute)UnityEngine.Random.Range(1,6); // TODO
        var attackId = UnityEngine.Random.Range(1, 16); // TODO
        return VisualFxManager.Instance.PlayPlayerAttackFxObservable(battleWindow.fxParentTransform, fromPosition, toPosition, attribute, attackId)
            .SelectMany(_ =>
            {
                if (enemyMonster.currentHp <= 0)
                {
                    return VisualFxManager.Instance.PlayDefeatMonsterFxObservable(enemyMonsterItem);
                }
                else
                {
                    return Observable.ReturnUnit();
                }
            });
    }

    /// <summary>
    /// 敵の攻撃フェイズを開始する
    /// </summary>
    private IObservable<Unit> StartEnemyAttackObservable()
    {
        // 勝敗がついていれば何もしない
        if(battleResult.wol != WinOrLose.Continue) return Observable.ReturnUnit();

        var enemyIndex = battleEnemyMonsterList.FindIndex(m => m.currentHp > 0);
        var damage = battleEnemyMonsterList.Where(m => m.currentHp > 0).Sum(m => 5);
        battlePlayer.currentHp -= damage;

        JudgeWinOrLose();
        
        if(damage <= 0 || enemyIndex < 0) return Observable.ReturnUnit();

        var item = enemyQuestMonsterItemList[enemyIndex];
        return VisualFxManager.Instance.PlayEnemyAttackFxObservable(battleWindow.fxParentTransform, item, battleWindow.backgroundImageTransform);
    }

    /// <summary>
    /// バトルを続行するか（勝敗がついたか）否かを判定する
    /// </summary>
    private IObservable<bool> JudgeContinueBattleObservable()
    {
        if (battleResult.wol == WinOrLose.Win || battleResult.wol == WinOrLose.Lose)
        {
            // 勝敗がついたのでバトルを終了
            if (battleTurnObserver != null)
            {
                Observable.NextFrame()
                    .Do(_ => {
                        battleTurnObserver.OnNext(Unit.Default);
                        battleTurnObserver.OnCompleted();
                        battleTurnObserver = null;
                    })
                    .Subscribe();
            }
            return Observable.NextFrame().Select(_ => false);
        }
        else
        {
            // まだ続行
            return Observable.NextFrame().Select(_ => true);
        }
    }

    /// <summary>
    /// 勝敗を判定する
    /// </summary>
    private void JudgeWinOrLose()
    {
        Debug.Log($"自分のHP：{battlePlayer.currentHp}, 相手のHP：{string.Join(",", battleEnemyMonsterList.Select(m => m.currentHp.ToString()))}");
        if(battleEnemyMonsterList.All(m => m.currentHp <= 0) && currentWaveCount == maxWaveCount){
            // 相手のHPが0なら勝利
            battleResult.wol = WinOrLose.Win;
        }else if(battlePlayer.currentHp <= 0){
            // 自分のHPが0なら敗北
            battleResult.wol = WinOrLose.Lose;
        }else if(!IsRemainPieceCanFit()){
            // ピースを置く場所が無ければ敗北
            battleResult.wol = WinOrLose.Lose;
        }else{
            // それ以外なら続行
            battleResult.wol = WinOrLose.Continue;
        }
    }

    /// <summary>
    /// ピースがハマった時の処理
    /// </summary>
    public void OnPieceFit(int dragablePieceIndex,List<BoardIndex> fitBoardIndexList)
    {
        currentMoveCountPerTurn++;
        dragablePieceList[dragablePieceIndex] = null;

        Fit(fitBoardIndexList);
        
        CrashObservable()
            .Do(_ => {
                JudgeWinOrLose();

                // 勝敗がついていれば次の処理へ
                if(battleResult.wol != WinOrLose.Continue) {
                    pieceMoveObserver.OnNext(Unit.Default);
                    pieceMoveObserver.OnCompleted();
                    pieceMoveObserver = null;
                    return;
                }

                // 全てのピースをはめ終わったら次の処理へ
                if (currentMoveCountPerTurn == ConstManager.Battle.MAX_PARTY_MEMBER_NUM)
                {
                    currentMoveCountPerTurn = 0;
                    pieceMoveObserver.OnNext(Unit.Default);
                    pieceMoveObserver.OnCompleted();
                    pieceMoveObserver = null;
                    return;
                }
            })
            .Subscribe();
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
                
                // アニメーションの都合上CanvasGroupのアルファが0になっていることがあるので1に戻す
                piece.canvasGroup.alpha = 1;
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
    private IObservable<Unit> CrashObservable()
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
        
        // ピース破壊演出を再生
        return VisualFxManager.Instance.PlayCrashPieceFxObservabe(crashRowIndexList, crashColumnIndexList)
            .Do(_ => {
                // UIを更新
                UpdateBoard();
            });
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
