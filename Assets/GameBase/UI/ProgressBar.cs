using System;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

namespace GameBase
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] protected RectTransform _progressBarBaseRT;
        [SerializeField] protected RectTransform _progressBarRT;
        [SerializeField] protected Text _text;

        private float maxValue;
        private float currentValue;
        private float maxWidth;

        private void Start()
        {
            maxWidth = _progressBarBaseRT.rect.width;
        }

        public void Init(float maxValue, float currentValue)
        {
            this.maxValue = maxValue;
            this.currentValue = currentValue;

            ChangeValueObservable(this.currentValue, 0.0f);
        }

        public IObservable<Unit> ChangeValueObservable(float toValue, float animationTime = 1.0f, Ease ease = Ease.InOutSine, Action<float> onValueChangeAction = null)
        {
            UIManager.Instance.ShowTapBlocker();
            return DOTween.Sequence()
                .Append(DOVirtual.Float(currentValue, toValue, animationTime, value =>
                {
                    var ratio = value / maxValue;
                    _progressBarRT.sizeDelta = new Vector2(maxWidth * ratio, _progressBarRT.rect.height);

                    if (onValueChangeAction != null) onValueChangeAction(value);
                }).SetEase(ease))
                .OnCompleteAsObservable()
                .Do(_ => UIManager.Instance.TryHideTapBlocker())
                .AsUnitObservable();
        }

        public void SetText(string text)
        {
            _text.text = text;
        }
    }
}