using UnityEngine;

namespace AreaX.Managers
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(MusicClock))]
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }
        private AudioSource _audioSource;
        private MusicClock _clock;

        [SerializeField] private AreaX.Audio.MusicData _musicData;

        public MusicClock Clock => _clock;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
            _clock = GetComponent<MusicClock>();
            if (_clock == null)
            {
                _clock = gameObject.AddComponent<MusicClock>();
            }
            
            // Standard settings for BGM
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;

            _clock.Initialize(_audioSource, _musicData);
        }

        public void PlayMusic()
        {
            if (_musicData == null)
            {
                Debug.LogWarning("MusicManager: No MusicData assigned.");
                return;
            }

            // Sync BPM
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.UpdateBPM(_musicData.BPM);
            }

            // Play
            if (_musicData.Clip != null)
            {
                _clock.Initialize(_audioSource, _musicData);
                _clock.PlayScheduled();
            }
        }
        
        public AudioSource GetAudioSource()
        {
            return _audioSource;
        }

        public double GetAudioTime()
        {
            if (_clock == null) return 0;
            return _clock.SongTime;
        }
        
        public bool IsPlaying()
        {
            if (_clock == null) return false;
            return _clock.IsSongActive();
        }
    }
}
