using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using EtherDomes.Input;
using UnityInput = UnityEngine.Input; // Alias to avoid conflict with EtherDomes.Input

namespace EtherDomes.Player
{
    /// <summary>
    /// Networked player controller with WoW-style movement:
    /// - W/S: Move forward/backward
    /// - A/D: Rotate player (turn left/right)
    /// - Q/E: Strafe (sidestep left/right)
    /// - Space: Jump
    /// - Left-click hold: Rotate camera only (free look)
    /// - Right-click hold: Rotate camera AND player direction, A/D become strafe
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : NetworkBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 7f;
        [SerializeField] private float _turnSpeed = 150f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _jumpHeight = 1.5f;

        private CharacterController _characterController;
        private EtherDomesInput _inputActions;
        
        private Vector2 _moveInput;
        private float _strafeInput;
        private Vector3 _velocity;
        private float _cameraYaw;
        private bool _isRightMouseHeld;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputActions = new EtherDomesInput();
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            Debug.Log($"[PlayerController] OnNetworkSpawn - IsOwner: {IsOwner}, IsHost: {IsHost}, IsClient: {IsClient}, OwnerClientId: {OwnerClientId}, LocalClientId: {NetworkManager.Singleton?.LocalClientId}");
            
            if (IsOwner)
            {
                _inputActions.Enable();
                _cameraYaw = transform.eulerAngles.y;
                
                if (_characterController != null)
                {
                    _characterController.enabled = true;
                    Debug.Log($"[PlayerController] Owner - CharacterController ENABLED");
                }
                else
                {
                    Debug.LogError($"[PlayerController] Owner - CharacterController is NULL!");
                }
            }
            else
            {
                if (_characterController != null)
                {
                    _characterController.enabled = false;
                    Debug.Log($"[PlayerController] Non-owner - CharacterController disabled");
                }
            }
        }
        
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsOwner)
                _inputActions.Disable();
        }

        private void Update()
        {
            if (!IsOwner) return;
            
            ReadInput();
            HandleCameraRotation();
            HandleMovement();
            
            // Debug: Log input every few seconds
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"[PlayerController] Update running - MoveInput: {_moveInput}, CharController enabled: {_characterController?.enabled}");
            }
        }

        private void ReadInput()
        {
            _moveInput = Vector2.zero;
            _strafeInput = 0f;
            
            // New Input System
            if (_inputActions != null && _inputActions.Player.enabled)
            {
                _moveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            }
            
            // Fallback to legacy Input
            if (_moveInput == Vector2.zero)
            {
                _moveInput.x = UnityInput.GetAxis("Horizontal");
                _moveInput.y = UnityInput.GetAxis("Vertical");
            }
            
            // Q/E strafe
            if (UnityInput.GetKey(KeyCode.Q)) _strafeInput -= 1f;
            if (UnityInput.GetKey(KeyCode.E)) _strafeInput += 1f;
            
            // Mouse buttons
            _isRightMouseHeld = UnityInput.GetMouseButton(1);
        }

        private void HandleCameraRotation()
        {
            if (!_isRightMouseHeld) return;
            
            var mainCam = Camera.main;
            if (mainCam != null)
            {
                _cameraYaw = mainCam.transform.eulerAngles.y;
                transform.rotation = Quaternion.Euler(0, _cameraYaw, 0);
            }
        }

        private void HandleMovement()
        {
            if (_characterController == null || !_characterController.enabled) return;
            
            // Gravity & Jump
            if (_characterController.isGrounded)
            {
                _velocity.y = -2f;
                if (UnityInput.GetKeyDown(KeyCode.Space))
                    _velocity.y = Mathf.Sqrt(_jumpHeight * -2f * _gravity);
            }
            _velocity.y += _gravity * Time.deltaTime;

            // A/D: rotate when no right mouse, strafe when right mouse held
            if (!_isRightMouseHeld && Mathf.Abs(_moveInput.x) > 0.1f)
            {
                transform.Rotate(0, _moveInput.x * _turnSpeed * Time.deltaTime, 0);
            }

            // Movement direction
            Vector3 moveDirection = Vector3.zero;
            
            // W/S forward/back
            if (Mathf.Abs(_moveInput.y) > 0.01f)
                moveDirection += transform.forward * _moveInput.y;
            
            // Strafe: Q/E always, A/D when right mouse held
            float totalStrafe = _strafeInput;
            if (_isRightMouseHeld)
                totalStrafe += _moveInput.x;
            
            if (Mathf.Abs(totalStrafe) > 0.01f)
                moveDirection += transform.right * Mathf.Clamp(totalStrafe, -1f, 1f);
            
            if (moveDirection.sqrMagnitude > 1f)
                moveDirection.Normalize();

            Vector3 finalMove = moveDirection * _moveSpeed + Vector3.up * _velocity.y;
            _characterController.Move(finalMove * Time.deltaTime);
        }

        public void Teleport(Vector3 position)
        {
            if (!IsOwner) return;
            _characterController.enabled = false;
            transform.position = position;
            _characterController.enabled = true;
        }

        public bool IsGrounded => _characterController != null && _characterController.isGrounded;
        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0, value);
        }
    }
}
