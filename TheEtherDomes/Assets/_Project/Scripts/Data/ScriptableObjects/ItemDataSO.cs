using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// ScriptableObject for defining items in the Unity Editor.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "EtherDomes/Item")]
    public class ItemDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string ItemId;
        public string ItemName;
        [TextArea(2, 4)]
        public string Description;
        public Sprite Icon;

        [Header("Item Properties")]
        public int ItemLevel = 1;
        public ItemRarity Rarity = ItemRarity.Common;
        public EquipmentSlot Slot;
        public int RequiredLevel = 1;

        [Header("Class Restrictions")]
        [Tooltip("Leave empty for all classes")]
        public CharacterClass[] AllowedClasses;

        [Header("Stats")]
        public int BonusHealth = 0;
        public int BonusMana = 0;
        public int BonusStrength = 0;
        public int BonusIntellect = 0;
        public int BonusStamina = 0;
        public int BonusAttackPower = 0;
        public int BonusSpellPower = 0;
        public int BonusArmor = 0;

        public ItemData ToItemData()
        {
            // Convert stats to Dictionary<string, int> format
            var stats = new Dictionary<string, int>();
            
            if (BonusHealth > 0) stats["MaxHealth"] = BonusHealth;
            if (BonusMana > 0) stats["MaxMana"] = BonusMana;
            if (BonusStrength > 0) stats["Strength"] = BonusStrength;
            if (BonusIntellect > 0) stats["Intellect"] = BonusIntellect;
            if (BonusStamina > 0) stats["Stamina"] = BonusStamina;
            if (BonusAttackPower > 0) stats["AttackPower"] = BonusAttackPower;
            if (BonusSpellPower > 0) stats["SpellPower"] = BonusSpellPower;
            if (BonusArmor > 0) stats["Armor"] = BonusArmor;

            return new ItemData
            {
                ItemId = ItemId,
                ItemName = ItemName,
                ItemLevel = ItemLevel,
                Rarity = Rarity,
                Slot = Slot,
                RequiredLevel = RequiredLevel,
                AllowedClasses = AllowedClasses ?? new CharacterClass[0],
                Stats = stats
            };
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(ItemId))
            {
                ItemId = System.Guid.NewGuid().ToString();
            }
        }
    }
}
