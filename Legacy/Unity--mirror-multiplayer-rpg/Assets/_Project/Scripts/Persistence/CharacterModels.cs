using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Character data model for persistence and network transfer.
    /// Designed for Cross-World functionality with encryption support.
    /// </summary>
    [Serializable]
    public class CharacterData
    {
        public string CharacterId;
        public string CharacterName;
        public int Level;
        public CharacterClass Class;
        public EquipmentData Equipment;
        public Vector3 LastPosition;
        public string LastWorldId;
        public DateTime LastSaveTime;
        public byte[] IntegrityHash;

        // Stats
        public int Health;
        public int MaxHealth;
        public long Experience;
        public int Armor;
        public int Strength;
        public int Intelligence;
        public int Stamina;

        public CharacterData()
        {
            CharacterId = Guid.NewGuid().ToString();
            Equipment = new EquipmentData();
            LastSaveTime = DateTime.UtcNow;
        }

        public CharacterData(string name, CharacterClass characterClass) : this()
        {
            CharacterName = name;
            Class = characterClass;
            Level = 1;
            Health = 100;
            MaxHealth = 100;
        }
    }

    /// <summary>
    /// Equipment data containing all equipped items and calculated bonuses.
    /// </summary>
    [Serializable]
    public class EquipmentData
    {
        public List<ItemData> EquippedItems;
        public int TotalArmorValue;
        public int TotalDamageBonus;

        public EquipmentData()
        {
            EquippedItems = new List<ItemData>();
        }

        public void RecalculateTotals()
        {
            TotalArmorValue = 0;
            TotalDamageBonus = 0;

            foreach (var item in EquippedItems)
            {
                if (item.Stats.TryGetValue("armor", out int armor))
                    TotalArmorValue += armor;
                if (item.Stats.TryGetValue("damage", out int damage))
                    TotalDamageBonus += damage;
            }
        }
    }

    /// <summary>
    /// Individual item data with stats dictionary.
    /// </summary>
    [Serializable]
    public class ItemData
    {
        public string ItemId;
        public string ItemName;
        public int ItemLevel;
        public ItemRarity Rarity;
        public Dictionary<string, int> Stats;
        public bool IsBound;

        public ItemData()
        {
            ItemId = Guid.NewGuid().ToString();
            Stats = new Dictionary<string, int>();
        }

        public ItemData(string name, int level, ItemRarity rarity) : this()
        {
            ItemName = name;
            ItemLevel = level;
            Rarity = rarity;
        }
    }

    /// <summary>
    /// Character class enumeration.
    /// </summary>
    public enum CharacterClass
    {
        Warrior = 0,
        Mage = 1,
        Rogue = 2,
        Healer = 3
    }

    /// <summary>
    /// Item rarity levels.
    /// </summary>
    public enum ItemRarity
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4
    }
}
