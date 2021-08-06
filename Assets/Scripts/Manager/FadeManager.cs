using System;
using GameBase;
using UniRx;
using UnityEngine;
using DG.Tweening;

public class FadeManager : SingletonMonoBehaviour<FadeManager>
{
    [SerializeField] RectTransform _fadeCanvasRT;
    [SerializeField] FadeImage _fade;

    private const float ANIMATION_TIME = 1.0f;

    private void Start()
    {
        _fade.Range = 0.0f;
    }

    public IObservable<Unit> PlayFadeAnimationObservable(float toValue)
    {
        return PlayFadeAnimationObservable(toValue, ANIMATION_TIME);
    }

    public IObservable<Unit> PlayFadeAnimationObservable(float toValue, float animationTime)
    {
        UIManager.Instance.ShowTapBlocker();
        return DOVirtual.Float(_fade.Range, toValue, animationTime, value => _fade.Range = value)
            .OnCompleteAsObservable()
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable();
    }

    public RectTransform GetFadeCanvasRT()
    {
        return _fadeCanvasRT;
    }
}

