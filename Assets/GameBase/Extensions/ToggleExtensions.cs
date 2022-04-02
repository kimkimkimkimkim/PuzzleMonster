using PM.Enum.Sound;
using System;
using UniRx;
using UnityEngine.UI;

namespace GameBase
{
    public static class ToggleExtension
    {

        /// <summary>
        /// タップしなくても宣言時に一度呼ばれる
        /// </summary>
        public static IObservable<bool> OnValueChangedIntentAsObservable(this Toggle toggle)
        {
            return toggle.OnValueChangedAsObservable()
                .Do(_ =>
                {
                    // 効果音をならす
                    SoundManager.Instance.sfx.Play(SE.Click);
                });

        }
    }
}