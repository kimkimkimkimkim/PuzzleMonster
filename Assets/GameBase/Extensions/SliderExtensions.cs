using System;
using UniRx;
using UnityEngine.UI;

namespace GameBase
{
    public static class SliderExtensions
    {
        public static IObservable<float> OnValueChangedIntentAsObservable(this Slider slider)
        {
            // 初期化時には呼ばれず、最初に値が変化した時点から呼ばれる
            return slider.OnValueChangedAsObservable().Skip(1);
        }
    }
}
