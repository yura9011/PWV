using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using EtherDomes.Input;

namespace EtherDomes.Player
{
    /// <summary>
    /// Networked player controller handling movement and input.
    /// Only processes input for the local owner.
    /// Uses Unity Input System for input handling.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _rotationSpeed = 720f;
        [SerializeField] private float _gravity = -9.81f;

        [Header("Camera")]
        [SerializeField] private Transform _cameraTransform;

        private CharacterController _characterController;
        private Vector3 _velocity;
        private Vector3 _moveDirection;
        
        // Input System
        private EtherDomesInput _inputActions;
        private Vector2 _moveInput;

        // Network synced position for smoothing
        private NetworkVariable<Vector3> _networkPosition = new NetworkVariable<Vector3>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private NetworkVariable<Quaternion> _networkRotation = new NetworkVariable<Quaternion>(
            Quaternion.identity,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public Vector3 CurrentVelocity => _velocity;
        public bool IsMoving => _moveDirection.sqrMagnitude > 0.01f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputActions = new EtherDomesInput();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                // Find or create camera reference
                if (_cameraTransform == null)
                {
                    _cameraTransform = Camera.main?.transform;
                }
                
                // Enable input actions for owner
                EnableInput();
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                DisableInput();
            }
            base.OnNetworkDespawn();
        }

        protected new void OnDestroy()
        {
            base.OnDestroy();
            _inputActions?.Dispose();
        }

        private void EnableInput()
        {
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Enable();
        }

        private void DisableInput()
        {
            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Disable();
        }

        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            _moveInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            _moveInput = Vector2.zero;
        }

        private void Update()
        {
            // Only process input for owner
            if (!IsOwner)
            {
                // Interpolate to network position for non-owners
                InterpolateToNetworkPosition();
                return;
            }

            ProcessInput();
            ApplyMovement();
            UpdateNetworkState();
        }

        private void ProcessInput()
        {
            // Calculate movement direction relative to camera
            Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

            if (inputDirection.sqrMagnitude > 0.01f && _cameraTransform != null)
            {
                // Get camera forward and right (flattened to horizontal plane)
                Vector3 cameraForward = _cameraTransform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = _cameraTransform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                // Calculate world-space movement direction
                _moveDirection = (cameraForward * _moveInput.y + cameraRight * _moveInput.x).normalized;
            }
            else
            {
                _moveDirection = inputDirection;
            }
        }

        private void ApplyMovement()
        {
            // Apply gravity
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            _velocity.y += _gravity * Time.deltaTime;

            // Calculate horizontal movement
            Vector3 horizontalMove = _moveDirection * _moveSpeed;

            // Combine with vertical velocity
            Vector3 finalMove = new Vector3(horizontalMove.x, _velocity.y, horizontalMove.z);

            // Move character
            _characterController.Move(finalMove * Time.deltaTime);

            // Rotate to face movement direction
            if (_moveDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }

        private void UpdateNetworkState()
        {
            _networkPosition.Value = transform.position;
            _networkRotation.Value = transform.rotation;
        }

        private void InterpolateToNetworkPosition()
        {
            // Smooth interpolation for non-owners
            transform.position = Vector3.Lerp(
                transform.position,
                _networkPosition.Value,
                Time.deltaTime * 10f
            );

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                _networkRotation.Value,
                Time.deltaTime * 10f
            );
        }

        /// <summary>
        /// Set the camera transform for relative movement.
        /// </summary>
        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        /// <summary>
        /// Teleport the player to a position.
        /// </summary>
        public void Teleport(Vector3 position)
        {
            if (!IsOwner) return;

            _characterController.enabled = false;
            transform.position = position;
            _characterController.enabled = true;

            _networkPosition.Value = position;
        }

        /// <summary>
        /// Stop all movement (for death, stun, etc.).
        /// </summary>
        public void StopMovement()
        {
            _moveDirection = Vector3.zero;
            _moveInput = Vector2.zero;
            _velocity = new Vector3(0, _velocity.y, 0);
        }

        /// <summary>
        /// Check if the player is grounded.
        /// </summary>
        public bool IsGrounded => _characterController.isGrounded;

        /// <summary>
        /// Get the current move speed.
        /// </summary>
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0, value);
        }
        
        /// <summary>
        /// Get the input actions for external systems to subscribe to.
        /// </summary>
        public EtherDomesInput InputActions => _inputActions;
    }
}
