using System;
using DG.Tweening;
using GameBase;
using UniRx;
using UnityEngine;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
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

    public IObservable<QuestMonsterItem> PlayCreateMonsterFxObservable(Transform parent, long monsterId)
    {
        var item = UIManager.Instance.CreateContent<QuestMonsterItem>(parent);
        item.GetCanvasGroup().alpha = 0;

        return item.SetMonsterImageObservable(monsterId)
            .SelectMany(_ =>
            {
                return DOTween.Sequence()
                    .Append(item.GetCanvasGroup().DOFade(1.0f, 0.5f))
                    .OnCompleteAsObservable()
                    .AsUnitObservable();
            })
            .Select(_ => item);
    }
}
