using UnityEngine;
using System.Collections.Generic;
using AreaX.Player;
using AreaX.Targets;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AreaX.Combat
{
    public class ProjectileManager : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private LockOnSystem _lockOnSystem;
        [SerializeField] private ImpactScheduler _scheduler;
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField] private Transform _firePoint;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
        private InputAction _attackAction;
#endif
        private bool _wasPressed = false;

        private void Start()
        {
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>(); // Assuming attached to Player
            if (_playerInput == null) _playerInput = FindFirstObjectByType<PlayerInput>();
            
            if (_playerInput != null)
            {
                _attackAction = _playerInput.actions["Attack"];
            }
#endif
        }

        private void Update()
        {
            // Input Handling (Release detection)
            bool isPressed = false;
#if ENABLE_INPUT_SYSTEM
            if (_attackAction != null) isPressed = _attackAction.IsPressed();
            else if (Mouse.current != null) isPressed = Mouse.current.leftButton.isPressed;
#else
            isPressed = Input.GetButton("Fire1");
#endif

            // Detect Release (Pressed -> Not Pressed)
            if (_wasPressed && !isPressed)
            {
                Fire();
            }
            _wasPressed = isPressed;
        }

        private void Fire()
        {
            if (_lockOnSystem == null || _scheduler == null || _projectilePrefab == null) return;

            var targets = _lockOnSystem.LockedTargets;
            if (targets.Count == 0) return;

            // Schedule
            var schedule = _scheduler.ScheduleImpacts(targets);

            // Spawn Projectiles
            foreach (var item in schedule)
            {
                SpawnProjectile(item.Target, item.ImpactTime);
            }

            // Clear Locks
            _lockOnSystem.ClearLocks();
        }

        private void SpawnProjectile(Target target, double impactTime)
        {
            Vector3 spawnPos = _firePoint != null ? _firePoint.position : transform.position;
            GameObject obj = Instantiate(_projectilePrefab, spawnPos, Quaternion.identity);
            Projectile proj = obj.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(target, impactTime);
            }
        }
    }
}
