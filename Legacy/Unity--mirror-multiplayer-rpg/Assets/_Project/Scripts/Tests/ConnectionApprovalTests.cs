using NUnit.Framework;
using UnityEngine;
using EtherDomes.Network;
using EtherDomes.Persistence;
using System;
using System.Collections.Generic;

namespace EtherDomes.Tests
{
    /// <summary>
    /// Property-based and unit tests for ConnectionApprovalAuthenticator.
    /// </summary>
    [TestFixture]
    public class ConnectionApprovalTests
    {
        private GameObject _authenticatorObject;
        private ConnectionApprovalAuthenticator _authenticator;
        private MockPersistenceService _mockPersistence;

        [SetUp]
        public void SetUp()
        {
            _authenticatorObject = new GameObject("TestAuthenticator");
            _authenticator = _authenticatorObject.AddComponent<ConnectionApprovalAuthenticator>();
            
            _mockPersistence = new MockPersistenceService();
            _authenticator.SetPersistenceService(_mockPersistence);
        }

        [TearDown]
        public void TearDown()
        {
            if (_authenticatorObject != null)
            {
                UnityEngine.Object.DestroyImmediate(_authenticatorObject);
            }
        }

        #region Property 9: Character Data Validation Correctness

        /// <summary>
        /// Feature: network-player-foundation, Property 9: Character Data Validation Correctness
        /// For any CharacterData with stats outside valid ranges [0, MaxStatValue],
        /// ValidateConnectionRequest SHALL return Approved=false with ErrorCode=StatsOutOfRange.
        /// Validates: Requirements 6.2, 6.3, 6.4
        /// </summary>
        [Test]
        public void Property9_CharacterDataValidation_InvalidStatsRejected()
        {
            var random = new System.Random(42);
            int maxStat = _authenticator.MaxStatValue;

            for (int i = 0; i < 50; i++)
            {
                // Create character with one invalid stat
                var character = CreateValidCharacter($"TestChar{i}");
                
                // Randomly make one stat invalid
                int invalidChoice = random.Next(6);
                switch (invalidChoice)
                {
                    case 0: character.Health = maxStat + random.Next(1, 1000); break;
                    case 1: character.Armor = maxStat + random.Next(1, 1000); break;
                    case 2: character.Strength = maxStat + random.Next(1, 1000); break;
                    case 3: character.Intelligence = maxStat + random.Next(1, 1000); break;
                    case 4: character.Stamina = maxStat + random.Next(1, 1000); break;
                    case 5: character.Level = 101 + random.Next(0, 100); break;
                }

                _mockPersistence.SetCharacterToReturn(character);
                
                var msg = new AuthRequestMessage
                {
                    EncryptedCharacterData = new byte[] { 1, 2, 3 },
                    ClientVersion = "1.0"
                };

                var result = _authenticator.ValidateConnectionRequest(null, msg);

                Assert.IsFalse(result.Approved, 
                    $"Character with invalid stat (choice {invalidChoice}) should be rejected");
                Assert.AreEqual(ApprovalErrorCode.StatsOutOfRange, result.ErrorCode,
                    "Error code should be StatsOutOfRange");
            }
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 9: Character Data Validation Correctness
        /// For any CharacterData with all stats within valid ranges,
        /// ValidateConnectionRequest SHALL return Approved=true.
        /// Validates: Requirements 6.2, 6.3, 6.4
        /// </summary>
        [Test]
        public void Property9_CharacterDataValidation_ValidStatsApproved()
        {
            var random = new System.Random(123);
            int maxStat = _authenticator.MaxStatValue;

            for (int i = 0; i < 50; i++)
            {
                var character = new CharacterData($"ValidChar{i}", CharacterClass.Warrior)
                {
                    Level = random.Next(1, 100),
                    Health = random.Next(0, maxStat),
                    MaxHealth = random.Next(1, maxStat),
                    Armor = random.Next(0, maxStat),
                    Strength = random.Next(0, maxStat),
                    Intelligence = random.Next(0, maxStat),
                    Stamina = random.Next(0, maxStat),
                    Equipment = new EquipmentData
                    {
                        TotalArmorValue = random.Next(0, maxStat),
                        TotalDamageBonus = random.Next(0, maxStat)
                    }
                };

                _mockPersistence.SetCharacterToReturn(character);
                _mockPersistence.SetValidationResult(true);

                var msg = new AuthRequestMessage
                {
                    EncryptedCharacterData = new byte[] { 1, 2, 3 },
                    ClientVersion = "1.0"
                };

                var result = _authenticator.ValidateConnectionRequest(null, msg);

                Assert.IsTrue(result.Approved, 
                    $"Character {i} with valid stats should be approved");
                Assert.AreEqual(ApprovalErrorCode.None, result.ErrorCode);
            }
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 9: Character Data Validation Correctness
        /// Tests boundary values for stats.
        /// Validates: Requirements 6.2, 6.3, 6.4
        /// </summary>
        [Test]
        public void Property9_CharacterDataValidation_BoundaryValues()
        {
            int maxStat = _authenticator.MaxStatValue;

            // Test at max boundary (should pass)
            var characterAtMax = CreateValidCharacter("BoundaryMax");
            characterAtMax.Health = maxStat;
            characterAtMax.Armor = maxStat;
            
            _mockPersistence.SetCharacterToReturn(characterAtMax);
            _mockPersistence.SetValidationResult(true);

            var msg = new AuthRequestMessage
            {
                EncryptedCharacterData = new byte[] { 1, 2, 3 },
                ClientVersion = "1.0"
            };

            var resultMax = _authenticator.ValidateConnectionRequest(null, msg);
            Assert.IsTrue(resultMax.Approved, "Stats at max boundary should be approved");

            // Test just over max boundary (should fail)
            var characterOverMax = CreateValidCharacter("BoundaryOver");
            characterOverMax.Health = maxStat + 1;
            
            _mockPersistence.SetCharacterToReturn(characterOverMax);

            var resultOver = _authenticator.ValidateConnectionRequest(null, msg);
            Assert.IsFalse(resultOver.Approved, "Stats over max boundary should be rejected");
            Assert.AreEqual(ApprovalErrorCode.StatsOutOfRange, resultOver.ErrorCode);
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 9: Character Data Validation Correctness
        /// Tests that invalid equipment item stats are rejected.
        /// Validates: Requirements 6.2, 6.3, 6.4
        /// </summary>
        [Test]
        public void Property9_CharacterDataValidation_InvalidEquipmentStatsRejected()
        {
            int maxStat = _authenticator.MaxStatValue;

            var character = CreateValidCharacter("EquipTest");
            character.Equipment = new EquipmentData
            {
                TotalArmorValue = 100,
                TotalDamageBonus = 50,
                EquippedItems = new List<ItemData>
                {
                    new ItemData("Sword", 10, ItemRarity.Rare)
                    {
                        Stats = new Dictionary<string, int>
                        {
                            { "damage", maxStat + 1000 } // Invalid!
                        }
                    }
                }
            };

            _mockPersistence.SetCharacterToReturn(character);
            _mockPersistence.SetValidationResult(true);

            var msg = new AuthRequestMessage
            {
                EncryptedCharacterData = new byte[] { 1, 2, 3 },
                ClientVersion = "1.0"
            };

            var result = _authenticator.ValidateConnectionRequest(null, msg);
            Assert.IsFalse(result.Approved, "Character with invalid equipment stats should be rejected");
            Assert.AreEqual(ApprovalErrorCode.StatsOutOfRange, result.ErrorCode);
        }

        #endregion

        #region Unit Tests

        [Test]
        public void ValidateConnectionRequest_EmptyPayload_ReturnsInvalidDataFormat()
        {
            var msg = new AuthRequestMessage
            {
                EncryptedCharacterData = null,
                ClientVersion = "1.0"
            };

            var result = _authenticator.ValidateConnectionRequest(null, msg);

            Assert.IsFalse(result.Approved);
            Assert.AreEqual(ApprovalErrorCode.InvalidDataFormat, result.ErrorCode);
        }

        [Test]
        public void ValidateConnectionRequest_CorruptedData_ReturnsCorruptedData()
        {
            _mockPersistence.SetCharacterToReturn(null); // Simulate decryption failure

            var msg = new AuthRequestMessage
            {
                EncryptedCharacterData = new byte[] { 1, 2, 3 },
                ClientVersion = "1.0"
            };

            var result = _authenticator.ValidateConnectionRequest(null, msg);

            Assert.IsFalse(result.Approved);
            Assert.AreEqual(ApprovalErrorCode.CorruptedData, result.ErrorCode);
        }

        [Test]
        public void ValidateConnectionRequest_FailedIntegrity_ReturnsCorruptedData()
        {
            var character = CreateValidCharacter("IntegrityTest");
            _mockPersistence.SetCharacterToReturn(character);
            _mockPersistence.SetValidationResult(false); // Simulate integrity failure

            var msg = new AuthRequestMessage
            {
                EncryptedCharacterData = new byte[] { 1, 2, 3 },
                ClientVersion = "1.0"
            };

            var result = _authenticator.ValidateConnectionRequest(null, msg);

            Assert.IsFalse(result.Approved);
            Assert.AreEqual(ApprovalErrorCode.CorruptedData, result.ErrorCode);
        }

        #endregion

        #region Helper Methods

        private CharacterData CreateValidCharacter(string name)
        {
            return new CharacterData(name, CharacterClass.Warrior)
            {
                Level = 50,
                Health = 500,
                MaxHealth = 500,
                Armor = 100,
                Strength = 50,
                Intelligence = 30,
                Stamina = 40,
                Equipment = new EquipmentData
                {
                    TotalArmorValue = 100,
                    TotalDamageBonus = 50
                }
            };
        }

        #endregion

        #region Mock Persistence Service

        private class MockPersistenceService : ICharacterPersistenceService
        {
            private CharacterData _characterToReturn;
            private bool _validationResult = true;

            public void SetCharacterToReturn(CharacterData character)
            {
                _characterToReturn = character;
            }

            public void SetValidationResult(bool result)
            {
                _validationResult = result;
            }

            public System.Threading.Tasks.Task<bool> SaveCharacterAsync(CharacterData data) 
                => System.Threading.Tasks.Task.FromResult(true);

            public System.Threading.Tasks.Task<CharacterData> LoadCharacterAsync(string characterId) 
                => System.Threading.Tasks.Task.FromResult(_characterToReturn);

            public bool ValidateCharacterIntegrity(CharacterData data) 
                => _validationResult;

            public byte[] ExportCharacterForNetwork(CharacterData data) 
                => new byte[] { 1, 2, 3 };

            public CharacterData ImportCharacterFromNetwork(byte[] payload) 
                => _characterToReturn;

            public System.Threading.Tasks.Task<bool> DeleteCharacterAsync(string characterId) 
                => System.Threading.Tasks.Task.FromResult(true);

            public System.Threading.Tasks.Task<string[]> GetAllCharacterIdsAsync() 
                => System.Threading.Tasks.Task.FromResult(new string[0]);
        }

        #endregion
    }
}
