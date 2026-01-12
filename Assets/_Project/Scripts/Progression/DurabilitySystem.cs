using System;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Manages item durability degradation and repair.
    /// 
    /// - Items degrade durability when hit in combat
    /// - Broken items (durability = 0) have 50% stat penalty
    /// - Repair cost based on item level and rarity
    /// 
    /// Requirements: 8.5, 8.6
    /// </summary>
    public class DurabilitySystem : IDurabilitySystem
    {
        /// <summary>
        /// Stat penalty for broken items (50% = 0.5).
        /// Requirements: 8.6
        /// </summary>
        public const float BROKEN_ITEM_STAT_PENALTY = 0.5f;

        /// <summary>
        /// Base repair cost per durability point.
        /// </summary>
        public const int BASE_REPAIR_COST_PER_POINT = 1;

        /// <summary>
        /// Rarity multiplier for repair costs.
        /// </summary>
        private static readonly float[] RarityRepairMultiplier = new float[]
        {
            1.0f,   // Common
            1.5f,   // Uncommon
            2.0f,   // Rare
            3.0f,   // Epic
            5.0f    // Legendary
        };

        public float BrokenItemStatPenalty => BROKEN_ITEM_STAT_PENALTY;

        public event Action<ItemData> OnItemBroken;
        public event Action<ItemData> OnItemRepaired;
        public event Action<ItemData, int, int> OnDurabilityChanged;

        public void DegradeDurability(ItemData item, int amount = 1)
        {
            if (item == null || item.MaxDurability <= 0) return;
            if (amount <= 0) return;

            int previousDurability = item.CurrentDurability;
            
            // Don't degrade already broken items
            if (previousDurability <= 0) return;

            item.CurrentDurability = Math.Max(0, item.CurrentDurability - amount);

            OnDurabilityChanged?.Invoke(item, previousDurability, item.CurrentDurability);

            // Check if item just broke
            if (previousDurability > 0 && item.CurrentDurability <= 0)
            {
                Debug.Log($"[DurabilitySystem] Item {item.ItemName} has broken!");
                OnItemBroken?.Invoke(item);
            }
        }

        public int RepairItem(ItemData item)
        {
            if (item == null || item.MaxDurability <= 0) return 0;
            if (item.CurrentDurability >= item.MaxDurability) return 0;

            int cost = GetRepairCost(item);
            int previousDurability = item.CurrentDurability;
            
            item.CurrentDurability = item.MaxDurability;

            OnDurabilityChanged?.Invoke(item, previousDurability, item.CurrentDurability);
            OnItemRepaired?.Invoke(item);

            Debug.Log($"[DurabilitySystem] Repaired {item.ItemName} for {cost} gold");
            return cost;
        }

        public float GetStatPenalty(ItemData item)
        {
            if (item == null) return 1f;
            
            // Items without durability have no penalty
            if (item.MaxDurability <= 0) return 1f;

            // Broken items have 50% stat penalty
            if (item.CurrentDurability <= 0)
            {
                return BROKEN_ITEM_STAT_PENALTY;
            }

            // Functional items have no penalty
            return 1f;
        }

        public int GetRepairCost(ItemData item)
        {
            if (item == null || item.MaxDurability <= 0) return 0;
            if (item.CurrentDurability >= item.MaxDurability) return 0;

            int durabilityLost = item.MaxDurability - item.CurrentDurability;
            float rarityMultiplier = GetRarityMultiplier(item.Rarity);
            float itemLevelMultiplier = 1f + (item.ItemLevel * 0.1f);

            int cost = (int)(durabilityLost * BASE_REPAIR_COST_PER_POINT * rarityMultiplier * itemLevelMultiplier);
            return Math.Max(1, cost);
        }

        public bool NeedsRepair(ItemData item)
        {
            if (item == null || item.MaxDurability <= 0) return false;
            return item.CurrentDurability < item.MaxDurability;
        }

        private float GetRarityMultiplier(ItemRarity rarity)
        {
            int index = (int)rarity;
            if (index >= 0 && index < RarityRepairMultiplier.Length)
            {
                return RarityRepairMultiplier[index];
            }
            return 1f;
        }
    }
}
