using System;
using NUnit.Framework;
using UnityEngine;
using EtherDomes.Data;
using EtherDomes.Persistence;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for character persistence (save/load round-trip).
    /// </summary>
    [TestFixture]
    public class PersistencePropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 1: Session Lock Mutual Exclusion
        /// For any character ID, if TryAcquireLock returns true, subsequent calls 
        /// to TryAcquireLock with the same ID SHALL return false until ReleaseLock is called.
        /// Validates: Requirements 1.1, 1.2
        /// </summary>
        [Test]
        [Repeat(100)] // Property test: run 100 iterations
        public void SessionLock_MutualExclusion_SecondLockFails()
        {
            // Arrange
            var lockManager = new SessionLockManager(TimeSpan.FromMinutes(5));
            var characterId = $"test-char-{Guid.NewGuid()}";
            
            try
            {
                // Act - First lock should succeed
                bool firstLock = lockManager.TryAcquireLock(characterId);
                
                // Act - Second lock should fail (mutual exclusion)
                bool secondLock = lockManager.TryAcquireLock(characterId);
                
                // Assert
                Assert.That(firstLock, Is.True, "First lock acquisition should succeed");
                Assert.That(secondLock, Is.False, "Second lock acquisition should fail while first is held");
                
                // Act - Release and try again
                lockManager.ReleaseLock(characterId);
                bool thirdLock = lockManager.TryAcquireLock(characterId);
                
                // Assert - After release, lock should be available
                Assert.That(thirdLock, Is.True, "Lock should be available after release");
            }
            finally
            {
                // Cleanup
                lockManager.ReleaseAllLocks();
            }
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 1: Session Lock Mutual Exclusion (IsLocked check)
        /// Validates: Requirements 1.1, 1.2
        /// </summary>
        [Test]
        [Repeat(100)]
        public void SessionLock_IsLocked_ReflectsLockState()
        {
            // Arrange
            var lockManager = new SessionLockManager(TimeSpan.FromMinutes(5));
            var characterId = $"test-char-{Guid.NewGuid()}";
            
            try
            {
                // Assert - Initially not locked
                Assert.That(lockManager.IsLocked(characterId), Is.False, "Character should not be locked initially");
                
                // Act - Acquire lock
                lockManager.TryAcquireLock(characterId);
                
                // Assert - Now locked
                Assert.That(lockManager.IsLocked(characterId), Is.True, "Character should be locked after acquisition");
                
                // Act - Release lock
                lockManager.ReleaseLock(characterId);
                
                // Assert - No longer locked
                Assert.That(lockManager.IsLocked(characterId), Is.False, "Character should not be locked after release");
            }
            finally
            {
                lockManager.ReleaseAllLocks();
            }
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 2: Stale Lock Cleanup
        /// For any lock older than StaleLockThreshold (5 minutes), CleanupStaleLocks 
        /// SHALL remove it and allow new acquisition.
        /// Validates: Requirements 1.4
        /// </summary>
        [Test]
        public void SessionLock_StaleLockCleanup_AllowsNewAcquisition()
        {
            // Arrange - Use very short threshold for testing
            var shortThreshold = TimeSpan.FromMilliseconds(50);
            var lockManager = new SessionLockManager(shortThreshold);
            var characterId = $"test-char-{Guid.NewGuid()}";
            
            try
            {
                // Act - Acquire lock
                bool firstLock = lockManager.TryAcquireLock(characterId);
                Assert.That(firstLock, Is.True);
                
                // Wait for lock to become stale
                System.Threading.Thread.Sleep(100);
                
                // Cleanup stale locks
                lockManager.CleanupStaleLocks(shortThreshold);
                
                // Act - Try to acquire again (should succeed because old lock was stale)
                bool secondLock = lockManager.TryAcquireLock(characterId);
                
                // Assert
                Assert.That(secondLock, Is.True, "Lock should be available after stale cleanup");
            }
            finally
            {
                lockManager.ReleaseAllLocks();
            }
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 4: Migration Idempotence
        /// For any CharacterData at CurrentVersion, calling Migrate SHALL return 
        /// an equivalent object (no changes).
        /// Validates: Requirements 1.6
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DataMigration_Idempotence_NoChangesForCurrentVersion()
        {
            // Arrange
            var migrationService = new DataMigrationService();
            var originalData = new CharacterData
            {
                CharacterId = Guid.NewGuid().ToString(),
                Name = $"TestChar_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Level = UnityEngine.Random.Range(1, 60),
                CurrentXP = UnityEngine.Random.Range(0, 100000),
                ClassID = (int)CharacterClass.Cruzado
            };

            // Store original values
            string originalId = originalData.CharacterId;
            string originalName = originalData.Name;
            int originalLevel = originalData.Level;
            float originalExp = originalData.CurrentXP;

            // Act - Migrate (should be no-op for new CharacterData)
            var migratedData = migrationService.Migrate(originalData);

            // Assert - All values should be unchanged
            Assert.That(migratedData.CharacterId, Is.EqualTo(originalId), "CharacterId should be unchanged");
            Assert.That(migratedData.Name, Is.EqualTo(originalName), "Name should be unchanged");
            Assert.That(migratedData.Level, Is.EqualTo(originalLevel), "Level should be unchanged");
            Assert.That(migratedData.CurrentXP, Is.EqualTo(originalExp), "CurrentXP should be unchanged");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 5: Migration Preserves Identity
        /// For any CharacterData before and after migration, CharacterId, Name, 
        /// Level, and CurrentXP SHALL remain unchanged.
        /// Validates: Requirements 1.7
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DataMigration_PreservesIdentity_CoreFieldsUnchanged()
        {
            // Arrange
            var migrationService = new DataMigrationService();
            var originalData = new CharacterData
            {
                CharacterId = Guid.NewGuid().ToString(),
                Name = $"TestChar_{Guid.NewGuid().ToString().Substring(0, 8)}",
                Level = UnityEngine.Random.Range(1, 60),
                CurrentXP = UnityEngine.Random.Range(0, 100000),
                ClassID = (int)CharacterClass.MaestroElemental
            };

            // Store original identity values
            string originalId = originalData.CharacterId;
            string originalName = originalData.Name;
            int originalLevel = originalData.Level;
            float originalExp = originalData.CurrentXP;
            int originalClassId = originalData.ClassID;

            // Act - Migrate
            var migratedData = migrationService.Migrate(originalData);

            // Assert - Identity fields should be preserved
            Assert.That(migratedData.CharacterId, Is.EqualTo(originalId), "CharacterId must be preserved");
            Assert.That(migratedData.Name, Is.EqualTo(originalName), "Name must be preserved");
            Assert.That(migratedData.Level, Is.EqualTo(originalLevel), "Level must be preserved");
            Assert.That(migratedData.CurrentXP, Is.EqualTo(originalExp), "CurrentXP must be preserved");
            Assert.That(migratedData.ClassID, Is.EqualTo(originalClassId), "ClassID must be preserved");
        }

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
                Name = "TestHero",
                Level = 25,
                CurrentXP = 5000,
                ClassID = (int)CharacterClass.Cruzado
            };

            // Act - Simulate serialization round-trip
            string json = JsonUtility.ToJson(original);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            // Assert - All fields should match
            Assert.That(loaded.CharacterId, Is.EqualTo(original.CharacterId), "CharacterId mismatch");
            Assert.That(loaded.Name, Is.EqualTo(original.Name), "Name mismatch");
            Assert.That(loaded.Level, Is.EqualTo(original.Level), "Level mismatch");
            Assert.That(loaded.CurrentXP, Is.EqualTo(original.CurrentXP), "CurrentXP mismatch");
            Assert.That(loaded.ClassID, Is.EqualTo(original.ClassID), "ClassID mismatch");
        }

        /// <summary>
        /// Property: All CharacterClass values can be serialized
        /// </summary>
        [Test]
        public void AllCharacterClasses_CanBeSerialized(
            [Values(CharacterClass.Cruzado, CharacterClass.Protector, CharacterClass.Berserker, 
                    CharacterClass.Arquero, CharacterClass.MaestroElemental, CharacterClass.CaballeroRunico,
                    CharacterClass.Clerigo, CharacterClass.MedicoBrujo)] CharacterClass characterClass)
        {
            var data = new CharacterData
            {
                CharacterId = "test",
                Name = "Test",
                ClassID = (int)characterClass
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.ClassID, Is.EqualTo((int)characterClass),
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
                Name = "Test",
                Level = level
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.Level, Is.EqualTo(level),
                $"Level {level} should be preserved exactly");
        }

        /// <summary>
        /// Property: Float values maintain precision
        /// </summary>
        [Test]
        public void FloatValues_MaintainPrecision(
            [Values(0f, 100f, 5000f, 999999f)] float value)
        {
            var data = new CharacterData
            {
                CharacterId = "test",
                Name = "Test",
                CurrentXP = value
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.CurrentXP, Is.EqualTo(value),
                $"Float value {value} should maintain precision");
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
                Name = ""
            };

            string json = JsonUtility.ToJson(data);
            var loaded = JsonUtility.FromJson<CharacterData>(json);

            Assert.That(loaded.CharacterId, Is.EqualTo(""),
                "Empty CharacterId should be preserved");
            Assert.That(loaded.Name, Is.EqualTo(""),
                "Empty Name should be preserved");
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
                    Name = name
                };

                string json = JsonUtility.ToJson(data);
                var loaded = JsonUtility.FromJson<CharacterData>(json);

                Assert.That(loaded.Name, Is.EqualTo(name),
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
            Assert.That(data1.CurrentXP, Is.EqualTo(data2.CurrentXP),
                "Default CurrentXP should be consistent");
            Assert.That(data1.ClassID, Is.EqualTo(data2.ClassID),
                "Default ClassID should be consistent");
        }
    }
}
