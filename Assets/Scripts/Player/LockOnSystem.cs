using UnityEngine;
using System.Collections.Generic;
using AreaX.Targets;

namespace AreaX.Player
{
    public class LockOnSystem : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _lockDistance = 200f;
        [SerializeField] private float _lockRadius = 1.0f; // SphereCast radius
        [SerializeField] private AudioSource _lockAudioSource;
        [SerializeField] private AudioClip _lockSound;

        private List<Target> _lockedTargets = new List<Target>();
        private Camera _mainCamera;

        public IReadOnlyList<Target> LockedTargets => _lockedTargets;

        private void Start()
        {
            _mainCamera = Camera.main;
            if (_lockAudioSource == null)
            {
                _lockAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Update()
        {
            PerformLockOnRaycast();
        }

        private void PerformLockOnRaycast()
        {
            if (_mainCamera == null) return;

            Ray ray = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            // SphereCast for easier aiming
            if (Physics.SphereCast(ray, _lockRadius, out RaycastHit hit, _lockDistance))
            {
                Target target = hit.collider.GetComponentInParent<Target>();
                if (target != null)
                {
                    TryLock(target);
                }
            }
        }

        private void TryLock(Target target)
        {
            if (target.State != TargetState.Idle) return; // Already locked or processed
            if (!target.IsLockable) return;
            if (_lockedTargets.Contains(target)) return;

            // Register Lock
            _lockedTargets.Add(target);
            target.OnLockOn();

            // Play Sound
            PlayLockSound();
        }

        private void PlayLockSound()
        {
            if (_lockAudioSource != null && _lockSound != null)
            {
                _lockAudioSource.PlayOneShot(_lockSound);
            }
        }

        public void ClearLocks()
        {
             // Called after firing
            _lockedTargets.Clear();
        }
        
        // When firing is cancelled or cleared without valid hit logic (rare in this game type)
        // But for Rez style, we only clear on Fire.
    }
}
