using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using AreaX.Audio;
using System.Linq;
using TMPro; // Add TMP Namespace

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AreaX.Tools
{
    [RequireComponent(typeof(AudioSource))]
    public class BPMCalibrationTool : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private InputField _clipNameInput; 
        [SerializeField] private AudioSource _previewSource;
        [SerializeField] private Text _statusText;
        [SerializeField] private TMP_Dropdown _bpmDropdown; 
        [SerializeField] private Button _analyzeButton;
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _playButton;
        
        [Header("Target")]
        [SerializeField] private AudioClip _targetClip;
        
        private List<float> _candidates = new List<float>();
        private float _currentBPM = 120f;
        private bool _isPlaying = false;
        private double _nextBeatTime = 0;
        
        [SerializeField] private Image _metronomeVisual;

        private void Awake()
        {
            if (_previewSource == null) _previewSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            if (_analyzeButton) _analyzeButton.onClick.AddListener(OnAnalyze);
            if (_saveButton) _saveButton.onClick.AddListener(OnSave);
            if (_playButton) _playButton.onClick.AddListener(MockPlay);
            
            if (_bpmDropdown) _bpmDropdown.onValueChanged.AddListener(OnDropdownChanged);
            
            UpdateUI();
        }

        private void Update()
        {
            if (_isPlaying && _previewSource != null && _previewSource.isPlaying && _targetClip != null)
            {
                // Metronome Logic
                if (_targetClip.frequency <= 0) return;
                
                double time = (double)_previewSource.timeSamples / _targetClip.frequency;
                if (time >= _nextBeatTime)
                {
                    // Flash
                    if (_metronomeVisual) _metronomeVisual.color = Color.white;
                    // Schedule next
                    double spb = 60d / _currentBPM;
                    _nextBeatTime += spb;
                }
                else
                {
                    // Fade
                    if (_metronomeVisual) _metronomeVisual.color = Color.Lerp(_metronomeVisual.color, Color.black, Time.deltaTime * 10f);
                }
                
                // Debug info
                if (_statusText) _statusText.text = $"Playing: {time:F2}s / BPM: {_currentBPM} / Next: {_nextBeatTime:F2}";
            }
            else if (_isPlaying)
            {
                // IsPlaying is true but source stopped?
                 if (_statusText) _statusText.text = "Stopped (Source is not playing)";
                 _isPlaying = false;
                 SetButtonText(_playButton, "Preview");
            }
        }
        
        private void OnAnalyze()
        {
            if (_targetClip == null)
            {
                _statusText.text = "No Clip Assigned!";
                return;
            }
            
            _statusText.text = "Analyzing...";
            _candidates = BPMAnalyzer.AnalyzeBPM(_targetClip);
            
            // Populate Dropdown
            _bpmDropdown.ClearOptions();
            _bpmDropdown.AddOptions(_candidates.Select(c => c.ToString()).ToList());
            
            if (_candidates.Count > 0)
            {
                _currentBPM = _candidates[0];
                _statusText.text = $"Analyzed. Top candidate: {_currentBPM}";
            }
            else
            {
                _statusText.text = "Analysis failed. Using default.";
            }
        }

        private void OnDropdownChanged(int index)
        {
             if (index >= 0 && index < _candidates.Count)
             {
                 _currentBPM = _candidates[index];
                 _statusText.text = $"Selected: {_currentBPM}";
             }
        }

        private void MockPlay()
        {
            if (_isPlaying)
            {
                _previewSource.Stop();
                _isPlaying = false;
                SetButtonText(_playButton, "Preview");
            }
            else
            {
                if(_targetClip == null || _previewSource == null) return;
                
                _previewSource.clip = _targetClip;
                _previewSource.Play();
                _isPlaying = true;
                _nextBeatTime = 0;
                SetButtonText(_playButton, "Stop");
            }
        }
        
        private void SetButtonText(Button btn, string text)
        {
            if (btn == null) return;
            var tmpText = btn.GetComponentInChildren<TMP_Text>();
            if (tmpText != null)
            {
                tmpText.text = text;
                return;
            }
            var legacyText = btn.GetComponentInChildren<Text>();
            if (legacyText != null) 
            {
                legacyText.text = text;
            }
        }


        private void OnSave()
        {
#if UNITY_EDITOR
            if (_targetClip == null) return;
            
            // Create Asset
            MusicData data = ScriptableObject.CreateInstance<MusicData>();
            data.Clip = _targetClip;
            data.BPM = _currentBPM;
            
            string path = $"Assets/Scripts/Audio/MusicData_{_targetClip.name}.asset";
            AssetDatabase.CreateAsset(data, path);
            AssetDatabase.SaveAssets();
            
            _statusText.text = $"Saved to {path}";
            
            // Auto-assign to MusicManager in scene if present?
            var musicMgr = FindObjectOfType<Managers.MusicManager>();
            // musicMgr is private serialized field, can't easily access without reflection or public method.
            // But user task will handle assignment.
#else
            _statusText.text = "Save only works in Editor";
#endif
        }

        private void UpdateUI()
        {
            if (_targetClip != null) _statusText.text = $"Ready to analyze {_targetClip.name}";
            else _statusText.text = "Assign Audio Clip in Inspector";
        }
    }
}
