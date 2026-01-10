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
            return new ItemData
            {
                ItemId = ItemId,
                ItemName = ItemName,
                Description = Description ?? "",
                ItemLevel = ItemLevel,
                Rarity = Rarity,
                Type = ItemType.Equipment,
                Slot = Slot,
                RequiredLevel = RequiredLevel,
                AllowedClasses = AllowedClasses ?? new CharacterClass[0],
                Stats = new ItemStats
                {
                    Strength = BonusStrength,
                    Intellect = BonusIntellect,
                    Stamina = BonusStamina,
                    AttackPower = BonusAttackPower,
                    SpellPower = BonusSpellPower,
                    Armor = BonusArmor
                }
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
