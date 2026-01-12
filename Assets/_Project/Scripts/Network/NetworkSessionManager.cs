using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using EtherDomes.Core;
using EtherDomes.Persistence;

namespace EtherDomes.Network
{
    /// <summary>
    /// Manages network sessions for The Ether Domes using Unity Netcode for GameObjects (NGO).
    /// Wraps NetworkManager with a simplified API for Host/Client modes.
    /// </summary>
    public class NetworkSessionManager : MonoBehaviour, INetworkSessionManager
    {
        public const int DEFAULT_MAX_PLAYERS = 10;

        [Header("References")]
        [SerializeField] private ConnectionApprovalManager _connectionApprovalManager;
        
        // Events
        public event Action<ulong> OnPlayerConnected;
        public event Action<ulong> OnPlayerDisconnected;
        public event Action<string> OnConnectionFailed;

        // State Properties
        public bool IsHost => NetworkManager.Singleton != null && NetworkManager.Singleton.IsHost;
        public bool IsClient => NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient;
        public bool IsServer => NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer;
        
        public int ConnectedPlayerCount => NetworkManager.Singleton != null ? NetworkManager.Singleton.ConnectedClients.Count : 0;
        public ulong LocalClientId => NetworkManager.Singleton != null ? NetworkManager.Singleton.LocalClientId : 0;

        // In NGO, MaxPlayers is usually set in Transport, but we can manage limits in ConnectionApproval
        public int MaxPlayers { get; private set; } = DEFAULT_MAX_PLAYERS;

        private void Start()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogError("[NetworkSessionManager] NetworkManager.Singleton is null! Make sure a NetworkManager exists in the scene.");
                return;
            }

            // Subscribe to events
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
            
            // Suscribirse al evento de cliente detenido para capturar razón de desconexión
            NetworkManager.Singleton.OnClientStopped += OnClientStoppedCallback;

            Debug.Log("[NetworkSessionManager] Initialized and listening to NetworkManager events.");
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
                NetworkManager.Singleton.OnClientStopped -= OnClientStoppedCallback;
            }
        }
        
        private void OnClientStoppedCallback(bool wasHost)
        {
            if (!wasHost && NetworkManager.Singleton != null)
            {
                // Obtener la razón de desconexión si está disponible
                string reason = NetworkManager.Singleton.DisconnectReason;
                if (!string.IsNullOrEmpty(reason))
                {
                    Debug.Log($"[NetworkSessionManager] Client stopped with reason: {reason}");
                    OnConnectionFailed?.Invoke(reason);
                }
            }
        }

        #region Session Management

        public bool StartAsHost(ushort port = 7777)
        {
            ConfigureTransport(port);
            
            // NOTE: Connection Approval must be set up BEFORE starting host
            // This is handled by ConnectionApprovalManager in Start() usually, 
            // but we ensure it here if needed.
            
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log($"[NetworkSessionManager] Started as Host on port {port}");
                return true;
            }
            else
            {
                Debug.LogError("[NetworkSessionManager] Failed to start Host!");
                OnConnectionFailed?.Invoke("Failed to start Host");
                return false;
            }
        }

        public void StartAsClient(string ipAddress, ushort port = 7777)
        {
            StartAsClientWithPayload(ipAddress, port);
        }

        public void StartAsClientWithPayload(string ipAddress, ushort port = 7777)
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                OnConnectionFailed?.Invoke("Invalid IP address");
                return;
            }

            // Configure Transport IP/Port
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Address = ipAddress;
                transport.ConnectionData.Port = port;
            }

            // Prepare Payload
            SetConnectionPayload();

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log($"[NetworkSessionManager] Connecting as Client to {ipAddress}:{port} with Payload");
            }
            else
            {
                Debug.LogError("[NetworkSessionManager] Failed to start Client!");
                OnConnectionFailed?.Invoke("Failed to start Client");
            }
        }

        public async System.Threading.Tasks.Task<string> StartHostWithRelay(int maxPlayers = DEFAULT_MAX_PLAYERS)
        {
            if (RelayManager.Instance == null)
            {
                Debug.LogError("[NetworkSessionManager] RelayManager not found!");
                return null;
            }

            // 0. Ensure clean state
            if (NetworkManager.Singleton.IsListening)
            {
                Debug.LogWarning("[NetworkSessionManager] Network was already active. Shutting down...");
                NetworkManager.Singleton.Shutdown();
            }

            // 1. Create Relay Allocation
            string joinCode = await RelayManager.Instance.CreateRelay(maxPlayers);
            if (string.IsNullOrEmpty(joinCode))
            {
                OnConnectionFailed?.Invoke("Relay Allocation Failed");
                return null;
            }

            // 2. Transport is already configured by CreateRelay
            
            // 3. Start Host
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log($"[NetworkSessionManager] Started Relay Host. Join Code: {joinCode}");
                return joinCode;
            }
            else
            {
                string msg = "NetManager StartHost Failed (Firewall?)";
                Debug.LogError($"[NetworkSessionManager] {msg}");
                
                // Hack: Set LastError so UI displays it
                if (RelayManager.Instance != null) 
                    RelayManager.Instance.SetLastError(msg); // Need to expose setter or make public field
                    
                OnConnectionFailed?.Invoke(msg);
                return null;
            }
        }

        public async System.Threading.Tasks.Task<bool> StartClientWithRelay(string joinCode)
        {
            if (RelayManager.Instance == null)
            {
                Debug.LogError("[NetworkSessionManager] RelayManager not found!");
                return false;
            }

            bool success = await RelayManager.Instance.JoinRelay(joinCode);
            if (!success)
            {
                OnConnectionFailed?.Invoke("Relay Join Failed");
                return false;
            }

            SetConnectionPayload();

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log($"[NetworkSessionManager] Connecting to Relay with code: {joinCode}");
                return true;
            }
            else
            {
                Debug.LogError("[NetworkSessionManager] Failed to start Relay Client!");
                OnConnectionFailed?.Invoke("Failed to start Relay Client");
                return false;
            }
        }

        public void StartAsDedicatedServer(ushort port = 7777)
        {
            ConfigureTransport(port);
            
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log($"[NetworkSessionManager] Started as Dedicated Server on port {port}");
            }
            else
            {
                Debug.LogError("[NetworkSessionManager] Failed to start Server!");
            }
        }

        private void SetConnectionPayload()
        {
            SetConnectionPayload("");
        }
        
        private void SetConnectionPayload(string passwordHash)
        {
            Vector3 defaultSpawnPos = Vector3.zero;
            Vector3 lastPos = PositionPersistenceManager.LoadPosition(defaultSpawnPos);
            bool hasPos = PositionPersistenceManager.HasSavedPosition();

            var payloadData = new ConnectionPayloadMessage(
                ClassSelectionData.GetClassID(),
                lastPos,
                hasPos,
                passwordHash
            );

            string payloadJson = JsonUtility.ToJson(payloadData);
            byte[] payloadBytes = System.Text.Encoding.UTF8.GetBytes(payloadJson);

            NetworkManager.Singleton.NetworkConfig.ConnectionData = payloadBytes;
        }
        
        /// <summary>
        /// Conecta como cliente con contraseña
        /// </summary>
        public async System.Threading.Tasks.Task<bool> StartClientWithRelay(string joinCode, string passwordHash)
        {
            if (RelayManager.Instance == null)
            {
                Debug.LogError("[NetworkSessionManager] RelayManager not found!");
                return false;
            }

            bool success = await RelayManager.Instance.JoinRelay(joinCode);
            if (!success)
            {
                OnConnectionFailed?.Invoke("Relay Join Failed");
                return false;
            }

            SetConnectionPayload(passwordHash);

            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log($"[NetworkSessionManager] Connecting to Relay with code: {joinCode}");
                return true;
            }
            else
            {
                Debug.LogError("[NetworkSessionManager] Failed to start Relay Client!");
                OnConnectionFailed?.Invoke("Failed to start Relay Client");
                return false;
            }
        }

        public void Disconnect()
        {
            if (NetworkManager.Singleton == null) return;
            NetworkManager.Singleton.Shutdown();
            Debug.Log("[NetworkSessionManager] Shutting down network.");
        }

        private void ConfigureTransport(ushort port)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
            {
                transport.ConnectionData.Port = port;
                transport.ConnectionData.ServerListenAddress = "0.0.0.0"; // Listen on all interfaces
            }
        }

        #endregion

        #region Callbacks

        private void OnClientConnected(ulong clientId)
        {
            OnPlayerConnected?.Invoke(clientId);
            Debug.Log($"[NetworkSessionManager] Client {clientId} connected.");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            // If local client disconnected, check for rejection reason
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                string reason = NetworkManager.Singleton.DisconnectReason;
                Debug.Log($"[NetworkSessionManager] Local client disconnected. Reason: {reason}");
                
                // Si hay una razón de desconexión, es un rechazo del servidor
                if (!string.IsNullOrEmpty(reason))
                {
                    OnConnectionFailed?.Invoke(reason);
                }
                else
                {
                    OnPlayerDisconnected?.Invoke(clientId);
                }
            }
            else
            {
                OnPlayerDisconnected?.Invoke(clientId);
                Debug.Log($"[NetworkSessionManager] Client {clientId} disconnected.");
            }
        }

        private void OnTransportFailure()
        {
            OnConnectionFailed?.Invoke("Transport Failure");
            Debug.LogError("[NetworkSessionManager] Transport failure occurred.");
        }

        #endregion
    }
}
