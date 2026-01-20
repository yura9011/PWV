using UnityEngine;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using EtherDomes.Data;
using System;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Gestor principal de guardado/carga con encriptación AES-256.
    /// Utiliza EncryptionService para máxima seguridad anti-trampas.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILE_NAME = "etherdomes_save.ted";
        private const string BACKUP_FILE_NAME = "etherdomes_save_backup.ted";
        
        // Clave de desarrollo - En producción vendría del servidor de autenticación
        private const string DEV_ENCRYPTION_KEY = "EtherDomes2025SecureKey!@#$%^&*()";

        [Header("Configuration")]
        [SerializeField] private bool _enableBackups = true;
        [SerializeField] private bool _enableIntegrityCheck = true;
        [SerializeField] private int _maxBackupFiles = 3;

        public SaveFile CurrentSave { get; private set; }
        public bool IsLoaded { get; private set; }
        
        private IEncryptionService _encryptionService;
        private string _saveFilePath;
        private string _backupFilePath;

        private void Awake()
        {
            if (Instance != null) 
            { 
                Destroy(gameObject); 
                return; 
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeEncryption();
            InitializePaths();
            Load();
        }

        private void InitializeEncryption()
        {
            _encryptionService = new EncryptionService();
            _encryptionService.SetKeyFromString(DEV_ENCRYPTION_KEY);
            
            if (!_encryptionService.IsInitialized)
            {
                Debug.LogError("[SaveManager] Failed to initialize encryption service!");
            }
            else
            {
                Debug.Log("[SaveManager] Encryption service initialized successfully");
            }
        }

        private void InitializePaths()
        {
            _saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            _backupFilePath = Path.Combine(Application.persistentDataPath, BACKUP_FILE_NAME);
            
            Debug.Log($"[SaveManager] Save path: {_saveFilePath}");
        }

        /// <summary>
        /// Guarda el archivo actual con encriptación AES-256.
        /// </summary>
        public bool Save()
        {
            if (CurrentSave == null)
            {
                Debug.LogWarning("[SaveManager] No save data to save");
                return false;
            }

            if (_encryptionService == null)
            {
                Debug.LogError("[SaveManager] Encryption service is null - cannot save");
                return false;
            }

            if (string.IsNullOrEmpty(_saveFilePath))
            {
                Debug.LogError("[SaveManager] Save file path is null or empty");
                return false;
            }

            try 
            {
                // Crear backup antes de guardar
                if (_enableBackups && File.Exists(_saveFilePath))
                {
                    CreateBackup();
                }

                // Actualizar timestamp
                CurrentSave.LastSaved = DateTime.UtcNow;

                // Serializar a JSON
                string json = JsonConvert.SerializeObject(CurrentSave, Formatting.None);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(json);

                // Encriptar datos
                byte[] encryptedData = _encryptionService.Encrypt(jsonBytes);
                if (encryptedData == null)
                {
                    Debug.LogError("[SaveManager] Encryption failed!");
                    return false;
                }

                // Calcular hash de integridad si está habilitado
                byte[] integrityHash = null;
                if (_enableIntegrityCheck)
                {
                    integrityHash = _encryptionService.ComputeHash(jsonBytes);
                }

                // Crear estructura final del archivo
                var saveFileStructure = new EncryptedSaveFile
                {
                    Version = SaveFile.SAVE_VERSION,
                    EncryptedData = Convert.ToBase64String(encryptedData),
                    IntegrityHash = integrityHash != null ? Convert.ToBase64String(integrityHash) : null,
                    SavedAt = DateTime.UtcNow,
                    ClientVersion = Application.version
                };

                // Escribir archivo final
                string finalJson = JsonConvert.SerializeObject(saveFileStructure, Formatting.Indented);
                File.WriteAllText(_saveFilePath, finalJson);

                Debug.Log($"[SaveManager] Save successful - {CurrentSave.Characters.Count} characters, {encryptedData.Length} bytes encrypted");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Save failed: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// Carga el archivo de guardado con desencriptación y verificación de integridad.
        /// </summary>
        public bool Load()
        {
            if (!File.Exists(_saveFilePath))
            {
                Debug.Log("[SaveManager] No save file found, creating new save");
                CurrentSave = new SaveFile();
                IsLoaded = true;
                return true;
            }

            try
            {
                // Leer archivo
                string fileContent = File.ReadAllText(_saveFilePath);
                
                // Intentar cargar como archivo encriptado nuevo
                if (TryLoadEncryptedFile(fileContent))
                {
                    IsLoaded = true;
                    return true;
                }

                // Fallback: intentar cargar archivo legacy (sin encriptar)
                if (TryLoadLegacyFile(fileContent))
                {
                    Debug.LogWarning("[SaveManager] Loaded legacy unencrypted file - will be encrypted on next save");
                    IsLoaded = true;
                    return true;
                }

                // Si todo falla, intentar backup
                return TryLoadBackup();
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load failed: {e.Message}");
                return TryLoadBackup();
            }
        }

        private bool TryLoadEncryptedFile(string fileContent)
        {
            try
            {
                // Deserializar estructura del archivo
                var saveFileStructure = JsonConvert.DeserializeObject<EncryptedSaveFile>(fileContent);
                if (saveFileStructure == null || string.IsNullOrEmpty(saveFileStructure.EncryptedData))
                {
                    return false;
                }

                // Desencriptar datos
                byte[] encryptedData = Convert.FromBase64String(saveFileStructure.EncryptedData);
                byte[] decryptedData = _encryptionService.Decrypt(encryptedData);
                
                if (decryptedData == null)
                {
                    Debug.LogError("[SaveManager] Decryption failed!");
                    return false;
                }

                // Verificar integridad si está disponible
                if (_enableIntegrityCheck && !string.IsNullOrEmpty(saveFileStructure.IntegrityHash))
                {
                    byte[] expectedHash = Convert.FromBase64String(saveFileStructure.IntegrityHash);
                    if (!_encryptionService.VerifyHash(decryptedData, expectedHash))
                    {
                        Debug.LogError("[SaveManager] Integrity check failed! Save file may be corrupted or tampered with.");
                        return false;
                    }
                    Debug.Log("[SaveManager] Integrity check passed");
                }

                // Deserializar datos del juego
                string json = Encoding.UTF8.GetString(decryptedData);
                CurrentSave = JsonConvert.DeserializeObject<SaveFile>(json);
                
                if (CurrentSave == null)
                {
                    Debug.LogError("[SaveManager] Failed to deserialize save data");
                    return false;
                }

                Debug.Log($"[SaveManager] Encrypted save loaded successfully - {CurrentSave.Characters.Count} characters");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Failed to load encrypted file: {e.Message}");
                return false;
            }
        }

        private bool TryLoadLegacyFile(string fileContent)
        {
            try
            {
                // Intentar cargar como JSON directo (legacy)
                CurrentSave = JsonConvert.DeserializeObject<SaveFile>(fileContent);
                if (CurrentSave != null)
                {
                    Debug.Log($"[SaveManager] Legacy save loaded - {CurrentSave.Characters.Count} characters");
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private bool TryLoadBackup()
        {
            if (!_enableBackups || !File.Exists(_backupFilePath))
            {
                Debug.LogError("[SaveManager] No backup available, creating new save");
                CurrentSave = new SaveFile();
                IsLoaded = true;
                return true;
            }

            try
            {
                Debug.LogWarning("[SaveManager] Attempting to load backup file");
                string backupContent = File.ReadAllText(_backupFilePath);
                
                if (TryLoadEncryptedFile(backupContent) || TryLoadLegacyFile(backupContent))
                {
                    Debug.Log("[SaveManager] Backup loaded successfully");
                    IsLoaded = true;
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Backup load failed: {e.Message}");
            }

            // Último recurso: archivo nuevo
            Debug.LogError("[SaveManager] All load attempts failed, creating new save");
            CurrentSave = new SaveFile();
            IsLoaded = true;
            return true;
        }

        private void CreateBackup()
        {
            try
            {
                if (File.Exists(_saveFilePath))
                {
                    File.Copy(_saveFilePath, _backupFilePath, true);
                    Debug.Log("[SaveManager] Backup created");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveManager] Backup creation failed: {e.Message}");
            }
        }

        /// <summary>
        /// Crea un nuevo personaje y lo guarda.
        /// </summary>
        public bool CreateCharacter(string name, CharacterClass characterClass)
        {
            if (CurrentSave == null)
            {
                Debug.LogError("[SaveManager] Save system not initialized");
                return false;
            }
            
            if (!CurrentSave.CanCreateMore)
            {
                Debug.LogWarning($"[SaveManager] Cannot create more characters (max: {SaveFile.MAX_CHARACTERS})");
                return false;
            }
            
            var newCharacter = new CharacterData(name, characterClass);
            CurrentSave.Characters.Add(newCharacter);
            
            bool success = Save();
            if (success)
            {
                Debug.Log($"[SaveManager] Character '{name}' created successfully");
            }
            
            return success;
        }

        /// <summary>
        /// Actualiza los datos de un personaje existente.
        /// </summary>
        public bool UpdateCharacter(CharacterData character)
        {
            if (CurrentSave == null || character == null)
                return false;

            var existingIndex = CurrentSave.Characters.FindIndex(c => c.CharacterId == character.CharacterId);
            if (existingIndex == -1)
            {
                Debug.LogWarning($"[SaveManager] Character {character.CharacterId} not found for update");
                return false;
            }

            CurrentSave.Characters[existingIndex] = character;
            return Save();
        }

        /// <summary>
        /// Elimina un personaje.
        /// </summary>
        public bool DeleteCharacter(string characterId)
        {
            if (CurrentSave == null)
                return false;

            int removed = CurrentSave.Characters.RemoveAll(c => c.CharacterId == characterId);
            if (removed > 0)
            {
                Debug.Log($"[SaveManager] Character {characterId} deleted");
                return Save();
            }

            return false;
        }

        /// <summary>
        /// Obtiene un personaje por ID.
        /// </summary>
        public CharacterData GetCharacter(string characterId)
        {
            return CurrentSave?.Characters.Find(c => c.CharacterId == characterId);
        }

        /// <summary>
        /// Fuerza un guardado inmediato.
        /// </summary>
        public void ForceSave()
        {
            if (CurrentSave != null)
            {
                Save();
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && CurrentSave != null && _encryptionService != null)
            {
                try
                {
                    Save();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to save on pause: {e.Message}");
                }
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && CurrentSave != null && _encryptionService != null)
            {
                try
                {
                    Save();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to save on focus lost: {e.Message}");
                }
            }
        }

        private void OnDestroy()
        {
            // Solo intentar guardar si el SaveManager está completamente inicializado
            if (CurrentSave != null && _encryptionService != null && !string.IsNullOrEmpty(_saveFilePath))
            {
                try
                {
                    Save();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveManager] Failed to save on destroy: {e.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Estructura del archivo encriptado en disco.
    /// </summary>
    [Serializable]
    public class EncryptedSaveFile
    {
        public int Version;
        public string EncryptedData;
        public string IntegrityHash;
        public DateTime SavedAt;
        public string ClientVersion;
    }
}
