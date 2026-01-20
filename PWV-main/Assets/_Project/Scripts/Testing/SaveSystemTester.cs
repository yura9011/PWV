using UnityEngine;
using EtherDomes.Persistence;
using EtherDomes.Data;
using System.IO;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Tester para verificar el sistema de guardado/carga con encriptación AES.
    /// </summary>
    public class SaveSystemTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _runTestsOnStart = false;
        [SerializeField] private bool _enableDebugLogs = true;

        private void Start()
        {
            if (_runTestsOnStart)
            {
                RunAllTests();
            }
        }

        [ContextMenu("Run All Tests")]
        public void RunAllTests()
        {
            Debug.Log("=== SAVE SYSTEM TESTS STARTING ===");
            
            TestEncryptionService();
            TestSaveManagerBasic();
            TestCharacterCRUD();
            TestFileIntegrity();
            
            Debug.Log("=== SAVE SYSTEM TESTS COMPLETED ===");
        }

        [ContextMenu("Test Encryption Service")]
        public void TestEncryptionService()
        {
            Debug.Log("--- Testing EncryptionService ---");
            
            var encryptionService = new EncryptionService();
            encryptionService.SetKeyFromString("TestKey123");
            
            // Test data
            string testData = "This is secret character data!";
            byte[] originalBytes = System.Text.Encoding.UTF8.GetBytes(testData);
            
            // Encrypt
            byte[] encrypted = encryptionService.Encrypt(originalBytes);
            if (encrypted == null)
            {
                Debug.LogError("❌ Encryption failed!");
                return;
            }
            Debug.Log($"✅ Encryption successful - {encrypted.Length} bytes");
            
            // Decrypt
            byte[] decrypted = encryptionService.Decrypt(encrypted);
            if (decrypted == null)
            {
                Debug.LogError("❌ Decryption failed!");
                return;
            }
            
            string decryptedText = System.Text.Encoding.UTF8.GetString(decrypted);
            if (decryptedText == testData)
            {
                Debug.Log("✅ Encryption/Decryption test passed!");
            }
            else
            {
                Debug.LogError($"❌ Data mismatch! Expected: '{testData}', Got: '{decryptedText}'");
            }
            
            // Test hash verification
            byte[] hash = encryptionService.ComputeHash(originalBytes);
            bool hashValid = encryptionService.VerifyHash(originalBytes, hash);
            
            if (hashValid)
            {
                Debug.Log("✅ Hash verification test passed!");
            }
            else
            {
                Debug.LogError("❌ Hash verification failed!");
            }
        }

        [ContextMenu("Test SaveManager Basic")]
        public void TestSaveManagerBasic()
        {
            Debug.Log("--- Testing SaveManager Basic Operations ---");
            
            if (SaveManager.Instance == null)
            {
                Debug.LogError("❌ SaveManager instance not found!");
                return;
            }
            
            var saveManager = SaveManager.Instance;
            
            // Test initial state
            if (saveManager.CurrentSave != null)
            {
                Debug.Log($"✅ SaveManager initialized - {saveManager.CurrentSave.Characters.Count} characters loaded");
            }
            else
            {
                Debug.LogError("❌ SaveManager CurrentSave is null!");
                return;
            }
            
            // Test save operation
            bool saveResult = saveManager.Save();
            if (saveResult)
            {
                Debug.Log("✅ Save operation successful");
            }
            else
            {
                Debug.LogError("❌ Save operation failed!");
            }
        }

        [ContextMenu("Test Character CRUD")]
        public void TestCharacterCRUD()
        {
            Debug.Log("--- Testing Character CRUD Operations ---");
            
            var saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                Debug.LogError("❌ SaveManager not available!");
                return;
            }
            
            int initialCount = saveManager.CurrentSave.Characters.Count;
            
            // Create test character
            string testName = "TestHero_" + System.DateTime.Now.Ticks;
            bool createResult = saveManager.CreateCharacter(testName, CharacterClass.Cruzado);
            
            if (createResult)
            {
                Debug.Log($"✅ Character '{testName}' created successfully");
            }
            else
            {
                Debug.LogError("❌ Character creation failed!");
                return;
            }
            
            // Verify character exists
            var createdChar = saveManager.CurrentSave.Characters.Find(c => c.Name == testName);
            if (createdChar != null)
            {
                Debug.Log($"✅ Character found - ID: {createdChar.CharacterId}");
                
                // Update character
                createdChar.Level = 5;
                createdChar.CurrentXP = 1000;
                bool updateResult = saveManager.UpdateCharacter(createdChar);
                
                if (updateResult)
                {
                    Debug.Log("✅ Character update successful");
                }
                else
                {
                    Debug.LogError("❌ Character update failed!");
                }
                
                // Delete character (cleanup)
                bool deleteResult = saveManager.DeleteCharacter(createdChar.CharacterId);
                if (deleteResult)
                {
                    Debug.Log("✅ Character deletion successful");
                }
                else
                {
                    Debug.LogError("❌ Character deletion failed!");
                }
            }
            else
            {
                Debug.LogError("❌ Created character not found!");
            }
        }

        [ContextMenu("Test File Integrity")]
        public void TestFileIntegrity()
        {
            Debug.Log("--- Testing File Integrity ---");
            
            var saveManager = SaveManager.Instance;
            if (saveManager == null)
            {
                Debug.LogError("❌ SaveManager not available!");
                return;
            }
            
            // Force save to create encrypted file
            bool saveResult = saveManager.Save();
            if (!saveResult)
            {
                Debug.LogError("❌ Could not create save file for integrity test!");
                return;
            }
            
            // Check if file exists and is encrypted
            string savePath = Path.Combine(Application.persistentDataPath, "etherdomes_save.ted");
            if (File.Exists(savePath))
            {
                string fileContent = File.ReadAllText(savePath);
                
                // Verify it's not plain JSON (should be encrypted structure)
                if (fileContent.Contains("\"EncryptedData\"") && fileContent.Contains("\"IntegrityHash\""))
                {
                    Debug.Log("✅ Save file is properly encrypted with integrity hash");
                }
                else if (fileContent.Contains("\"Characters\""))
                {
                    Debug.LogWarning("⚠️ Save file appears to be unencrypted (legacy format)");
                }
                else
                {
                    Debug.LogError("❌ Save file format unrecognized!");
                }
                
                // Test reload
                bool loadResult = saveManager.Load();
                if (loadResult)
                {
                    Debug.Log("✅ File integrity test passed - reload successful");
                }
                else
                {
                    Debug.LogError("❌ File integrity test failed - reload failed!");
                }
            }
            else
            {
                Debug.LogError("❌ Save file not found!");
            }
        }

        [ContextMenu("Show Save File Info")]
        public void ShowSaveFileInfo()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "etherdomes_save.ted");
            Debug.Log($"Save file path: {savePath}");
            
            if (File.Exists(savePath))
            {
                FileInfo fileInfo = new FileInfo(savePath);
                Debug.Log($"File size: {fileInfo.Length} bytes");
                Debug.Log($"Last modified: {fileInfo.LastWriteTime}");
                
                if (SaveManager.Instance?.CurrentSave != null)
                {
                    var save = SaveManager.Instance.CurrentSave;
                    Debug.Log($"Characters: {save.Characters.Count}/{SaveFile.MAX_CHARACTERS}");
                    Debug.Log($"Account ID: {save.AccountID}");
                    Debug.Log($"Version: {save.Version}");
                    Debug.Log($"Last saved: {save.LastSaved}");
                }
            }
            else
            {
                Debug.Log("Save file does not exist");
            }
        }

        [ContextMenu("Clear Save File")]
        public void ClearSaveFile()
        {
            string savePath = Path.Combine(Application.persistentDataPath, "etherdomes_save.ted");
            string backupPath = Path.Combine(Application.persistentDataPath, "etherdomes_save_backup.ted");
            
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted");
            }
            
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
                Debug.Log("Backup file deleted");
            }
            
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Load(); // Reload to create fresh save
                Debug.Log("SaveManager reloaded with fresh data");
            }
        }
    }
}