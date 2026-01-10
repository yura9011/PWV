using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace EtherDomes.Persistence
{
    /// <summary>
    /// Provides AES encryption and SHA256 hashing for character data.
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        // Default key and IV - in production, these should be securely generated/stored
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptionService()
        {
            // Generate deterministic key from a seed (for demo purposes)
            // In production, use secure key management
            _key = GenerateKeyFromSeed("EtherDomes_CharacterData_Key_2024");
            _iv = GenerateIVFromSeed("EtherDomes_IV_Seed");
        }

        public EncryptionService(byte[] key, byte[] iv)
        {
            if (key == null || key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes for AES-256");
            if (iv == null || iv.Length != 16)
                throw new ArgumentException("IV must be 16 bytes for AES");

            _key = key;
            _iv = iv;
        }

        public byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var encryptor = aes.CreateEncryptor())
                    using (var ms = new MemoryStream())
                    using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
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


        public byte[] Decrypt(byte[] encryptedData)
        {
            if (encryptedData == null || encryptedData.Length == 0)
                return null;

            try
            {
                using (var aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var ms = new MemoryStream(encryptedData))
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

        public byte[] ComputeHash(byte[] data)
        {
            if (data == null || data.Length == 0)
                return null;

            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(data);
            }
        }

        public bool VerifyHash(byte[] data, byte[] hash)
        {
            if (data == null || hash == null)
                return false;

            var computedHash = ComputeHash(data);
            if (computedHash == null || computedHash.Length != hash.Length)
                return false;

            // Constant-time comparison to prevent timing attacks
            int diff = 0;
            for (int i = 0; i < hash.Length; i++)
            {
                diff |= hash[i] ^ computedHash[i];
            }
            return diff == 0;
        }

        private byte[] GenerateKeyFromSeed(string seed)
        {
            using (var sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(seed));
            }
        }

        private byte[] GenerateIVFromSeed(string seed)
        {
            using (var md5 = MD5.Create())
            {
                return md5.ComputeHash(Encoding.UTF8.GetBytes(seed));
            }
        }
    }
}
