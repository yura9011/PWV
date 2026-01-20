using Unity.Netcode;
using UnityEngine;

namespace EtherDomes.Core
{
    /// <summary>
    /// Network message containing player connection data.
    /// Sent from client to server during connection via ConnectionApproval.
    /// Adapted for Unity Netcode for GameObjects (NGO).
    /// </summary>
    public struct ConnectionPayloadMessage : INetworkSerializable
    {
        /// <summary>
        /// Player's selected class (0 = Guerrero, 1 = Mago).
        /// </summary>
        public int ClassID;
        
        /// <summary>
        /// Player's last known position for spawn.
        /// </summary>
        public Vector3 LastPosition;
        
        /// <summary>
        /// Whether the player has a saved position.
        /// </summary>
        public bool HasSavedPosition;
        
        /// <summary>
        /// Password hash for server validation (empty if no password).
        /// </summary>
        public string PasswordHash;

        /// <summary>
        /// JSON serializado de CharacterData para validación en el Host.
        /// </summary>
        public string CharacterDataJson;
        
        public ConnectionPayloadMessage(int classID, Vector3 lastPosition, bool hasSavedPosition, string passwordHash = "", string characterDataJson = "")
        {
            ClassID = classID;
            LastPosition = lastPosition;
            HasSavedPosition = hasSavedPosition;
            PasswordHash = passwordHash ?? "";
            CharacterDataJson = characterDataJson ?? "";
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClassID);
            serializer.SerializeValue(ref LastPosition);
            serializer.SerializeValue(ref HasSavedPosition);
            
            // Asegurar que strings no sean null antes de serializar
            if (serializer.IsWriter)
            {
                if (PasswordHash == null) PasswordHash = "";
                if (CharacterDataJson == null) CharacterDataJson = "";
            }
            serializer.SerializeValue(ref PasswordHash);
            serializer.SerializeValue(ref CharacterDataJson);
        }
    }
}
