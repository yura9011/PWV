using NUnit.Framework;
using UnityEngine;
using EtherDomes.Persistence;
using System;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace EtherDomes.Tests
{
    /// <summary>
    /// Property-based and unit tests for CharacterPersistenceService.
    /// </summary>
    [TestFixture]
    public class CharacterPersistenceTests
    {
        private CharacterPersistenceService _persistenceService;
        private byte[] _testKey;
        private byte[] _testIV;

        [SetUp]
        public void SetUp()
        {
            // Use fixed key/IV for deterministic tests
            _testKey = new byte[32];
            _testIV = new byte[16];
            for (int i = 0; i < 32; i++) _testKey[i] = (byte)(i + 1);
            for (int i = 0; i < 16; i++) _testIV[i] = (byte)(i + 100);
            
            _persistenceService = new CharacterPersistenceService(_testKey, _testIV);
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test files
            string savePath = Path.Combine(Application.persistentDataPath, "Characters");
            if (Directory.Exists(savePath))
            {
                foreach (var file in Directory.GetFiles(savePath, "*.edc"))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }

        #region Property 10: Character Persistence Round-Trip

        /// <summary>
        /// Feature: network-player-foundation, Property 10: Character Persistence Round-Trip
        /// For any valid CharacterData, SaveCharacterAsync followed by LoadCharacterAsync
        /// SHALL return an equivalent CharacterData object.
        /// Validates: Requirements 7.1, 7.3
        /// </summary>
        [Test]
        public async Task Property10_CharacterPersistenceRoundTrip_PreservesData()
        {
            // Generate test character
            var original = CreateTestCharacter("TestHero", CharacterClass.Warrior);
            original.Level = 42;
            original.Health = 85;
            original.Experience = 12345;

            // Save
            bool saved = await _persistenceService.SaveCharacterAsync(original);
            Assert.IsTrue(saved, "Save should succeed");

            // Load
            var loaded = await _persistenceService.LoadCharacterAsync(original.CharacterId);
            
            // Verify round-trip
            Assert.IsNotNull(loaded, "Loaded character should not be null");
            Assert.AreEqual(original.CharacterId, loaded.CharacterId);
            Assert.AreEqual(original.CharacterName, loaded.CharacterName);
            Assert.AreEqual(original.Level, loaded.Level);
            Assert.AreEqual(original.Class, loaded.Class);
            Assert.AreEqual(original.Health, loaded.Health);
            Assert.AreEqual(original.Experience, loaded.Experience);
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 10: Character Persistence Round-Trip
        /// Tests with multiple random characters.
        /// Validates: Requirements 7.1, 7.3
        /// </summary>
        [Test]
        public async Task Property10_CharacterPersistenceRoundTrip_MultipleCharacters()
        {
            var random = new System.Random(42);
            
            for (int i = 0; i < 10; i++)
            {
                var original = CreateRandomCharacter(random, $"Hero{i}");
                
                bool saved = await _persistenceService.SaveCharacterAsync(original);
                Assert.IsTrue(saved, $"Save should succeed for character {i}");

                var loaded = await _persistenceService.LoadCharacterAsync(original.CharacterId);
                
                Assert.IsNotNull(loaded, $"Loaded character {i} should not be null");
                Assert.AreEqual(original.CharacterId, loaded.CharacterId);
                Assert.AreEqual(original.CharacterName, loaded.CharacterName);
                Assert.AreEqual(original.Level, loaded.Level);
            }
        }

        #endregion

        #region Property 11: Network Export/Import Round-Trip

        /// <summary>
        /// Feature: network-player-foundation, Property 11: Network Export/Import Round-Trip
        /// For any valid CharacterData, ExportCharacterForNetwork followed by ImportCharacterFromNetwork
        /// SHALL return an equivalent CharacterData object.
        /// Validates: Requirements 7.5
        /// </summary>
        [Test]
        public void Property11_NetworkExportImportRoundTrip_PreservesData()
        {
            var original = CreateTestCharacter("NetworkHero", CharacterClass.Mage);
            original.Level = 50;
            original.LastWorldId = "world-123";

            // Export
            byte[] exported = _persistenceService.ExportCharacterForNetwork(original);
            Assert.IsNotNull(exported, "Export should produce data");
            Assert.Greater(exported.Length, 0, "Exported data should not be empty");

            // Import
            var imported = _persistenceService.ImportCharacterFromNetwork(exported);

            // Verify round-trip
            Assert.IsNotNull(imported, "Imported character should not be null");
            Assert.AreEqual(original.CharacterId, imported.CharacterId);
            Assert.AreEqual(original.CharacterName, imported.CharacterName);
            Assert.AreEqual(original.Level, imported.Level);
            Assert.AreEqual(original.LastWorldId, imported.LastWorldId);
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 11: Network Export/Import Round-Trip
        /// Tests with multiple random characters.
        /// Validates: Requirements 7.5
        /// </summary>
        [Test]
        public void Property11_NetworkExportImportRoundTrip_MultipleCharacters()
        {
            var random = new System.Random(123);
            
            for (int i = 0; i < 20; i++)
            {
                var original = CreateRandomCharacter(random, $"NetHero{i}");
                
                byte[] exported = _persistenceService.ExportCharacterForNetwork(original);
                Assert.IsNotNull(exported, $"Export should succeed for character {i}");

                var imported = _persistenceService.ImportCharacterFromNetwork(exported);
                
                Assert.IsNotNull(imported, $"Import should succeed for character {i}");
                Assert.AreEqual(original.CharacterId, imported.CharacterId);
                Assert.AreEqual(original.CharacterName, imported.CharacterName);
            }
        }

        #endregion

        #region Property 12: Encrypted Storage Non-Plaintext

        /// <summary>
        /// Feature: network-player-foundation, Property 12: Encrypted Storage Non-Plaintext
        /// For any saved CharacterData, the raw bytes SHALL NOT contain the CharacterName as plaintext.
        /// Validates: Requirements 7.2
        /// </summary>
        [Test]
        public async Task Property12_EncryptedStorageNonPlaintext_NameNotVisible()
        {
            string uniqueName = "UniqueTestName12345";
            var character = CreateTestCharacter(uniqueName, CharacterClass.Rogue);

            await _persistenceService.SaveCharacterAsync(character);

            // Get raw bytes
            byte[] rawBytes = _persistenceService.GetRawSavedBytes(character.CharacterId);
            Assert.IsNotNull(rawBytes, "Raw bytes should exist");

            // Check that name is not in plaintext
            string rawString = Encoding.UTF8.GetString(rawBytes);
            Assert.IsFalse(rawString.Contains(uniqueName), 
                "Character name should NOT appear in plaintext in encrypted storage");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 12: Encrypted Storage Non-Plaintext
        /// Tests with multiple characters with unique names.
        /// Validates: Requirements 7.2
        /// </summary>
        [Test]
        public void Property12_EncryptedStorageNonPlaintext_ExportedDataNotPlaintext()
        {
            var random = new System.Random(456);
            
            for (int i = 0; i < 10; i++)
            {
                string uniqueName = $"SecretName_{Guid.NewGuid()}";
                var character = CreateTestCharacter(uniqueName, CharacterClass.Healer);

                byte[] exported = _persistenceService.ExportCharacterForNetwork(character);
                string exportedString = Encoding.UTF8.GetString(exported);

                Assert.IsFalse(exportedString.Contains(uniqueName),
                    $"Character name should NOT appear in plaintext in exported data (iteration {i})");
            }
        }

        #endregion

        #region Property 13: Corrupted Data Rejection

        /// <summary>
        /// Feature: network-player-foundation, Property 13: Corrupted Data Rejection
        /// For any saved CharacterData where stored bytes have been modified,
        /// LoadCharacterAsync SHALL return null or throw an integrity exception.
        /// Validates: Requirements 7.4
        /// </summary>
        [Test]
        public async Task Property13_CorruptedDataRejection_ModifiedBytesRejected()
        {
            var character = CreateTestCharacter("CorruptTest", CharacterClass.Warrior);
            await _persistenceService.SaveCharacterAsync(character);

            // Get file path and corrupt the data
            string savePath = Path.Combine(Application.persistentDataPath, "Characters");
            string filePath = Path.Combine(savePath, $"{character.CharacterId}.edc");

            byte[] originalBytes = File.ReadAllBytes(filePath);
            
            // Corrupt some bytes in the middle
            byte[] corruptedBytes = (byte[])originalBytes.Clone();
            int midPoint = corruptedBytes.Length / 2;
            corruptedBytes[midPoint] ^= 0xFF;
            corruptedBytes[midPoint + 1] ^= 0xFF;
            
            File.WriteAllBytes(filePath, corruptedBytes);

            // Attempt to load - should fail
            var loaded = await _persistenceService.LoadCharacterAsync(character.CharacterId);
            Assert.IsNull(loaded, "Loading corrupted data should return null");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 13: Corrupted Data Rejection
        /// Tests that corrupted network payload is rejected.
        /// Validates: Requirements 7.4
        /// </summary>
        [Test]
        public void Property13_CorruptedDataRejection_CorruptedNetworkPayloadRejected()
        {
            var character = CreateTestCharacter("NetCorruptTest", CharacterClass.Mage);
            byte[] exported = _persistenceService.ExportCharacterForNetwork(character);

            // Corrupt the exported data
            byte[] corrupted = (byte[])exported.Clone();
            corrupted[corrupted.Length / 2] ^= 0xFF;

            // Attempt to import - should fail
            var imported = _persistenceService.ImportCharacterFromNetwork(corrupted);
            Assert.IsNull(imported, "Importing corrupted network data should return null");
        }

        #endregion

        #region Helper Methods

        private CharacterData CreateTestCharacter(string name, CharacterClass characterClass)
        {
            return new CharacterData(name, characterClass)
            {
                Level = 1,
                Health = 100,
                MaxHealth = 100,
                Experience = 0,
                LastPosition = Vector3.zero,
                LastWorldId = "test-world"
            };
        }

        private CharacterData CreateRandomCharacter(System.Random random, string name)
        {
            var classes = (CharacterClass[])Enum.GetValues(typeof(CharacterClass));
            
            return new CharacterData(name, classes[random.Next(classes.Length)])
            {
                Level = random.Next(1, 100),
                Health = random.Next(1, 1000),
                MaxHealth = random.Next(100, 1000),
                Experience = random.Next(0, 1000000),
                Armor = random.Next(0, 500),
                Strength = random.Next(1, 100),
                Intelligence = random.Next(1, 100),
                Stamina = random.Next(1, 100),
                LastPosition = new Vector3(
                    (float)random.NextDouble() * 1000,
                    (float)random.NextDouble() * 100,
                    (float)random.NextDouble() * 1000
                ),
                LastWorldId = $"world-{random.Next(1, 100)}"
            };
        }

        #endregion
    }
}
