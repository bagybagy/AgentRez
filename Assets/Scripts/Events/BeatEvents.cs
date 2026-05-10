using UnityEngine;

namespace AreaX.Events
{
    [System.Serializable]
    public struct BeatEvent
    {
        public double BeatTime;
        public double BeatDuration;
        public double NextBeatTime;
        public int BeatIndex;
    }

    [System.Serializable]
    public struct MeasureEvent
    {
        public double MeasureTime;
        public int MeasureIndex;
    }
}
