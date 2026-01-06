using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;
using EtherDomes.Input;

namespace EtherDomes.Player
{
    /// <summary>
    /// Networked player controller handling movement and input.
    /// Uses Mirror networking and Unity Input System.
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
        
        private EtherDomesInput _inputActions;
        private Vector2 _moveInput;

        [SyncVar]
        private Vector3 _networkPosition;

        [SyncVar]
        private Quaternion _networkRotation = Quaternion.identity;

        public Vector3 CurrentVelocity => _velocity;
        public bool IsMoving => _moveDirection.sqrMagnitude > 0.01f;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _inputActions = new EtherDomesInput();
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            if (_cameraTransform == null)
            {
                _cameraTransform = Camera.main?.transform;
            }
            
            EnableInput();
        }

        public override void OnStopLocalPlayer()
        {
            DisableInput();
            base.OnStopLocalPlayer();
        }

        private void OnDestroy()
        {
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
            if (!isLocalPlayer)
            {
                InterpolateToNetworkPosition();
                return;
            }

            ProcessInput();
            ApplyMovement();
            
            if (isServer)
            {
                _networkPosition = transform.position;
                _networkRotation = transform.rotation;
            }
            else
            {
                CmdUpdatePosition(transform.position, transform.rotation);
            }
        }

        [Command]
        private void CmdUpdatePosition(Vector3 position, Quaternion rotation)
        {
            _networkPosition = position;
            _networkRotation = rotation;
        }

        private void ProcessInput()
        {
            Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

            if (inputDirection.sqrMagnitude > 0.01f && _cameraTransform != null)
            {
                Vector3 cameraForward = _cameraTransform.forward;
                cameraForward.y = 0f;
                cameraForward.Normalize();

                Vector3 cameraRight = _cameraTransform.right;
                cameraRight.y = 0f;
                cameraRight.Normalize();

                _moveDirection = (cameraForward * _moveInput.y + cameraRight * _moveInput.x).normalized;
            }
            else
            {
                _moveDirection = inputDirection;
            }
        }

        private void ApplyMovement()
        {
            if (_characterController.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }
            _velocity.y += _gravity * Time.deltaTime;

            Vector3 horizontalMove = _moveDirection * _moveSpeed;
            Vector3 finalMove = new Vector3(horizontalMove.x, _velocity.y, horizontalMove.z);

            _characterController.Move(finalMove * Time.deltaTime);

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

        private void InterpolateToNetworkPosition()
        {
            transform.position = Vector3.Lerp(transform.position, _networkPosition, Time.deltaTime * 10f);
            transform.rotation = Quaternion.Slerp(transform.rotation, _networkRotation, Time.deltaTime * 10f);
        }

        public void SetCameraTransform(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        public void Teleport(Vector3 position)
        {
            if (!isLocalPlayer) return;

            _characterController.enabled = false;
            transform.position = position;
            _characterController.enabled = true;
        }

        public void StopMovement()
        {
            _moveDirection = Vector3.zero;
            _moveInput = Vector2.zero;
            _velocity = new Vector3(0, _velocity.y, 0);
        }

        public bool IsGrounded => _characterController.isGrounded;

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = Mathf.Max(0, value);
        }
        
        public EtherDomesInput InputActions => _inputActions;
    }
}
