using UnityEngine;
using AreaX.Audio;

namespace AreaX.Managers
{
    public class MusicClock : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private double _scheduleLeadTime = 0.1d;

        private AudioSource _audioSource;
        private MusicData _musicData;
        private double _dspStartTime;
        private bool _hasStarted;

        public double SongTime
        {
            get
            {
                if (!_hasStarted || _musicData == null) return 0d;
                return AudioSettings.dspTime - _dspStartTime - _musicData.Offset;
            }
        }

        public double DspStartTime => _dspStartTime;
        public bool HasStarted => _hasStarted;

        public double SecondsPerBeat
        {
            get
            {
                if (_musicData == null || _musicData.BPM <= 0f) return 0.5d;
                return 60d / _musicData.BPM;
            }
        }

        public int CurrentBeatIndex => Mathf.FloorToInt((float)(SongTime / SecondsPerBeat));

        public double BeatPhase
        {
            get
            {
                double secondsPerBeat = SecondsPerBeat;
                if (secondsPerBeat <= 0d) return 0d;
                double phase = SongTime % secondsPerBeat;
                if (phase < 0d) phase += secondsPerBeat;
                return phase / secondsPerBeat;
            }
        }

        public void Initialize(AudioSource audioSource, MusicData musicData)
        {
            _audioSource = audioSource;
            _musicData = musicData;
            _hasStarted = false;
        }

        public void PlayScheduled()
        {
            if (_audioSource == null || _musicData == null || _musicData.Clip == null) return;

            _audioSource.clip = _musicData.Clip;
            _dspStartTime = AudioSettings.dspTime + _scheduleLeadTime;
            _audioSource.PlayScheduled(_dspStartTime);
            _hasStarted = true;
        }

        public double SongTimeToDspTime(double songTime)
        {
            return _dspStartTime + _musicData.Offset + songTime;
        }

        public double GetNextQuantizedSongTime(double division = 1d)
        {
            double quantum = SecondsPerBeat * division;
            if (quantum <= 0d) return SongTime;

            double currentTime = SongTime;
            double remainder = currentTime % quantum;
            if (remainder < 0d) remainder += quantum;
            if (remainder <= double.Epsilon) return currentTime;

            return currentTime + (quantum - remainder);
        }

        public bool IsSongActive()
        {
            if (!_hasStarted || _audioSource == null || _audioSource.clip == null) return false;

            double now = AudioSettings.dspTime;
            double songEndTime = _dspStartTime + _musicData.Offset + _audioSource.clip.length;
            return now >= _dspStartTime && now <= songEndTime;
        }
    }
}
