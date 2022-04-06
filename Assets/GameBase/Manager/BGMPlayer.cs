using PM.Enum.Sound;
using UnityEngine;
using UnityEngine.Audio;
using DG.Tweening;
using UniRx;
using System;

namespace GameBase
{

    [RequireComponent(typeof(AudioSource))]
    public class BGMPlayer : MonoBehaviour
    {
        private const string TAG = "BGM";
        private const float FADE_IN_START_VOLUME = 0.0625f;
        private AudioSource source;
        private AudioClip _currentlyPlayingClip;
        private float _startVolume = 1.0f;
        private IDisposable fadeoutObservable;

        public void Awake()
        {
            _currentlyPlayingClip = null;
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
        }

        public void Play(BGM bgm, float volume = 1.0f, float fadeTime = 0.0f)
        {
            var filePath = $"StaticAssets/Sound/BGM/{bgm}";
            Play(filePath, volume, fadeTime);
        }

        public void Play(string filePath, float volume = 1f, float fadeTime = 0.0f)
        {
            if (fadeoutObservable != null) fadeoutObservable.Dispose();

            var clip = ResourceManager.Instance.LoadAsset<AudioClip>(filePath);

            // アセット読み込み失敗の際には、処理を止める
            if (clip == null)
            {
                Debug.LogError("Unabled to load requested BGM Asset : " + filePath);
                return;
            }

            if (clip == _currentlyPlayingClip)
            {
                // 再生中BGMと同じ場合は処理をスキップ
                Debug.Log("同じBGMを再生。FilePath : " + filePath);
            }
            else
            {
                if (fadeTime > 0f)
                {
                    source.volume = FADE_IN_START_VOLUME;
                    source.DOFade(volume, fadeTime);
                }
                else
                {
                    source.volume = volume;
                }
                _startVolume = volume;
                source.clip = _currentlyPlayingClip = clip;
                source.Play();
            }
        }

        public void Volume(float volume = 1f)
        {
            if (source != null) source.volume = volume;
        }

        public void Stop(float fadeTime = 0f)
        {
            _currentlyPlayingClip = null;
            if (fadeTime > 0f)
            {
                fadeoutObservable = source.DOFade(0f, fadeTime).OnCompleteAsObservable()
                    .Do(_ => Stop(source))
                    .Subscribe();
            }
            else
            {
                Stop(source);
            }
        }

        private void Stop(AudioSource source)
        {
            source.Stop();
            if (source.clip != null)
            {
                source.clip.UnloadAudioData();
                source.clip = null;
            }
        }

        public void Pause()
        {
            source.Pause();
        }

        public void UnPause()
        {
            source.UnPause();
        }

        public IObservable<Unit> FadeObservable(float endVolume, float fadeTime)
        {
            return source.DOFade(endVolume, fadeTime).OnCompleteAsObservable().AsUnitObservable();
        }

        public void SetMixerGroup(AudioMixerGroup mixerGroup)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }

        /// <summary>
        /// 指定したパスのAudioClipを現在再生中か否かを返します
        /// </summary>
        public bool IsSameClipPlaying(string filePath)
        {
            var clip = ResourceManager.Instance.LoadAsset<AudioClip>(filePath);

            return IsSameClipPlaying(clip);
        }

        public bool IsSameClipPlaying(AudioClip clip)
        {
            return clip != null && clip == _currentlyPlayingClip;
        }

        private bool IsPlaying
        {
            get
            {
                return source.isPlaying;
            }
        }
    }
}