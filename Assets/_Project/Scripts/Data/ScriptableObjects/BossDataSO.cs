using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// ScriptableObject for defining boss encounters in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBoss", menuName = "EtherDomes/Boss")]
    public class BossDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string BossId;
        public string BossName;
        [TextArea(2, 4)]
        public string Description;

        [Header("Stats")]
        public int BaseHealth = 10000;
        public int BaseDamage = 100;
        public int Level = 15;

        [Header("Scaling")]
        [Tooltip("Health multiplier per additional player (e.g., 0.2 = +20% per player)")]
        public float HealthScalingPerPlayer = 0.2f;
        [Tooltip("Damage multiplier per additional player")]
        public float DamageScalingPerPlayer = 0.1f;

        [Header("Phases")]
        [Tooltip("Health percentages that trigger phase transitions")]
        public float[] PhaseThresholds = { 0.75f, 0.50f, 0.25f };

        [Header("Loot")]
        public LootTableEntry[] LootTable;

        [Header("Trophy")]
        public string TrophyId;
        public string TrophyName;

        public int GetScaledHealth(int groupSize)
        {
            float multiplier = 1f + (HealthScalingPerPlayer * (groupSize - 1));
            return Mathf.RoundToInt(BaseHealth * multiplier);
        }

        public int GetScaledDamage(int groupSize)
        {
            float multiplier = 1f + (DamageScalingPerPlayer * (groupSize - 1));
            return Mathf.RoundToInt(BaseDamage * multiplier);
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(BossId))
            {
                BossId = System.Guid.NewGuid().ToString();
            }
        }
    }

    /// <summary>
    /// Entry in a boss's loot table.
    /// </summary>
    [System.Serializable]
    public class LootTableEntry
    {
        public ItemDataSO Item;
        [Range(0f, 1f)]
        public float DropChance = 0.1f;
    }
}
