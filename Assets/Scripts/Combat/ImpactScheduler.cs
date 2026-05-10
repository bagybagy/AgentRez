using UnityEngine;
using System.Collections.Generic;
using AreaX.Targets;
using AreaX.Managers;

namespace AreaX.Combat
{
    public struct ScheduledImpact
    {
        public Target Target;
        public double ImpactTime;
    }

    public class ImpactScheduler : MonoBehaviour
    {
        public static ImpactScheduler Instance { get; private set; }

        [SerializeField] private float _slotInterval = 0.5f; // In beats

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public List<ScheduledImpact> ScheduleImpacts(IReadOnlyList<Target> targets)
        {
            List<ScheduledImpact> results = new List<ScheduledImpact>();
            if (targets == null || targets.Count == 0 || BeatManager.Instance == null) return results;

            // 1. Determine Base Start Time
            // Rules from spec:
            // - If < 0.5 beat passed since last beat -> Next Beat
            // - If >= 0.5 beat passed -> Next Next Beat
            // Basically, we need enough buffer.

            double currentTime = MusicManager.Instance.GetAudioTime();
            double secondsPerBeat = BeatManager.Instance.GetSecondsPerBeat();
            double beatDuration = secondsPerBeat; // 1 beat duration
            
            // Simple logic: Find next quantized Beat time that is at least X seconds away
            // Specification says:
            // Get last beat time? No, BeatManager fires events but we might need to ask it for "Phase".
            // Let's assume BeatManager aligns beats to (Time % SecondsPerBeat == 0) roughly (for simple calculation),
            // or we track it.
            // Since BeatManager implementation uses "NextBeatTime", let's use that if possible, but 
            // calculating from simple modulus is robust for fixed BPM.
            
            // Let's rely on BeatManager's helper if I added one, or calc manually.
            // I added GetNextQuantizedTime(currentTime, 1.0) to BeatManager.
            
            double nextBeatTime = MusicManager.Instance != null && MusicManager.Instance.Clock != null
                ? MusicManager.Instance.Clock.GetNextQuantizedSongTime(1.0)
                : BeatManager.Instance.GetNextQuantizedTime(currentTime, 1.0);
            
            // Time until next beat
            double timeToNext = nextBeatTime - currentTime;
            
            double startBaseTime;
            double threshold = beatDuration * 0.5;

            // If we exist close to the next beat (less than 0.5 beat away), we might be "too late" to start immediately on it?
            // The spec says:
            // "Current beat start + elapsed < 0.5 beat" -> Next Beat
            // "Current beat start + elapsed >= 0.5 beat" -> Next Next Beat
            // This phrasing implies we are looking at the *current* beat we are in.
            
            // Let's derive "Phase" within current beat.
            // Current Beat Start = nextBeatTime - beatDuration;
            // Elapsed = currentTime - (nextBeatTime - beatDuration);
            // Elapsed = beatDuration - timeToNext;
            
            double elapsedInBeat = beatDuration - timeToNext;
            
            if (elapsedInBeat < threshold) 
            {
                startBaseTime = nextBeatTime;
            }
            else
            {
                startBaseTime = nextBeatTime + beatDuration;
            }

            // 2. Assign Slots
            // Limit hits per beat? Spec says: "Max 2 hits per beat" (section 7.3). 
            // "1拍内で処理できる命中数は最大2とする"
            // "1拍は1/2拍単位に分割される" -> So 2 slots per beat.
            // This aligns perfectly. We fill slots 0.0, 0.5, 1.0, 1.5... from startBaseTime.
            
            double slotDuration = beatDuration * _slotInterval; // 0.5 beat
            
            for (int i = 0; i < targets.Count; i++)
            {
                double impactTime = startBaseTime + (i * slotDuration);
                results.Add(new ScheduledImpact
                {
                    Target = targets[i],
                    ImpactTime = impactTime
                });
            }

            return results;
        }
    }
}
