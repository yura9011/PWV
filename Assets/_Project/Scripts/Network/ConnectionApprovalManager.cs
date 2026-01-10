using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using EtherDomes.Data;
using EtherDomes.Persistence;
using EtherDomes.Core;

namespace EtherDomes.Network
{
    /// <summary>
    /// NGO Connection Approval Manager.
    /// Hooks into NetworkManager to validate incoming connections and payloads.
    /// </summary>
    public class ConnectionApprovalManager : MonoBehaviour
    {
        [Header("Validation Settings")]
        [SerializeField] private bool _skipAuthenticationForTesting = true;
        [SerializeField] private Transform _defaultSpawnPoint;

        private IConnectionApprovalHandler _approvalHandler;

        private void Start()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[ConnectionApprovalManager] NetworkManager missing!");
                return;
            }

            // Initialize handler
            _approvalHandler = new ConnectionApprovalHandler();

            // Set the callback
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            
            Debug.Log("[ConnectionApprovalManager] Hooked into NetworkManager ConnectionApprovalCallback");
        }

        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            try
            {
                Debug.Log($"[ConnectionApprovalManager] Validating connection from ClientID: {request.ClientNetworkId}");

                // Default response settings
                response.Approved = false;
                response.CreatePlayerObject = false; // We will spawn player manually to set data
                response.PlayerPrefabHash = null;

                if (_skipAuthenticationForTesting)
                {
                    Debug.Log("[ConnectionApprovalManager] Skipping auth (Testing Mode)");
                    response.Approved = true;
                    response.CreatePlayerObject = true; // Auto spawn for testing
                    // Set default pos
                    response.Position = (_defaultSpawnPoint != null) ? _defaultSpawnPoint.position : Vector3.zero;
                    response.Rotation = Quaternion.identity;
                    return;
                }

                // 1. Validate Payload
                byte[] payloadBytes = request.Payload;
                if (payloadBytes == null || payloadBytes.Length == 0)
                {
                    Debug.LogWarning("[ConnectionApprovalManager] Connection rejected: Empty Payload");
                    response.Reason = "Empty Payload";
                    return;
                }

                // 2. Decode Payload
                string payloadJson = Encoding.UTF8.GetString(payloadBytes);
                ConnectionPayloadMessage payloadMsg;
                
                try 
                {
                    payloadMsg = JsonUtility.FromJson<ConnectionPayloadMessage>(payloadJson);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ConnectionApprovalManager] Payload parsing error: {e.Message}");
                    response.Reason = "Invalid Payload Format";
                    return;
                }

                // 4. Approve
                response.Approved = true;
                response.CreatePlayerObject = true;
                
                // 5. Set Spawn Position from Payload
                Vector3 spawnPos = Vector3.zero;
                if (payloadMsg.HasSavedPosition)
                {
                    spawnPos = payloadMsg.LastPosition;
                }
                else
                {
                    spawnPos = (_defaultSpawnPoint != null) ? _defaultSpawnPoint.position : Vector3.zero;
                }

                response.Position = spawnPos;
                response.Rotation = Quaternion.identity;

                Debug.Log($"[ConnectionApprovalManager] Approved {request.ClientNetworkId}. Class: {payloadMsg.ClassID}, Spawn: {spawnPos}");
                
                StorePlayerData(request.ClientNetworkId, payloadMsg);
            }
            catch (Exception e)
            {
                Debug.LogError($"[ConnectionApprovalManager] CRASH: {e.Message}\n{e.StackTrace}");
                response.Approved = false;
                response.Reason = "ServerInternalError";
            }
        }

        private Vector3 GetSpawnPosition(ConnectionPayloadMessage? msg)
        {
            if (msg.HasValue && msg.Value.HasSavedPosition)
            {
                return msg.Value.LastPosition;
            }
            return _defaultSpawnPoint != null ? _defaultSpawnPoint.position : Vector3.zero;
        }

        private void StorePlayerData(ulong clientId, ConnectionPayloadMessage startData)
        {
            // This is a temporary storage pattern often used in NGO
            // We can have a static map or a singleton service
            // For now, let's just log it. Real implementation in Phase 2 Player Migration.
            Debug.Log($"[ConnectionApprovalManager] Storing data for Client {clientId}: Class {startData.ClassID}");
            
            // In a real implementation we would have:
            // PlayerDataManager.Instance.SetData(clientId, startData);
        }
    }
}
