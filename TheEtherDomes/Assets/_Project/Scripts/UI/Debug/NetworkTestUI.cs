using UnityEngine;
using Unity.Netcode;
using EtherDomes.Network;

namespace EtherDomes.UI.Debug
{
    /// <summary>
    /// Simple UI for testing network connections.
    /// Shows Host/Client/Server buttons and connection status.
    /// Uses OnGUI for simplicity (no Canvas required).
    /// </summary>
    public class NetworkTestUI : MonoBehaviour
    {
        private NetworkSessionManager _sessionManager;
        private bool _isConnected;
        private string _statusText = "Disconnected";
        private string _ipAddress = "127.0.0.1";
        private string _port = "7777";

        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _boxStyle;

        private void Start()
        {
            _sessionManager = FindFirstObjectByType<NetworkSessionManager>();
            
            if (_sessionManager != null)
            {
                _sessionManager.OnPlayerConnected += OnPlayerConnected;
                _sessionManager.OnPlayerDisconnected += OnPlayerDisconnected;
                _sessionManager.OnConnectionFailed += OnConnectionFailed;
            }
        }

        private void OnDestroy()
        {
            if (_sessionManager != null)
            {
                _sessionManager.OnPlayerConnected -= OnPlayerConnected;
                _sessionManager.OnPlayerDisconnected -= OnPlayerDisconnected;
                _sessionManager.OnConnectionFailed -= OnConnectionFailed;
            }
        }

        private void OnPlayerConnected(ulong clientId)
        {
            _statusText = $"Connected! Client ID: {clientId}";
            _isConnected = true;
        }

        private void OnPlayerDisconnected(ulong clientId)
        {
            _statusText = $"Player {clientId} disconnected";
        }

        private void OnConnectionFailed(string reason)
        {
            _statusText = $"Connection failed: {reason}";
            _isConnected = false;
        }

        private void InitStyles()
        {
            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 40
                };

                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };

                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 12
                };
            }
        }

        private void OnGUI()
        {
            InitStyles();

            // Main panel
            GUILayout.BeginArea(new Rect(10, 10, 280, 350));
            GUILayout.BeginVertical(_boxStyle);

            // Title
            GUILayout.Label("The Ether Domes - Network Test", _labelStyle);
            GUILayout.Space(10);

            // Status
            GUI.color = _isConnected ? Color.green : Color.white;
            GUILayout.Label($"Status: {_statusText}", _labelStyle);
            GUI.color = Color.white;
            GUILayout.Space(10);

            if (!_isConnected)
            {
                // Connection options
                GUILayout.Label("IP Address:");
                _ipAddress = GUILayout.TextField(_ipAddress);
                
                GUILayout.Label("Port:");
                _port = GUILayout.TextField(_port);
                GUILayout.Space(10);

                // Buttons
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                if (GUILayout.Button("Start as Host", _buttonStyle))
                {
                    StartHost();
                }

                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
                if (GUILayout.Button("Join as Client", _buttonStyle))
                {
                    StartClient();
                }

                GUI.backgroundColor = new Color(0.8f, 0.6f, 0.2f);
                if (GUILayout.Button("Start Dedicated Server", _buttonStyle))
                {
                    StartServer();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                // Connected info
                if (_sessionManager != null)
                {
                    var state = _sessionManager.GetSessionState();
                    GUILayout.Label($"Mode: {GetModeString(state)}");
                    GUILayout.Label($"Players: {state.ConnectedPlayerCount}/{_sessionManager.MaxPlayers}");
                    GUILayout.Label($"Local ID: {state.LocalClientId}");
                }

                GUILayout.Space(10);

                GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
                if (GUILayout.Button("Disconnect", _buttonStyle))
                {
                    Disconnect();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(10);
            GUILayout.Label("Controls: WASD to move", _labelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();

            // Player list (if connected)
            if (_isConnected && NetworkManager.Singleton != null)
            {
                DrawPlayerList();
            }
        }

        private void DrawPlayerList()
        {
            GUILayout.BeginArea(new Rect(10, 370, 280, 200));
            GUILayout.BeginVertical(_boxStyle);
            
            GUILayout.Label("Connected Players:", _labelStyle);
            
            // ConnectedClientsList is only accessible on server/host
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    string playerInfo = $"Player {client.ClientId}";
                    if (client.ClientId == NetworkManager.Singleton.LocalClientId)
                    {
                        playerInfo += " (You)";
                    }
                    GUILayout.Label(playerInfo);
                }
            }
            else if (NetworkManager.Singleton != null)
            {
                // For clients, just show local player info
                GUILayout.Label($"Player {NetworkManager.Singleton.LocalClientId} (You)");
                GUILayout.Label("(Full list only visible on Host/Server)");
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private string GetModeString(SessionState state)
        {
            if (state.IsHost) return "Host";
            if (state.IsClient) return "Client";
            if (state.IsServer) return "Dedicated Server";
            return "Unknown";
        }

        private void StartHost()
        {
            if (_sessionManager == null)
            {
                _sessionManager = FindFirstObjectByType<NetworkSessionManager>();
            }

            if (_sessionManager != null)
            {
                ushort port = ushort.TryParse(_port, out var p) ? p : (ushort)7777;
                _sessionManager.StartAsHost(port);
                _statusText = "Starting as Host...";
            }
            else
            {
                _statusText = "Error: NetworkSessionManager not found!";
            }
        }

        private void StartClient()
        {
            if (_sessionManager == null)
            {
                _sessionManager = FindFirstObjectByType<NetworkSessionManager>();
            }

            if (_sessionManager != null)
            {
                ushort port = ushort.TryParse(_port, out var p) ? p : (ushort)7777;
                _sessionManager.StartAsClient(_ipAddress, port);
                _statusText = $"Connecting to {_ipAddress}:{port}...";
            }
            else
            {
                _statusText = "Error: NetworkSessionManager not found!";
            }
        }

        private void StartServer()
        {
            if (_sessionManager == null)
            {
                _sessionManager = FindFirstObjectByType<NetworkSessionManager>();
            }

            if (_sessionManager != null)
            {
                ushort port = ushort.TryParse(_port, out var p) ? p : (ushort)7777;
                _sessionManager.StartAsDedicatedServer(port);
                _statusText = "Starting Dedicated Server...";
            }
            else
            {
                _statusText = "Error: NetworkSessionManager not found!";
            }
        }

        private void Disconnect()
        {
            if (_sessionManager != null)
            {
                _sessionManager.Disconnect();
                _statusText = "Disconnected";
                _isConnected = false;
            }
        }
    }
}
