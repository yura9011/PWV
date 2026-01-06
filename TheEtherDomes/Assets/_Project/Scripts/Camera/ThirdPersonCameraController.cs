using UnityEngine;
using Mirror;
using Unity.Cinemachine;

namespace EtherDomes.Camera
{
    /// <summary>
    /// Simple third-person camera controller that works with Cinemachine.
    /// Rotates camera orbit with right mouse button held.
    /// </summary>
    public class ThirdPersonCameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _minVerticalAngle = -30f;
        [SerializeField] private float _maxVerticalAngle = 60f;
        [SerializeField] private float _distance = 8f;
        [SerializeField] private float _height = 3f;
        [SerializeField] private Vector3 _lookAtOffset = new Vector3(0, 1.5f, 0);
        
        private Transform _target;
        private float _currentYaw;
        private float _currentPitch = 15f;
        private bool _isInitialized;
        
        private void LateUpdate()
        {
            if (_target == null)
            {
                FindLocalPlayer();
                return;
            }
            
            // Rotate camera with right mouse (affects character direction) or left mouse (camera only)
            bool rightMouse = Input.GetMouseButton(1);
            bool leftMouse = Input.GetMouseButton(0);
            
            if (rightMouse || leftMouse)
            {
                float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
                
                _currentYaw += mouseX;
                _currentPitch -= mouseY;
                _currentPitch = Mathf.Clamp(_currentPitch, _minVerticalAngle, _maxVerticalAngle);
            }
            
            // Calculate camera position
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
            Vector3 offset = rotation * new Vector3(0, 0, -_distance);
            offset.y += _height;
            
            Vector3 targetPos = _target.position + _lookAtOffset;
            transform.position = targetPos + offset;
            transform.LookAt(targetPos);
        }
        
        private void FindLocalPlayer()
        {
            // Find the local player
            var players = FindObjectsByType<NetworkIdentity>(FindObjectsSortMode.None);
            foreach (var player in players)
            {
                if (player.isLocalPlayer)
                {
                    _target = player.transform;
                    _currentYaw = _target.eulerAngles.y;
                    _isInitialized = true;
                    Debug.Log($"[ThirdPersonCamera] Found local player: {_target.name}");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Gets the current camera yaw for character rotation sync.
        /// </summary>
        public float GetYaw() => _currentYaw;
    }
}
