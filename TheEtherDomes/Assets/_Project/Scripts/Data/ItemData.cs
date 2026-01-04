using System;
using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Data for an equipment item.
    /// </summary>
    [Serializable]
    public class ItemData
    {
        public string ItemId;
        public string ItemName;
        public int ItemLevel;
        public ItemRarity Rarity;
        public EquipmentSlot Slot;
        public int RequiredLevel;
        public CharacterClass[] AllowedClasses;
        
        /// <summary>
        /// Item stats as key-value pairs (e.g., "Strength" -> 10).
        /// </summary>
        public Dictionary<string, int> Stats;

        public ItemData()
        {
            ItemId = Guid.NewGuid().ToString();
            Stats = new Dictionary<string, int>();
            AllowedClasses = new CharacterClass[0];
        }

        public bool CanBeEquippedBy(CharacterClass charClass, int playerLevel)
        {
            if (playerLevel < RequiredLevel)
                return false;

            if (AllowedClasses == null || AllowedClasses.Length == 0)
                return true;

            foreach (var allowedClass in AllowedClasses)
            {
                if (allowedClass == charClass)
                    return true;
            }

            return false;
        }
    }

    /// <summary>
    /// Equipment data for a character.
    /// </summary>
    [Serializable]
    public class EquipmentData
    {
        public Dictionary<EquipmentSlot, ItemData> EquippedItems;
        public int TotalItemLevel;

        public EquipmentData()
        {
            EquippedItems = new Dictionary<EquipmentSlot, ItemData>();
            TotalItemLevel = 0;
        }

        public void Equip(ItemData item)
        {
            if (item == null) return;
            
            EquippedItems[item.Slot] = item;
            RecalculateTotalItemLevel();
        }

        public ItemData Unequip(EquipmentSlot slot)
        {
            if (EquippedItems.TryGetValue(slot, out var item))
            {
                EquippedItems.Remove(slot);
                RecalculateTotalItemLevel();
                return item;
            }
            return null;
        }

        public CharacterStats GetTotalStats()
        {
            var total = new CharacterStats();
            foreach (var item in EquippedItems.Values)
            {
                if (item?.Stats != null)
                {
                    foreach (var stat in item.Stats)
                    {
                        AddStatToCharacterStats(total, stat.Key, stat.Value);
                    }
                }
            }
            return total;
        }

        private void AddStatToCharacterStats(CharacterStats stats, string statName, int value)
        {
            switch (statName.ToLower())
            {
                case "health":
                case "maxhealth":
                    stats.MaxHealth += value;
                    stats.Health += value;
                    break;
                case "mana":
                case "maxmana":
                    stats.MaxMana += value;
                    stats.Mana += value;
                    break;
                case "strength":
                    stats.Strength += value;
                    break;
                case "intellect":
                    stats.Intellect += value;
                    break;
                case "stamina":
                    stats.Stamina += value;
                    stats.MaxHealth += value * 10;
                    stats.Health += value * 10;
                    break;
                case "attackpower":
                    stats.AttackPower += value;
                    break;
                case "spellpower":
                    stats.SpellPower += value;
                    break;
                case "armor":
                    stats.Armor += value;
                    break;
            }
        }

        private void RecalculateTotalItemLevel()
        {
            TotalItemLevel = 0;
            foreach (var item in EquippedItems.Values)
            {
                if (item != null)
                    TotalItemLevel += item.ItemLevel;
            }
        }
    }
}
