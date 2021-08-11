using System;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
    #region FxItem
    public IObservable<Unit> PlayQuestTitleFxObservable(string title)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<QuestTitleFx>(FadeManager.Instance.GetFadeCanvasRT())
            .SelectMany(fx => {
                fx.text.SetAlpha(0);
                fx.text.text = title;
                gameObject.SetActive(true);

                return DOTween.Sequence()
                    .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .AppendInterval(2.0f)
                    .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .OnCompleteAsObservable()
                    .Do(_ => {
                        if(fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                    })
                    .AsUnitObservable();
            });
    }

    public IObservable<Unit> PlayWaveTitleFxObservable(Transform parent, int currentWaveCount, int maxWaveCount)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<WaveTitleFx>(parent)
            .SelectMany(fx => {
                var distance = 100.0f;

                fx.text.SetAlpha(0);
                fx.text.text = $"Wave {currentWaveCount}/{maxWaveCount}";
                gameObject.SetActive(true);

                return DOTween.Sequence()
                    .Append(transform.DOLocalMoveX(distance, 0.0f))
                    .Append(DOVirtual.Float(0.0f, 1.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .Join(transform.DOLocalMoveX(0.0f, 1.0f))
                    .AppendInterval(1.0f)
                    .Append(DOVirtual.Float(1.0f, 0.0f, 1.0f, value => fx.text.SetAlpha(value)))
                    .Join(transform.DOLocalMoveX(-distance, 1.0f))
                    .OnCompleteAsObservable()
                    .Do(_ => {
                        if(fx.gameObject != null) Addressables.ReleaseInstance(fx.gameObject);
                    })
                    .AsUnitObservable();
            });
    }
    #endregion

    #region Prefab
    public IObservable<QuestMonsterItem> PlayCreateMonsterFxObservable(Transform parent, long monsterId)
    {
        // 最初に一度に生成する必要があるため、生成処理はObservableの外に出す
        var item = UIManager.Instance.CreateContent<QuestMonsterItem>(parent);
        var canvasGroup = item.GetCanvasGroup();
        canvasGroup.alpha = 0;

        return item.SetMonsterImageObservable(monsterId)
            .SelectMany(_ =>
            {
                return DOTween.Sequence()
                    .Append(canvasGroup.DOFade(1.0f, 0.5f))
                    .OnCompleteAsObservable()
                    .AsUnitObservable();
            })
            .Select(_ => item);
    }

    public IObservable<Unit> PlayDefeatMonsterFxObservable(QuestMonsterItem item)
    {
        var canvasGroup = item.GetCanvasGroup();
        return DOTween.Sequence()
            .Append(canvasGroup.DOFade(0.0f, 0.5f))
            .OnCompleteAsObservable()
            .AsUnitObservable();
    }
    #endregion
}
