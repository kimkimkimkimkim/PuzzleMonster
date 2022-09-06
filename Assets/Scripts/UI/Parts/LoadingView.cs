using GameBase;
using UnityEngine;
using System.Collections.Generic;
using System;
using UniRx;
using DG.Tweening;

[ResourcePath("UI/Parts/Parts-LoadingView")]
public class LoadingView : MonoBehaviour
{
    [SerializeField] protected CanvasGroup _parentCanvasGroup;
    [SerializeField] protected List<CanvasGroup> _dotCanvasGroupList;

    private IDisposable _parentCanvasGroupAnimationObservable;
    private float parentWaitTime = 1.0f;
    private float parentFadeInTime = 1.0f;
    private float roundTime = 1.5f;
    private float minAlpha = 0.2f;
    private float maxAlpha = 1.0f;
    private float minScale = 0.2f;
    private float maxScale = 1.0f;
    private float time = 0.0f;

    private void OnEnable()
    {
        _parentCanvasGroup.alpha = 0.0f;

        _parentCanvasGroupAnimationObservable = Observable.Timer(TimeSpan.FromSeconds(parentWaitTime))
            .SelectMany(_ =>
            {
                var sequence = DOTween.Sequence()
                    .Append(_parentCanvasGroup.DOFade(1.0f, parentFadeInTime));
                return sequence.OnCompleteAsObservable();
            })
            .Subscribe();
    }

    private void FixedUpdate()
    {
        time += Time.deltaTime;

        _dotCanvasGroupList.ForEach((canvasGroup, index) =>
        {
            var fixedTime = time + roundTime * ((float)index / (_dotCanvasGroupList.Count - 1));
            canvasGroup.alpha = minAlpha + (maxAlpha - minAlpha) * (roundTime - fixedTime % roundTime);
            canvasGroup.transform.localScale = Vector3.one * (minScale + (maxScale - minScale) * (roundTime - fixedTime % roundTime));
        });
    }

    private void OnDisable()
    {
        _parentCanvasGroup.alpha = 0.0f;

        if(_parentCanvasGroupAnimationObservable != null)
        {
            _parentCanvasGroupAnimationObservable.Dispose();
            _parentCanvasGroupAnimationObservable = null;
        }
    }
} 