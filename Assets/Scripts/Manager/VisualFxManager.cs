using System;
using GameBase;
using UniRx;
using UnityEngine;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
    public IObservable<Unit> PlayQuestTitleFxObservable()
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<QuestTitleFx>("QuestTitleFx",FadeManager.Instance.GetFadeCanvasRT())
            .SelectMany(fx => fx.PlayFxObservable());
    }

    public IObservable<Unit> PlayWaveTitleFxObservable(Transform parent, int currentWaveCount, int maxWaveCount)
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<WaveTitleFx>("WaveTitleFx", parent)
            .SelectMany(fx => fx.PlayFxObservable(currentWaveCount, maxWaveCount));
    }
}