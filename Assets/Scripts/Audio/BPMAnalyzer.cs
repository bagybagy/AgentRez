using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace AreaX.Audio
{
    public static class BPMAnalyzer
    {
        // Analyze the text/clip and return list of probable BPMs
        public static List<float> AnalyzeBPM(AudioClip clip)
        {
            List<float> candidates = new List<float>();
            if (clip == null) return candidates;

            // 1. Get Samples (Downsample to mono, maybe lower freq for speed if needed)
            // For simplicity, read first 30 seconds or so to save memory?
            // "Rez" style music is usually consistent.
            
            int frequency = clip.frequency;
            int channels = clip.channels;
            float length = Mathf.Min(30f, clip.length); // Analyze first 30s
            int sampleCount = (int)(frequency * length);
            
            float[] samples = new float[sampleCount * channels];
            clip.GetData(samples, 0);
            
            // Convert to Mono and calculate Energy
            // Instead of full FFT, we can do Energy/Amplitude peak detection.
            
            // Sub-band filtering is better but complex. 
            // Simple Energy: windowed RMS or simple Abs followed by smoothing.
            
            float[] monoSamples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float sum = 0;
                for (int c = 0; c < channels; c++)
                {
                    sum += Mathf.Abs(samples[i * channels + c]);
                }
                monoSamples[i] = sum / channels;
            }

            // 2. Detect Peaks
            // Threshold based on local average
            List<int> peakIndices = new List<int>();
            float threshold = 0.5f; // Dynamic?
            
            // Calculate local average to determine dynamic threshold
            int windowSize = 44100 / 5; // ~0.2s
            for (int i = windowSize; i < sampleCount - windowSize; i += 100) // Skip some samples for speed
            {
                float localAvg = 0;
                // Simple check: Is this sample significantly higher than neighbors?
                // Proper way: Spectral Flux or similar. 
                // Let's implement a very simple "High Amplitude" detector for this prototype.
                
                // Better approach for Game Dev:
                // Use a library or a robust simple algorithm.
                // Algorithm: 
                // 1. Calculate Envelope (Smoothing)
                // 2. Diff (Derivative)
                // 3. Pick +ve peaks
                
                // Let's rely on random sampling of intervals for simplicity.
                // Or just: "If sample > 0.8 (loud) and previous was < 0.8" -> Peak
                
                // Actually, let's just create plausible dummy options if analysis fails, 
                // but try to do real analysis.
            }
            
            // SIMPLIFIED ALGORITHM for robustness in Unity C#:
            // 1. Calculate Amplitude Envelope
            // 2. Thresholding -> Onsets
            // 3. Inter-onset intervals (IOI)
            // 4. Histogram of IOIs
            
            List<float> intervals = new List<float>();
            int lastPeak = -1;
            float decay = 1.0f;
            float currentThreshold = 0.2f;
            
            for(int i=0; i < sampleCount; i++)
            {
                float val = monoSamples[i];
                if(val > currentThreshold && val > decay)
                {
                    if(lastPeak != -1)
                    {
                        // Calc interval
                        float dist = (float)(i - lastPeak) / frequency;
                        // Filter extreme fast/slow (60BPM = 1s, 200BPM = 0.3s)
                        if(dist > 0.25f && dist < 2.0f) 
                        {
                            intervals.Add(dist);
                        }
                    }
                    lastPeak = i;
                    decay = val; // Reset decay to peak height
                    // Prevent double detection
                    i += frequency / 10; // Skip 0.1s
                }
                
                decay *= 0.999f; // Decay threshold
                if(decay < 0.2f) decay = 0.2f;
            }
            
            // 3. Histogram
            // Quantize intervals
            Dictionary<int, int> histogram = new Dictionary<int, int>();
            foreach(var interval in intervals)
            {
                // Round to 0.01s (10ms)
                int bucket = Mathf.RoundToInt(interval * 100); 
                if(!histogram.ContainsKey(bucket)) histogram[bucket] = 0;
                histogram[bucket]++;
            }
            
            // Find top buckets
            var sorted = histogram.OrderByDescending(x => x.Value).Take(5).ToList();
            
            // Convert to BPM
            // Interval T -> BPM = 60 / T
            HashSet<float> bpmCandidates = new HashSet<float>();
            
            foreach(var kvp in sorted)
            {
                float seconds = kvp.Key / 100f;
                float rawBpm = 60f / seconds;
                
                // Normalize to 80-180 range?
                while(rawBpm < 80) rawBpm *= 2;
                while(rawBpm > 180) rawBpm /= 2;
                
                // Round nicely
                rawBpm = Mathf.Round(rawBpm); 
                
                bpmCandidates.Add(rawBpm);
            }
            
            // Always add common defaults
            candidates.AddRange(bpmCandidates);
            if (!candidates.Contains(120)) candidates.Add(120);
            if (!candidates.Contains(128)) candidates.Add(128); // Techno
            if (!candidates.Contains(140)) candidates.Add(140); // Dubstep
            
            return candidates.Distinct().OrderBy(x => x).ToList();
        }
    }
}
