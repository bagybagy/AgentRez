using UnityEngine;

namespace AreaX.Audio
{
    [CreateAssetMenu(fileName = "NewMusicData", menuName = "AreaX/Music Data")]
    public class MusicData : ScriptableObject
    {
        [Header("Audio")]
        public AudioClip Clip;

        [Header("Rhythm Data")]
        [Tooltip("Beats Per Minute")]
        public float BPM = 120f;

        [Tooltip("Beats per Measure (e.g. 4 for 4/4)")]
        public int Signature = 4;

        [Tooltip("Offset in seconds to the first beat")]
        public float Offset = 0f;
    }
}
