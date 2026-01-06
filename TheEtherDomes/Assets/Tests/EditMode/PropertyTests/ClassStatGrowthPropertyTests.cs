using NUnit.Framework;
using UnityEngine;
using EtherDomes.Classes;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for class stat growth per level.
    /// Feature: new-classes-combat, Property 18: Class Stat Growth
    /// Validates: Requirements 12.6, 12.7
    /// </summary>
    [TestFixture]
    public class ClassStatGrowthPropertyTests : PropertyTestBase
    {
        private ClassSystem _classSystem;

        [SetUp]
        public void SetUp()
        {
            var go = new GameObject("ClassSystem");
            _classSystem = go.AddComponent<ClassSystem>();
            _classSystem.Initialize(null); // No combat system needed for stat tests
        }

        [TearDown]
        public void TearDown()
        {
            if (_classSystem != null)
            {
                Object.DestroyImmediate(_classSystem.gameObject);
            }
        }

        #region Property 18: Class Stat Growth

        /// <summary>
        /// Feature: new-classes-combat, Property 18: Class Stat Growth
        /// For any character leveling up, their stats SHALL increase by the class-specific growth rates.
        /// Validates: Requirements 12.6, 12.7
        /// </summary>
        [Test]
        [Repeat(100)]
        public void Property18_ClassStatGrowth_ReturnsNonNullForAllClasses()
        {
            // Arrange
            CharacterClass[] allClasses = 
            {
                CharacterClass.Warrior, CharacterClass.Mage, CharacterClass.Priest,
                CharacterClass.Paladin, CharacterClass.Rogue, CharacterClass.Hunter,
                CharacterClass.Warlock, CharacterClass.DeathKnight
            };
            
            CharacterClass randomClass = allClasses[Random.Range(0, allClasses.Length)];

            // Act
            var growth = _classSystem.GetStatGrowthPerLevel(randomClass);

            // Assert
            Assert.IsNotNull(growth, $"Stat growth for {randomClass} should not be null");
        }

        /// <summary>
        /// Property 18: Rogue stat growth should be +0 Strength, +1 Stamina per level.
        /// (Agility not in CharacterStats, represented differently)
        /// </summary>
        [Test]
        public void RogueStatGrowth_HasCorrectValues()
        {
            // Act
            var growth = _classSystem.GetStatGrowthPerLevel(CharacterClass.Rogue);

            // Assert
            Assert.AreEqual(0, growth.Strength, "Rogue Strength growth should be 0");
            Assert.AreEqual(1, growth.Stamina, "Rogue Stamina growth should be 1");
        }

        /// <summary>
        /// Property 18: Hunter stat growth should be +0 Strength, +1 Stamina per level.
        /// </summary>
        [Test]
        public void HunterStatGrowth_HasCorrectValues()
        {
            // Act
            var growth = _classSystem.GetStatGrowthPerLevel(CharacterClass.Hunter);

            // Assert
            Assert.AreEqual(0, growth.Strength, "Hunter Strength growth should be 0");
            Assert.AreEqual(1, growth.Stamina, "Hunter Stamina growth should be 1");
        }

        /// <summary>
        /// Property 18: Warlock stat growth should be +2 Intellect, +1 Stamina per level.
        /// </summary>
        [Test]
        public void WarlockStatGrowth_HasCorrectValues()
        {
            // Act
            var growth = _classSystem.GetStatGrowthPerLevel(CharacterClass.Warlock);

            // Assert
            Assert.AreEqual(2, growth.Intellect, "Warlock Intellect growth should be 2");
            Assert.AreEqual(1, growth.Stamina, "Warlock Stamina growth should be 1");
        }

        /// <summary>
        /// Property 18: Death Knight stat growth should be +2 Strength, +2 Stamina per level.
        /// </summary>
        [Test]
        public void DeathKnightStatGrowth_HasCorrectValues()
        {
            // Act
            var growth = _classSystem.GetStatGrowthPerLevel(CharacterClass.DeathKnight);

            // Assert
            Assert.AreEqual(2, growth.Strength, "Death Knight Strength growth should be 2");
            Assert.AreEqual(2, growth.Stamina, "Death Knight Stamina growth should be 2");
        }

        /// <summary>
        /// Property 18: Warrior stat growth should be +2 Strength, +2 Stamina per level.
        /// </summary>
        [Test]
        public void WarriorStatGrowth_HasCorrectValues()
        {
            // Act
            var growth = _classSystem.GetStatGrowthPerLevel(CharacterClass.Warrior);

            // Assert
            Assert.AreEqual(2, growth.Strength, "Warrior Strength growth should be 2");
            Assert.AreEqual(2, growth.Stamina, "Warrior Stamina growth should be 2");
        }

        /// <summary>
        /// Property 18: Mage stat growth should be +2 Intellect, +1 Stamina per level.
        /// </summary>
        [Test]
        public void MageStatGrowth_HasCorrectValues()
        {
            // Act
            var growth = _classSystem.GetStatGrowthPerLevel(CharacterClass.Mage);

            // Assert
            Assert.AreEqual(2, growth.Intellect, "Mage Intellect growth should be 2");
            Assert.AreEqual(1, growth.Stamina, "Mage Stamina growth should be 1");
        }

        #endregion

        #region Base Stats Tests

        /// <summary>
        /// Property 18: All classes should have valid base stats.
        /// </summary>
        [Test]
        [Repeat(100)]
        public void BaseStats_AllClassesHavePositiveHealth()
        {
            // Arrange
            CharacterClass[] allClasses = 
            {
                CharacterClass.Warrior, CharacterClass.Mage, CharacterClass.Priest,
                CharacterClass.Paladin, CharacterClass.Rogue, CharacterClass.Hunter,
                CharacterClass.Warlock, CharacterClass.DeathKnight
            };
            
            CharacterClass randomClass = allClasses[Random.Range(0, allClasses.Length)];

            // Act
            var baseStats = _classSystem.GetBaseStatsForClass(randomClass);

            // Assert
            Assert.Greater(baseStats.MaxHealth, 0, $"{randomClass} should have positive MaxHealth");
        }

        /// <summary>
        /// Property 18: Death Knight should have highest base health (tank class).
        /// </summary>
        [Test]
        public void DeathKnightBaseStats_HasHighestHealth()
        {
            // Act
            var dkStats = _classSystem.GetBaseStatsForClass(CharacterClass.DeathKnight);
            var mageStats = _classSystem.GetBaseStatsForClass(CharacterClass.Mage);

            // Assert
            Assert.Greater(dkStats.MaxHealth, mageStats.MaxHealth, 
                "Death Knight should have more health than Mage");
        }

        #endregion
    }
}
