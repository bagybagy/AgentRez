using UnityEngine;

namespace AreaX.Managers
{
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }
        private AudioSource _audioSource;

        [SerializeField] private AreaX.Audio.MusicData _musicData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
            
            // Standard settings for BGM
            _audioSource.playOnAwake = false;
            _audioSource.loop = false;
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
                _audioSource.clip = _musicData.Clip;
                _audioSource.Play();
            }
        }
        
        public AudioSource GetAudioSource()
        {
            return _audioSource;
        }

        public double GetAudioTime()
        {
            if (_audioSource.clip == null) return 0;
            // Subtract offset if needed? 
            // For now, raw time. Offset logic involves rescheduling hits which is complex.
            // Let's assume audio starts at 0 for simplicity or handle Offset later.
            return (double)_audioSource.timeSamples / _audioSource.clip.frequency;
        }
        
        public bool IsPlaying()
        {
            return _audioSource.isPlaying;
        }
    }
}
