using UnityEngine;
using AreaX.Managers;

namespace AreaX.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class HitSoundSynthesizer : MonoBehaviour
    {
        public static HitSoundSynthesizer Instance { get; private set; }

        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private float _basePitch = 1.0f;
        [SerializeField, Min(1)] private int _scheduledSourceCount = 12;
        
        // Simple Pentatonic Scale (Major) : 0, 2, 4, 7, 9, 12
        private int[] _scaleIntervals = new int[] { 0, 2, 4, 7, 9, 12 };
        private int _lastNoteIndex = 0; // Index in the scale arrays (virtual index)

        private AudioSource _audioSource;
        private AudioSource[] _scheduledSources;
        private int _nextScheduledSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
            BuildScheduledSources();
        }

        public void PlayHitSound()
        {
            if (_audioSource == null || _hitClip == null) return;

            _audioSource.pitch = PickNextPitch();
            _audioSource.PlayOneShot(_hitClip);
        }

        public bool ScheduleHitSoundAtSongTime(double songTime)
        {
            if (_hitClip == null || MusicManager.Instance == null || MusicManager.Instance.Clock == null)
            {
                return false;
            }

            double dspTime = MusicManager.Instance.Clock.SongTimeToDspTime(songTime);
            if (dspTime <= AudioSettings.dspTime)
            {
                return false;
            }

            AudioSource source = GetScheduledSource();
            source.clip = _hitClip;
            source.pitch = PickNextPitch();
            source.PlayScheduled(dspTime);
            return true;
        }

        private float PickNextPitch()
        {
            int stride = Random.Range(-1, 2);
            int nextIndex = Mathf.Clamp(_lastNoteIndex + stride, 0, _scaleIntervals.Length - 1);

            _lastNoteIndex = nextIndex;
            int semiTone = _scaleIntervals[nextIndex];

            return _basePitch * Mathf.Pow(2f, semiTone / 12f);
        }

        private void BuildScheduledSources()
        {
            _scheduledSources = new AudioSource[_scheduledSourceCount];
            for (int i = 0; i < _scheduledSources.Length; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                source.volume = _audioSource != null ? _audioSource.volume : 1f;
                _scheduledSources[i] = source;
            }
        }

        private AudioSource GetScheduledSource()
        {
            if (_scheduledSources == null || _scheduledSources.Length == 0)
            {
                BuildScheduledSources();
            }

            AudioSource source = _scheduledSources[_nextScheduledSource];
            _nextScheduledSource = (_nextScheduledSource + 1) % _scheduledSources.Length;
            return source;
        }
    }
}
