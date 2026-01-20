using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// Configuration for enemy AI behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyAIConfig", menuName = "EtherDomes/Enemy/AI Config")]
    public class EnemyAIConfigSO : ScriptableObject
    {
        [Header("Detection")]
        [Tooltip("Distance at which enemy detects and aggros players")]
        [Range(5f, 30f)]
        public float AggroRange = 10f;

        [Tooltip("Distance at which enemy alerts nearby enemies")]
        [Range(5f, 30f)]
        public float AlertRadius = 15f;

        [Tooltip("Whether this enemy alerts nearby enemies when entering combat")]
        public bool AlertNearbyEnemies = true;

        [Header("Combat")]
        [Tooltip("Distance at which enemy can attack")]
        [Range(1f, 10f)]
        public float AttackRange = 2f;

        [Tooltip("Time between attacks in seconds")]
        [Range(0.5f, 5f)]
        public float AttackCooldown = 2f;

        [Header("Leashing")]
        [Tooltip("Maximum distance from spawn before returning")]
        [Range(20f, 100f)]
        public float LeashDistance = 40f;

        [Tooltip("Speed multiplier when returning to spawn")]
        [Range(1f, 3f)]
        public float ReturnSpeedMultiplier = 1.5f;

        [Header("Movement")]
        [Tooltip("Base movement speed")]
        [Range(1f, 10f)]
        public float MoveSpeed = 3.5f;

        [Tooltip("Rotation speed in degrees per second")]
        [Range(90f, 720f)]
        public float RotationSpeed = 360f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Ensure alert radius doesn't exceed aggro range significantly
            if (AlertRadius > AggroRange * 2f)
            {
                AlertRadius = AggroRange * 2f;
            }
        }
#endif
    }
}
