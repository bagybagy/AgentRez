using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace AreaX.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _baseSpeed = 10f;
        [SerializeField] private float _boostMultiplier = 2.0f;
        [SerializeField] private float _brakeMultiplier = 0.5f;

        [Header("Look Settings")]
        [SerializeField] private float _lookSensitivity = 0.5f;
        [SerializeField] private float _smoothTime = 0.05f;

        private Vector2 _currentLookInput;
        private Vector2 _currentLookVelocity; // For SmoothDamp if we implemented custom smoothing, 
                                              // but Vector2.SmoothDamp is easier on input value.
        private Vector2 _smoothedLookInput;
        private Vector2 _smoothDampVelocity;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
        private InputAction _moveAction;
        private InputAction _lookAction;
#endif

        private void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
        }
        
        private void Start()
        {
#if ENABLE_INPUT_SYSTEM
            if (_playerInput != null) {
                // Try to find actions by name as defined in the input actions file
                _moveAction = _playerInput.actions["Move"];
                _lookAction = _playerInput.actions["Look"];
            }
#endif
            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            // Init rotation
            Vector3 angles = transform.localEulerAngles;
            _currentYaw = angles.y;
            _currentPitch = angles.x;
            if (_currentPitch > 180) _currentPitch -= 360f;
        }

        private void Update()
        {
            HandleMovement();
            HandleLook();
        }

        private void HandleMovement()
        {
            float speedMultiplier = 1.0f;
            
            // Input
            Vector2 moveInput = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (_moveAction != null) moveInput = _moveAction.ReadValue<Vector2>();
            else if (Keyboard.current != null) // Fallback if PlayerInput not set but InputSystem enabled
            {
                 // Minimal fallback for debugging without setup
                 if (Keyboard.current.wKey.isPressed) moveInput.y = 1;
                 else if (Keyboard.current.sKey.isPressed) moveInput.y = -1;
            }
#else
            moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif

            if (moveInput.y > 0.1f) speedMultiplier = _boostMultiplier;
            else if (moveInput.y < -0.1f) speedMultiplier = _brakeMultiplier;

            float targetSpeed = _baseSpeed * speedMultiplier;
            // Simple lerp for speed change
            _currentSpeed = Mathf.Lerp(_currentSpeed, targetSpeed, Time.deltaTime * 5f);
            
            transform.Translate(Vector3.forward * _currentSpeed * Time.deltaTime);
        }
        
        // Exposed for UI or other systems
        public float GetCurrentSpeed() => _currentSpeed;

        private void HandleLook()
        {
            Vector2 lookInput = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
            if (_lookAction != null) lookInput = _lookAction.ReadValue<Vector2>();
            else if (Mouse.current != null)
            {
                lookInput = Mouse.current.delta.ReadValue();
            }
#else
            lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
            
            // Apply smoothing to the input
            _smoothedLookInput = Vector2.SmoothDamp(_smoothedLookInput, lookInput, ref _smoothDampVelocity, _smoothTime);
            
            // Apply rotation
            float xRot = _smoothedLookInput.x * _lookSensitivity;
            float yRot = _smoothedLookInput.y * _lookSensitivity;
            
            // Yaw (Horizontal) - simply rotate the body around World Up
            transform.Rotate(Vector3.up, xRot, Space.World);
            
            // Pitch (Vertical) - Needs clamping to prevent flipping
            _currentPitch -= yRot;
            _currentPitch = Mathf.Clamp(_currentPitch, -70f, 70f);
            
            // Apply pitch to local X axis (Resetting logic required if we just Rotate, so better to control localRotation)
            // But since we are rotating the whole object (Camera is likely the Player object itself as per UserTask instructions),
            // We need to be careful not to mess up Yaw.
            
            // Approach: Get current Yaw, apply new Pitch.
            // transform.localEulerAngles is tricky with invalid angles (>180).
            
            // Robust Approach:
            // Store rotations? 
            // Since we are PlayerController on the camera object:
            // transform.localRotation = Quaternion.Euler(_currentPitch, transform.localEulerAngles.y, 0); 
            // Wait, if we rotated Y via Rotate(Space.World), local Y might change?
            // If parent is null (root), localY == worldY.
            
            // Let's rely on accumulation for Yaw, but overwrite Pitch.
            
            Quaternion currentRot = transform.localRotation;
            Quaternion yawRot = Quaternion.AngleAxis(xRot, Vector3.up) * Quaternion.Euler(0, currentRot.eulerAngles.y, 0); 
            // Wait, Rotate(Vector3.up, xRot, Space.World) is effective.
            
            // Better: 
            // 1. Rotate Yaw (as before)
            // 2. Set Pitch directly
            
            //transform.Rotate(Vector3.up, xRot, Space.World); 
            // ^ This might introduce some Roll if Pitch is non-zero? No, Space.World UP is safe.
            
            // Re-implementation:
            _currentYaw += xRot;
            transform.localRotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
        }
        
        // State for clamping
        private float _currentPitch = 0f;
        private float _currentYaw = 0f;
        private float _currentSpeed = 0f;
    }
}
