using UnityEngine;
using EtherDomes.Combat.Abilities;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Helper script to automatically assign abilities to OfflinePlayerController
    /// if they're not assigned manually in the Inspector.
    /// </summary>
    public class AbilityAssigner : MonoBehaviour
    {
        [Header("Abilities to assign")]
        [SerializeField] private AbilityDefinition _basicAttack;
        [SerializeField] private AbilityDefinition _heavyAttack;
        [SerializeField] private AbilityDefinition _heal;
        [SerializeField] private AbilityDefinition _drainLife;

        [Header("Auto-assign abilities if not set")]
        [SerializeField] private bool _autoAssignOnStart = true;

        private void Start()
        {
            if (!_autoAssignOnStart) return;

            var playerController = GetComponent<OfflinePlayerController>();
            if (playerController == null) 
            {
                Debug.LogError("[AbilityAssigner] No OfflinePlayerController found on this GameObject!");
                return;
            }

            Debug.Log($"[AbilityAssigner] Found abilities: Basic={_basicAttack != null}, Heavy={_heavyAttack != null}, Heal={_heal != null}, Drain={_drainLife != null}");

            // Try to assign using reflection (since fields are private)
            try
            {
                var playerType = typeof(OfflinePlayerController);
                
                if (_basicAttack != null)
                {
                    var basicField = playerType.GetField("_basicAttack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (basicField != null)
                    {
                        basicField.SetValue(playerController, _basicAttack);
                        Debug.Log("[AbilityAssigner] Assigned BasicAttack");
                    }
                }

                if (_heavyAttack != null)
                {
                    var heavyField = playerType.GetField("_heavyAttack", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (heavyField != null)
                    {
                        heavyField.SetValue(playerController, _heavyAttack);
                        Debug.Log("[AbilityAssigner] Assigned HeavyAttack");
                    }
                }

                if (_heal != null)
                {
                    var healField = playerType.GetField("_heal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (healField != null)
                    {
                        healField.SetValue(playerController, _heal);
                        Debug.Log("[AbilityAssigner] Assigned Heal");
                    }
                }

                if (_drainLife != null)
                {
                    var drainField = playerType.GetField("_drainLife", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (drainField != null)
                    {
                        drainField.SetValue(playerController, _drainLife);
                        Debug.Log("[AbilityAssigner] Assigned DrainLife");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[AbilityAssigner] Failed to assign abilities: {e.Message}");
            }
        }
    }
}