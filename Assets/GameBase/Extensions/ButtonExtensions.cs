using System;
using System.Linq;
using DG.Tweening;
using PM.Enum.Sound;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace GameBase {

    public static class ButtonExtension {

        public static IObservable<long> OnHoldAsObservable(this Button button, double ms = 350, Action releaseEvent = null) {
            return button.OnPointerDownAsObservable()
                .SelectMany(_ => Observable.Timer(TimeSpan.FromMilliseconds(ms)))
                .TakeUntil(button.OnPointerUpAsObservable().
                    Do(_ => {
                        if (releaseEvent != null)
                            releaseEvent();
                    }))
                .RepeatUntilDestroy(button)
                .AsObservable();
        }

        public static IObservable<Unit> OnClickIntentAsObservable(this Button button, ButtonClickIntent intent = ButtonClickIntent.IntervalTap, bool isAnimation = true, bool isSE = true) {
            var clickObservable = button.onClick.AsObservable()
                .Where(_ =>
                    Input.touchSupported == false ||
                    Input.touchSupported == true & Input.touches.Length <= 1
                )
                .AsUnitObservable();

            switch (intent) {
                case ButtonClickIntent.OnlyOneTap:
                    clickObservable = clickObservable.First();
                    break;
                case ButtonClickIntent.IntervalTap:
                    clickObservable = clickObservable.ThrottleFirst(TimeSpan.FromSeconds(0.5f), Scheduler.MainThreadIgnoreTimeScale);
                    break;
                case ButtonClickIntent.Normal:
                default:
                    break;
            }

            return clickObservable
                .Do(_ => {
                    // 効果音
                    if(isSE) SoundManager.Instance.sfx.Play(SE.Click);

                    // アニメーション
                    if (isAnimation) 
                    {
                        var scale = button.transform.localScale;
                        var rectTransform = button.GetComponent<RectTransform>();
                        var sequence = DOTween.Sequence();
                        sequence.Append(rectTransform.DOScale(scale * 0.95f, 0.05f));
                        sequence.Append(rectTransform.DOScale(scale, 0.05f));
                        sequence.SetUpdate(true).PlayAsObservable().Subscribe();
                    }
                })
                .Delay(TimeSpan.FromSeconds(0.1f), Scheduler.MainThreadIgnoreTimeScale)// アニメーション終了まで待つ
                .AsUnitObservable();
        }
    }

    public enum ButtonClickIntent
    {
        Normal,
        OnlyOneTap,
        IntervalTap,
    }
}

