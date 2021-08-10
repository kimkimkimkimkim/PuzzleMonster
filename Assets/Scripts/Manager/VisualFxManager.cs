using System;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
    #region FxItem
    public IObservable<Unit> PlayQuestTitleFxObservable(string title)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<QuestTitleFx>("QuestTitleFx",FadeManager.Instance.GetFadeCanvasRT())
            .SelectMany(fx => fx.PlayFxObservable(title));
    }

    public IObservable<Unit> PlayWaveTitleFxObservable(Transform parent, int currentWaveCount, int maxWaveCount)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<WaveTitleFx>("WaveTitleFx", parent)
            .SelectMany(fx => fx.PlayFxObservable(currentWaveCount, maxWaveCount));
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
