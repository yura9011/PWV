using System;
using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Stats for an item.
    /// </summary>
    [Serializable]
    public class ItemStats
    {
        public int Strength;
        public int Agility;
        public int Intellect;
        public int Stamina;
        public int AttackPower;
        public int SpellPower;
        public int Armor;
        public int CriticalStrike;
        public int Haste;
        
        public ItemStats()
        {
            Strength = 0;
            Agility = 0;
            Intellect = 0;
            Stamina = 0;
            AttackPower = 0;
            SpellPower = 0;
            Armor = 0;
            CriticalStrike = 0;
            Haste = 0;
        }
    }

    /// <summary>
    /// Data for an equipment item.
    /// </summary>
    [Serializable]
    public class ItemData
    {
        public string ItemId;
        public string ItemName;
        public string Description;
        public int ItemLevel;
        public ItemRarity Rarity;
        public ItemType Type;
        public EquipmentSlot Slot;
        public int RequiredLevel;
        public CharacterClass[] AllowedClasses;
        public int StackCount = 1;
        
        /// <summary>
        /// Item stats.
        /// </summary>
        public ItemStats Stats;

        /// <summary>
        /// Maximum durability of the item.
        /// Requirements: 8.5
        /// </summary>
        public int MaxDurability;

        /// <summary>
        /// Current durability of the item.
        /// Requirements: 8.5
        /// </summary>
        public int CurrentDurability;

        /// <summary>
        /// Returns true if the item is broken (durability = 0).
        /// </summary>
        public bool IsBroken => CurrentDurability <= 0 && MaxDurability > 0;

        /// <summary>
        /// Returns the durability percentage (0-1).
        /// </summary>
        public float DurabilityPercent => MaxDurability > 0 ? (float)CurrentDurability / MaxDurability : 1f;

        public ItemData()
        {
            ItemId = Guid.NewGuid().ToString();
            Stats = new ItemStats();
            AllowedClasses = new CharacterClass[0];
            MaxDurability = 100;
            CurrentDurability = 100;
            StackCount = 1;
            Type = ItemType.Equipment;
            Description = "";
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
                    total.Strength += item.Stats.Strength;
                    total.Intellect += item.Stats.Intellect;
                    total.Stamina += item.Stats.Stamina;
                    total.MaxHealth += item.Stats.Stamina * 10;
                    total.Health += item.Stats.Stamina * 10;
                    total.AttackPower += item.Stats.AttackPower;
                    total.SpellPower += item.Stats.SpellPower;
                    total.Armor += item.Stats.Armor;
                }
            }
            return total;
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
