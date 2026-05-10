using System;
using System.Collections.Generic;
using UnityEngine;

namespace AreaX.Audio
{
    public enum MusicCueType
    {
        Section,
        Spawn,
        Visual,
        BossPhase
    }

    [Serializable]
    public class MusicTimelineCue
    {
        public string Id;
        public MusicCueType Type;
        [Min(0)] public int Measure;
        [Min(0)] public int Beat;
        public float Value = 1f;
        public Color Color = Color.white;
    }

    [CreateAssetMenu(fileName = "NewMusicTimeline", menuName = "AreaX/Music Timeline")]
    public class MusicTimeline : ScriptableObject
    {
        [Header("Structure")]
        [SerializeField] private List<MusicTimelineCue> _cues = new List<MusicTimelineCue>();

        [Header("Intensity")]
        [SerializeField] private AnimationCurve _intensity = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public IReadOnlyList<MusicTimelineCue> Cues => _cues;

        public float EvaluateIntensity(double songTime, double songLength)
        {
            if (songLength <= 0d) return 0f;

            float normalizedTime = Mathf.Clamp01((float)(songTime / songLength));
            return Mathf.Clamp01(_intensity.Evaluate(normalizedTime));
        }

        public IEnumerable<MusicTimelineCue> GetCuesAtMeasure(int measure)
        {
            for (int i = 0; i < _cues.Count; i++)
            {
                if (_cues[i].Measure == measure)
                {
                    yield return _cues[i];
                }
            }
        }
    }
}
