using System;
using System.Collections.Generic;
using System.Linq;
using GameBase;
using PM.Enum.Battle;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[ResourcePath("UI/Window/Window-Battle")]
public class BattleWindowUIScript : DummyWindowBase
{
    public Transform fxParentTransform;
    public Transform backgroundImageTransform;

    [SerializeField] protected RectTransform _dummyBoardPanelRT;
    [SerializeField] protected RectTransform _boardPanelRT;
    [SerializeField] protected GridLayoutGroup _dummyBoardGridLayoutGroup;
    [SerializeField] protected GridLayoutGroup _boardGridLayoutGroup;
    [SerializeField] protected List<RectTransform> _dragablePieceBaseRTList;
    [SerializeField] protected Transform _dragablePieceInitialPositionTransform;
    [SerializeField] protected Transform _enemyParentTransform;
    [SerializeField] protected Transform _playerMonsterParentTransform;

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

        // 背景用のダミーボードも作成
        for (var i = 0; i < BOARD_HEIGHT; i++)
        {
            for (var j = 0; j < BOARD_WIDTH; j++)
            {
                var boardPiece = UIManager.Instance.CreateContent<BattleBoardPieceItem>(_dummyBoardPanelRT);
                boardPiece.SetPieceStatus(PieceStatus.Free);
                boardPiece.SetColor(PieceColor.DarkBrown);
                boardPiece.SetBoardIndex(new BoardIndex(i, j));
            }
        }
    }

    public IObservable<List<QuestMonsterItem>> CreateEnemyObservable(long questId, int waveCount)
    {
        // すでに生成済みのPrefabを削除する
        foreach(Transform t in _enemyParentTransform)
        {
            Destroy(t.gameObject);
        }

        // 生成
        var list = new List<QuestMonsterItem>();
        var quest = MasterRecord.GetMasterOf<QuestMB>().Get(questId);
        var questMonsterIdList = waveCount == 1 ? quest.wave1QuestMonsterIdList : waveCount == 2 ? quest.wave2QuestMonsterIdList : quest.wave3QuestMonsterIdList;
        var observableList = questMonsterIdList.Select(questMonsterId =>
        {
            var questMonster = MasterRecord.GetMasterOf<QuestMonsterMB>().Get(questMonsterId);
            return VisualFxManager.Instance.PlayCreateMonsterFxObservable(_enemyParentTransform, questMonster.monsterId);
        }).ToList();
        return Observable.Concat(observableList)
            .Do(item => list.Add(item))
            .Buffer(observableList.Count)
            .Select(_ => list);
    }

    public IObservable<List<QuestMonsterItem>> CreatePlayerMonsterObservable(List<BattlePlayerMonsterInfo> battlePlayerMonsterList)
    {
        // すでに生成済みのPrefabを削除する
        foreach (Transform t in _playerMonsterParentTransform)
        {
            Destroy(t.gameObject);
        }

        // 生成
        var list = new List<QuestMonsterItem>();
        var observableList = battlePlayerMonsterList.Select(battlePlayerMonster =>
        {
            return Observable.ReturnUnit()
                .SelectMany(_ =>
                {
                    var item = UIManager.Instance.CreateContent<QuestMonsterItem>(_playerMonsterParentTransform);
                    return item.SetMonsterImageObservable(battlePlayerMonster.monsterId).Select(res => item);
                });
        }).ToList();
        return Observable.Concat(observableList)
            .Do(item => list.Add(item))
            .Buffer(observableList.Count)
            .Select(_ => list);
    }

    public void CreateDragablePiece(int index,long id)
    {
        var dragablePieceBaseRT = _dragablePieceBaseRTList[index];
        var dragablePiece = UIManager.Instance.CreateContent<BattleDragablePieceItem>(dragablePieceBaseRT);
        dragablePiece.SetPiece(index, BOARD_PIECE_SPACE, pieceWidth, id);
        BattleManager.Instance.dragablePieceList[index] = dragablePiece;
    }
    
    /// <summary>
    /// ドラッガブルピースを生成しアニメーションまで実行します
    /// </summary>
    public IObservable<Unit> CreateDragablePieceAndPlayAnimationObservable(int index, long id){
        var animationTime = 0.1f;
        var ease = Ease.InSine;
        
        var dragablePieceBaseRT = _dragablePieceBaseRTList[index];
        var dragablePiece = UIManager.Instance.CreateContent<BattleDragablePieceItem>(dragablePieceBaseRT);
        dragablePiece.transform.position = _dragablePieceInitialPositionTransform.position;
        dragablePiece.SetPiece(index, BOARD_PIECE_SPACE, pieceWidth, id);
        BattleManager.Instance.dragablePieceList[index] = dragablePiece;
        
        UIManager.Instance.ShowTapBlocker();
        return DOTween.Sequence()
            .Append(dragablePiece.transform.DOMove(dragablePieceBaseRT.position, animationTime).SetEase(ease))
            .OnCompleteAsObservable()
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable();
    }

    private void SetBoard()
    {
        var boardWidth = _boardPanelRT.sizeDelta.x;
        pieceWidth = (boardWidth - ((BOARD_WIDTH + 1) * BOARD_PIECE_SPACE)) / BOARD_WIDTH;

        _boardGridLayoutGroup.cellSize = new Vector2(pieceWidth, pieceWidth);
        _boardGridLayoutGroup.spacing = new Vector2(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);
        _boardGridLayoutGroup.padding = new RectOffset(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);

        _dummyBoardGridLayoutGroup.cellSize = new Vector2(pieceWidth, pieceWidth);
        _dummyBoardGridLayoutGroup.spacing = new Vector2(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);
        _dummyBoardGridLayoutGroup.padding = new RectOffset(BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE, BOARD_PIECE_SPACE);
    }
}
