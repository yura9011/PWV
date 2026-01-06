using UnityEngine;
using Mirror;
using Unity.Cinemachine;

namespace EtherDomes.Camera
{
    /// <summary>
    /// Assigns Cinemachine camera to follow the local player.
    /// Attach to the player prefab.
    /// </summary>
    public class CinemachinePlayerFollow : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Vector3 _lookAtOffset = new Vector3(0, 1.5f, 0);
        
        private Transform _lookAtTarget;

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            SetupCamera();
        }

        private void SetupCamera()
        {
            Debug.Log("[CinemachinePlayerFollow] SetupCamera called for local player");
            
            // Create look-at target with offset
            var lookAtGO = new GameObject("LookAtTarget");
            lookAtGO.transform.SetParent(transform);
            lookAtGO.transform.localPosition = _lookAtOffset;
            _lookAtTarget = lookAtGO.transform;
            
            // Find any CinemachineCamera in scene (Cinemachine 3.x)
            var cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();
            if (cinemachineCamera != null)
            {
                cinemachineCamera.Follow = transform;
                cinemachineCamera.LookAt = _lookAtTarget;
                Debug.Log($"[CinemachinePlayerFollow] CinemachineCamera '{cinemachineCamera.name}' assigned to local player at {transform.position}");
                return;
            }
            
            // Fallback: Try to use main camera directly
            var mainCamera = UnityEngine.Camera.main;
            if (mainCamera != null)
            {
                Debug.Log("[CinemachinePlayerFollow] No CinemachineCamera found, using Main Camera fallback");
                // Position camera behind player
                mainCamera.transform.position = transform.position + new Vector3(0, 5, -10);
                mainCamera.transform.LookAt(transform.position + Vector3.up);
            }
            else
            {
                Debug.LogError("[CinemachinePlayerFollow] No camera found at all!");
            }
            
            Debug.LogWarning("[CinemachinePlayerFollow] No CinemachineCamera found in scene. Create one via EtherDomes > Create Cinemachine Camera");
        }

        private void OnDestroy()
        {
            if (_lookAtTarget != null)
            {
                Destroy(_lookAtTarget.gameObject);
            }
        }
    }
}
