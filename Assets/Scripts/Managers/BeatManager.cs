using UnityEngine;
using UnityEngine.Events;
using AreaX.Events;

namespace AreaX.Managers
{
    public class BeatManager : MonoBehaviour
    {
        public static BeatManager Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private float _bpm = 120f;
        [SerializeField] private int _beatsPerMeasure = 4;

        [Header("Events")]
        public UnityEvent<BeatEvent> OnBeat;
        public UnityEvent<MeasureEvent> OnMeasure;

        private double _secondsPerBeat;
        private double _nextBeatTime;
        private int _beatCount;
        private bool _isPlaying;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Initial calculation, can be updated if BPM changes
            UpdateBPM(_bpm);
        }
        
        public void UpdateBPM(float bpm)
        {
            _secondsPerBeat = 60d / bpm;
            _bpm = bpm; // Update internal state if needed
        }

        public void StartRhythm()
        {
            _nextBeatTime = 0;
            _beatCount = 0;
            _isPlaying = true;
        }

        private void Update()
        {
            if (!_isPlaying) return;
            if (MusicManager.Instance == null || !MusicManager.Instance.IsPlaying()) return;

            // Sync with Audio Time from MusicManager
            double currentTime = MusicManager.Instance.GetAudioTime();

            if (currentTime >= _nextBeatTime)
            {
                FireBeat(currentTime);
                // Correct drift by adding logical interval rather than current time
                _nextBeatTime += _secondsPerBeat;
            }
        }

        private void FireBeat(double currentTime)
        {
            var beatEvent = new BeatEvent
            {
                BeatTime = currentTime,
                BeatDuration = _secondsPerBeat,
                NextBeatTime = currentTime + _secondsPerBeat,
                BeatIndex = _beatCount
            };
            OnBeat?.Invoke(beatEvent);

            if (_beatCount % _beatsPerMeasure == 0)
            {
                OnMeasure?.Invoke(new MeasureEvent
                {
                    MeasureTime = currentTime,
                    MeasureIndex = _beatCount / _beatsPerMeasure
                });
            }

            _beatCount++;
        }
        
        // Returns the absolute time of the next quantized beat/sub-beat
        // Used for scheduling impacts
        public double GetNextQuantizedTime(double currentTime, double division = 1.0)
        {
             // Simple implementation for main beat quantization
             // Division 1.0 = Beat, 0.5 = Eighth note, etc.
             
             double duration = _secondsPerBeat * division;
             double remainder = currentTime % duration;
             if (remainder < 0d) remainder += duration;
             return currentTime + (duration - remainder);
        }
        
        public double GetSecondsPerBeat() => _secondsPerBeat;
    }
}
