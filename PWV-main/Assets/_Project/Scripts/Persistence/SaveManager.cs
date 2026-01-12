using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using EtherDomes.Data;
using System;

namespace EtherDomes.Persistence
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILE_NAME = "etherdomes_save.ted";
        private const string MOCK_KEY = "12345678901234561234567890123456"; // 32 bytes for AES-256

        public SaveFile CurrentSave { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }

        public void Save()
        {
            if (CurrentSave == null) CurrentSave = new SaveFile();
            
            try 
            {
                string json = JsonConvert.SerializeObject(CurrentSave, Formatting.Indented);
                string encrypted = Encrypt(json, MOCK_KEY);
                string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
                File.WriteAllText(path, encrypted);
                Debug.Log($"[SaveManager] Saved to {path}");
            }
            catch (Exception e)
            {
                 Debug.LogError($"[SaveManager] Save Failed: {e.Message}");
            }
        }

        public void Load()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            if (!File.Exists(path))
            {
                CurrentSave = new SaveFile();
                return;
            }

            try
            {
                string encrypted = File.ReadAllText(path);
                string json = Decrypt(encrypted, MOCK_KEY);
                CurrentSave = JsonConvert.DeserializeObject<SaveFile>(json);
                 Debug.Log($"[SaveManager] Loaded save file with {CurrentSave.Characters.Count} characters.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveManager] Load Failed: {e.Message}. Starting fresh.");
                CurrentSave = new SaveFile();
            }
        }
        
        // AES Implementation
        private string Encrypt(string plainText, string key)
        {
            byte[] iv = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }

            byte[] array;
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }
                        array = memoryStream.ToArray();
                    }
                }
            }

            var result = new byte[iv.Length + array.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(array, 0, result, iv.Length, array.Length);

            return Convert.ToBase64String(result);
        }

        private string Decrypt(string cipherText, string key)
        {
            // If the file is plain JSON (dev legacy), this might fail. 
            // Phase 2 assumes strict encryption.
            
            byte[] fullCipher = Convert.FromBase64String(cipherText);
            
            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - 16];
            
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(cipher))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        
        // Helper
        public void CreateCharacter(string name, int classId)
        {
            if (CurrentSave == null) CurrentSave = new SaveFile();
            
            var newChar = new CharacterData();
            newChar.Name = name;
            newChar.ClassID = classId;
            // newChar.Level = 1; // Default set in constructor
            
            CurrentSave.Characters.Add(newChar);
            Save();
        }
    }
}
