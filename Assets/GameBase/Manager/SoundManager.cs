using UnityEngine;
using UnityEngine.Audio;

namespace GameBase 
{ 
    public class SoundManager : SingletonMonoBehaviour<SoundManager>
    {
        [SerializeField] protected AudioMixer _audioMixer;

        public float masterVolume = 1;

        public AudioMixer audioMixer { get { return _audioMixer; } }
        public BGMPlayer bgm { get; private set; }
        public SFXPlayer sfx { get; private set; }
        public BGMPlayer subBgm { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(transform.gameObject);

            bgm = new GameObject("BgmPlayer").AddComponent<BGMPlayer>();
            bgm.SetMixerGroup(_audioMixer.FindMatchingGroups("BGM")[0]);
            bgm.transform.parent = transform;
            sfx = new GameObject("SfxPlayer").AddComponent<SFXPlayer>();
            sfx.SetMixerGroup(_audioMixer.FindMatchingGroups("SE")[0]);
            sfx.transform.parent = transform;

            subBgm = new GameObject("SubBGMPlayer").AddComponent<BGMPlayer>();
            subBgm.SetMixerGroup(_audioMixer.FindMatchingGroups("BGM")[0]);
            subBgm.transform.parent = transform;

            _audioMixer.SetFloat("MasterVolume", VolumeToDecibel(masterVolume));
        }

        /// <summary>BGM音量</summary>
        /// <param name="volume">音量(0～1)</param>
        public void SetBgmVolume(float volume)
        {
            // PlayerPrefsUtil.Config.SetBgmVolume(volume);
            // if (PlayerPrefsUtil.Config.GetBgmMute()) volume = 0;

            _audioMixer.SetFloat("BGMVolume", VolumeToDecibel(volume));
        }

        /// <summary>BGMミュートON/OFF</summary>
        /// <param name="isMute">1:ON、0:OFF</param>
        public void SetBgmMute(int isMute)
        {
            // PlayerPrefsUtil.Config.SetBgmMute(isMute > 0);
            float volume = 0;
            // if (isMute == 0) volume = PlayerPrefsUtil.Config.GetBgmVolume();

            _audioMixer.SetFloat("BGMVolume", VolumeToDecibel(volume));
        }

        /// <summary>SE音量</summary>
        /// <param name="volume">音量(0～1)</param>
        public void SetSeVolume(float volume)
        {
            // PlayerPrefsUtil.Config.SetSeVolume(volume);
            // if (PlayerPrefsUtil.Config.GetSeMute()) volume = 0;
            _audioMixer.SetFloat("SEVolume", VolumeToDecibel(volume));
        }

        /// <summary>SEミュートON/OFF</summary>
        /// <param name="isMute">1:ON、0:OFF</param>
        public void SetSeMute(int isMute)
        {
            // PlayerPrefsUtil.Config.SetSeMute(isMute > 0);
            float volume = 0;
            // if (isMute == 0) volume = PlayerPrefsUtil.Config.GetSeVolume();

            _audioMixer.SetFloat("SEVolume", VolumeToDecibel(volume));
        }

        /// <summary>VOICE音量</summary>
        /// <param name="volume">音量(0～1)</param>
        public void SetVoiceVolume(float volume)
        {
            // PlayerPrefsUtil.Config.SetVoiceVolume(volume);
            // if (PlayerPrefsUtil.Config.GetVoiceMute()) volume = 0;
            _audioMixer.SetFloat("VOICEVolume", VolumeToDecibel(volume));
        }

        /// <summary>VOICEミュートON/OFF</summary>
        /// <param name="isMute">1:ON、0:OFF</param>
        public void SetVoiceMute(int isMute)
        {
            // PlayerPrefsUtil.Config.SetVoiceMute(isMute > 0);
            float volume = 0;
            // if (isMute == 0) volume = PlayerPrefsUtil.Config.GetVoiceVolume();

            _audioMixer.SetFloat("VOICEVolume", VolumeToDecibel(volume));
        }

        private float VolumeToDecibel(float volume)
        {
            float volumeDB = 20 * Mathf.Log10(volume);

            return Mathf.Clamp(volumeDB, -80.0f, 0.0f);
        }
    }
}