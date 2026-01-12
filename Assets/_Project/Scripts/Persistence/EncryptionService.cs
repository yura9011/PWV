using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Servicio de encriptación AES-256 CBC con PKCS7 Padding.
    /// El IV se genera aleatoriamente en cada guardado y se pre-pende al archivo.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        // Clave hardcodeada para desarrollo (16 bytes = 128 bits, se expande a 256)
        // En producción, esta clave vendría de un servidor de autenticación
        private const string DEV_KEY = "1234567890123456";
        
        private byte[] _key;
        private bool _isInitialized = false;

        public EncryptionService()
        {
            // Usar clave de desarrollo por defecto
            SetKeyFromString(DEV_KEY);
        }

        /// <summary>
        /// Constructor con clave personalizada (para producción con server handshake).
        /// </summary>
        public EncryptionService(byte[] key)
        {
            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes for AES-256");
            
            _key = key;
            _isInitialized = true;
        }

        /// <summary>
        /// Establece la clave desde un string (se hashea a 256 bits).
        /// </summary>
        public void SetKeyFromString(string keyString)
        {
            if (string.IsNullOrEmpty(keyString))
            {
                Debug.LogError("[EncryptionService] Key string cannot be null or empty");
                return;
            }

            using (var sha256 = SHA256.Create())
            {
                _key = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyString));
            }
            _isInitialized = true;
            Debug.Log("[EncryptionService] Key initialized (dev mode)");
        }

        /// <summary>
        /// Establece la clave desde bytes (para server handshake).
        /// </summary>
        public void SetKey(byte[] key)
        {
            if (key == null || key.Length != 32)
            {
                Debug.LogError("[EncryptionService] Key must be 32 bytes for AES-256");
                return;
            }
            _key = key;
            _isInitialized = true;
            Debug.Log("[EncryptionService] Key set from server");
        }

        /// <summary>
        /// Encripta datos. El IV aleatorio se pre-pende al resultado.
        /// Formato: [IV 16 bytes][Encrypted Data]
        /// </summary>
        public byte[] Encrypt(byte[] data)
        {
            if (!_isInitialized || _key == null)
            {
                Debug.LogError("[EncryptionService] Not initialized. Call SetKey first.");
                return null;
            }

            if (data == null || data.Length == 0)
                return null;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.GenerateIV(); // IV aleatorio para cada encriptación
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    {
                        // Escribir IV al inicio
                        ms.Write(aes.IV, 0, aes.IV.Length);
                        
                        // Escribir datos encriptados
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
                Debug.LogError($"[EncryptionService] Encryption failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Desencripta datos. Extrae el IV de los primeros 16 bytes.
        /// </summary>
        public byte[] Decrypt(byte[] encryptedData)
        {
            if (!_isInitialized || _key == null)
            {
                Debug.LogError("[EncryptionService] Not initialized. Call SetKey first.");
                return null;
            }

            if (encryptedData == null || encryptedData.Length < 17) // Mínimo: 16 IV + 1 byte data
                return null;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // Extraer IV de los primeros 16 bytes
                    byte[] iv = new byte[16];
                    Array.Copy(encryptedData, 0, iv, 0, 16);
                    aes.IV = iv;

                    // Datos encriptados (sin IV)
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
                Debug.LogError($"[EncryptionService] Decryption failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Calcula hash SHA256 de los datos.
        /// </summary>
        public byte[] ComputeHash(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        /// <summary>
        /// Verifica que el hash coincida con los datos.
        /// Usa comparación de tiempo constante para evitar timing attacks.
        /// </summary>
        public bool VerifyHash(byte[] data, byte[] hash)
        {
            if (data == null || hash == null)
                return false;

            var computedHash = ComputeHash(data);
            if (computedHash == null || computedHash.Length != hash.Length)
                return false;

            // Comparación de tiempo constante
            int diff = 0;
            for (int i = 0; i < hash.Length; i++)
            {
                diff |= hash[i] ^ computedHash[i];
            }
            return diff == 0;
        }

        /// <summary>
        /// Convierte hash a string hexadecimal.
        /// </summary>
        public static string HashToHex(byte[] hash)
        {
            if (hash == null) return "";
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Convierte string hexadecimal a bytes.
        /// </summary>
        public static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return null;
            
            int length = hex.Length / 2;
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        public bool IsInitialized => _isInitialized;
    }
}
