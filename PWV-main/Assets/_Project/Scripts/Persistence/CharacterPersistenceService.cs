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
    /// Handles character data persistence with encryption, session locking, and data migration.
    /// NOTE: This service is being deprecated in favor of LocalDataManager which handles
    /// the new SaveFile/CharacterData format with AES-256 encryption.
    /// </summary>
    [Obsolete("Use LocalDataManager instead for new character persistence")]
    public class CharacterPersistenceService : ICharacterPersistenceService
    {
        private const string SAVE_FOLDER = "Characters";
        private const string FILE_EXTENSION = ".edc"; // Ether Domes Character

        private readonly IEncryptionService _encryption;
        private readonly ISessionLockManager _sessionLock;
        private readonly IDataMigrationService _migration;
        private readonly string _savePath;

        /// <summary>
        /// Event fired when a character load is denied due to session lock.
        /// </summary>
        public event Action<string> OnCharacterInUse;

        public CharacterPersistenceService() 
            : this(new EncryptionService(), new SessionLockManager(), new DataMigrationService())
        {
        }

        public CharacterPersistenceService(IEncryptionService encryptionService) 
            : this(encryptionService, new SessionLockManager(), new DataMigrationService())
        {
        }

        public CharacterPersistenceService(
            IEncryptionService encryptionService, 
            ISessionLockManager sessionLockManager)
            : this(encryptionService, sessionLockManager, new DataMigrationService())
        {
        }

        public CharacterPersistenceService(
            IEncryptionService encryptionService, 
            ISessionLockManager sessionLockManager,
            IDataMigrationService migrationService)
        {
            _encryption = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _sessionLock = sessionLockManager ?? throw new ArgumentNullException(nameof(sessionLockManager));
            _migration = migrationService ?? throw new ArgumentNullException(nameof(migrationService));
            _savePath = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
            
            EnsureSaveDirectoryExists();
            
            // Cleanup stale locks on startup
            _sessionLock.CleanupStaleLocks(_sessionLock.StaleLockThreshold);
        }

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(_savePath))
            {
                Directory.CreateDirectory(_savePath);
            }
        }

        /// <summary>
        /// Releases all session locks. Should be called on application quit.
        /// </summary>
        public void ReleaseAllLocks()
        {
            _sessionLock.ReleaseAllLocks();
        }

        /// <summary>
        /// Releases the session lock for a specific character.
        /// </summary>
        public void ReleaseLock(string characterId)
        {
            _sessionLock.ReleaseLock(characterId);
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
                data.LastPlayedAt = DateTime.UtcNow;

                // Serialize to JSON
                string json = JsonUtility.ToJson(data, true);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

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

                Debug.Log($"[CharacterPersistence] Saved character: {data.Name} ({data.CharacterId})");
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

            // Try to acquire session lock first (Requirement 1.1, 1.2)
            if (!_sessionLock.TryAcquireLock(characterId))
            {
                Debug.LogWarning($"[CharacterPersistence] Character in use: {characterId}");
                OnCharacterInUse?.Invoke(characterId);
                return null;
            }

            string filePath = GetCharacterFilePath(characterId);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[CharacterPersistence] Character file not found: {characterId}");
                _sessionLock.ReleaseLock(characterId); // Release lock if file doesn't exist
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
                    _sessionLock.ReleaseLock(characterId);
                    return null;
                }

                // Deserialize
                string json = Encoding.UTF8.GetString(decrypted);
                CharacterData data = JsonUtility.FromJson<CharacterData>(json);

                // Validate integrity
                if (!ValidateCharacterIntegrity(data))
                {
                    Debug.LogError("[CharacterPersistence] Character data integrity check failed");
                    _sessionLock.ReleaseLock(characterId);
                    return null;
                }

                Debug.Log($"[CharacterPersistence] Loaded character: {data.Name} ({data.CharacterId})");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Load failed: {ex.Message}");
                _sessionLock.ReleaseLock(characterId);
                return null;
            }
        }

        public bool ValidateCharacterIntegrity(CharacterData data)
        {
            if (data == null)
                return false;

            // Basic validation
            if (string.IsNullOrEmpty(data.CharacterId))
                return false;

            if (string.IsNullOrEmpty(data.Name))
                return false;

            if (data.Level < 1 || data.Level > 60)
                return false;

            return true;
        }

        /// <summary>
        /// Full validation including stat range checks.
        /// Used during network import to detect tampering.
        /// </summary>
        public bool ValidateCharacterFull(CharacterData data)
        {
            // Basic structural validation
            if (!ValidateCharacterIntegrity(data))
                return false;

            // Stat range validation (anti-cheat)
            if (!ValidateStatRanges(data))
                return false;

            return true;
        }

        /// <summary>
        /// Validates that character stats are within acceptable ranges.
        /// Prevents cheated characters from connecting.
        /// </summary>
        private bool ValidateStatRanges(CharacterData data)
        {
            if (data.Level < 1 || data.Level > 60)
            {
                Debug.LogWarning($"[CharacterPersistence] Invalid level: {data.Level}");
                return false;
            }

            if (data.CurrentXP < 0)
            {
                Debug.LogWarning($"[CharacterPersistence] Invalid experience: {data.CurrentXP}");
                return false;
            }

            // Basic sanity checks - stats should scale with level
            int maxStatValue = data.Level * 50; // Generous upper bound
            
            if (data.TotalStrength < 0 || data.TotalStrength > maxStatValue ||
                data.TotalIntellect < 0 || data.TotalIntellect > maxStatValue ||
                data.TotalStamina < 0 || data.TotalStamina > maxStatValue ||
                data.TotalAttackPower < 0 || data.TotalAttackPower > maxStatValue * 2 ||
                data.TotalSpellPower < 0 || data.TotalSpellPower > maxStatValue * 2)
            {
                Debug.LogWarning("[CharacterPersistence] Stats out of acceptable range");
                return false;
            }

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
