using System;
using NUnit.Framework;
using UnityEngine;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for character persistence (save/load round-trip).
    /// </summary>
    [TestFixture]
    public class PersistencePropertyTests
    {
        /// <summary>
        /// Property: Character Persistence Round-Trip
        /// Saving and loading a character should preserve all data.
        /// </summary>
        [Test]
        public void CharacterData_RoundTrip_PreservesAllFields()
        {
            // Arrange
            var original = new CharacterData
            {
                CharacterId = Guid.NewGuid().ToString(),
                CharacterName = "TestHero",
                Level = 25,
                Experience = 5000,
                Class = CharacterClass.Warrior,
                CurrentSpec = Specialization.Arms
            };

            // Act - Simulate serialization round-trip
            string json = JsonUtility.ToJson(original);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            // Assert - All fields should match
            Assert.That(loaded.CharacterId, Is.EqualTo(original.CharacterId), "CharacterId mismatch");
            Assert.That(loaded.CharacterName, Is.EqualTo(original.CharacterName), "CharacterName mismatch");
            Assert.That(loaded.Level, Is.EqualTo(original.Level), "Level mismatch");
            Assert.That(loaded.Experience, Is.EqualTo(original.Experience), "Experience mismatch");
            Assert.That(loaded.Class, Is.EqualTo(original.Class), "Class mismatch");
            Assert.That(loaded.CurrentSpec, Is.EqualTo(original.CurrentSpec), "CurrentSpec mismatch");
        }

        /// <summary>
        /// Property: All CharacterClass values can be serialized
        /// </summary>
        [Test]
        public void AllCharacterClasses_CanBeSerialized(
            [Values] CharacterClass characterClass)
        {
            var data = new CharacterData
            {
                CharacterId = "test",
                CharacterName = "Test",
                Class = characterClass
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.Class, Is.EqualTo(characterClass),
                $"CharacterClass {characterClass} should survive serialization");
        }

        /// <summary>
        /// Property: Level values are preserved exactly
        /// </summary>
        [Test]
        public void LevelValues_ArePreservedExactly(
            [Values(1, 10, 25, 50)] int level)
        {
            var data = new CharacterData
            {
                CharacterId = "test",
                CharacterName = "Test",
                Level = level
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.Level, Is.EqualTo(level),
                $"Level {level} should be preserved exactly");
        }

        /// <summary>
        /// Property: Float values maintain precision (using Experience as int example)
        /// </summary>
        [Test]
        public void IntValues_MaintainPrecision(
            [Values(0, 100, 5000, 999999)] int value)
        {
            var data = new CharacterData
            {
                CharacterId = "test",
                CharacterName = "Test",
                Experience = value
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.Experience, Is.EqualTo(value),
                $"Int value {value} should maintain precision");
        }

        /// <summary>
        /// Property: Empty strings are handled correctly
        /// </summary>
        [Test]
        public void EmptyStrings_AreHandledCorrectly()
        {
            var data = new CharacterData
            {
                CharacterId = "",
                CharacterName = ""
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.CharacterId, Is.EqualTo(""),
                "Empty CharacterId should be preserved");
            Assert.That(loaded.CharacterName, Is.EqualTo(""),
                "Empty CharacterName should be preserved");
        }

        /// <summary>
        /// Property: Special characters in names are preserved
        /// </summary>
        [Test]
        public void SpecialCharacters_ArePreserved()
        {
            var specialNames = new[]
            {
                "Test Name",
                "Test-Name",
                "Test_Name",
                "TestName123",
                "Tëst Nàmé"
            };

            foreach (var name in specialNames)
            {
                var data = new CharacterData
                {
                    CharacterId = "test",
                    CharacterName = name
                };

                string json = JsonUtility.ToJson(data);
                var loaded = JsonUtility.FromJson<CharacterData>(json);

                Assert.That(loaded.CharacterName, Is.EqualTo(name),
                    $"Special name '{name}' should be preserved");
            }
        }

        /// <summary>
        /// Property: Default values are consistent
        /// </summary>
        [Test]
        public void DefaultValues_AreConsistent()
        {
            var data1 = new CharacterData();
            var data2 = new CharacterData();

            Assert.That(data1.Level, Is.EqualTo(data2.Level),
                "Default Level should be consistent");
            Assert.That(data1.Experience, Is.EqualTo(data2.Experience),
                "Default Experience should be consistent");
            Assert.That(data1.Class, Is.EqualTo(data2.Class),
                "Default Class should be consistent");
        }
    }
}
