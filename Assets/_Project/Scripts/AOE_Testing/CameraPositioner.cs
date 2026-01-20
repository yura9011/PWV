using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Simple script to position the camera correctly for AOE testing
    /// </summary>
    public class CameraPositioner : MonoBehaviour
    {
        [Header("Camera Position Settings")]
        public Vector3 targetPosition = new Vector3(0, 12, -8);
        public Vector3 targetRotation = new Vector3(35, 0, 0);
        
        void Start()
        {
            PositionCamera();
        }
        
        [ContextMenu("Position Camera")]
        public void PositionCamera()
        {
            transform.position = targetPosition;
            transform.rotation = Quaternion.Euler(targetRotation);
            
            Debug.Log($"[CameraPositioner] Camera positioned at {targetPosition} with rotation {targetRotation}");
        }
        
        void OnDrawGizmos()
        {
            // Draw a small sphere to show the camera position
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetPosition, 0.5f);
            
            // Draw a line showing the camera direction
            Vector3 forward = Quaternion.Euler(targetRotation) * Vector3.forward;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(targetPosition, forward * 5f);
        }
    }
}