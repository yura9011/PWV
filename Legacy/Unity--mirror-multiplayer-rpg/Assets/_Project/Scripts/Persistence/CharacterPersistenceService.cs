using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Character persistence service with AES encryption and SHA256 integrity validation.
    /// Supports local storage and Cross-World network transfer.
    /// </summary>
    public class CharacterPersistenceService : ICharacterPersistenceService
    {
        private const string SAVE_FOLDER = "Characters";
        private const string FILE_EXTENSION = ".edc"; // Ether Domes Character
        
        // Encryption key (in production, this should be securely stored/derived)
        private readonly byte[] _encryptionKey;
        private readonly byte[] _encryptionIV;

        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FOLDER);

        public CharacterPersistenceService()
        {
            // Generate deterministic key from machine-specific data
            // In production, use a more secure key derivation
            string keySource = SystemInfo.deviceUniqueIdentifier + "EtherDomes2024";
            using (var sha256 = SHA256.Create())
            {
                _encryptionKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(keySource));
            }
            
            // IV should be unique per encryption, but for simplicity using fixed IV
            // In production, store IV with encrypted data
            _encryptionIV = new byte[16];
            Array.Copy(_encryptionKey, _encryptionIV, 16);

            EnsureSaveDirectoryExists();
        }

        public CharacterPersistenceService(byte[] customKey, byte[] customIV)
        {
            _encryptionKey = customKey ?? throw new ArgumentNullException(nameof(customKey));
            _encryptionIV = customIV ?? throw new ArgumentNullException(nameof(customIV));
            EnsureSaveDirectoryExists();
        }

        private void EnsureSaveDirectoryExists()
        {
            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }
        }

        #region Save/Load

        public async Task<bool> SaveCharacterAsync(CharacterData data)
        {
            if (data == null || string.IsNullOrEmpty(data.CharacterId))
                return false;

            try
            {
                data.LastSaveTime = DateTime.UtcNow;
                data.IntegrityHash = ComputeIntegrityHash(data);

                string json = JsonUtility.ToJson(data);
                byte[] encrypted = Encrypt(Encoding.UTF8.GetBytes(json));

                string filePath = GetCharacterFilePath(data.CharacterId);
                await Task.Run(() => File.WriteAllBytes(filePath, encrypted));

                Debug.Log($"[CharacterPersistence] Saved character: {data.CharacterName} ({data.CharacterId})");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Failed to save character: {ex.Message}");
                return false;
            }
        }

        public async Task<CharacterData> LoadCharacterAsync(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return null;

            string filePath = GetCharacterFilePath(characterId);
            if (!File.Exists(filePath))
            {
                Debug.LogWarning($"[CharacterPersistence] Character file not found: {characterId}");
                return null;
            }

            try
            {
                byte[] encrypted = await Task.Run(() => File.ReadAllBytes(filePath));
                byte[] decrypted = Decrypt(encrypted);
                
                if (decrypted == null)
                {
                    Debug.LogError($"[CharacterPersistence] Failed to decrypt character: {characterId}");
                    return null;
                }

                string json = Encoding.UTF8.GetString(decrypted);
                CharacterData data = JsonUtility.FromJson<CharacterData>(json);

                if (!ValidateCharacterIntegrity(data))
                {
                    Debug.LogError($"[CharacterPersistence] Character integrity check failed: {characterId}");
                    return null;
                }

                Debug.Log($"[CharacterPersistence] Loaded character: {data.CharacterName} ({data.CharacterId})");
                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Failed to load character: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Validation

        public bool ValidateCharacterIntegrity(CharacterData data)
        {
            if (data == null || data.IntegrityHash == null)
                return false;

            byte[] storedHash = data.IntegrityHash;
            byte[] computedHash = ComputeIntegrityHash(data);

            if (storedHash.Length != computedHash.Length)
                return false;

            for (int i = 0; i < storedHash.Length; i++)
            {
                if (storedHash[i] != computedHash[i])
                    return false;
            }

            return true;
        }

        private byte[] ComputeIntegrityHash(CharacterData data)
        {
            // Temporarily clear hash to compute
            byte[] originalHash = data.IntegrityHash;
            data.IntegrityHash = null;

            string json = JsonUtility.ToJson(data);
            
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                data.IntegrityHash = originalHash;
                return hash;
            }
        }

        #endregion

        #region Network Export/Import

        public byte[] ExportCharacterForNetwork(CharacterData data)
        {
            if (data == null)
                return null;

            try
            {
                data.IntegrityHash = ComputeIntegrityHash(data);
                string json = JsonUtility.ToJson(data);
                return Encrypt(Encoding.UTF8.GetBytes(json));
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Failed to export character: {ex.Message}");
                return null;
            }
        }

        public CharacterData ImportCharacterFromNetwork(byte[] payload)
        {
            if (payload == null || payload.Length == 0)
                return null;

            try
            {
                byte[] decrypted = Decrypt(payload);
                if (decrypted == null)
                    return null;

                string json = Encoding.UTF8.GetString(decrypted);
                CharacterData data = JsonUtility.FromJson<CharacterData>(json);

                if (!ValidateCharacterIntegrity(data))
                {
                    Debug.LogError("[CharacterPersistence] Imported character failed integrity check");
                    return null;
                }

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Failed to import character: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Encryption

        private byte[] Encrypt(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = _encryptionKey;
                aes.IV = _encryptionIV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor())
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                    }
                    return ms.ToArray();
                }
            }
        }

        private byte[] Decrypt(byte[] data)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _encryptionKey;
                    aes.IV = _encryptionIV;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(data))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var result = new MemoryStream())
                    {
                        cs.CopyTo(result);
                        return result.ToArray();
                    }
                }
            }
            catch (CryptographicException)
            {
                return null;
            }
        }

        #endregion

        #region File Management

        public async Task<bool> DeleteCharacterAsync(string characterId)
        {
            if (string.IsNullOrEmpty(characterId))
                return false;

            string filePath = GetCharacterFilePath(characterId);
            if (!File.Exists(filePath))
                return false;

            try
            {
                await Task.Run(() => File.Delete(filePath));
                Debug.Log($"[CharacterPersistence] Deleted character: {characterId}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Failed to delete character: {ex.Message}");
                return false;
            }
        }

        public async Task<string[]> GetAllCharacterIdsAsync()
        {
            try
            {
                return await Task.Run(() =>
                {
                    if (!Directory.Exists(SavePath))
                        return Array.Empty<string>();

                    string[] files = Directory.GetFiles(SavePath, $"*{FILE_EXTENSION}");
                    string[] ids = new string[files.Length];
                    
                    for (int i = 0; i < files.Length; i++)
                    {
                        ids[i] = Path.GetFileNameWithoutExtension(files[i]);
                    }
                    
                    return ids;
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CharacterPersistence] Failed to get character IDs: {ex.Message}");
                return Array.Empty<string>();
            }
        }

        private string GetCharacterFilePath(string characterId)
        {
            return Path.Combine(SavePath, $"{characterId}{FILE_EXTENSION}");
        }

        /// <summary>
        /// Gets the raw encrypted bytes for a saved character (for testing).
        /// </summary>
        public byte[] GetRawSavedBytes(string characterId)
        {
            string filePath = GetCharacterFilePath(characterId);
            if (!File.Exists(filePath))
                return null;
            return File.ReadAllBytes(filePath);
        }

        #endregion
    }
}
