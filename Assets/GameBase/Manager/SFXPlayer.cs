using PM.Enum.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using UniRx;

namespace GameBase
{

    [RequireComponent(typeof(AudioSource))]
    public class SFXPlayer : MonoBehaviour
    {
        private const string TAG = "SFX";
        private const float INTERVAL_SECONDS = 0.25f;
        private Dictionary<string, IDisposable> disposableTimerDict = new Dictionary<string, IDisposable>();

        public bool IsPlaying
        {
            get
            {
                if (source == null) return false;
                return source.isPlaying;
            }
        }

        private AudioSource source;
        private bool canPlay = true;

        public void Awake()
        {
            source = GetComponent<AudioSource>();
        }

        public bool ForcePlay(SE se, bool isStopCurrentSfx = true)
        {
            var filePath = $"StaticAssets/Sound/SE/{se}";
            return ForcePlay(filePath,isStopCurrentSfx);
        }

        private bool ForcePlay(string filePath, bool isStopCurrentSfx = true)
        {
            canPlay = true;
            if (isStopCurrentSfx) Stop();
            return Play(filePath);
        }

        public bool Play(SE se, float volume = 1.0f)
        {
            var filePath = $"StaticAssets/Sound/SE/{se}";
            return Play(filePath, volume);
        }

        private bool Play(string filePath, float volume = 1.0f)
        {
            var clip = ResourceManager.Instance.LoadAsset<AudioClip>(filePath);

            if (clip != null)
            {
                Play(clip, volume);
                return true;
            }
            else
            {
                Debug.LogWarning("Unabled to load requested SFX Asset : " + filePath);
                return false;
            }
        }

        public void Play(AudioClip clip, float volume)
        {
            // 今から再生しようとしている音源に解放タイマーが予約されていた場合はタイマー破棄する
            DisposeTimer(clip);

            if (source.loop)
            {
                if (source.clip != null)
                {
                    source.clip.UnloadAudioData();
                    source.clip = null;
                }

                source.volume = volume;
                source.clip = clip;
                source.Play();
            }
            else
            {
                if (canPlay)
                {
                    var disposeTimer = Observable.Timer(TimeSpan.FromSeconds(clip.length))
                        .First()
                        .Do(_ => {
                            if (clip != null)
                            {
                                disposableTimerDict.Remove(clip.name);
                                clip.UnloadAudioData();
                                clip = null;
                            }
                        })
                        .Subscribe();

                    disposableTimerDict[clip.name] = disposeTimer;

                    source.PlayOneShot(clip, volume);
                    canPlay = false;

                    Observable.Timer(TimeSpan.FromSeconds(INTERVAL_SECONDS))
                        .First()
                        .Do(_ => canPlay = true)
                        .Subscribe();
                }
            }
        }

        private void DisposeTimer(AudioClip clip)
        {
            if (clip != null && disposableTimerDict.ContainsKey(clip.name))
            {
                var timer = disposableTimerDict[clip.name];
                timer.Dispose();
                disposableTimerDict.Remove(clip.name);
            }
        }

        public void Stop()
        {
            if (source != null)
            {
                source.Stop();
                if (source.clip != null)
                {
                    source.clip.UnloadAudioData();
                    source.clip = null;
                }
            }
        }

        internal void SetMixerGroup(AudioMixerGroup mixerGroup)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }
    }
}