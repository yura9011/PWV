using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Data
{
    /// <summary>
    /// Datos de un mundo/partida guardado localmente
    /// </summary>
    [Serializable]
    public class WorldSaveData
    {
        public string WorldId;
        public string WorldName;
        public string PasswordHash; // SHA256 hash, vacío si no tiene contraseña
        public DateTime CreatedAt;
        public DateTime LastPlayedAt;
        
        public WorldSaveData()
        {
            WorldId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
            LastPlayedAt = DateTime.Now;
        }
        
        public WorldSaveData(string name, string password = "")
        {
            WorldId = Guid.NewGuid().ToString();
            WorldName = name;
            PasswordHash = string.IsNullOrEmpty(password) ? "" : ComputeHash(password);
            CreatedAt = DateTime.Now;
            LastPlayedAt = DateTime.Now;
        }
        
        public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
        
        public bool ValidatePassword(string password)
        {
            if (!HasPassword) return true;
            return PasswordHash == ComputeHash(password);
        }
        
        private static string ComputeHash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(input);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
    
    /// <summary>
    /// Contenedor para la lista de mundos guardados
    /// </summary>
    [Serializable]
    public class WorldSaveDataList
    {
        public List<WorldSaveData> Worlds = new List<WorldSaveData>();
    }
}
