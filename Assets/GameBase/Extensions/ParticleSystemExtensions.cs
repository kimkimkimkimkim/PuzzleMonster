using System;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace GameBase
{
    public static class ParticleSystemExtensions
    {
        /// <summary>
        /// アニメーションを再生し、指定時間後にメモリ解放
        /// </summary>
        public static void PlayWithRelease(this ParticleSystem ps, float time)
        {
            ps.Play();
            Observable.Timer(TimeSpan.FromSeconds(time))
                .Do(_ =>
                {
                    if (ps.gameObject != null) Addressables.ReleaseInstance(ps.gameObject);
                })
                .Subscribe()
                .AddTo(ps.gameObject);
        }
    }
}