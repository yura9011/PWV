using UnityEngine;
using EtherDomes.Network;
using EtherDomes.Data;
using EtherDomes.Core;
using EtherDomes.Persistence;
using Newtonsoft.Json;
using System.Text;
using System.Collections.Generic;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Tester para validar el sistema de handshake server-side y detección de trampas.
    /// Simula diferentes escenarios de validación de personajes.
    /// </summary>
    public class ServerValidationTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _runTestsOnStart = false;
        [SerializeField] private ClassDefinition[] _testClassDefinitions;
        [SerializeField] private ItemDatabase _testItemDatabase;

        [Header("Test Scenarios")]
        [SerializeField] private bool _testValidCharacter = true;
        [SerializeField] private bool _testCheatStats = true;
        [SerializeField] private bool _testInvalidLevel = true;
        [SerializeField] private bool _testInvalidItems = true;
        [SerializeField] private bool _testCorruptedData = true;

        private ConnectionApprovalHandler _approvalHandler;
        private IEncryptionService _encryptionService;

        private void Start()
        {
            if (_runTestsOnStart)
            {
                RunAllTests();
            }
        }

        [ContextMenu("Run All Validation Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== SERVER VALIDATION TESTS STARTING ===");
            
            InitializeServices();
            
            if (_testValidCharacter) TestValidCharacter();
            if (_testCheatStats) TestCheatStats();
            if (_testInvalidLevel) TestInvalidLevel();
            if (_testInvalidItems) TestInvalidItems();
            if (_testCorruptedData) TestCorruptedData();
            
            Debug.Log("=== SERVER VALIDATION TESTS COMPLETED ===");
        }

        private void InitializeServices()
        {
            _approvalHandler = new ConnectionApprovalHandler();
            _encryptionService = new EncryptionService();
            _encryptionService.SetKeyFromString("1234567890123456"); // Misma clave que CharacterPersistenceService
            
            Debug.Log("✅ Services initialized for testing");
        }

        [ContextMenu("Test Valid Character")]
        public void TestValidCharacter()
        {
            Debug.Log("--- Testing Valid Character ---");
            
            // Crear personaje válido
            var validCharacter = CreateTestCharacter("ValidHero", CharacterClass.Cruzado, 10);
            validCharacter.TotalStrength = 25; // Valor esperado para nivel 10
            validCharacter.TotalAgility = 20;
            validCharacter.TotalIntellect = 15;
            validCharacter.TotalStamina = 30;
            validCharacter.MaxHP = 400; // 100 base + (30 stamina * 10)
            validCharacter.MaxMana = 175; // 100 base + (15 intellect * 5)
            validCharacter.CurrentHP = 400;
            validCharacter.CurrentMana = 175;

            byte[] payload = CreateEncryptedPayload(validCharacter);
            var result = _approvalHandler.ValidateConnectionRequest(payload);

            if (result.Approved)
            {
                Debug.Log("✅ Valid character approved correctly");
            }
            else
            {
                Debug.LogError($"❌ Valid character rejected: {result.RejectionReason}");
            }
        }

        [ContextMenu("Test Cheat Stats (99999 Strength)")]
        public void TestCheatStats()
        {
            Debug.Log("--- Testing Cheat Stats ---");
            
            // Crear personaje con stats hackeados
            var cheatCharacter = CreateTestCharacter("CheatHero", CharacterClass.Cruzado, 10);
            cheatCharacter.TotalStrength = 99999; // ¡TRAMPA DETECTADA!
            cheatCharacter.TotalAgility = 50000;  // ¡TRAMPA DETECTADA!
            cheatCharacter.TotalIntellect = 15;   // Normal
            cheatCharacter.TotalStamina = 30;     // Normal
            cheatCharacter.MaxHP = 999999;       // ¡TRAMPA DETECTADA!
            cheatCharacter.MaxMana = 175;        // Normal
            cheatCharacter.CurrentHP = 999999;
            cheatCharacter.CurrentMana = 175;

            byte[] payload = CreateEncryptedPayload(cheatCharacter);
            var result = _approvalHandler.ValidateConnectionRequest(payload);

            if (!result.Approved)
            {
                Debug.Log($"✅ Cheat stats detected and rejected: {result.RejectionReason}");
            }
            else
            {
                Debug.LogError("❌ Cheat stats were NOT detected - SECURITY BREACH!");
            }
        }

        [ContextMenu("Test Invalid Level")]
        public void TestInvalidLevel()
        {
            Debug.Log("--- Testing Invalid Level ---");
            
            // Crear personaje con nivel imposible
            var invalidLevelCharacter = CreateTestCharacter("HackerHero", CharacterClass.MaestroElemental, 999);
            invalidLevelCharacter.TotalStrength = 15;
            invalidLevelCharacter.TotalAgility = 20;
            invalidLevelCharacter.TotalIntellect = 25;
            invalidLevelCharacter.TotalStamina = 20;
            invalidLevelCharacter.MaxHP = 300;
            invalidLevelCharacter.MaxMana = 225;

            byte[] payload = CreateEncryptedPayload(invalidLevelCharacter);
            var result = _approvalHandler.ValidateConnectionRequest(payload);

            if (!result.Approved)
            {
                Debug.Log($"✅ Invalid level detected and rejected: {result.RejectionReason}");
            }
            else
            {
                Debug.LogError("❌ Invalid level was NOT detected - SECURITY BREACH!");
            }
        }

        [ContextMenu("Test Invalid Items")]
        public void TestInvalidItems()
        {
            Debug.Log("--- Testing Invalid Items ---");
            
            // Crear personaje con demasiados items equipados
            var invalidItemsCharacter = CreateTestCharacter("ItemHoarder", CharacterClass.Cruzado, 5);
            
            // Agregar demasiados items (más de 20)
            invalidItemsCharacter.EquippedItemIDs = new List<string>();
            for (int i = 0; i < 25; i++)
            {
                invalidItemsCharacter.EquippedItemIDs.Add($"fake_item_{i}");
            }

            byte[] payload = CreateEncryptedPayload(invalidItemsCharacter);
            var result = _approvalHandler.ValidateConnectionRequest(payload);

            if (!result.Approved)
            {
                Debug.Log($"✅ Invalid items detected and rejected: {result.RejectionReason}");
            }
            else
            {
                Debug.LogError("❌ Invalid items were NOT detected - SECURITY BREACH!");
            }
        }

        [ContextMenu("Test Corrupted Data")]
        public void TestCorruptedData()
        {
            Debug.Log("--- Testing Corrupted Data ---");
            
            // Crear payload corrupto
            byte[] corruptedPayload = Encoding.UTF8.GetBytes("This is not valid encrypted data!");
            var result = _approvalHandler.ValidateConnectionRequest(corruptedPayload);

            if (!result.Approved)
            {
                Debug.Log($"✅ Corrupted data detected and rejected: {result.RejectionReason}");
            }
            else
            {
                Debug.LogError("❌ Corrupted data was NOT detected - SECURITY BREACH!");
            }

            // Test payload vacío
            var emptyResult = _approvalHandler.ValidateConnectionRequest(null);
            if (!emptyResult.Approved)
            {
                Debug.Log($"✅ Empty payload detected and rejected: {emptyResult.RejectionReason}");
            }
            else
            {
                Debug.LogError("❌ Empty payload was NOT detected - SECURITY BREACH!");
            }
        }

        [ContextMenu("Test Direct Validation (No Encryption)")]
        public void TestDirectValidation()
        {
            Debug.Log("=== TESTING DIRECT VALIDATION (NO ENCRYPTION) ===");
            
            // Test 1: Personaje válido
            var validCharacter = CreateTestCharacter("ValidHero", CharacterClass.Cruzado, 10);
            validCharacter.TotalStrength = 30; // Valor razonable
            validCharacter.TotalAgility = 20;
            validCharacter.MaxHP = 400;
            
            bool isValid = TestCharacterValidation(validCharacter, "Valid Character");
            
            // Test 2: Personaje con stats hackeados
            var cheatCharacter = CreateTestCharacter("CheatHero", CharacterClass.Cruzado, 10);
            cheatCharacter.TotalStrength = 99999; // ¡TRAMPA!
            cheatCharacter.TotalAgility = 50000;  // ¡TRAMPA!
            cheatCharacter.MaxHP = 999999;       // ¡TRAMPA!
            
            bool cheatDetected = !TestCharacterValidation(cheatCharacter, "Cheat Character");
            
            // Test 3: Nivel imposible
            var levelHacker = CreateTestCharacter("LevelHacker", CharacterClass.MaestroElemental, 999);
            bool levelCheatDetected = !TestCharacterValidation(levelHacker, "Level Hacker");
            
            // Resumen
            Debug.Log("=== VALIDATION SUMMARY ===");
            Debug.Log($"Valid character: {(isValid ? "✅ PASSED" : "❌ FAILED")}");
            Debug.Log($"Cheat detection: {(cheatDetected ? "✅ PASSED" : "❌ FAILED")}");
            Debug.Log($"Level hack detection: {(levelCheatDetected ? "✅ PASSED" : "❌ FAILED")}");
        }

        private bool TestCharacterValidation(CharacterData character, string testName)
        {
            Debug.Log($"\n--- Testing: {testName} ---");
            Debug.Log($"Level: {character.Level}");
            Debug.Log($"Strength: {character.TotalStrength}");
            Debug.Log($"Agility: {character.TotalAgility}");
            Debug.Log($"HP: {character.CurrentHP}/{character.MaxHP}");
            
            // Validar nivel
            if (character.Level < 1 || character.Level > 60)
            {
                Debug.Log($"❌ Invalid level: {character.Level}");
                return false;
            }
            
            // Validar stats
            int maxStat = 10000;
            if (character.TotalStrength > maxStat)
            {
                Debug.Log($"❌ Strength too high: {character.TotalStrength} > {maxStat}");
                return false;
            }
            
            if (character.TotalAgility > maxStat)
            {
                Debug.Log($"❌ Agility too high: {character.TotalAgility} > {maxStat}");
                return false;
            }
            
            if (character.MaxHP > 100000) // HP razonable
            {
                Debug.Log($"❌ HP too high: {character.MaxHP}");
                return false;
            }
            
            Debug.Log($"✅ {testName} validation passed");
            return true;
        }

        [ContextMenu("Create Test Character with Cheats")]
        public void CreateTestCharacterWithCheats()
        {
            Debug.Log("--- Creating Test Character with Obvious Cheats ---");
            
            var cheatCharacter = CreateTestCharacter("ObviousCheat", CharacterClass.MaestroElemental, 1);
            
            // Stats imposibles para nivel 1
            cheatCharacter.TotalStrength = 99999;
            cheatCharacter.TotalAgility = 88888;
            cheatCharacter.TotalIntellect = 77777;
            cheatCharacter.TotalStamina = 66666;
            cheatCharacter.TotalArmor = 55555;
            cheatCharacter.TotalAttackPower = 44444;
            cheatCharacter.TotalSpellPower = 33333;
            cheatCharacter.MaxHP = 999999;
            cheatCharacter.MaxMana = 888888;
            cheatCharacter.CurrentHP = 999999;
            cheatCharacter.CurrentMana = 888888;
            cheatCharacter.Level = 999; // Nivel imposible

            // Mostrar los datos del personaje trampa
            Debug.Log($"Cheat Character Created:");
            Debug.Log($"  Name: {cheatCharacter.Name}");
            Debug.Log($"  Level: {cheatCharacter.Level}");
            Debug.Log($"  Strength: {cheatCharacter.TotalStrength}");
            Debug.Log($"  HP: {cheatCharacter.CurrentHP}/{cheatCharacter.MaxHP}");
            Debug.Log($"  Mana: {cheatCharacter.CurrentMana}/{cheatCharacter.MaxMana}");

            // Probar validación
            byte[] payload = CreateEncryptedPayload(cheatCharacter);
            var result = _approvalHandler.ValidateConnectionRequest(payload);

            Debug.Log($"Validation Result: {(result.Approved ? "APPROVED" : "REJECTED")}");
            if (!result.Approved)
            {
                Debug.Log($"Rejection Reason: {result.RejectionReason}");
                Debug.Log($"Error Code: {result.ErrorCode}");
            }
        }

        private CharacterData CreateTestCharacter(string name, CharacterClass characterClass, int level)
        {
            var character = new CharacterData(name, characterClass);
            character.Level = level;
            character.CurrentXP = level * 100; // XP aproximada
            character.CreatedAt = System.DateTime.Now;
            character.LastPlayedAt = System.DateTime.Now;
            
            // Stats base para el nivel (valores aproximados)
            character.TotalStrength = 10 + (level * 2);
            character.TotalAgility = 10 + (level * 1);
            character.TotalIntellect = 10 + (level * 1);
            character.TotalStamina = 15 + (level * 2);
            character.TotalArmor = 5 + level;
            character.TotalAttackPower = level * 3;
            character.TotalSpellPower = level * 2;
            
            // HP/Mana basados en stats
            character.MaxHP = 100 + (character.TotalStamina * 10);
            character.MaxMana = 100 + (character.TotalIntellect * 5);
            character.CurrentHP = character.MaxHP;
            character.CurrentMana = character.MaxMana;
            
            // Items equipados vacíos por defecto
            character.EquippedItemIDs = new List<string>();
            character.Inventory = new List<InventorySlot>();
            
            return character;
        }

        private byte[] CreateEncryptedPayload(CharacterData character)
        {
            try
            {
                // Usar CharacterPersistenceService para crear el payload exactamente como lo haría un cliente real
                var persistenceService = new CharacterPersistenceService();
                return persistenceService.ExportCharacterForNetwork(character);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to create encrypted payload: {e.Message}");
                return null;
            }
        }

        [ContextMenu("Show Validation Limits")]
        public void ShowValidationLimits()
        {
            if (_approvalHandler == null)
            {
                InitializeServices();
            }

            Debug.Log("=== VALIDATION LIMITS ===");
            Debug.Log($"Max Stat Value: {_approvalHandler.MaxStatValue}");
            Debug.Log($"Min Stat Value: {_approvalHandler.MinStatValue}");
            Debug.Log($"Max Level: {ConnectionApprovalHandler.DEFAULT_MAX_LEVEL}");
            Debug.Log($"Min Level: {ConnectionApprovalHandler.DEFAULT_MIN_LEVEL}");
            Debug.Log($"Validation Timeout: {_approvalHandler.ValidationTimeout}");
        }

        [ContextMenu("Simulate Network Connection Test")]
        public void SimulateNetworkConnectionTest()
        {
            Debug.Log("=== SIMULATING NETWORK CONNECTION ===");
            
            // Simular diferentes tipos de conexiones
            var scenarios = new[]
            {
                ("Legitimate Player", CreateTestCharacter("LegitPlayer", CharacterClass.Cruzado, 25)),
                ("Speed Hacker", CreateSpeedHacker()),
                ("Stat Hacker", CreateStatHacker()),
                ("Item Duper", CreateItemDuper()),
                ("Level Hacker", CreateLevelHacker())
            };

            foreach (var (name, character) in scenarios)
            {
                Debug.Log($"\n--- Testing: {name} ---");
                byte[] payload = CreateEncryptedPayload(character);
                var result = _approvalHandler.ValidateConnectionRequest(payload);
                
                string status = result.Approved ? "✅ APPROVED" : "❌ REJECTED";
                Debug.Log($"{status} - {name}");
                
                if (!result.Approved)
                {
                    Debug.Log($"  Reason: {result.RejectionReason}");
                    Debug.Log($"  Code: {result.ErrorCode}");
                }
            }
        }

        private CharacterData CreateSpeedHacker()
        {
            var character = CreateTestCharacter("SpeedHacker", CharacterClass.Cruzado, 20);
            character.TotalAgility = 99999; // Velocidad imposible
            return character;
        }

        private CharacterData CreateStatHacker()
        {
            var character = CreateTestCharacter("StatHacker", CharacterClass.MaestroElemental, 15);
            character.TotalStrength = 50000;
            character.TotalIntellect = 75000;
            character.MaxHP = 999999;
            character.MaxMana = 888888;
            return character;
        }

        private CharacterData CreateItemDuper()
        {
            var character = CreateTestCharacter("ItemDuper", CharacterClass.Cruzado, 10);
            // Demasiados items equipados
            character.EquippedItemIDs = new List<string>();
            for (int i = 0; i < 50; i++)
            {
                character.EquippedItemIDs.Add($"legendary_sword_{i}");
            }
            return character;
        }

        private CharacterData CreateLevelHacker()
        {
            var character = CreateTestCharacter("LevelHacker", CharacterClass.MaestroElemental, 999);
            character.CurrentXP = -1000; // XP negativa
            return character;
        }
    }
}