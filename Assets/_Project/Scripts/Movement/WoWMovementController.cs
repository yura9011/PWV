using Unity.Netcode;
using UnityEngine;

namespace EtherDomes.Movement
{
    /// <summary>
    /// WoW-style hybrid movement controller.
    /// Mouse Free Mode: A/D turn, cursor visible
    /// Mouse Locked Mode: A/D strafe, cursor locked, camera rotates character
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(NetworkObject))]
    public class WoWMovementController : NetworkBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float _moveSpeed = 7f;
        [SerializeField] private float _turnSpeed = 180f;
        [SerializeField] private float _strafeSpeed = 5f;
        
        [Header("Jump Settings")]
        [SerializeField] private float _jumpForce = 8f;
        [SerializeField] private float _gravity = 20f;
        
        [Header("References")]
        [SerializeField] private Transform _cameraTransform;
        
        // Components
        private CharacterController _controller;
        
        // State
        private Vector3 _velocity;
        private Vector3 _airborneHorizontalVelocity;
        private bool _isMouseLocked;
        private bool _wasGrounded;
        
        // Input cache
        private Vector2 _moveInput;
        private float _strafeInput; // Q/E strafe
        private bool _jumpInput;
        private bool _rightMouseHeld;

        // Properties
        public bool IsGrounded => _controller != null && _controller.isGrounded;
        public bool IsMouseLockedMode => _isMouseLocked;
        public Vector3 Velocity => _velocity;
        public float MoveSpeed => _moveSpeed;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                // [FIX] Force Enable
                enabled = true;
                
                // Find main camera
                if (_cameraTransform == null)
                {
                    var mainCam = Camera.main;
                    if (mainCam != null)
                        _cameraTransform = mainCam.transform;
                }
                
                // Start with cursor visible
                SetCursorState(false);
                
                Debug.Log($"[WoWMovement] Local player started. Enabled: {enabled}, GameObject Active: {gameObject.activeSelf}");
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                SetCursorState(false);
            }
        }

        private void OnEnable()
        {
            Debug.Log($"[WoWMovement] ENABLED. Owner: {(NetworkObject != null ? IsOwner : "null")}");
        }

        private void OnDisable()
        {
            Debug.Log($"[WoWMovement] DISABLED! StackTrace: {System.Environment.StackTrace}");
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            ReadInput();
            UpdateMouseMode();
        }

        private void FixedUpdate()
        {
            if (!IsOwner) return;
            
            ProcessMovement();
        }

        private void OnGUI()
        {
            if (!IsOwner) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200), "WoW Movement Debug", GUI.skin.window);
            GUILayout.Label($"Looking for Input...");
            GUILayout.Label($"Enabled: {enabled}");
            GUILayout.Label($"Active: {gameObject.activeSelf}");
            GUILayout.Label($"Time.timeScale: {Time.timeScale}");
            GUILayout.Label($"IsOwner: {IsOwner}");
            GUILayout.Label($"Input: {_moveInput}");
            GUILayout.Label($"Mouse Locked: {_isMouseLocked}");
            
            if (GUILayout.Button("FORCE UNLOCK CURSOR"))
            {
                 Cursor.lockState = CursorLockMode.None;
                 Cursor.visible = true;
            }
            GUILayout.EndArea();
        }

        private void ReadInput()
        {
            _moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
            
            // Q/E strafe input
            _strafeInput = 0f;
            if (Input.GetKey(KeyCode.Q)) _strafeInput = -1f;
            if (Input.GetKey(KeyCode.E)) _strafeInput = 1f;
            
            // Jump input - capture and hold until processed
            if (Input.GetKeyDown(KeyCode.Space))
            {
                _jumpInput = true;
                Debug.Log("[WoWMovement] Jump input detected");
            }
            
            _rightMouseHeld = Input.GetMouseButton(1);
            
            // [DEBUG] Check if we have any input
            if (_moveInput.sqrMagnitude > 0 || _rightMouseHeld)
            {
                 // Trace log only when moving
                 // Debug.Log($"[WoWMovement] Input: {_moveInput}, Mouse: {_rightMouseHeld}, Locked: {_isMouseLocked}");
            }
            if (Input.GetKeyDown(KeyCode.W)) Debug.Log("[WoWMovement] W Key Pressed");
            else if (Input.GetKeyDown(KeyCode.S)) Debug.Log("[WoWMovement] S Key Pressed");
        }

        private void UpdateMouseMode()
        {
            if (_rightMouseHeld != _isMouseLocked)
            {
                _isMouseLocked = _rightMouseHeld;
                SetCursorState(_isMouseLocked);
            }
        }

        private void SetCursorState(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private void ProcessMovement()
        {
            bool grounded = IsGrounded;
            
            // Handle landing
            if (grounded && !_wasGrounded)
            {
                OnLanded();
            }
            
            // Handle takeoff
            if (!grounded && _wasGrounded)
            {
                OnBecameAirborne();
            }
            
            _wasGrounded = grounded;
            
            // Calculate movement
            if (grounded)
            {
                ProcessGroundedMovement();
            }
            else
            {
                ProcessAirborneMovement();
            }
            
            // Apply gravity
            if (!grounded)
            {
                _velocity.y -= _gravity * Time.fixedDeltaTime;
            }
            else if (_velocity.y < 0)
            {
                _velocity.y = -2f; // Small downward force to stay grounded
            }
            
            // Handle jump
            if (_jumpInput && grounded)
            {
                _velocity.y = _jumpForce;
                Debug.Log($"[WoWMovement] Jumping! Force: {_jumpForce}");
            }
            _jumpInput = false; // Always reset after checking
            
            // Apply movement
            _controller.Move(_velocity * Time.fixedDeltaTime);
            
            // Note: Synchronization is handled by NetworkTransform (ClientNetworkTransform)
        }

        private void ProcessGroundedMovement()
        {
            Vector3 horizontalVelocity = Vector3.zero;
            
            if (_isMouseLocked)
            {
                // Mouse Locked Mode: Camera controls character rotation, A/D strafe
                ProcessMouseLockedMovement(ref horizontalVelocity);
            }
            else
            {
                // Mouse Free Mode: A/D turn character
                ProcessMouseFreeMovement(ref horizontalVelocity);
            }
            
            _velocity.x = horizontalVelocity.x;
            _velocity.z = horizontalVelocity.z;
        }

        private void ProcessMouseFreeMovement(ref Vector3 horizontalVelocity)
        {
            // A/D = Turn character
            if (Mathf.Abs(_moveInput.x) > 0.1f)
            {
                float turnAmount = _moveInput.x * _turnSpeed * Time.fixedDeltaTime;
                transform.Rotate(0, turnAmount, 0);
            }
            
            // W/S = Move forward/backward relative to character facing
            if (Mathf.Abs(_moveInput.y) > 0.1f)
            {
                horizontalVelocity = transform.forward * _moveInput.y * _moveSpeed;
            }
            
            // Q/E = Strafe left/right
            if (Mathf.Abs(_strafeInput) > 0.1f)
            {
                horizontalVelocity += transform.right * _strafeInput * _strafeSpeed;
            }
        }

        private void ProcessMouseLockedMovement(ref Vector3 horizontalVelocity)
        {
            // Sync character rotation with camera Y rotation
            if (_cameraTransform != null)
            {
                float cameraY = _cameraTransform.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0, cameraY, 0);
            }
            
            // W/S = Forward/backward relative to character
            // A/D = Strafe left/right
            Vector3 forward = transform.forward * _moveInput.y;
            Vector3 strafe = transform.right * _moveInput.x;
            
            // Q/E = Additional strafe
            strafe += transform.right * _strafeInput;
            
            horizontalVelocity = (forward + strafe).normalized;
            
            if (horizontalVelocity.sqrMagnitude > 0.01f)
            {
                // Use strafe speed for lateral, move speed for forward
                float speed = Mathf.Abs(_moveInput.y) > Mathf.Abs(_moveInput.x + _strafeInput) ? _moveSpeed : _strafeSpeed;
                horizontalVelocity *= speed;
            }
        }

        private void ProcessAirborneMovement()
        {
            // Preserve horizontal velocity from takeoff (no air control)
            _velocity.x = _airborneHorizontalVelocity.x;
            _velocity.z = _airborneHorizontalVelocity.z;
        }

        private void OnBecameAirborne()
        {
            // Store horizontal velocity at takeoff
            _airborneHorizontalVelocity = new Vector3(_velocity.x, 0, _velocity.z);
            Debug.Log($"[WoWMovement] Airborne with velocity: {_airborneHorizontalVelocity}");
        }

        private void OnLanded()
        {
            _airborneHorizontalVelocity = Vector3.zero;
            Debug.Log("[WoWMovement] Landed");
        }

        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        public void SetMoveSpeed(float speed)
        {
            _moveSpeed = Mathf.Max(0, speed);
        }
    }
}
