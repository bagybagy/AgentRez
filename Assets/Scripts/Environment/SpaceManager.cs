using UnityEngine;
using UnityEngine.VFX;
using AreaX.Managers;

namespace AreaX.Environment
{
    public class SpaceManager : MonoBehaviour
    {
        [SerializeField] private VisualEffect _backgroundVFX;
        
        [Header("VFX Parameters")]
        [SerializeField] private string _beatEventName = "OnBeat";
        [SerializeField] private string _measureEventName = "OnMeasure";
        
        private void Start()
        {
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.OnBeat.AddListener(HandleBeat);
                BeatManager.Instance.OnMeasure.AddListener(HandleMeasure);
            }
        }
        
        private void OnDestroy()
        {
            if (BeatManager.Instance != null)
            {
                BeatManager.Instance.OnBeat.RemoveListener(HandleBeat);
                BeatManager.Instance.OnMeasure.RemoveListener(HandleMeasure);
            }
        }
        
        private void HandleBeat(Events.BeatEvent evt)
        {
            // Send Event to VFX Graph
            if (_backgroundVFX != null)
            {
                _backgroundVFX.SendEvent(_beatEventName);
            }
        }

        private void HandleMeasure(Events.MeasureEvent evt)
        {
            if (_backgroundVFX != null)
            {
                 _backgroundVFX.SendEvent(_measureEventName);
            }
        }
    }
}
