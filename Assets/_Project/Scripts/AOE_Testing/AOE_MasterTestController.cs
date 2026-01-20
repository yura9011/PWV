using UnityEngine;

namespace AOETesting
{
    /// <summary>
    /// Master controller that combines all AOE test functionality.
    /// Attach this to the TestPlayer to enable all AOE testing features.
    /// </summary>
    public class AOE_MasterTestController : MonoBehaviour
    {
        [Header("Test Components")]
        [SerializeField] private GroundTargetingTest groundTargeting;
        [SerializeField] private PlayerCenteredTest playerCentered;
        [SerializeField] private ConeAttackTest coneAttack;
        [SerializeField] private AOE_TestSceneSetup sceneSetup;
        
        [Header("Auto-Setup")]
        [SerializeField] private bool autoAddComponents = true;
        [SerializeField] private bool showInstructions = true;
        
        void Start()
        {
            if (autoAddComponents)
            {
                SetupComponents();
            }
            
            LogInstructions();
        }
        
        void SetupComponents()
        {
            // Add components if they don't exist
            if (groundTargeting == null)
            {
                groundTargeting = GetComponent<GroundTargetingTest>();
                if (groundTargeting == null)
                {
                    groundTargeting = gameObject.AddComponent<GroundTargetingTest>();
                }
            }
            
            if (playerCentered == null)
            {
                playerCentered = GetComponent<PlayerCenteredTest>();
                if (playerCentered == null)
                {
                    playerCentered = gameObject.AddComponent<PlayerCenteredTest>();
                }
            }
            
            if (coneAttack == null)
            {
                coneAttack = GetComponent<ConeAttackTest>();
                if (coneAttack == null)
                {
                    coneAttack = gameObject.AddComponent<ConeAttackTest>();
                }
            }
            
            Debug.Log("[AOE_MasterTestController] All AOE test components added");
        }
        
        void LogInstructions()
        {
            Debug.Log("=== AOE TESTING INSTRUCTIONS ===");
            Debug.Log("G - Ground Targeting AOE (mouse to select area)");
            Debug.Log("R - Player-Centered AOE (area around player)");
            Debug.Log("T - Cone Attack AOE (frontal cone)");
            Debug.Log("Right-click or ESC - Cancel ground targeting");
            Debug.Log("Move with WASD, rotate with mouse");
            Debug.Log("================================");
        }
        
        void OnGUI()
        {
            if (!showInstructions) return;
            
            // Master instructions panel
            GUILayout.BeginArea(new Rect(Screen.width - 300, Screen.height - 200, 290, 190));
            GUILayout.Label("AOE Testing Master Controller", GUI.skin.box);
            
            GUILayout.Label("Controls:");
            GUILayout.Label("G - Ground Targeting AOE");
            GUILayout.Label("R - Player-Centered AOE");
            GUILayout.Label("T - Cone Attack AOE");
            GUILayout.Label("ESC/Right-click - Cancel targeting");
            
            GUILayout.Space(10);
            
            // Status indicators
            if (groundTargeting != null && groundTargeting.IsCurrentlyTargeting)
            {
                GUILayout.Label("GROUND TARGETING ACTIVE", GUI.skin.box);
            }
            
            if (playerCentered != null && playerCentered.IsOnCooldown)
            {
                GUILayout.Label($"Player AOE Cooldown: {playerCentered.RemainingCooldown:F1}s");
            }
            
            if (coneAttack != null && coneAttack.IsOnCooldown)
            {
                GUILayout.Label($"Cone Attack Cooldown: {coneAttack.RemainingCooldown:F1}s");
            }
            
            GUILayout.EndArea();
        }
        
        // Public methods for external control
        public void TriggerGroundTargeting()
        {
            if (groundTargeting != null && !groundTargeting.IsCurrentlyTargeting)
            {
                // Simulate G key press
                Debug.Log("[AOE_MasterTestController] Triggering ground targeting");
            }
        }
        
        public void TriggerPlayerCenteredAOE()
        {
            if (playerCentered != null)
            {
                playerCentered.TriggerAOE();
            }
        }
        
        public void TriggerConeAttack()
        {
            if (coneAttack != null)
            {
                coneAttack.TriggerAttack();
            }
        }
        
        public void PreviewConeAttack()
        {
            if (coneAttack != null)
            {
                coneAttack.PreviewCone(1f);
            }
        }
        
        // Utility methods
        public bool IsAnyAOEActive()
        {
            return (groundTargeting != null && groundTargeting.IsCurrentlyTargeting) ||
                   (playerCentered != null && playerCentered.IsOnCooldown) ||
                   (coneAttack != null && coneAttack.IsOnCooldown);
        }
        
        public void SetAllAOERadius(float radius)
        {
            if (groundTargeting != null) groundTargeting.SetAOERadius(radius);
            if (playerCentered != null) playerCentered.SetAOERadius(radius);
        }
        
        public void SetConeParameters(float angle, float range)
        {
            if (coneAttack != null)
            {
                coneAttack.SetConeAngle(angle);
                coneAttack.SetConeRange(range);
            }
        }
    }
}