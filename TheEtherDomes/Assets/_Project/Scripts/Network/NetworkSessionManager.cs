using System;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace EtherDomes.Network
{
    /// <summary>
    /// Manages network sessions using Unity Netcode for GameObjects.
    /// Supports Host, Client, and Dedicated Server modes.
    /// </summary>
    public class NetworkSessionManager : MonoBehaviour, INetworkSessionManager
    {
        public const int MAX_PLAYERS = 10;
        public const ushort DEFAULT_PORT = 7777;

        [SerializeField] private NetworkManager _networkManager;
        [SerializeField] private UnityTransport _transport;

        private IConnectionApprovalHandler _approvalHandler;

        public event Action<ulong> OnPlayerConnected;
        public event Action<ulong> OnPlayerDisconnected;
        public event Action<string> OnConnectionFailed;

        public bool IsHost => _networkManager != null && _networkManager.IsHost;
        public bool IsClient => _networkManager != null && _networkManager.IsClient;
        public bool IsServer => _networkManager != null && _networkManager.IsServer;
        public int ConnectedPlayerCount => _networkManager?.ConnectedClientsIds?.Count ?? 0;
        public int MaxPlayers => MAX_PLAYERS;
        public ulong LocalClientId => _networkManager?.LocalClientId ?? 0;

        private void Awake()
        {
            if (_networkManager == null)
                _networkManager = GetComponent<NetworkManager>();
            
            if (_transport == null)
                _transport = GetComponent<UnityTransport>();

            if (_networkManager == null)
            {
                Debug.LogError("NetworkSessionManager: NetworkManager component not found!");
                return;
            }

            ConfigureNetworkManager();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void ConfigureNetworkManager()
        {
            // Enable connection approval since we use the callback
            if (_networkManager.NetworkConfig != null)
            {
                _networkManager.NetworkConfig.ConnectionApproval = true;
                
                // Ensure transport is assigned directly to NetworkConfig
                if (_transport != null && _networkManager.NetworkConfig.NetworkTransport == null)
                {
                    _networkManager.NetworkConfig.NetworkTransport = _transport;
                }
            }
            
            _networkManager.ConnectionApprovalCallback = OnConnectionApproval;
            SubscribeToEvents();
        }


        private void SubscribeToEvents()
        {
            if (_networkManager == null) return;

            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            _networkManager.OnTransportFailure += HandleTransportFailure;
        }

        private void UnsubscribeFromEvents()
        {
            if (_networkManager == null) return;

            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            _networkManager.OnTransportFailure -= HandleTransportFailure;
        }

        /// <summary>
        /// Set the connection approval handler for validating incoming connections.
        /// </summary>
        public void SetApprovalHandler(IConnectionApprovalHandler handler)
        {
            _approvalHandler = handler;
        }

        public void StartAsHost(ushort port = DEFAULT_PORT)
        {
            if (_networkManager == null)
            {
                OnConnectionFailed?.Invoke("NetworkManager not initialized");
                return;
            }

            ConfigureTransport("0.0.0.0", port);
            
            if (!_networkManager.StartHost())
            {
                OnConnectionFailed?.Invoke("Failed to start as Host");
            }
            else
            {
                Debug.Log($"[NetworkSessionManager] Started as Host on port {port}");
            }
        }

        public void StartAsClient(string ipAddress, ushort port = DEFAULT_PORT)
        {
            if (_networkManager == null)
            {
                OnConnectionFailed?.Invoke("NetworkManager not initialized");
                return;
            }

            if (string.IsNullOrEmpty(ipAddress))
            {
                OnConnectionFailed?.Invoke("Invalid IP address");
                return;
            }

            ConfigureTransport(ipAddress, port);

            if (!_networkManager.StartClient())
            {
                OnConnectionFailed?.Invoke("Failed to start as Client");
            }
            else
            {
                Debug.Log($"[NetworkSessionManager] Connecting to {ipAddress}:{port}");
            }
        }

        public void StartAsDedicatedServer(ushort port = DEFAULT_PORT)
        {
            if (_networkManager == null)
            {
                OnConnectionFailed?.Invoke("NetworkManager not initialized");
                return;
            }

            ConfigureTransport("0.0.0.0", port);

            if (!_networkManager.StartServer())
            {
                OnConnectionFailed?.Invoke("Failed to start as Dedicated Server");
            }
            else
            {
                Debug.Log($"[NetworkSessionManager] Started as Dedicated Server on port {port}");
            }
        }

        public void Disconnect()
        {
            if (_networkManager == null) return;

            _networkManager.Shutdown();
            Debug.Log("[NetworkSessionManager] Disconnected");
        }

        private void ConfigureTransport(string address, ushort port)
        {
            if (_transport == null) return;

            _transport.ConnectionData.Address = address;
            _transport.ConnectionData.Port = port;
        }


        private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, 
                                          NetworkManager.ConnectionApprovalResponse response)
        {
            // Check player count limit
            if (ConnectedPlayerCount >= MAX_PLAYERS)
            {
                response.Approved = false;
                response.Reason = "Server is full";
                Debug.Log($"[NetworkSessionManager] Connection rejected: Server full ({ConnectedPlayerCount}/{MAX_PLAYERS})");
                return;
            }

            // If no approval handler, approve by default
            if (_approvalHandler == null)
            {
                response.Approved = true;
                response.CreatePlayerObject = true;
                return;
            }

            // Validate using the approval handler
            var result = _approvalHandler.ValidateConnectionRequest(request.Payload);
            
            response.Approved = result.Approved;
            response.CreatePlayerObject = result.Approved;
            
            if (!result.Approved)
            {
                response.Reason = result.RejectionReason ?? "Validation failed";
                Debug.Log($"[NetworkSessionManager] Connection rejected: {result.RejectionReason} (Code: {result.ErrorCode})");
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"[NetworkSessionManager] Client connected: {clientId}");
            OnPlayerConnected?.Invoke(clientId);
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            Debug.Log($"[NetworkSessionManager] Client disconnected: {clientId}");
            OnPlayerDisconnected?.Invoke(clientId);
        }

        private void HandleTransportFailure()
        {
            Debug.LogError("[NetworkSessionManager] Transport failure");
            OnConnectionFailed?.Invoke("Network transport failure");
        }

        /// <summary>
        /// Get session state information for debugging.
        /// </summary>
        public SessionState GetSessionState()
        {
            return new SessionState
            {
                IsHost = IsHost,
                IsClient = IsClient,
                IsServer = IsServer,
                ConnectedPlayerCount = ConnectedPlayerCount,
                LocalClientId = LocalClientId
            };
        }
    }

    /// <summary>
    /// Snapshot of the current session state.
    /// </summary>
    public struct SessionState
    {
        public bool IsHost;
        public bool IsClient;
        public bool IsServer;
        public int ConnectedPlayerCount;
        public ulong LocalClientId;

        /// <summary>
        /// Validates that the state flags are consistent.
        /// Host: IsHost=true, IsClient=true, IsServer=true
        /// Client: IsHost=false, IsClient=true, IsServer=false
        /// DedicatedServer: IsHost=false, IsClient=false, IsServer=true
        /// </summary>
        public bool IsConsistent()
        {
            // Host mode
            if (IsHost)
                return IsClient && IsServer;
            
            // Client mode (not host)
            if (IsClient)
                return !IsServer;
            
            // Dedicated server mode
            if (IsServer)
                return !IsClient;
            
            // Not connected (all false is valid)
            return true;
        }
    }
}
