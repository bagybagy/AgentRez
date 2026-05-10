using UnityEngine;
using UnityEngine.VFX;

namespace AreaX.Targets
{
    public enum TargetState
    {
        Idle,
        Locked,
        Processed
    }

    public class Target : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private VisualEffect _targetVFX;
        [SerializeField] private Collider _collider;
        
        public TargetState State { get; private set; } = TargetState.Idle;

        private void Start()
        {
            if (AreaX.Managers.StageManager.Instance != null)
            {
                AreaX.Managers.StageManager.Instance.RegisterTarget(this);
            }
        }

        public void OnLockOn()
        {
            if (State == TargetState.Idle)
            {
                State = TargetState.Locked;
                // Visual feedback for lock-on will be handled here or by LockOnSystem
                // e.g., show a marker or change emission color
            }
        }

        public void OnLockedCancelled()
        {
            if (State == TargetState.Locked)
            {
                State = TargetState.Idle;
            }
        }

        public void OnHit()
        {
            if (State == TargetState.Processed) return;

            State = TargetState.Processed;
            // Disable collider so it can't be locked again
            if (_collider != null) _collider.enabled = false;

            // Trigger destruction VFX
            if (_targetVFX != null)
            {
                _targetVFX.SendEvent("OnHit"); // Assumes VFX Graph has OnHit event
            }
            else
            {
                // Fallback if no VFX
                gameObject.SetActive(false);
            }
        }
    }
}
