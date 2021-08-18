using System;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using PM.Enum.Monster;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
    /// <summary>
    /// クエストタイトル表示演出を実行
    /// </summary>
    public IObservable<Unit> PlayQuestTitleFxObservable(string title)
    {
        UIManager.Instance.ShowTapBlocker();
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<QuestTitleFx>(FadeManager.Instance.GetFadeCanvasRT())
            .SelectMany(fx => {
                fx.text.SetAlpha(0);
                fx.text.text = title;
                fx.gameObject.SetActive(true);

                return DOTween.Sequence()
                    .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .AppendInterval(2.0f)
                    .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .OnCompleteAsObservable()
                    .Do(_ => {
                        if(fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                        UIManager.Instance.TryHideTapBlocker();
                    })
                    .AsUnitObservable();
            });
    }

    /// <summary>
    /// ウェーブ表示演出を実行
    /// </summary>
    public IObservable<Unit> PlayWaveTitleFxObservable(Transform parent, int currentWaveCount, int maxWaveCount)
    {
        UIManager.Instance.ShowTapBlocker();
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<WaveTitleFx>(parent)
            .SelectMany(fx => {
                var distance = 100.0f;

                fx.text.SetAlpha(0);
                fx.text.text = $"Wave {currentWaveCount}/{maxWaveCount}";
                fx.gameObject.SetActive(true);

                return DOTween.Sequence()
                    .Append(fx.transform.DOLocalMoveX(distance, 0.0f))
                    .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .Join(fx.transform.DOLocalMoveX(0.0f, 1.0f))
                    .AppendInterval(1.0f)
                    .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .Join(fx.transform.DOLocalMoveX(-distance, 1.0f))
                    .OnCompleteAsObservable()
                    .Do(_ => {
                        if(fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                        UIManager.Instance.TryHideTapBlocker();
                    })
                    .AsUnitObservable();
            });
    }

    /// <summary>
    /// 敵モンスターの攻撃演出を実行
    /// </summary>
    public IObservable<Unit> PlayEnemyAttackFxObservable(Transform parent, QuestMonsterItem item, Transform backgroundImageTransform)
    {
        var animationTime = 0.25f;
        var scaleEndValue = 1.4f;
        var hopDistanceY = 20.0f;
        var downMagnification = 2f;

        UIManager.Instance.ShowTapBlocker();
        return DOTween.Sequence()
            .Append(DOTween.Sequence().Append(item.transform.DOLocalMoveY(hopDistanceY, animationTime / 2).SetRelative().SetEase(Ease.OutSine)).Append(item.transform.DOLocalMoveY(-hopDistanceY * downMagnification, animationTime / 2).SetRelative().SetEase(Ease.InSine)))
            .Join(item.transform.DOScaleX(scaleEndValue, animationTime))
            .Join(item.transform.DOScaleY(scaleEndValue, animationTime))
            .Append(backgroundImageTransform.DOShakePosition(0.25f, new Vector3(50,50,0)))
            .Append(DOTween.Sequence().Append(item.transform.DOLocalMoveY(hopDistanceY * downMagnification, animationTime / 2).SetRelative().SetEase(Ease.OutSine)).Append(item.transform.DOLocalMoveY(-hopDistanceY, animationTime / 2).SetRelative().SetEase(Ease.InSine)))
            .Join(item.transform.DOScaleX(1, animationTime))
            .Join(item.transform.DOScaleY(1, animationTime))
            .OnCompleteAsObservable()
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable();
    }

    /// <summary>
    /// プレイヤーモンスターの攻撃演出を実行
    /// </summary>
    public IObservable<Unit> PlayPlayerAttackFxObservable(Transform parent, Vector3 fromPosition, Vector3 toPosition, MonsterAttribute attribute, long attackId)
    {
        ParticleSystem orbitPS = null;
        ParticleSystem fxPS = null;
        
        UIManager.Instance.ShowTapBlocker();
        return Observable.WhenAll(
            PMAddressableAssetUtil.InstantiateNormalAttackOrbitObservable(parent, attribute).Do(ps => orbitPS = ps).AsUnitObservable(),
            PMAddressableAssetUtil.InstantiateNormalAttackFxObservable(parent, attackId).Do(ps => fxPS = ps).AsUnitObservable()
        )
            .SelectMany(_ =>
            {
                orbitPS.transform.position = new Vector3(fromPosition.x, fromPosition.y, 0);
                fxPS.transform.position = new Vector3(toPosition.x, toPosition.y, 0);

                return DOTween.Sequence()
                    .AppendCallback(() => orbitPS.Play())
                    .Append(orbitPS.transform.DOMoveX(toPosition.x, 0.5f).SetEase(Ease.Linear))
                    .Join(orbitPS.transform.DOMoveY(toPosition.y, 0.5f).SetEase(Ease.OutQuad))
                    .AppendCallback(() =>
                    {
                        if (orbitPS.gameObject != null) Addressables.ReleaseInstance(orbitPS.gameObject);
                        fxPS.Play();
                    })
                    .AppendInterval(1.0f)
                    .AppendCallback(() =>
                    {
                        if (fxPS.gameObject != null) Addressables.ReleaseInstance(fxPS.gameObject);
                    })
                    .OnCompleteAsObservable()
                    .Do(res => UIManager.Instance.TryHideTapBlocker())
                    .AsUnitObservable();
            });
    }

    /// <summary>
    /// 敵モンスターの生成演出を実行
    /// </summary>
    public IObservable<QuestMonsterItem> PlayCreateMonsterFxObservable(Transform parent, long monsterId)
    {
        // 最初に一度に生成する必要があるため、生成処理はObservableの外に出す
        var item = UIManager.Instance.CreateContent<QuestMonsterItem>(parent);
        var canvasGroup = item.GetCanvasGroup();
        canvasGroup.alpha = 0;

        UIManager.Instance.ShowTapBlocker();
        return item.SetMonsterImageObservable(monsterId)
            .SelectMany(_ =>
            {
                return DOTween.Sequence()
                    .Append(canvasGroup.DOFade(1.0f, 0.5f))
                    .OnCompleteAsObservable()
                    .AsUnitObservable();
            })
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .Select(_ => item);
    }

    /// <summary>
    /// 敵モンスターの死亡演出を実行
    /// </summary>
    public IObservable<Unit> PlayDefeatMonsterFxObservable(QuestMonsterItem item)
    {
        var canvasGroup = item.GetCanvasGroup();
        
        UIManager.Instance.ShowTapBlocker();
        return DOTween.Sequence()
            .Append(canvasGroup.DOFade(0.0f, 0.5f))
            .OnCompleteAsObservable()
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable();
    }
    
    /// <summary>
    /// ドラッガブルピースを押下した際に広がる演出を実行
    /// </summary>
    public IObservable<Unit> PlayOnPointerDownAtDragablePieceFxObservable(List<PieceData> pieceDataList)
    {
        var animationTime = 0.5f;
        
        var observableList = pieceDataList.Select(pieceData =>
        {
            var targetPosition = pieceData.targetPieceItem.transform.localPosition + new Vector3(0, 250, 0);
            var targetPieceSize = pieceData.targetPieceItem.GetRectTransform().sizeDelta;
            var pieceSize = pieceData.pieceItem.GetRectTransform().sizeDelta;
            var scale = targetPieceSize.x / pieceSize.x; // 現状ピースは正方形なのでxの値だけで指定

            return Observable.ReturnUnit()
                .SelectMany(_ => {
                    return DOTween.Sequence()
                        .Join(pieceData.pieceItem.transform.DOLocalMove(targetPosition, animationTime))
                        .Join(pieceData.pieceItem.transform.DOScale(scale, animationTime));
                        .OnCompleteAsObservable()
                        .AsUnitObservable();
                });
        });
        
        UIManager.Instance.ShowTapBlocker();
        return Observable.WhenAll(observableList)
            .Do(_ => UIManager.Instance.TryHideTapBlocker());
    }
    
    /// <summary>
    /// ドラッガブルピースが盤面にはまる際の演出を実行
    /// </summary>
    public IObservable<Unit> PlayOnDragablePieceFitFxObservable(BoardIndex nearestPieceBoardIndex)
    {
        var animationTime = 0.1f;

        var board = BattleManager.Instance.board;
        var horizontalConstraint = piece.horizontalConstraint;
        var observableList = pieceDataList.Select((p, index) => {
            var additiveRow = index / horizontalConstraint;
            var additiveColumn = index % horizontalConstraint;
            var targetBoardIndex = new BoardIndex(nearestPieceBoardIndex.row + additiveRow, nearestPieceBoardIndex.column + additiveColumn);
            return Observable.ReturnUnit()
                .SelectMany(_ => {
                    var toPosition = board[targetBoardIndex.row, targetBoardIndex.column].transform.position;
                    return DOTween.Sequence()
                        .Append(p.pieceItem.transform.DOMove(toPosition, animationTime))
                        .OnCompleteAsObservable()
                        .AsUnitObservable();
                });
        });
        
        UIManager.Instance.ShowTapBlocker();
        return Observable.WhenAll(observableList)
            .Do(_ => UIManager.Instance.TryHideTapBlocker());
    }
    
    /// <summary>
    /// ドラッガブルピースから指を離した際に元の位置に戻る演出を実行
    /// </summary>
    public IObservable<Unit> PlayDragablePieceMoveInitialPositionFxObservable(Transform dragablePieceTransform, List<PieceData> pieceDataList)
    {
        var animationTime = 0.5f;
        
        // ドラッガブル内のピースの位置元に戻すアニメーション
        var observableList = pieceDataList.Select(pieceData =>
        {
            return Observable.ReturnUnit()
                .SelectMany(_ => {
                    return DOTween.Sequence()
                        .Join(pieceData.pieceItem.transform.DOLocalMove(pieceData.initialPos, animationTime))
                        .Join(pieceData.pieceItem.transform.DOScale(1, animationTime));
                })
                .AsUnitObservable();
        });
        
        // ドラッガブルピースを元の位置に戻すアニメーション
        var observable = Observable.ReturnUnit()
            .SelectMany(_ => {
                return DOTween.Sequence()
                    .Append(dragablePieceTransform..DOLocalMove(new Vector3(0, 0, 0), animationTime))
            })
            .AsUnitObservable();
            
        UIManager.Instance.ShowTapBlocker();
        return Observable.WhenAll(
            observableList,
            observable
        )
            .Do(_ => UIManager.Instance.TryHideTapBlocker());
    }
}
