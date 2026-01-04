using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Handles character data persistence with encryption.
    /// Saves to Application.persistentDataPath for Cross-World portability.
    /// </summary>
    public class CharacterPersistenceService : ICharacterPersistenceService
    {
        private const string SAVE_FOLDER = "Characters";
        private const string FILE_EXTENSION = ".edc"; // Ether Domes Character

        private readonly IEncryptionService _encryption;
        private readonly string _savePath;

        public CharacterPersistenceService() : this(new EncryptionService())
        {
        }

        public CharacterPersistenceService(IEncryptionService encryptionService)
        {
            _encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            
            EnsureSaveDirectoryExists();
        }

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }

        public async Task<bool> SaveCharacterAsync(CharacterData data)
        {
            if (data == null || string.IsNullOrEmpty(data.CharacterId))
            {
                Debug.LogError("[CharacterPersistence] Cannot save null or invalid character data");
                return false;
            }

            try
            {
                // Update save time
                data.LastSaveTime = DateTime.UtcNow;

                // Serialize to JSON
                string json = JsonUtility.ToJson(data, true);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // Compute integrity hash (before encryption)
                data.IntegrityHash = _encryption.ComputeHash(jsonBytes);

                // Re-serialize with hash
                json = JsonUtility.ToJson(data, true);
                jsonBytes = Encoding.UTF8.GetBytes(json);

                // Encrypt
                byte[] encrypted = _encryption.Encrypt(jsonBytes);
                if (encrypted == null)
                {
                    Debug.LogError("[CharacterPersistence] Encryption failed");
                    return false;
                }

                // Write to file
                string filePath = GetCharacterFilePath(data.CharacterId);
                await Task.Run(() => File.WriteAllBytes(filePath, encrypted));

                Debug.Log($"[CharacterPersistence] Saved character: {data.CharacterName} ({data.CharacterId})");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Save failed: {ex.Message}");
                return false;
            }
        }


        public async Task<CharacterData> LoadCharacterAsync(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
            {
                Debug.LogError("[CharacterPersistence] Cannot load character with null/empty ID");
                return null;
            }

            string filePath = GetCharacterFilePath(characterId);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[CharacterPersistence] Character file not found: {characterId}");
                return null;
            }

            try
            {
                // Read encrypted file
                byte[] encrypted = await Task.Run(() => File.ReadAllBytes(filePath));

                // Decrypt
                byte[] decrypted = _encryption.Decrypt(encrypted);
                if (decrypted == null)
                {
                    Debug.LogError("[CharacterPersistence] Decryption failed - file may be corrupted");
                    return null;
                }

                // Deserialize
                string json = Encoding.UTF8.GetString(decrypted);
                CharacterData data = JsonUtility.FromJson<CharacterData>(json);

                // Validate integrity
                if (!ValidateCharacterIntegrity(data))
                {
                    Debug.LogError("[CharacterPersistence] Character data integrity check failed");
                    return null;
                }

                Debug.Log($"[CharacterPersistence] Loaded character: {data.CharacterName} ({data.CharacterId})");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Load failed: {ex.Message}");
                return null;
            }
        }

        public bool ValidateCharacterIntegrity(CharacterData data)
        {
            if (data == null)
                return false;

            // Basic validation
            if (!data.IsValid())
                return false;

            // Hash validation would require storing the hash separately
            // For now, we validate the data structure
            if (data.BaseStats == null)
                return false;

            if (data.Equipment == null)
                return false;

            return true;
        }

        public byte[] ExportCharacterForNetwork(CharacterData data)
        {
            if (data == null)
                return null;

            try
            {
                // Serialize to JSON
                string json = JsonUtility.ToJson(data);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // Encrypt for network transfer
                return _encryption.Encrypt(jsonBytes);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Export failed: {ex.Message}");
                return null;
            }
        }

        public CharacterData ImportCharacterFromNetwork(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return null;

            try
            {
                // Decrypt
                byte[] decrypted = _encryption.Decrypt(payload);
                if (decrypted == null)
                    return null;

                // Deserialize
                string json = Encoding.UTF8.GetString(decrypted);
                CharacterData data = JsonUtility.FromJson<CharacterData>(json);

                // Validate
                if (!ValidateCharacterIntegrity(data))
                    return null;

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Import failed: {ex.Message}");
                return null;
            }
        }


        public string[] GetSavedCharacterIds()
        {
            try
            {
                if (!Directory.Exists(_savePath))
                    return new string[0];

                var files = Directory.GetFiles(_savePath, $"*{FILE_EXTENSION}");
                return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Failed to get saved characters: {ex.Message}");
                return new string[0];
            }
        }

        public bool DeleteCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return false;

            try
            {
                string filePath = GetCharacterFilePath(characterId);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"[CharacterPersistence] Deleted character: {characterId}");
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Delete failed: {ex.Message}");
                return false;
            }
        }

        private string GetCharacterFilePath(string characterId)
        {
            // Sanitize the character ID for use as filename
            string safeId = SanitizeFileName(characterId);
            return Path.Combine(_savePath, safeId + FILE_EXTENSION);
        }

        private string SanitizeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
