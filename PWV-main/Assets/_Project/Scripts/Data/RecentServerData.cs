using System;
using System.Collections.Generic;

namespace EtherDomes.Data
{
    /// <summary>
    /// Datos de un servidor visitado recientemente
    /// </summary>
    [Serializable]
    public class RecentServerData
    {
        public string ServerId;
        public string ServerName;
        public string ConnectionCode; // Relay code o IP:Puerto
        public bool IsRelay;
        public DateTime LastVisited;
        public int LastKnownPlayerCount;
        public int LastKnownPing;
        public string PasswordHash; // Hash de contraseña si el servidor la tiene
        
        public RecentServerData()
        {
            ServerId = Guid.NewGuid().ToString();
            LastVisited = DateTime.Now;
            PasswordHash = "";
        }
        
        public RecentServerData(string name, string connectionCode, bool isRelay) : this()
        {
            ServerName = name;
            ConnectionCode = connectionCode;
            IsRelay = isRelay;
        }
        
        public bool HasPassword => !string.IsNullOrEmpty(PasswordHash);
    }
    
    /// <summary>
    /// Contenedor para la lista de servidores recientes (máximo 10)
    /// </summary>
    [Serializable]
    public class RecentServerDataList
    {
        public const int MAX_RECENT_SERVERS = 10;
        public List<RecentServerData> Servers = new List<RecentServerData>();
        
        /// <summary>
        /// Agrega o actualiza un servidor en la lista de recientes
        /// </summary>
        public void AddOrUpdate(RecentServerData server)
        {
            // Buscar si ya existe
            var existing = Servers.Find(s => s.ConnectionCode == server.ConnectionCode);
            if (existing != null)
            {
                existing.LastVisited = DateTime.Now;
                existing.ServerName = server.ServerName;
                existing.LastKnownPlayerCount = server.LastKnownPlayerCount;
                existing.LastKnownPing = server.LastKnownPing;
                // Actualizar password solo si viene uno nuevo
                if (!string.IsNullOrEmpty(server.PasswordHash))
                {
                    existing.PasswordHash = server.PasswordHash;
                }
            }
            else
            {
                // Agregar nuevo
                Servers.Insert(0, server);
                
                // Mantener máximo 10
                while (Servers.Count > MAX_RECENT_SERVERS)
                {
                    Servers.RemoveAt(Servers.Count - 1);
                }
            }
            
            // Ordenar por fecha más reciente
            Servers.Sort((a, b) => b.LastVisited.CompareTo(a.LastVisited));
        }
    }
}
