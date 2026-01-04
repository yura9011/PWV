using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EtherDomes.Player
{
    /// <summary>
    /// Network-aware player controller with 8-direction movement and camera-relative input.
    /// Uses client authority for responsive movement with server synchronization.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(NetworkIdentity))]
    public class NetworkPlayerController : NetworkBehaviour, IPlayerController
    {
        [Header("Movement Settings")]
        [SerializeField] private float _movementSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 10f;

        [Header("References")]
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private InputActionAsset _inputActions;

        private Rigidbody _rigidbody;
        private Vector3 _currentVelocity;
        private Vector2 _lastInput;
        
        // Input System
        private InputAction _moveAction;
        private bool _useNewInputSystem = false;

        // IPlayerController implementation
        public bool IsLocalPlayer => isLocalPlayer;
        public bool IsOwned => isOwned;
        public Vector3 CurrentVelocity => _currentVelocity;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            
            // Setup Input System if available
            if (_inputActions != null)
            {
                _moveAction = _inputActions.FindAction("Player/Move");
                if (_moveAction != null)
                {
                    _useNewInputSystem = true;
                }
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            // Find main camera if not assigned
            if (_cameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    _cameraTransform = mainCamera.transform;
                }
            }
            
            // Enable input actions for local player
            if (_inputActions != null)
            {
                _inputActions.Enable();
            }

            Debug.Log("[NetworkPlayerController] Local player started");
        }

        public override void OnStopLocalPlayer()
        {
            base.OnStopLocalPlayer();
            
            // Disable input actions
            if (_inputActions != null)
            {
                _inputActions.Disable();
            }
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            // Read input from new Input System or legacy
            Vector2 input;
            if (_useNewInputSystem && _moveAction != null)
            {
                input = _moveAction.ReadValue<Vector2>();
            }
            else
            {
                input = new Vector2(
                    UnityEngine.Input.GetAxisRaw("Horizontal"),
                    UnityEngine.Input.GetAxisRaw("Vertical")
                );
            }

            ProcessMovementInput(input);
        }

        private void FixedUpdate()
        {
            if (!isLocalPlayer) return;

            // Apply movement
            if (_currentVelocity.sqrMagnitude > 0.01f)
            {
                _rigidbody.MovePosition(_rigidbody.position + _currentVelocity * Time.fixedDeltaTime);

                // Rotate towards movement direction
                Quaternion targetRotation = Quaternion.LookRotation(_currentVelocity.normalized);
                _rigidbody.rotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            }
        }

        #region IPlayerController Implementation

        public void ProcessMovementInput(Vector2 input)
        {
            // Only process input for local player
            if (!isLocalPlayer)
            {
                _currentVelocity = Vector3.zero;
                return;
            }

            // Normalize diagonal movement
            Vector2 normalizedInput = NormalizeToEightDirections(input);
            _lastInput = normalizedInput;

            if (normalizedInput.sqrMagnitude < 0.01f)
            {
                _currentVelocity = Vector3.zero;
                return;
            }

            // Transform input relative to camera
            Vector3 worldDirection = TransformInputToWorldSpace(normalizedInput);
            _currentVelocity = worldDirection * _movementSpeed;

            // Send to server for synchronization
            if (normalizedInput != _lastInput)
            {
                CmdUpdateMovement(_currentVelocity);
            }
        }

        public void SetMovementSpeed(float speed)
        {
            _movementSpeed = Mathf.Max(0, speed);
        }

        #endregion

        #region Movement Helpers

        /// <summary>
        /// Normalizes input to one of 8 directions or zero.
        /// </summary>
        public static Vector2 NormalizeToEightDirections(Vector2 input)
        {
            if (input.sqrMagnitude < 0.01f)
                return Vector2.zero;

            // Clamp to -1, 0, 1 for each axis
            float x = Mathf.Sign(input.x);
            float y = Mathf.Sign(input.y);

            if (Mathf.Abs(input.x) < 0.1f) x = 0;
            if (Mathf.Abs(input.y) < 0.1f) y = 0;

            Vector2 direction = new Vector2(x, y);

            // Normalize diagonal movement
            if (direction.sqrMagnitude > 1f)
            {
                direction = direction.normalized;
            }

            return direction;
        }

        /// <summary>
        /// Transforms 2D input to world space relative to camera Y rotation.
        /// </summary>
        public Vector3 TransformInputToWorldSpace(Vector2 input)
        {
            if (_cameraTransform == null)
            {
                // No camera, use world space directly
                return new Vector3(input.x, 0, input.y).normalized;
            }

            // Get camera's forward and right vectors (flattened to XZ plane)
            Vector3 cameraForward = _cameraTransform.forward;
            cameraForward.y = 0;
            cameraForward.Normalize();

            Vector3 cameraRight = _cameraTransform.right;
            cameraRight.y = 0;
            cameraRight.Normalize();

            // Transform input relative to camera
            Vector3 worldDirection = (cameraRight * input.x + cameraForward * input.y);
            
            if (worldDirection.sqrMagnitude > 0.01f)
            {
                worldDirection.Normalize();
            }

            return worldDirection;
        }

        /// <summary>
        /// Static version for testing - transforms input by camera Y rotation angle.
        /// </summary>
        public static Vector3 TransformInputByCameraAngle(Vector2 input, float cameraYRotation)
        {
            if (input.sqrMagnitude < 0.01f)
                return Vector3.zero;

            float radians = cameraYRotation * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);

            // Rotate input by camera Y angle
            float rotatedX = input.x * cos + input.y * sin;
            float rotatedZ = -input.x * sin + input.y * cos;

            return new Vector3(rotatedX, 0, rotatedZ).normalized;
        }

        #endregion

        #region Network Commands

        [Command]
        private void CmdUpdateMovement(Vector3 velocity)
        {
            // Server receives movement update
            RpcSyncMovement(velocity);
        }

        [ClientRpc(includeOwner = false)]
        private void RpcSyncMovement(Vector3 velocity)
        {
            // Other clients receive movement update
            _currentVelocity = velocity;
        }

        #endregion

        #region Public API for Testing

        /// <summary>
        /// Sets the Input Action Asset for the new Input System.
        /// </summary>
        public void SetInputActions(InputActionAsset inputActions)
        {
            _inputActions = inputActions;
            if (_inputActions != null)
            {
                _moveAction = _inputActions.FindAction("Player/Move");
                _useNewInputSystem = _moveAction != null;
                
                if (isLocalPlayer)
                {
                    _inputActions.Enable();
                }
            }
        }

        /// <summary>
        /// Sets the camera transform reference.
        /// </summary>
        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        /// <summary>
        /// Gets the current movement speed.
        /// </summary>
        public float GetMovementSpeed() => _movementSpeed;

        #endregion
    }
}
