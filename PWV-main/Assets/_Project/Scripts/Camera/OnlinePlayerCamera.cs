using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

namespace EtherDomes.Camera
{
    /// <summary>
    /// Online player camera for multiplayer with networking support.
    /// - Left/Right mouse: Rotate camera
    /// - Mouse wheel: Zoom in/out (first person to max distance)
    /// - Ground collision prevents camera from going below floor
    /// </summary>
    public class OnlinePlayerCamera : MonoBehaviour
    {
        [Header("Rotation")]
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _minVerticalAngle = -30f;
        [SerializeField] private float _maxVerticalAngle = 60f;
        
        [Header("Distance/Zoom")]
        [SerializeField] private float _defaultDistance = 8f;
        [SerializeField] private float _minDistance = 0f; // First person
        [SerializeField] private float _maxDistance = 12f; // Max zoom out
        [SerializeField] private float _zoomSpeed = 1f; // Slower zoom
        [SerializeField] private float _zoomSmoothTime = 0.2f; // Smoother
        [SerializeField] private float _firstPersonThreshold = 0.3f; // Below this = first person mode
        
        [Header("Position")]
        [SerializeField] private float _heightOffset = 0.5f; // Height above lookAt point
        [SerializeField] private Vector3 _lookAtOffset = new Vector3(0, 1.6f, 0); // Look at neck/head level
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private float _eyeHeight = 1.65f; // Height of eyes for first person
        
        [Header("Collision")]
        [SerializeField] private float _collisionRadius = 0.3f;
        [SerializeField] private float _minHeightAboveGround = 0.5f;
        [SerializeField] private LayerMask _collisionLayers = ~0; // All layers by default
        
        private Transform _target;
        private float _currentYaw;
        private float _currentPitch = 15f;
        private float _currentDistance;
        private float _targetDistance;
        private float _zoomVelocity;
        private Vector3 _currentVelocity;

        private void Awake()
        {
            _currentDistance = _defaultDistance;
            _targetDistance = _defaultDistance;
            
            // Ensure NO CinemachineBrain is interfering
#if UNITY_6000_0_OR_NEWER
            var brain = GetComponent<Unity.Cinemachine.CinemachineBrain>();
#else
            var brain = GetComponent<CinemachineBrain>();
#endif
            if (brain != null)
            {
                Debug.LogWarning("[ThirdPersonCamera] Destroying conflicting CinemachineBrain.");
                Destroy(brain); 
            }
        }
        
        private void LateUpdate()
        {
            if (_target == null)
            {
                FindLocalPlayer();
                return;
            }
            
            HandleRotation();
            HandleZoom();
            UpdateCameraPosition();
        }
        
        private void HandleRotation()
        {
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
        }
        
        private void HandleZoom()
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.01f)
            {
                _targetDistance -= scrollInput * _zoomSpeed * 10f;
                _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
            }
            
            // Smooth zoom
            _currentDistance = Mathf.SmoothDamp(_currentDistance, _targetDistance, ref _zoomVelocity, _zoomSmoothTime);
        }
        
        private void UpdateCameraPosition()
        {
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
            
            // Target point is at neck/head level (where we look at)
            Vector3 lookAtPoint = _target.position + _lookAtOffset;
            
            // Camera position: behind the lookAt point
            // As distance decreases, camera moves through the head to eye position
            Vector3 desiredPosition;
            
            if (_currentDistance < _firstPersonThreshold)
            {
                // First person: camera at eye level
                desiredPosition = _target.position + new Vector3(0, _eyeHeight, 0);
                transform.position = desiredPosition;
                transform.rotation = rotation;
            }
            else
            {
                // Third person: position behind and slightly above the lookAt point
                // Camera approaches from behind the neck, not from above
                Vector3 offset = rotation * new Vector3(0, 0, -_currentDistance);
                offset.y += _heightOffset; // Small height offset
                
                desiredPosition = lookAtPoint + offset;
                
                // Ground collision
                desiredPosition = ApplyGroundCollision(lookAtPoint, desiredPosition);
                
                // Obstacle collision
                desiredPosition = ApplyObstacleCollision(lookAtPoint, desiredPosition);
                
                transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, _smoothTime);
                transform.LookAt(lookAtPoint);
            }
        }
        
        private Vector3 ApplyGroundCollision(Vector3 targetPos, Vector3 desiredPosition)
        {
            // Raycast down from desired position to check ground
            if (Physics.Raycast(desiredPosition + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, _collisionLayers))
            {
                float minY = hit.point.y + _minHeightAboveGround;
                if (desiredPosition.y < minY)
                {
                    desiredPosition.y = minY;
                }
            }
            
            return desiredPosition;
        }
        
        private Vector3 ApplyObstacleCollision(Vector3 targetPos, Vector3 desiredPosition)
        {
            // Raycast from target to desired camera position
            Vector3 direction = desiredPosition - targetPos;
            float distance = direction.magnitude;
            
            if (distance > 0.1f && Physics.SphereCast(targetPos, _collisionRadius, direction.normalized, out RaycastHit hit, distance, _collisionLayers))
            {
                // Pull camera closer to avoid obstacle
                desiredPosition = targetPos + direction.normalized * (hit.distance - _collisionRadius);
            }
            
            return desiredPosition;
        }

        private void FindLocalPlayer()
        {
            // 1. Try networked player first
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
                {
                    _target = NetworkManager.Singleton.LocalClient.PlayerObject.transform;
                    _currentYaw = _target.eulerAngles.y;
                    Debug.Log($"[ThirdPersonCamera] LOCKED ON LocalClient.PlayerObject: {_target.name}");
                    return;
                }
            }
            
            var networkPlayers = FindObjectsByType<EtherDomes.Player.NetworkPlayer>(FindObjectsSortMode.None);
            foreach (var player in networkPlayers)
            {
                if (player.IsOwner)
                {
                    _target = player.transform;
                    _currentYaw = _target.eulerAngles.y;
                    Debug.Log($"[ThirdPersonCamera] Found NetworkPlayer owner: {_target.name}");
                    return;
                }
            }
            
            var playerControllers = FindObjectsByType<EtherDomes.Player.PlayerController>(FindObjectsSortMode.None);
            foreach (var pc in playerControllers)
            {
                if (pc.IsOwner)
                {
                    _target = pc.transform;
                    _currentYaw = _target.eulerAngles.y;
                    Debug.Log($"[ThirdPersonCamera] Found PlayerController owner: {_target.name}");
                    return;
                }
            }
            
            // 2. Fallback for offline testing - find any GameObject tagged "Player" or named "TestPlayer"
            var testPlayerGO = GameObject.Find("TestPlayer");
            if (testPlayerGO != null)
            {
                _target = testPlayerGO.transform;
                _currentYaw = _target.eulerAngles.y;
                Debug.Log($"[ThirdPersonCamera] Found TestPlayer for offline testing: {_target.name}");
                return;
            }
            
            // 2.5. Try finding "Player (Local Test)" specifically
            var localTestPlayer = GameObject.Find("Player (Local Test)");
            if (localTestPlayer != null)
            {
                _target = localTestPlayer.transform;
                _currentYaw = _target.eulerAngles.y;
                Debug.Log($"[ThirdPersonCamera] Found Player (Local Test) for offline testing: {_target.name}");
                return;
            }
            
            // 3. Try finding by Player tag
            var playerByTag = GameObject.FindWithTag("Player");
            if (playerByTag != null)
            {
                _target = playerByTag.transform;
                _currentYaw = _target.eulerAngles.y;
                Debug.Log($"[ThirdPersonCamera] Found player by tag: {_target.name}");
                return;
            }
        }
        
        /// <summary>
        /// Manually set the camera target (useful for offline testing)
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                _currentYaw = _target.eulerAngles.y;
                Debug.Log($"[ThirdPersonCamera] Target manually set to: {_target.name}");
            }
        }
    }
}
