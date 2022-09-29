﻿using System;
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

    public IObservable<Unit> PlayFadeAnimationObservable(float toValue, float animationTime = ANIMATION_TIME)
    {
        UIManager.Instance.ShowTapBlocker();
        return DOVirtual.Float(_fade.Range, toValue, animationTime, value => _fade.Range = value)
            .SetUpdate(true) // 一時停止中でも動作するように
            .OnCompleteAsObservable()
            .Do(_ => UIManager.Instance.TryHideTapBlocker())
            .AsUnitObservable();
    }

    public RectTransform GetFadeCanvasRT()
    {
        return _fadeCanvasRT;
    }
}

