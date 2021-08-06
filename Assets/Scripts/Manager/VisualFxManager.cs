using System;
using GameBase;
using UniRx;

public class VisualFxManager : SingletonMonoBehaviour<VisualFxManager>
{
    public IObservable<Unit> PlayQuestTitleFxObservable()
    {
        return PMAddressableAssetUtil.InstantiateVisualFxItemObservable<QuestTitleFx>("QuestTitleFx",FadeManager.Instance.GetFadeCanvasRT())
            .SelectMany(fx =>
            {
                UnityEngine.Debug.Log(fx);
                return fx.PlayFxObservable();
            });
    }
}