using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Configuration ScriptableObject for combat system parameters.
    /// Centralizes all combat-related settings for easy tuning.
    /// </summary>
    [CreateAssetMenu(fileName = "CombatConfig", menuName = "EtherDomes/Combat/Combat Configuration")]
    public class CombatConfigSO : ScriptableObject
    {
        [Header("Targeting")]
        [Tooltip("Maximum distance to search for targets with Tab")]
        [Range(10f, 100f)]
        public float MaxTargetRange = 40f;
        
        [Tooltip("Range to search for new target when current target dies")]
        [Range(5f, 30f)]
        public float AutoSwitchRange = 10f;
        
        [Tooltip("Cone angle for Tab targeting priority (degrees from center)")]
        [Range(30f, 180f)]
        public float TabConeAngle = 90f;
        
        [Tooltip("Minimum dot product for target to be considered 'in front'")]
        [Range(-1f, 1f)]
        public float MinTargetDotProduct = 0f;

        [Header("Combat Timing")]
        [Tooltip("Duration of Global Cooldown in seconds")]
        [Range(0.5f, 3f)]
        public float GlobalCooldownDuration = 1.5f;
        
        [Tooltip("Input buffer window for spell queue in seconds")]
        [Range(0.1f, 1f)]
        public float SpellQueueWindow = 0.4f;
        
        [Tooltip("Time after combat ends before out-of-combat state")]
        [Range(1f, 10f)]
        public float CombatDropoffTime = 5f;

        [Header("Layers")]
        [Tooltip("Layer mask for enemy detection")]
        public LayerMask EnemyLayer;
        
        [Tooltip("Layer mask for Line of Sight obstacles")]
        public LayerMask ObstacleLayer;
        
        [Tooltip("Layer mask for friendly targets")]
        public LayerMask FriendlyLayer;

        [Header("Damage Calculation")]
        [Tooltip("Base armor reduction percentage at level 1")]
        [Range(0f, 0.5f)]
        public float BaseArmorReduction = 0.1f;
        
        [Tooltip("Critical hit damage multiplier")]
        [Range(1.5f, 3f)]
        public float CriticalDamageMultiplier = 2f;
        
        [Tooltip("Base critical hit chance")]
        [Range(0f, 0.3f)]
        public float BaseCritChance = 0.05f;

        [Header("Visual Feedback")]
        [Tooltip("Duration floating combat text stays visible")]
        [Range(0.5f, 3f)]
        public float FloatingTextDuration = 1.5f;
        
        [Tooltip("Height floating text rises")]
        [Range(0.5f, 3f)]
        public float FloatingTextRiseHeight = 1f;

        #region Singleton Access

        private static CombatConfigSO _instance;
        
        /// <summary>
        /// Gets the combat configuration instance.
        /// Loads from Resources if not already loaded.
        /// </summary>
        public static CombatConfigSO Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CombatConfigSO>("CombatConfig");
                    if (_instance == null)
                    {
                        Debug.LogWarning("[CombatConfig] No CombatConfig found in Resources. Using defaults.");
                        _instance = CreateInstance<CombatConfigSO>();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure AutoSwitchRange doesn't exceed MaxTargetRange
            if (AutoSwitchRange > MaxTargetRange)
                AutoSwitchRange = MaxTargetRange;
        }

        #endregion
    }
}
