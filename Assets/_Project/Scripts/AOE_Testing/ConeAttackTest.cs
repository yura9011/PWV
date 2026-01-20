using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Test script for cone-shaped AOE attacks.
    /// Creates a cone attack that follows mouse direction for dynamic targeting.
    /// </summary>
    public class ConeAttackTest : MonoBehaviour
    {
        [Header("Cone Attack Settings")]
        [SerializeField] private float coneAngle = 60f; // Total angle in degrees
        [SerializeField] private float coneRange = 8f;
        [SerializeField] private Color indicatorColor = Color.green;
        [SerializeField] private Color previewColor = Color.yellow;
        [SerializeField] private KeyCode activationKey = KeyCode.T;
        [SerializeField] private float indicatorDuration = 2f;
        [SerializeField] private float cooldownTime = 1f;
        [SerializeField] private bool useDetailedCone = true; // Use arc visualization
        [SerializeField] private bool showPreview = true; // Show cone preview while aiming
        [SerializeField] private LayerMask groundLayer = 1; // Layer for ground raycast
        
        private AOEVisualIndicator visualIndicator;
        private AOEVisualIndicator previewIndicator;
        private bool isOnCooldown = false;
        private bool isAiming = false;
        private float lastActivationTime = 0f;
        private Vector3 currentMouseDirection = Vector3.forward;
        private Camera playerCamera;
        
        void Start()
        {
            // Create visual indicator for attacks
            GameObject indicatorObject = new GameObject("ConeAttackIndicator");
            indicatorObject.transform.SetParent(transform);
            visualIndicator = indicatorObject.AddComponent<AOEVisualIndicator>();
            
            // Create preview indicator for aiming
            GameObject previewObject = new GameObject("ConePreviewIndicator");
            previewObject.transform.SetParent(transform);
            previewIndicator = previewObject.AddComponent<AOEVisualIndicator>();
            
            // Find camera for mouse-to-world conversion
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
            
            Debug.Log("[ConeAttackTest] Initialized. Press T to trigger cone attack.");
            Debug.Log("[ConeAttackTest] Cone follows mouse direction for dynamic targeting!");
        }
        
        void Update()
        {
            HandleInput();
            UpdateCooldown();
            UpdateMouseDirection();
            UpdatePreview();
        }
        
        void HandleInput()
        {
            // Start aiming when T is pressed
            if (Input.GetKeyDown(activationKey) && !isOnCooldown)
            {
                StartAiming();
            }
            
            // Confirm attack when T is released
            if (Input.GetKeyUp(activationKey) && isAiming)
            {
                ConfirmConeAttack();
            }
            
            // Cancel aiming with right click or ESC
            if (isAiming && (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape)))
            {
                CancelAiming();
            }
        }
        
        void UpdateMouseDirection()
        {
            if (!isAiming || playerCamera == null) return;
            
            // Convert mouse position to world direction
            Vector3 mouseScreenPos = Input.mousePosition;
            Ray mouseRay = playerCamera.ScreenPointToRay(mouseScreenPos);
            
            // Raycast to ground to get target point
            if (Physics.Raycast(mouseRay, out RaycastHit hit, Mathf.Infinity, groundLayer))
            {
                Vector3 targetPoint = hit.point;
                Vector3 playerPosition = transform.position;
                
                // Calculate direction from player to target point
                Vector3 direction = (targetPoint - playerPosition).normalized;
                direction.y = 0; // Keep cone horizontal
                
                if (direction != Vector3.zero)
                {
                    currentMouseDirection = direction;
                }
            }
            else
            {
                // Fallback: use camera forward projected on ground
                Vector3 cameraForward = playerCamera.transform.forward;
                cameraForward.y = 0;
                currentMouseDirection = cameraForward.normalized;
            }
        }
        
        void UpdatePreview()
        {
            if (isAiming && showPreview)
            {
                // Show preview cone following mouse
                if (useDetailedCone)
                {
                    previewIndicator.ShowConeWithArc(transform.position, currentMouseDirection, coneAngle, coneRange, previewColor, 8);
                }
                else
                {
                    previewIndicator.ShowCone(transform.position, currentMouseDirection, coneAngle, coneRange, previewColor);
                }
            }
        }
        
        void StartAiming()
        {
            isAiming = true;
            currentMouseDirection = transform.forward; // Start with player forward
            
            Debug.Log("[ConeAttackTest] Cone aiming started. Move mouse to aim, release T to attack, right-click to cancel.");
        }
        
        void ConfirmConeAttack()
        {
            if (!isAiming) return;
            
            isAiming = false;
            previewIndicator.Hide();
            
            TriggerConeAttack();
        }
        
        void CancelAiming()
        {
            if (!isAiming) return;
            
            isAiming = false;
            previewIndicator.Hide();
            
            Debug.Log("[ConeAttackTest] Cone aiming cancelled.");
        }
        
        void UpdateCooldown()
        {
            if (isOnCooldown && Time.time >= lastActivationTime + cooldownTime)
            {
                isOnCooldown = false;
            }
        }
        
        void TriggerConeAttack()
        {
            Vector3 playerPosition = transform.position;
            Vector3 coneDirection = currentMouseDirection; // Use mouse direction instead of player forward
            
            // Detect enemies in cone area
            List<GameObject> enemiesHit = AreaDetector.GetEnemiesInCone(
                playerPosition, 
                coneDirection, 
                coneAngle, 
                coneRange
            );
            
            Debug.Log($"[ConeAttackTest] Cone attack triggered at {playerPosition}!");
            Debug.Log($"[ConeAttackTest] Cone direction (mouse): {coneDirection}");
            Debug.Log($"[ConeAttackTest] Cone angle: {coneAngle}°, Range: {coneRange}");
            Debug.Log($"[ConeAttackTest] Enemies hit: {enemiesHit.Count}");
            
            foreach (GameObject enemy in enemiesHit)
            {
                Debug.Log($"[ConeAttackTest] - Hit enemy: {enemy.name} at {enemy.transform.position}");
                
                // Optional: Add visual effect or damage application here
                StartCoroutine(HighlightEnemy(enemy));
            }
            
            // Show visual indicator with mouse direction
            if (useDetailedCone)
            {
                visualIndicator.ShowConeWithArc(playerPosition, coneDirection, coneAngle, coneRange, indicatorColor, 8);
            }
            else
            {
                visualIndicator.ShowCone(playerPosition, coneDirection, coneAngle, coneRange, indicatorColor);
            }
            
            // Start cooldown
            isOnCooldown = true;
            lastActivationTime = Time.time;
            
            // Hide indicator after duration
            StartCoroutine(HideIndicatorAfterDelay(indicatorDuration));
        }
        
        IEnumerator HideIndicatorAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            visualIndicator.Hide();
        }
        
        IEnumerator HighlightEnemy(GameObject enemy)
        {
            if (enemy == null) yield break;
            
            // Simple highlight effect - change color briefly
            Renderer enemyRenderer = enemy.GetComponent<Renderer>();
            if (enemyRenderer != null)
            {
                Color originalColor = enemyRenderer.material.color;
                enemyRenderer.material.color = Color.red;
                
                yield return new WaitForSeconds(0.5f);
                
                enemyRenderer.material.color = originalColor;
            }
        }
        
        void OnGUI()
        {
            // Simple UI instructions
            GUILayout.BeginArea(new Rect(10, 230, 350, 150));
            GUILayout.Label("Cone Attack Test (Mouse Targeting)", GUI.skin.box);
            
            if (isAiming)
            {
                GUILayout.Label($"AIMING: Move mouse to aim cone", GUI.skin.box);
                GUILayout.Label($"Release {activationKey} to attack, Right-click to cancel");
                GUILayout.Label($"Direction: {currentMouseDirection}");
            }
            else
            {
                GUILayout.Label($"Hold {activationKey} to aim cone attack");
            }
            
            GUILayout.Label($"Angle: {coneAngle}°, Range: {coneRange} units");
            
            if (isOnCooldown)
            {
                float remainingCooldown = cooldownTime - (Time.time - lastActivationTime);
                GUILayout.Label($"Cooldown: {remainingCooldown:F1}s", GUI.skin.box);
            }
            else if (!isAiming)
            {
                GUILayout.Label("Ready!", GUI.skin.box);
            }
            
            GUILayout.EndArea();
        }
        
        // Public methods for external control
        public void SetConeAngle(float angle)
        {
            coneAngle = angle;
        }
        
        public void SetConeRange(float range)
        {
            coneRange = range;
        }
        
        public void SetIndicatorColor(Color color)
        {
            indicatorColor = color;
        }
        
        public void SetCooldownTime(float cooldown)
        {
            cooldownTime = cooldown;
        }
        
        public bool IsOnCooldown => isOnCooldown;
        
        public float RemainingCooldown => isOnCooldown ? cooldownTime - (Time.time - lastActivationTime) : 0f;
        
        /// <summary>
        /// Manually trigger the cone attack (for external scripts)
        /// </summary>
        public void TriggerAttack()
        {
            if (!isOnCooldown)
            {
                TriggerConeAttack();
            }
        }
        
        /// <summary>
        /// Check if a position would be hit by the cone attack
        /// </summary>
        public bool IsPositionInCone(Vector3 position)
        {
            return AreaDetector.IsPointInCone(transform.position, currentMouseDirection, coneAngle, coneRange, position);
        }
        
        /// <summary>
        /// Get the current cone parameters for external systems
        /// </summary>
        public (Vector3 origin, Vector3 forward, float angle, float range) GetConeParameters()
        {
            return (transform.position, currentMouseDirection, coneAngle, coneRange);
        }
        
        /// <summary>
        /// Preview the cone area without triggering the attack
        /// </summary>
        public void PreviewCone(float duration = 1f)
        {
            Vector3 direction = isAiming ? currentMouseDirection : transform.forward;
            
            if (useDetailedCone)
            {
                visualIndicator.ShowConeWithArc(transform.position, direction, coneAngle, coneRange, 
                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0.3f), 8);
            }
            else
            {
                visualIndicator.ShowCone(transform.position, direction, coneAngle, coneRange, 
                    new Color(indicatorColor.r, indicatorColor.g, indicatorColor.b, 0.3f));
            }
            
            StartCoroutine(HideIndicatorAfterDelay(duration));
        }
        
        /// <summary>
        /// Get current aiming state
        /// </summary>
        public bool IsAiming => isAiming;
        
        /// <summary>
        /// Get current mouse direction
        /// </summary>
        public Vector3 CurrentDirection => currentMouseDirection;
    }
}