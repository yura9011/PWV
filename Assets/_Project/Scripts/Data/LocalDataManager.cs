using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace EtherDomes.Data
{
    /// <summary>
    /// Gestiona la persistencia local de mundos, personajes y servidores recientes.
    /// Usa Newtonsoft.Json para serialización y AES-256 para archivos .ted
    /// </summary>
    public static class LocalDataManager
    {
        private const string WORLDS_FILE = "worlds.json";
        private const string CHARACTERS_FILE = "characters.json"; // Legacy
        private const string SAVEFILE_NAME = "savefile.ted"; // Nuevo formato encriptado
        private const string RECENT_SERVERS_FILE = "recent_servers.json";
        
        // Clave de desarrollo (en producción vendría de un servidor)
        private const string DEV_KEY = "1234567890123456";
        
        private static string SavePath => Application.persistentDataPath;
        
        private static WorldSaveDataList _worldsCache;
        private static SaveFile _saveFileCache; // Nuevo formato
        private static CharacterSaveDataList _legacyCharactersCache; // Legacy para migración
        private static RecentServerDataList _recentServersCache;
        private static byte[] _encryptionKey;

        private static byte[] EncryptionKey
        {
            get
            {
                if (_encryptionKey == null)
                {
                    using (var sha256 = SHA256.Create())
                    {
                        _encryptionKey = sha256.ComputeHash(Encoding.UTF8.GetBytes(DEV_KEY));
                    }
                }
                return _encryptionKey;
            }
        }
        
        #region Worlds
        
        public static WorldSaveDataList GetWorlds()
        {
            if (_worldsCache == null)
            {
                _worldsCache = LoadFromFile<WorldSaveDataList>(WORLDS_FILE) ?? new WorldSaveDataList();
            }
            return _worldsCache;
        }
        
        public static WorldSaveData CreateWorld(string name, string password = "")
        {
            var worlds = GetWorlds();
            var newWorld = new WorldSaveData(name, password);
            worlds.Worlds.Add(newWorld);
            SaveWorlds();
            return newWorld;
        }
        
        public static bool DeleteWorld(string worldId)
        {
            var worlds = GetWorlds();
            var world = worlds.Worlds.Find(w => w.WorldId == worldId);
            if (world != null)
            {
                worlds.Worlds.Remove(world);
                SaveWorlds();
                return true;
            }
            return false;
        }
        
        public static void UpdateWorldLastPlayed(string worldId)
        {
            var worlds = GetWorlds();
            var world = worlds.Worlds.Find(w => w.WorldId == worldId);
            if (world != null)
            {
                world.LastPlayedAt = DateTime.Now;
                SaveWorlds();
            }
        }
        
        public static void SaveWorlds()
        {
            if (_worldsCache != null)
            {
                SaveToFile(WORLDS_FILE, _worldsCache);
            }
        }
        
        #endregion
        
        #region SaveFile (Nuevo Sistema)

        /// <summary>
        /// Obtiene el SaveFile principal. Migra automáticamente desde formato legacy si existe.
        /// </summary>
        public static SaveFile GetSaveFile()
        {
            if (_saveFileCache == null)
            {
                _saveFileCache = LoadSaveFile();
                
                // Si no existe, intentar migrar desde formato legacy
                if (_saveFileCache == null)
                {
                    _saveFileCache = MigrateFromLegacy();
                }
                
                if (_saveFileCache == null)
                {
                    _saveFileCache = new SaveFile();
                    Debug.Log("[LocalDataManager] Created new SaveFile");
                }
            }
            return _saveFileCache;
        }

        /// <summary>
        /// Carga el SaveFile encriptado (.ted)
        /// </summary>
        private static SaveFile LoadSaveFile()
        {
            string path = Path.Combine(SavePath, SAVEFILE_NAME);
            
            if (!File.Exists(path))
                return null;

            try
            {
                byte[] encryptedData = File.ReadAllBytes(path);
                byte[] decryptedData = DecryptData(encryptedData);
                
                if (decryptedData == null)
                {
                    Debug.LogError("[LocalDataManager] Failed to decrypt savefile");
                    return null;
                }

                string json = Encoding.UTF8.GetString(decryptedData);
                var saveFile = JsonConvert.DeserializeObject<SaveFile>(json);
                
                Debug.Log($"[LocalDataManager] Loaded SaveFile v{saveFile?.Version} with {saveFile?.Count} characters");
                return saveFile;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Error loading savefile: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// Guarda el SaveFile encriptado (.ted)
        /// </summary>
        public static void SaveSaveFile()
        {
            if (_saveFileCache == null) return;

            string path = Path.Combine(SavePath, SAVEFILE_NAME);

            try
            {
                _saveFileCache.LastSaved = DateTime.Now;
                string json = JsonConvert.SerializeObject(_saveFileCache, Formatting.Indented);
                byte[] data = Encoding.UTF8.GetBytes(json);
                byte[] encryptedData = EncryptData(data);

                if (encryptedData == null)
                {
                    Debug.LogError("[LocalDataManager] Failed to encrypt savefile");
                    return;
                }

                File.WriteAllBytes(path, encryptedData);
                Debug.Log($"[LocalDataManager] Saved {SAVEFILE_NAME}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Error saving savefile: {e.Message}");
            }
        }

        /// <summary>
        /// Migra desde el formato legacy (characters.json) al nuevo formato encriptado.
        /// </summary>
        private static SaveFile MigrateFromLegacy()
        {
            string legacyPath = Path.Combine(SavePath, CHARACTERS_FILE);
            
            if (!File.Exists(legacyPath))
                return null;

            Debug.Log("[LocalDataManager] Found legacy characters.json, migrating...");

            try
            {
                string json = File.ReadAllText(legacyPath);
                var legacyData = JsonConvert.DeserializeObject<CharacterSaveDataList>(json);
                
                if (legacyData == null || legacyData.Characters == null)
                    return null;

                var newSaveFile = new SaveFile();
                
                foreach (var legacy in legacyData.Characters)
                {
                    var newChar = legacy.ToCharacterData();
                    newSaveFile.Characters.Add(newChar);
                }

                // Guardar nuevo formato
                _saveFileCache = newSaveFile;
                SaveSaveFile();

                // Renombrar archivo legacy como backup
                string backupPath = legacyPath + ".backup";
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
                File.Move(legacyPath, backupPath);

                Debug.Log($"[LocalDataManager] Migrated {newSaveFile.Count} characters from legacy format");
                return newSaveFile;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Migration failed: {e.Message}");
                return null;
            }
        }

        #endregion

        #region Characters (Nuevo API)

        /// <summary>
        /// Obtiene todos los personajes del SaveFile.
        /// </summary>
        public static System.Collections.Generic.List<CharacterData> GetAllCharacters()
        {
            return GetSaveFile().Characters;
        }

        /// <summary>
        /// Obtiene un personaje por ID.
        /// </summary>
        public static CharacterData GetCharacter(string characterId)
        {
            return GetSaveFile().Characters.Find(c => c.CharacterId == characterId);
        }

        /// <summary>
        /// Crea un nuevo personaje.
        /// </summary>
        public static CharacterData CreateCharacter(string name, CharacterClass characterClass)
        {
            var saveFile = GetSaveFile();
            
            if (!saveFile.CanCreateMore)
            {
                Debug.LogWarning("[LocalDataManager] No se pueden crear más personajes (máximo 12)");
                return null;
            }

            var newCharacter = new CharacterData(name, characterClass);
            saveFile.Characters.Add(newCharacter);
            SaveSaveFile();
            
            Debug.Log($"[LocalDataManager] Created character: {name} ({characterClass})");
            return newCharacter;
        }

        /// <summary>
        /// Elimina un personaje por ID.
        /// </summary>
        public static bool DeleteCharacter(string characterId)
        {
            var saveFile = GetSaveFile();
            var character = saveFile.Characters.Find(c => c.CharacterId == characterId);
            
            if (character != null)
            {
                saveFile.Characters.Remove(character);
                SaveSaveFile();
                Debug.Log($"[LocalDataManager] Deleted character: {character.Name}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Actualiza un personaje existente.
        /// </summary>
        public static void UpdateCharacter(CharacterData character)
        {
            if (character == null) return;
            character.LastPlayedAt = DateTime.Now;
            SaveSaveFile();
        }

        /// <summary>
        /// Obtiene el número de slots disponibles.
        /// </summary>
        public static int GetAvailableSlots()
        {
            return SaveFile.MAX_CHARACTERS - GetSaveFile().Count;
        }

        /// <summary>
        /// Verifica si se pueden crear más personajes.
        /// </summary>
        public static bool CanCreateMoreCharacters()
        {
            return GetSaveFile().CanCreateMore;
        }

        #endregion

        #region Characters Legacy (Compatibilidad)
        
        [Obsolete("Use GetAllCharacters() instead")]
        public static CharacterSaveDataList GetCharacters()
        {
            if (_legacyCharactersCache == null)
            {
                _legacyCharactersCache = LoadFromFile<CharacterSaveDataList>(CHARACTERS_FILE) ?? new CharacterSaveDataList();
            }
            return _legacyCharactersCache;
        }
        
        [Obsolete("Use CreateCharacter(name, characterClass) instead")]
        public static CharacterSaveData CreateCharacter(string name)
        {
            var characters = GetCharacters();
            if (!characters.CanCreateMore)
            {
                Debug.LogWarning("[LocalDataManager] No se pueden crear más personajes (máximo 12)");
                return null;
            }
            
            var newCharacter = new CharacterSaveData(name);
            characters.Characters.Add(newCharacter);
            SaveCharacters();
            return newCharacter;
        }
        
        [Obsolete("Use UpdateCharacter(CharacterData) instead")]
        public static void UpdateCharacter(CharacterSaveData character)
        {
            character.LastPlayedAt = DateTime.Now;
            SaveCharacters();
        }
        
        [Obsolete("Use SaveSaveFile() instead")]
        public static void SaveCharacters()
        {
            if (_legacyCharactersCache != null)
            {
                SaveToFile(CHARACTERS_FILE, _legacyCharactersCache);
            }
        }
        
        #endregion
        
        #region Recent Servers
        
        public static RecentServerDataList GetRecentServers()
        {
            if (_recentServersCache == null)
            {
                _recentServersCache = LoadFromFile<RecentServerDataList>(RECENT_SERVERS_FILE) ?? new RecentServerDataList();
            }
            return _recentServersCache;
        }
        
        public static void AddRecentServer(string name, string connectionCode, bool isRelay, int playerCount = 0, int ping = 0, string passwordHash = "")
        {
            var servers = GetRecentServers();
            var server = new RecentServerData(name, connectionCode, isRelay)
            {
                LastKnownPlayerCount = playerCount,
                LastKnownPing = ping,
                PasswordHash = passwordHash
            };
            servers.AddOrUpdate(server);
            SaveRecentServers();
        }
        
        public static void SaveRecentServers()
        {
            if (_recentServersCache != null)
            {
                SaveToFile(RECENT_SERVERS_FILE, _recentServersCache);
            }
        }
        
        public static bool DeleteRecentServer(string serverId)
        {
            var servers = GetRecentServers();
            var server = servers.Servers.Find(s => s.ServerId == serverId);
            if (server != null)
            {
                servers.Servers.Remove(server);
                SaveRecentServers();
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region File Operations
        
        private static T LoadFromFile<T>(string fileName) where T : class
        {
            string path = Path.Combine(SavePath, fileName);
            
            if (!File.Exists(path))
            {
                return null;
            }
            
            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Error loading {fileName}: {e.Message}");
                return null;
            }
        }
        
        private static void SaveToFile<T>(string fileName, T data)
        {
            string path = Path.Combine(SavePath, fileName);
            
            try
            {
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(path, json);
                Debug.Log($"[LocalDataManager] Saved {fileName}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Error saving {fileName}: {e.Message}");
            }
        }
        
        /// <summary>
        /// Limpia la caché para forzar recarga desde disco
        /// </summary>
        public static void ClearCache()
        {
            _worldsCache = null;
            _saveFileCache = null;
            _legacyCharactersCache = null;
            _recentServersCache = null;
        }

        /// <summary>
        /// Obtiene la ruta completa del archivo de guardado.
        /// </summary>
        public static string GetSaveFilePath()
        {
            return Path.Combine(SavePath, SAVEFILE_NAME);
        }

        /// <summary>
        /// Verifica si existe un archivo de guardado.
        /// </summary>
        public static bool SaveFileExists()
        {
            return File.Exists(Path.Combine(SavePath, SAVEFILE_NAME));
        }

        /// <summary>
        /// Exporta el SaveFile como JSON (para debug).
        /// </summary>
        public static string ExportSaveFileAsJson()
        {
            var saveFile = GetSaveFile();
            return JsonConvert.SerializeObject(saveFile, Formatting.Indented);
        }
        
        #endregion

        #region Encryption (AES-256 CBC)

        /// <summary>
        /// Encripta datos con AES-256 CBC. El IV se pre-pende al resultado.
        /// </summary>
        private static byte[] EncryptData(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.GenerateIV();
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        {
                            cs.Write(data, 0, data.Length);
                            cs.FlushFinalBlock();
                        }
                        
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalDataManager] Encryption failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Desencripta datos AES-256 CBC. Extrae el IV de los primeros 16 bytes.
        /// </summary>
        private static byte[] DecryptData(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length < 17)
                return null;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = EncryptionKey;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    byte[] iv = new byte[16];
                    Array.Copy(encryptedData, 0, iv, 0, 16);
                    aes.IV = iv;

                    byte[] cipherText = new byte[encryptedData.Length - 16];
                    Array.Copy(encryptedData, 16, cipherText, 0, cipherText.Length);

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(cipherText))
                    using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (var output = new MemoryStream())
                    {
                        cs.CopyTo(output);
                        return output.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalDataManager] Decryption failed: {ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
