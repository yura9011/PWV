using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Tests.Generators
{
    /// <summary>
    /// Generators for test data used in property-based tests.
    /// Requirements: 22.3
    /// </summary>
    public static class TestDataGenerators
    {
        #region CharacterData Generator

        public static CharacterData GenerateCharacterData()
        {
            var classes = new[] { CharacterClass.Warrior, CharacterClass.Mage, CharacterClass.Priest, CharacterClass.Paladin };
            var characterClass = classes[Random.Range(0, classes.Length)];

            return new CharacterData
            {
                CharacterId = System.Guid.NewGuid().ToString(),
                CharacterName = GenerateRandomName(),
                Class = characterClass,
                Level = Random.Range(1, 61),
                Experience = Random.Range(0, 100000),
                CurrentMana = Random.Range(0f, 5000f),
                MaxMana = Random.Range(100f, 5000f),
                DataVersion = 1
            };
        }

        public static CharacterData GenerateCharacterDataWithClass(CharacterClass characterClass)
        {
            var data = GenerateCharacterData();
            data.Class = characterClass;
            return data;
        }

        #endregion

        #region ItemData Generator

        public static ItemData GenerateItemData()
        {
            var rarities = new[] { ItemRarity.Common, ItemRarity.Rare, ItemRarity.Epic };
            var slots = new[] { EquipmentSlot.Head, EquipmentSlot.Chest, EquipmentSlot.Hands, EquipmentSlot.Legs, EquipmentSlot.Feet };

            return new ItemData
            {
                ItemId = System.Guid.NewGuid().ToString(),
                ItemName = GenerateRandomItemName(),
                Rarity = rarities[Random.Range(0, rarities.Length)],
                Slot = slots[Random.Range(0, slots.Length)],
                RequiredLevel = Random.Range(1, 61),
                MaxDurability = Random.Range(50, 200),
                CurrentDurability = Random.Range(0, 200),
                Stats = GenerateItemStats()
            };
        }

        public static ItemData GenerateItemDataWithRarity(ItemRarity rarity)
        {
            var item = GenerateItemData();
            item.Rarity = rarity;
            return item;
        }

        public static ItemData GenerateBrokenItem()
        {
            var item = GenerateItemData();
            item.CurrentDurability = 0;
            return item;
        }

        #endregion

        #region AbilityData Generator

        public static AbilityData GenerateAbilityData()
        {
            var types = new[] { AbilityType.Damage, AbilityType.Healing, AbilityType.Buff, AbilityType.Debuff };
            var damageTypes = new[] { DamageType.Physical, DamageType.Fire, DamageType.Frost, DamageType.Holy, DamageType.Shadow };

            return new AbilityData
            {
                AbilityId = System.Guid.NewGuid().ToString(),
                AbilityName = GenerateRandomAbilityName(),
                Description = "A test ability",
                CastTime = Random.Range(0f, 3f),
                Cooldown = Random.Range(0f, 60f),
                ManaCost = Random.Range(0f, 100f),
                Range = Random.Range(0f, 40f),
                RequiresTarget = Random.value > 0.3f,
                AffectedByGCD = Random.value > 0.2f,
                Type = types[Random.Range(0, types.Length)],
                DamageType = damageTypes[Random.Range(0, damageTypes.Length)],
                BaseDamage = Random.Range(10f, 200f),
                BaseHealing = Random.Range(10f, 200f),
                UnlockLevel = Random.Range(1, 61)
            };
        }

        public static AbilityData GenerateInstantAbility()
        {
            var ability = GenerateAbilityData();
            ability.CastTime = 0f;
            return ability;
        }

        public static AbilityData GenerateCastTimeAbility()
        {
            var ability = GenerateAbilityData();
            ability.CastTime = Random.Range(1f, 4f);
            return ability;
        }

        #endregion

        #region CharacterStats Generator

        public static CharacterStats GenerateCharacterStats()
        {
            return new CharacterStats
            {
                Strength = Random.Range(1, 100),
                Intellect = Random.Range(1, 100),
                Stamina = Random.Range(1, 100),
                Armor = Random.Range(0, 1000),
                CritChance = Random.Range(0f, 100f),
                Haste = Random.Range(0f, 100f),
                Mastery = Random.Range(0f, 100f)
            };
        }

        #endregion

        #region ItemStats Generator

        public static ItemStats GenerateItemStats()
        {
            return new ItemStats
            {
                Strength = Random.Range(1, 100),
                Agility = Random.Range(1, 100),
                Intellect = Random.Range(1, 100),
                Stamina = Random.Range(1, 100),
                AttackPower = Random.Range(0, 50),
                SpellPower = Random.Range(0, 50),
                Armor = Random.Range(0, 500),
                CriticalStrike = Random.Range(0, 50),
                Haste = Random.Range(0, 50)
            };
        }

        #endregion

        #region Helper Methods

        private static string GenerateRandomName()
        {
            string[] prefixes = { "Shadow", "Light", "Dark", "Storm", "Fire", "Ice", "Thunder", "Earth" };
            string[] suffixes = { "walker", "blade", "heart", "soul", "fist", "eye", "hand", "mind" };
            return prefixes[Random.Range(0, prefixes.Length)] + suffixes[Random.Range(0, suffixes.Length)];
        }

        private static string GenerateRandomItemName()
        {
            string[] adjectives = { "Ancient", "Blessed", "Cursed", "Divine", "Enchanted", "Mystic", "Sacred", "Unholy" };
            string[] nouns = { "Helm", "Armor", "Gauntlets", "Greaves", "Boots", "Ring", "Amulet", "Cloak" };
            return adjectives[Random.Range(0, adjectives.Length)] + " " + nouns[Random.Range(0, nouns.Length)];
        }

        private static string GenerateRandomAbilityName()
        {
            string[] verbs = { "Strike", "Blast", "Heal", "Shield", "Curse", "Bless", "Smite", "Drain" };
            string[] elements = { "Fire", "Ice", "Shadow", "Holy", "Nature", "Arcane", "Lightning", "Void" };
            return elements[Random.Range(0, elements.Length)] + " " + verbs[Random.Range(0, verbs.Length)];
        }

        #endregion
    }
}
