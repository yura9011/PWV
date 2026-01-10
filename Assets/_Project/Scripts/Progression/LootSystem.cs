using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;
using Random = UnityEngine.Random;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Manages loot generation and distribution.
    /// </summary>
    public class LootSystem : MonoBehaviour, ILootSystem
    {
        // Drop rate constants (percentages)
        public const float COMMON_DROP_RATE = 0.70f;
        public const float RARE_DROP_RATE = 0.25f;
        public const float EPIC_DROP_RATE = 0.05f;

        // Loot count per boss
        public const int MIN_DROPS_PER_BOSS = 2;
        public const int MAX_DROPS_PER_BOSS = 4;

        // Round robin tracking
        private int _roundRobinIndex = 0;

        public event Action<ulong, ItemData> OnItemAwarded;

        public LootDrop[] GenerateLoot(string bossId, int groupSize)
        {
            // Determine number of drops (scales slightly with group size)
            int dropCount = Mathf.Clamp(
                MIN_DROPS_PER_BOSS + (groupSize / 3),
                MIN_DROPS_PER_BOSS,
                MAX_DROPS_PER_BOSS
            );

            var drops = new List<LootDrop>();

            for (int i = 0; i < dropCount; i++)
            {
                var rarity = RollRarity();
                var item = GenerateItem(bossId, rarity);
                
                drops.Add(new LootDrop { Item = item });
            }

            Debug.Log($"[LootSystem] Generated {drops.Count} items from boss {bossId}");
            return drops.ToArray();
        }

        private ItemRarity RollRarity()
        {
            float roll = Random.value;

            if (roll < EPIC_DROP_RATE)
                return ItemRarity.Epic;
            if (roll < EPIC_DROP_RATE + RARE_DROP_RATE)
                return ItemRarity.Rare;
            return ItemRarity.Common;
        }

        private ItemData GenerateItem(string bossId, ItemRarity rarity)
        {
            // Generate a random equipment slot
            var slots = (EquipmentSlot[])Enum.GetValues(typeof(EquipmentSlot));
            var slot = slots[Random.Range(0, slots.Length)];

            // Base item level from boss (would come from BossDataSO in production)
            int baseItemLevel = GetBossItemLevel(bossId);
            int itemLevel = baseItemLevel + GetRarityBonus(rarity);

            // Generate stats based on slot and rarity
            var stats = GenerateItemStats(slot, itemLevel, rarity);

            return new ItemData
            {
                ItemId = Guid.NewGuid().ToString(),
                ItemName = GenerateItemName(slot, rarity),
                ItemLevel = itemLevel,
                Rarity = rarity,
                Slot = slot,
                Stats = stats,
                RequiredLevel = Mathf.Max(1, itemLevel - 5),
                AllowedClasses = null // All classes can use
            };
        }

        private int GetBossItemLevel(string bossId)
        {
            // In production, this would come from BossDataSO
            // For now, extract level from boss ID or return default
            return 15;
        }

        private int GetRarityBonus(ItemRarity rarity)
        {
            return rarity switch
            {
                ItemRarity.Epic => 5,
                ItemRarity.Rare => 2,
                _ => 0
            };
        }

        private ItemStats GenerateItemStats(EquipmentSlot slot, int itemLevel, ItemRarity rarity)
        {
            var stats = new ItemStats();
            float rarityMultiplier = rarity switch
            {
                ItemRarity.Epic => 1.5f,
                ItemRarity.Rare => 1.25f,
                _ => 1f
            };

            int baseStat = Mathf.RoundToInt(itemLevel * rarityMultiplier);

            // Add stats based on slot type
            switch (slot)
            {
                case EquipmentSlot.Head:
                case EquipmentSlot.Chest:
                case EquipmentSlot.Legs:
                    stats.Stamina = baseStat;
                    stats.Armor = baseStat * 2;
                    break;
                case EquipmentSlot.Shoulders:
                case EquipmentSlot.Hands:
                case EquipmentSlot.Feet:
                    stats.Stamina = baseStat / 2;
                    stats.Armor = baseStat;
                    break;
                case EquipmentSlot.MainHand:
                    stats.AttackPower = baseStat;
                    stats.Strength = baseStat / 2;
                    break;
                case EquipmentSlot.OffHand:
                    stats.Armor = baseStat;
                    stats.Stamina = baseStat / 2;
                    break;
                case EquipmentSlot.Trinket1:
                case EquipmentSlot.Trinket2:
                    // Random primary stat
                    int statChoice = Random.Range(0, 3);
                    switch (statChoice)
                    {
                        case 0: stats.Strength = baseStat; break;
                        case 1: stats.Intellect = baseStat; break;
                        case 2: stats.Stamina = baseStat; break;
                    }
                    break;
            }

            return stats;
        }

        private string GenerateItemName(EquipmentSlot slot, ItemRarity rarity)
        {
            string prefix = rarity switch
            {
                ItemRarity.Epic => "Legendary",
                ItemRarity.Rare => "Superior",
                _ => "Standard"
            };

            string slotName = slot switch
            {
                EquipmentSlot.Head => "Helm",
                EquipmentSlot.Shoulders => "Pauldrons",
                EquipmentSlot.Chest => "Chestplate",
                EquipmentSlot.Hands => "Gauntlets",
                EquipmentSlot.Legs => "Legguards",
                EquipmentSlot.Feet => "Boots",
                EquipmentSlot.MainHand => "Weapon",
                EquipmentSlot.OffHand => "Shield",
                EquipmentSlot.Trinket1 or EquipmentSlot.Trinket2 => "Trinket",
                _ => "Item"
            };

            return $"{prefix} {slotName}";
        }


        public void DistributeLoot(LootDrop[] loot, ulong[] playerIds, LootDistributionMode mode)
        {
            if (loot == null || loot.Length == 0 || playerIds == null || playerIds.Length == 0)
                return;

            switch (mode)
            {
                case LootDistributionMode.RoundRobin:
                    DistributeRoundRobin(loot, playerIds);
                    break;
                case LootDistributionMode.NeedGreed:
                    // For now, treat as round robin (full implementation would have UI)
                    DistributeRoundRobin(loot, playerIds);
                    break;
                case LootDistributionMode.MasterLooter:
                    // Items stay unassigned until master looter assigns them
                    Debug.Log("[LootSystem] Master Looter mode - items await assignment");
                    break;
            }
        }

        private void DistributeRoundRobin(LootDrop[] loot, ulong[] playerIds)
        {
            foreach (var drop in loot)
            {
                if (drop.IsAwarded) continue;

                ulong playerId = playerIds[_roundRobinIndex % playerIds.Length];
                drop.AwardedTo = playerId;
                _roundRobinIndex++;

                Debug.Log($"[LootSystem] Awarded {drop.Item.ItemName} to player {playerId}");
                OnItemAwarded?.Invoke(playerId, drop.Item);
            }
        }

        /// <summary>
        /// Manually award an item to a player (for Master Looter mode).
        /// </summary>
        public void AwardItem(LootDrop drop, ulong playerId)
        {
            if (drop.IsAwarded)
            {
                Debug.LogWarning("[LootSystem] Item already awarded");
                return;
            }

            drop.AwardedTo = playerId;
            Debug.Log($"[LootSystem] Manually awarded {drop.Item.ItemName} to player {playerId}");
            OnItemAwarded?.Invoke(playerId, drop.Item);
        }

        /// <summary>
        /// Reset round robin index (call at start of dungeon).
        /// </summary>
        public void ResetRoundRobin()
        {
            _roundRobinIndex = 0;
        }
    }
}
