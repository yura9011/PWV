using UnityEngine;
using Unity.Netcode;
using EtherDomes.Network;

namespace EtherDomes.UI.Debug
{
    /// <summary>
    /// Simple UI for testing NGO network connections.
    /// Adapted for Unity Netcode for GameObjects (NGO).
    /// </summary>
    public class NetworkTestUI : MonoBehaviour
    {
        private NetworkSessionManager _sessionManager;
        private bool _isConnected;
        private string _statusText = "Disconnected";
        private string _ipAddress = "127.0.0.1";

        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _boxStyle;

        private void Start()
        {
            _sessionManager = FindFirstObjectByType<NetworkSessionManager>();
        }

        private void Update()
        {
            if (NetworkManager.Singleton == null) return;

            _isConnected = NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer;
            if (_isConnected)
            {
                if (NetworkManager.Singleton.IsHost) _statusText = "Host";
                else if (NetworkManager.Singleton.IsServer) _statusText = "Server";
                else if (NetworkManager.Singleton.IsClient) _statusText = "Client";
            }
            else
            {
                _statusText = "Disconnected";
            }
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

            GUILayout.BeginArea(new Rect(10, 10, 280, 300));
            GUILayout.BeginVertical(_boxStyle);

            GUILayout.Label("The Ether Domes - Network Test (NGO)", _labelStyle);
            GUILayout.Space(10);

            GUI.color = _isConnected ? Color.green : Color.white;
            GUILayout.Label($"Status: {_statusText}", _labelStyle);
            GUI.color = Color.white;
            GUILayout.Space(10);

            if (!_isConnected)
            {
                GUILayout.Label("IP Address:");
                _ipAddress = GUILayout.TextField(_ipAddress);
                GUILayout.Space(10);

                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
                if (GUILayout.Button("Start as Host", _buttonStyle))
                {
                    if (_sessionManager != null) _sessionManager.StartAsHost();
                }

                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
                if (GUILayout.Button("Join as Client", _buttonStyle))
                {
                    if (_sessionManager != null)
                    {
                        _sessionManager.StartAsClientWithPayload(_ipAddress);
                    }
                }

                GUI.backgroundColor = new Color(0.8f, 0.6f, 0.2f);
                if (GUILayout.Button("Start Server Only", _buttonStyle))
                {
                    // NGO dedicated server
                    NetworkManager.Singleton.StartServer();
                }
                GUI.backgroundColor = Color.white;
            }
            else
            {
                GUILayout.Label($"Players: {NetworkManager.Singleton.ConnectedClients.Count}");
                GUILayout.Space(10);

                GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);
                if (GUILayout.Button("Disconnect", _buttonStyle))
                {
                    // NetworkSessionManager doesn't expose Disconnect directly usually, but NetworkManager does
                    NetworkManager.Singleton.Shutdown();
                }
                GUI.backgroundColor = Color.white;
            }

            GUILayout.Space(10);
            GUILayout.Label("Controls: WASD to move", _labelStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }
}
