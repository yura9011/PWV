using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Converts items to crafting materials through salvaging.
    /// 
    /// Material yields by rarity:
    /// - Common: 1 material
    /// - Uncommon: 1-2 materials
    /// - Rare: 2-3 materials
    /// - Epic: 3-5 materials
    /// - Legendary: 5-7 materials
    /// 
    /// Requirements: 4.7, 4.8
    /// </summary>
    public class SalvageSystem : ISalvageSystem
    {
        // Material names by rarity
        private static readonly string[] MaterialNames = new string[]
        {
            "Scrap Metal",      // Common
            "Quality Parts",    // Uncommon
            "Rare Components",  // Rare
            "Epic Essence",     // Epic
            "Legendary Core"    // Legendary
        };

        // Min/Max material yields by rarity index
        private static readonly int[] MinYields = { 1, 1, 2, 3, 5 };
        private static readonly int[] MaxYields = { 1, 2, 3, 5, 7 };

        public event Action<SalvageResult> OnItemSalvaged;

        public SalvageResult Salvage(ItemData item)
        {
            var result = new SalvageResult
            {
                SalvagedItem = item,
                Materials = new Dictionary<string, int>(),
                Success = false
            };

            if (!CanSalvage(item))
            {
                Debug.LogWarning($"[SalvageSystem] Cannot salvage item: {item?.ItemName ?? "null"}");
                return result;
            }

            result.Materials = PreviewSalvage(item);
            result.Success = true;

            Debug.Log($"[SalvageSystem] Salvaged {item.ItemName} for {FormatMaterials(result.Materials)}");
            OnItemSalvaged?.Invoke(result);

            return result;
        }

        public Dictionary<string, int> PreviewSalvage(ItemData item)
        {
            var materials = new Dictionary<string, int>();

            if (item == null)
                return materials;

            int rarityIndex = (int)item.Rarity;
            if (rarityIndex < 0 || rarityIndex >= MaterialNames.Length)
                return materials;

            // Get material name and yield range for this rarity
            string materialName = MaterialNames[rarityIndex];
            int minYield = MinYields[rarityIndex];
            int maxYield = MaxYields[rarityIndex];

            // Random yield within range
            int yield = UnityEngine.Random.Range(minYield, maxYield + 1);

            // Higher item level gives bonus materials
            int bonusMaterials = item.ItemLevel / 20; // +1 per 20 item levels
            yield += bonusMaterials;

            materials[materialName] = yield;

            // Rare+ items also give lower tier materials
            if (rarityIndex >= (int)ItemRarity.Rare)
            {
                string lowerMaterial = MaterialNames[rarityIndex - 1];
                materials[lowerMaterial] = UnityEngine.Random.Range(1, 3);
            }

            return materials;
        }

        public bool CanSalvage(ItemData item)
        {
            if (item == null)
                return false;

            // All items can be salvaged
            return true;
        }

        /// <summary>
        /// Gets the minimum material yield for a rarity.
        /// Used for testing.
        /// </summary>
        public static int GetMinYield(ItemRarity rarity)
        {
            int index = (int)rarity;
            if (index >= 0 && index < MinYields.Length)
                return MinYields[index];
            return 0;
        }

        /// <summary>
        /// Gets the maximum material yield for a rarity.
        /// Used for testing.
        /// </summary>
        public static int GetMaxYield(ItemRarity rarity)
        {
            int index = (int)rarity;
            if (index >= 0 && index < MaxYields.Length)
                return MaxYields[index];
            return 0;
        }

        private string FormatMaterials(Dictionary<string, int> materials)
        {
            var parts = new List<string>();
            foreach (var kvp in materials)
            {
                parts.Add($"{kvp.Value}x {kvp.Key}");
            }
            return string.Join(", ", parts);
        }
    }
}
