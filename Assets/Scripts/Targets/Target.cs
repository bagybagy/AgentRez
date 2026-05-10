using UnityEngine;
using UnityEngine.VFX;
using System;

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
        
        public event Action<Target> Locked;
        public event Action<Target> Hit;

        public TargetState State { get; private set; } = TargetState.Idle;

        private void Start()
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
            }

            if (AreaX.Managers.StageManager.Instance != null)
            {
                AreaX.Managers.StageManager.Instance.RegisterTarget(this);
            }
        }

        public virtual void OnLockOn()
        {
            if (State == TargetState.Idle)
            {
                State = TargetState.Locked;
                Locked?.Invoke(this);
                // Visual feedback for lock-on will be handled here or by LockOnSystem
                // e.g., show a marker or change emission color
            }
        }

        public virtual void OnLockedCancelled()
        {
            if (State == TargetState.Locked)
            {
                State = TargetState.Idle;
            }
        }

        public virtual void OnHit()
        {
            if (State == TargetState.Processed) return;

            State = TargetState.Processed;
            Hit?.Invoke(this);

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
