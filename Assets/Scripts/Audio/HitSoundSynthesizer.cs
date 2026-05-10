using UnityEngine;

namespace AreaX.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class HitSoundSynthesizer : MonoBehaviour
    {
        public static HitSoundSynthesizer Instance { get; private set; }

        [SerializeField] private AudioClip _hitClip;
        [SerializeField] private float _basePitch = 1.0f;
        
        // Simple Pentatonic Scale (Major) : 0, 2, 4, 7, 9, 12
        private int[] _scaleIntervals = new int[] { 0, 2, 4, 7, 9, 12 };
        private int _lastNoteIndex = 0; // Index in the scale arrays (virtual index)

        private AudioSource _audioSource;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _audioSource = GetComponent<AudioSource>();
        }

        public void PlayHitSound()
        {
            if (_audioSource == null || _hitClip == null) return;

            // Select next note
            // Strategy: Random walk within scale, but biased towards Center?
            // Spec says: "Proximity to last note" (7.4)
            // "Cannot use purely random"
            
            // Random stride: -1, 0, +1 index in scale
            int stride = Random.Range(-1, 2); 
            int nextIndex = _lastNoteIndex + stride;
            
            // Clamp to octave (0 to 5 in array)
            if (nextIndex < 0) nextIndex = 0;
            if (nextIndex >= _scaleIntervals.Length) nextIndex = _scaleIntervals.Length - 1;
            
            _lastNoteIndex = nextIndex;
            int semiTone = _scaleIntervals[nextIndex];
            
            float pitch = _basePitch * Mathf.Pow(2f, semiTone / 12f);
            
            _audioSource.pitch = pitch;
            _audioSource.PlayOneShot(_hitClip);
        }
    }
}
