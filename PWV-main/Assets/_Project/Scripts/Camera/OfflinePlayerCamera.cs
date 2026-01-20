using UnityEngine;

namespace EtherDomes.Camera
{
    /// <summary>
    /// Offline player camera for testing without networking
    /// </summary>
    public class OfflinePlayerCamera : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private float _distance = 8f;
        [SerializeField] private float _height = 2f;
        [SerializeField] private float _mouseSensitivity = 2f;
        [SerializeField] private float _smoothTime = 0.1f;
        [SerializeField] private float _lookAtHeight = 1.6f; // Height offset for camera target (1.6f = eyes, 1.0f = waist, 1.8f = head)
        
        [Header("Zoom Settings")]
        [SerializeField] private float _minDistance = 1.5f; // Minimum zoom (close to player's eyes)
        [SerializeField] private float _maxDistance = 15f;  // Maximum zoom
        [SerializeField] private float _zoomSpeed = 2f;
        [SerializeField] private float _zoomSmoothTime = 0.1f;
        
        [Header("Collision Settings")]
        [SerializeField] private LayerMask _collisionLayers = -1; // What layers the camera collides with
        [SerializeField] private float _collisionRadius = 0.3f; // Camera collision sphere radius
        [SerializeField] private float _collisionBuffer = 0.1f; // Extra space from walls
        
        private Transform _target;
        private float _currentYaw;
        private float _currentPitch = 15f;
        private Vector3 _velocity;
        private float _targetDistance;
        private float _zoomVelocity;
        
        private void Start()
        {
            _targetDistance = _distance;
            FindPlayer();
        }
        
        private void LateUpdate()
        {
            if (_target == null)
            {
                FindPlayer();
                return;
            }
            
            HandleInput();
            UpdateCameraPosition();
        }
        
        private void FindPlayer()
        {
            // Try multiple ways to find the player
            var playerByTag = GameObject.FindWithTag("Player");
            if (playerByTag != null)
            {
                _target = playerByTag.transform;
                _currentYaw = _target.eulerAngles.y;
                Debug.Log($"[SimpleCamera] Found player by tag: {_target.name}");
                return;
            }
            
            var playerByName = GameObject.Find("Player (Local Test)");
            if (playerByName != null)
            {
                _target = playerByName.transform;
                _currentYaw = _target.eulerAngles.y;
                Debug.Log($"[SimpleCamera] Found player by name: {_target.name}");
                return;
            }
            
            Debug.LogWarning("[SimpleCamera] No player found!");
        }
        
        private void HandleInput()
        {
            // Camera rotation with right mouse button (existing functionality)
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
                
                _currentYaw += mouseX;
                _currentPitch -= mouseY;
                _currentPitch = Mathf.Clamp(_currentPitch, -30f, 60f);
            }
            
            // Camera rotation with left mouse button (new functionality)
            if (Input.GetMouseButton(0))
            {
                float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
                float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;
                
                _currentYaw += mouseX;
                _currentPitch -= mouseY;
                _currentPitch = Mathf.Clamp(_currentPitch, -30f, 60f);
            }
            
            // Zoom with mouse wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                _targetDistance -= scroll * _zoomSpeed;
                _targetDistance = Mathf.Clamp(_targetDistance, _minDistance, _maxDistance);
            }
        }
        
        private void UpdateCameraPosition()
        {
            // Smooth zoom transition
            _distance = Mathf.SmoothDamp(_distance, _targetDistance, ref _zoomVelocity, _zoomSmoothTime);
            
            // Calculate desired position - separate height from distance to avoid arc effect
            Quaternion rotation = Quaternion.Euler(_currentPitch, _currentYaw, 0);
            Vector3 targetPosition = _target.position + Vector3.up * _lookAtHeight; // Configurable look-at height
            
            // Calculate camera position: move back by distance, then apply height offset
            Vector3 backwardDirection = rotation * Vector3.back;
            Vector3 desiredPosition = targetPosition + backwardDirection * _distance + Vector3.up * _height;
            
            // Check for collisions and adjust camera position
            Vector3 finalPosition = CheckCameraCollision(targetPosition, desiredPosition);
            
            // Smooth movement
            transform.position = Vector3.SmoothDamp(transform.position, finalPosition, ref _velocity, _smoothTime);
            transform.LookAt(targetPosition);
        }
        
        private Vector3 CheckCameraCollision(Vector3 targetPosition, Vector3 desiredPosition)
        {
            Vector3 direction = desiredPosition - targetPosition;
            float desiredDistance = direction.magnitude;
            
            // Perform sphere cast from target to desired camera position
            if (Physics.SphereCast(targetPosition, _collisionRadius, direction.normalized, out RaycastHit hit, desiredDistance, _collisionLayers))
            {
                // Camera hit something, move it closer to avoid clipping
                float safeDistance = hit.distance - _collisionBuffer;
                safeDistance = Mathf.Max(safeDistance, _minDistance); // Don't go closer than minimum distance
                
                return targetPosition + direction.normalized * safeDistance;
            }
            
            // No collision, use desired position
            return desiredPosition;
        }
    }
}