using PM.Enum.Sound;
using UnityEngine;
using UnityEngine.Audio;

namespace GameBase
{
    [RequireComponent(typeof(AudioSource))]
    public class BGMPlayer : MonoBehaviour
    {
        private const string TAG = "BGM";
        private const float FADEOUT_DEFAULT_VOLUME = 0.05f;
        private AudioSource source;
        private AudioClip _currentlyPlayingClip;
        private AudioClip _nextClip;
        private float _startVolume = 1.0f;
        private float _nextVolume;
        private bool _isStopInFadeOut;

        public void Awake()
        {
            _isStopInFadeOut = false;
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
        }

        public void Play(BGM bgm, float volume = 1.0f, bool isCurrentBGMFadeOut = true)
        {
            var filePath = $"StaticAssets/Sound/BGM/{bgm}";
            Play(filePath, volume, isCurrentBGMFadeOut);
        }

        public void Play(string filePath, float volume = 1f, bool isCurrentBGMFadeOut = true)
        {
            var clip = ResourceManager.Instance.LoadAsset<AudioClip>(filePath);

            //アセット読み込み失敗の際には、処理を止める
            if (clip == null)
            {
                // KoiniwaLogger.LogError("Unabled to load requested SFX Asset : " + filePath, TAG);
                return;
            }

            if (!isCurrentBGMFadeOut || !IsPlaying)
            {
                //FadeOutを使わないかBGMが再生中ではない際には、すぐBGMを再生
                _isStopInFadeOut = false;
                _nextClip = null;
                _currentlyPlayingClip = source.clip = clip;
                source.volume = volume;
                source.Play();
            }
            else if (clip == _currentlyPlayingClip && _nextClip == null)
            {
                //再生中BGMと同じ且つFadeOut後再生予定のBGMがない場合は、処理をスキップ
                // KoiniwaLogger.Log("同じBGMを再生。FilePath : " + filePath, TAG);
            }
            else
            {
                //その以外は、FadeOut後再生
                _nextClip = clip;
                _nextVolume = volume;
            }
        }

        public void Volume(float volume = 1f)
        {
            if (source != null) source.volume = volume;
        }

        public void Stop(bool isFadeOut = true)
        {
            if (isFadeOut)
            {
                _isStopInFadeOut = true;
            }
            else
            {
                source.Stop();
            }
        }

        public void Pause()
        {
            source.Pause();
        }

        public void Unpause()
        {
            source.UnPause();
        }

        public void Update()
        {
            //次再生BGMが存在するかFadeOut停止の場合は、FadeOutを行う
            if (_nextClip != null || _isStopInFadeOut == true)
            {
                source.volume -= FADEOUT_DEFAULT_VOLUME;

                //ボリュームが0になったらBGMを止める
                if (source.volume <= 0.0f)
                {
                    _isStopInFadeOut = false;
                    source.Stop();
                    source.volume = _nextClip == null ? _startVolume : _nextVolume;

                    //次再生するBGMがあったら再生を行う
                    if (_nextClip != null)
                    {
                        source.clip = _currentlyPlayingClip = _nextClip;

                        source.Play();
                        _nextClip = null;
                    }
                }
            }
        }

        public void SetMixerGroup(AudioMixerGroup mixerGroup)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }

        private bool IsPlaying
        {
            get
            {
                return source.isPlaying || _isStopInFadeOut;
            }
        }
    }
}