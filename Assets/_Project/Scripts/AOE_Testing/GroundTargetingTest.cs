using System.Collections.Generic;
using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Test script for ground-targeted AOE abilities.
    /// Allows player to select an area on the ground using mouse cursor.
    /// </summary>
    public class GroundTargetingTest : MonoBehaviour
    {
        [Header("Ground Targeting Settings")]
        [SerializeField] private float aoeRadius = 5f;
        [SerializeField] private LayerMask groundLayerMask = 1; // Default layer
        [SerializeField] private Color indicatorColor = Color.red;
        [SerializeField] private KeyCode activationKey = KeyCode.G;
        
        [Header("References")]
        [SerializeField] private Camera targetCamera;
        
        private AOEVisualIndicator visualIndicator;
        private bool isTargeting = false;
        private Vector3 currentTargetPosition;
        
        void Start()
        {
            // Get or create camera reference
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                if (targetCamera == null)
                {
                    targetCamera = FindObjectOfType<Camera>();
                }
            }
            
            // Create visual indicator
            GameObject indicatorObject = new GameObject("GroundTargetIndicator");
            indicatorObject.transform.SetParent(transform);
            visualIndicator = indicatorObject.AddComponent<AOEVisualIndicator>();
            
            Debug.Log("[GroundTargetingTest] Initialized. Press G to start ground targeting.");
        }
        
        void Update()
        {
            HandleInput();
            
            if (isTargeting)
            {
                UpdateTargetPosition();
            }
        }
        
        void HandleInput()
        {
            // Toggle targeting mode
            if (Input.GetKeyDown(activationKey))
            {
                if (!isTargeting)
                {
                    StartTargeting();
                }
                else
                {
                    ConfirmTarget();
                }
            }
            
            // Cancel targeting with right click or escape
            if (isTargeting && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
            {
                CancelTargeting();
            }
        }
        
        void StartTargeting()
        {
            isTargeting = true;
            Debug.Log("[GroundTargetingTest] Ground targeting started. Move mouse to select area, press G to confirm, right-click to cancel.");
        }
        
        void UpdateTargetPosition()
        {
            if (targetCamera == null) return;
            
            Vector3 groundPosition = GetGroundPosition();
            if (groundPosition != Vector3.zero)
            {
                currentTargetPosition = groundPosition;
                visualIndicator.ShowCircle(currentTargetPosition, aoeRadius, indicatorColor);
            }
        }
        
        Vector3 GetGroundPosition()
        {
            // Convert mouse position to world ray
            Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
            
            // Raycast to find ground intersection
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                return hit.point;
            }
            
            // Fallback: project ray onto Y=0 plane
            if (ray.direction.y < 0)
            {
                float distance = -ray.origin.y / ray.direction.y;
                return ray.origin + ray.direction * distance;
            }
            
            return Vector3.zero;
        }
        
        void ConfirmTarget()
        {
            if (!isTargeting) return;
            
            // Detect enemies in the targeted area
            List<GameObject> enemiesHit = AreaDetector.GetEnemiesInRadius(currentTargetPosition, aoeRadius);
            
            Debug.Log($"[GroundTargetingTest] Ground AOE confirmed at {currentTargetPosition}!");
            Debug.Log($"[GroundTargetingTest] Enemies hit: {enemiesHit.Count}");
            
            foreach (GameObject enemy in enemiesHit)
            {
                Debug.Log($"[GroundTargetingTest] - Hit enemy: {enemy.name} at {enemy.transform.position}");
                
                // Optional: Add visual effect or damage application here
                // For now, just log the hit
            }
            
            // End targeting mode
            EndTargeting();
            
            // Keep indicator visible for a short time to show the effect
            StartCoroutine(HideIndicatorAfterDelay(2f));
        }
        
        void CancelTargeting()
        {
            Debug.Log("[GroundTargetingTest] Ground targeting cancelled.");
            EndTargeting();
        }
        
        void EndTargeting()
        {
            isTargeting = false;
        }
        
        System.Collections.IEnumerator HideIndicatorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            visualIndicator.Hide();
        }
        
        void OnGUI()
        {
            // Simple UI instructions
            GUILayout.BeginArea(new Rect(10, 10, 300, 100));
            GUILayout.Label("Ground Targeting Test", GUI.skin.box);
            GUILayout.Label($"Press {activationKey} to start/confirm targeting");
            GUILayout.Label("Right-click or ESC to cancel");
            
            if (isTargeting)
            {
                GUILayout.Label("TARGETING MODE ACTIVE", GUI.skin.box);
                GUILayout.Label($"Target: {currentTargetPosition}");
            }
            
            GUILayout.EndArea();
        }
        
        // Public methods for external control
        public void SetAOERadius(float radius)
        {
            aoeRadius = radius;
        }
        
        public void SetIndicatorColor(Color color)
        {
            indicatorColor = color;
        }
        
        public bool IsCurrentlyTargeting => isTargeting;
        
        public Vector3 CurrentTargetPosition => currentTargetPosition;
    }
}