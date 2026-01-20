using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Test script for player-centered AOE abilities.
    /// Creates an area effect centered on the player's position.
    /// </summary>
    public class PlayerCenteredTest : MonoBehaviour
    {
        [Header("Player-Centered AOE Settings")]
        [SerializeField] private float aoeRadius = 5f;
        [SerializeField] private Color indicatorColor = Color.blue;
        [SerializeField] private KeyCode activationKey = KeyCode.R;
        [SerializeField] private float indicatorDuration = 2f;
        [SerializeField] private float cooldownTime = 1f;
        
        private AOEVisualIndicator visualIndicator;
        private bool isOnCooldown = false;
        private float lastActivationTime = 0f;
        
        void Start()
        {
            // Create visual indicator
            GameObject indicatorObject = new GameObject("PlayerCenteredIndicator");
            indicatorObject.transform.SetParent(transform);
            visualIndicator = indicatorObject.AddComponent<AOEVisualIndicator>();
            
            Debug.Log("[PlayerCenteredTest] Initialized. Press R to trigger player-centered AOE.");
        }
        
        void Update()
        {
            HandleInput();
            UpdateCooldown();
        }
        
        void HandleInput()
        {
            if (Input.GetKeyDown(activationKey) && !isOnCooldown)
            {
                TriggerPlayerCenteredAOE();
            }
        }
        
        void UpdateCooldown()
        {
            if (isOnCooldown && Time.time >= lastActivationTime + cooldownTime)
            {
                isOnCooldown = false;
            }
        }
        
        void TriggerPlayerCenteredAOE()
        {
            Vector3 playerPosition = transform.position;
            
            // Detect enemies in radius around player
            List<GameObject> enemiesHit = AreaDetector.GetEnemiesInRadius(playerPosition, aoeRadius);
            
            Debug.Log($"[PlayerCenteredTest] Player-centered AOE triggered at {playerPosition}!");
            Debug.Log($"[PlayerCenteredTest] Enemies hit: {enemiesHit.Count}");
            
            foreach (GameObject enemy in enemiesHit)
            {
                Debug.Log($"[PlayerCenteredTest] - Hit enemy: {enemy.name} at {enemy.transform.position}");
                
                // Optional: Add visual effect or damage application here
                // For testing, we could add a simple effect to the enemy
                StartCoroutine(HighlightEnemy(enemy));
            }
            
            // Show visual indicator
            visualIndicator.ShowCircle(playerPosition, aoeRadius, indicatorColor);
            
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
                enemyRenderer.material.color = Color.yellow;
                
                yield return new WaitForSeconds(0.5f);
                
                enemyRenderer.material.color = originalColor;
            }
        }
        
        void OnGUI()
        {
            // Simple UI instructions
            GUILayout.BeginArea(new Rect(10, 120, 300, 100));
            GUILayout.Label("Player-Centered AOE Test", GUI.skin.box);
            GUILayout.Label($"Press {activationKey} to trigger AOE around player");
            GUILayout.Label($"Radius: {aoeRadius} units");
            
            if (isOnCooldown)
            {
                float remainingCooldown = cooldownTime - (Time.time - lastActivationTime);
                GUILayout.Label($"Cooldown: {remainingCooldown:F1}s", GUI.skin.box);
            }
            else
            {
                GUILayout.Label("Ready!", GUI.skin.box);
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
        
        public void SetCooldownTime(float cooldown)
        {
            cooldownTime = cooldown;
        }
        
        public bool IsOnCooldown => isOnCooldown;
        
        public float RemainingCooldown => isOnCooldown ? cooldownTime - (Time.time - lastActivationTime) : 0f;
        
        /// <summary>
        /// Manually trigger the AOE (for external scripts)
        /// </summary>
        public void TriggerAOE()
        {
            if (!isOnCooldown)
            {
                TriggerPlayerCenteredAOE();
            }
        }
        
        /// <summary>
        /// Get the current AOE area (for AI or other systems to query)
        /// </summary>
        public bool IsPositionInAOERange(Vector3 position)
        {
            return Vector3.Distance(transform.position, position) <= aoeRadius;
        }
    }
}