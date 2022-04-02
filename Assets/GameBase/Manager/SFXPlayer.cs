using PM.Enum.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace GameBase
{
    [RequireComponent(typeof(AudioSource))]
    public class SFXPlayer : MonoBehaviour
    {
        private const string TAG = "SFX";

        private List<AudioClip> _calledSEList;
        private AudioSource source;

        public bool IsPlaying { get { if (source != null) { return source.isPlaying; } else { return false; } } }

        public void Awake()
        {
            _calledSEList = new List<AudioClip>();
            source = GetComponent<AudioSource>();
        }

        public bool Play(SE se, float volume = 1.0f, AudioSource desiredSource = null)
        {
            var filePath = $"StaticAssets/Sound/SE/{se}";
            return Play(filePath, volume, desiredSource);
        }

        public bool Play(string filePath, float volume = 1.0f, AudioSource desiredSource = null)
        {
            var clip = ResourceManager.Instance.LoadAsset<AudioClip>(filePath);

            if (clip != null)
            {
                Play(clip, volume, desiredSource);
                return true;
            }
            else
            {
                // KoiniwaLogger.LogWarning("Unabled to load requested SFX Asset : " + filePath, TAG);
                return false;
            }
        }

        public void Play(AudioClip clip, float volume, AudioSource desiredSource = null)
        {
            foreach (AudioClip calledSE in _calledSEList)
            {
                if (clip == calledSE) return;
            }
            _calledSEList.Add(clip);
            if (desiredSource == null) desiredSource = source;

            desiredSource.outputAudioMixerGroup = source.outputAudioMixerGroup;
            if (desiredSource.loop)
            {
                desiredSource.volume = volume;
                desiredSource.clip = clip;
                desiredSource.Play();
            }
            else
            {
                desiredSource.PlayOneShot(clip, volume);
            }
        }

        public void Stop()
        {
            if (source != null)
            {
                source.Stop();
            }
        }

        public void LateUpdate()
        {
            _calledSEList.Clear();
        }

        internal void SetMixerGroup(AudioMixerGroup mixerGroup)
        {
            source.outputAudioMixerGroup = mixerGroup;
        }
    }
}