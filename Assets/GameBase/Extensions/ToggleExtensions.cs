using System;
using UniRx;
using UnityEngine.UI;

namespace GameBase
{
    public static class ToggleExtension
    {

        /// <summary>
        /// 一旦無条件に音を鳴らす
        /// </summary>
        public static IObservable<bool> OnValueChangedIntentAsObservable(this Toggle toggle, ToggleType type)
        {
            var observable = toggle.OnValueChangedAsObservable();
            if (type == ToggleType.Multiple) observable = observable.Where(isOn => isOn);
            return observable;
            // TODO: 一時的にサウンド再生処理をコメントアウト
            // .Do(_ => SoundManager.Instance.sfx.Play(AudioClipPath.SE_COMMON_BUTTON_CLICK));
        }
    }

    public enum ToggleType
    {
        None = 0,
        Single = 1,
        Multiple = 2,
    }
}