using UnityEngine;
using Unity.Netcode;
using EtherDomes.Network;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Simple tester to verify NGO Host/Client functionality works
    /// </summary>
    public class NetworkConnectionTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _autoTestOnStart = false;
        [SerializeField] private float _testDuration = 5f;
        
        private NetworkSessionManager _sessionManager;
        private bool _testInProgress = false;
        private float _testStartTime;
        
        private void Start()
        {
            _sessionManager = FindObjectOfType<NetworkSessionManager>();
            
            if (_sessionManager == null)
            {
                Debug.LogError("[NetworkConnectionTester] NetworkSessionManager not found!");
                return;
            }
            
            // Subscribe to events
            _sessionManager.OnPlayerConnected += OnPlayerConnected;
            _sessionManager.OnPlayerDisconnected += OnPlayerDisconnected;
            _sessionManager.OnConnectionFailed += OnConnectionFailed;
            
            if (_autoTestOnStart)
            {
                StartHostTest();
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
        
        [ContextMenu("Test Host Connection")]
        public void StartHostTest()
        {
            if (_testInProgress) return;
            
            Debug.Log("[NetworkConnectionTester] Starting Host test...");
            _testInProgress = true;
            _testStartTime = Time.time;
            
            bool success = _sessionManager.StartAsHost(7777);
            
            if (success)
            {
                Debug.Log("[NetworkConnectionTester] ✅ Host started successfully!");
            }
            else
            {
                Debug.LogError("[NetworkConnectionTester] ❌ Host failed to start!");
                _testInProgress = false;
            }
        }
        
        [ContextMenu("Test Client Connection")]
        public void StartClientTest()
        {
            if (_testInProgress) return;
            
            Debug.Log("[NetworkConnectionTester] Starting Client test (connecting to localhost)...");
            _testInProgress = true;
            _testStartTime = Time.time;
            
            _sessionManager.StartAsClient("127.0.0.1", 7777);
        }
        
        [ContextMenu("Stop Network")]
        public void StopNetwork()
        {
            Debug.Log("[NetworkConnectionTester] Stopping network...");
            _sessionManager.Disconnect();
            _testInProgress = false;
        }
        
        private void Update()
        {
            if (_testInProgress && Time.time - _testStartTime > _testDuration)
            {
                Debug.Log("[NetworkConnectionTester] Test completed after timeout");
                StopNetwork();
            }
        }
        
        private void OnPlayerConnected(ulong clientId)
        {
            Debug.Log($"[NetworkConnectionTester] ✅ Player {clientId} connected!");
            
            if (_sessionManager.IsHost)
            {
                Debug.Log($"[NetworkConnectionTester] ✅ HOST MODE: {_sessionManager.ConnectedPlayerCount} players connected");
            }
            else if (_sessionManager.IsClient)
            {
                Debug.Log($"[NetworkConnectionTester] ✅ CLIENT MODE: Connected to host");
            }
        }
        
        private void OnPlayerDisconnected(ulong clientId)
        {
            Debug.Log($"[NetworkConnectionTester] Player {clientId} disconnected");
        }
        
        private void OnConnectionFailed(string reason)
        {
            Debug.LogError($"[NetworkConnectionTester] ❌ Connection failed: {reason}");
            _testInProgress = false;
        }
        
        private void OnGUI()
        {
            if (_sessionManager == null) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== Network Connection Tester ===");
            GUILayout.Label($"Status: {GetNetworkStatus()}");
            GUILayout.Label($"Connected Players: {_sessionManager.ConnectedPlayerCount}");
            GUILayout.Label($"Local Client ID: {_sessionManager.LocalClientId}");
            
            GUILayout.Space(10);
            
            if (!_testInProgress)
            {
                if (GUILayout.Button("Test Host"))
                {
                    StartHostTest();
                }
                
                if (GUILayout.Button("Test Client"))
                {
                    StartClientTest();
                }
            }
            else
            {
                GUILayout.Label("Test in progress...");
            }
            
            if (_sessionManager.IsHost || _sessionManager.IsClient)
            {
                if (GUILayout.Button("Disconnect"))
                {
                    StopNetwork();
                }
            }
            
            GUILayout.EndArea();
        }
        
        private string GetNetworkStatus()
        {
            if (_sessionManager.IsHost) return "HOST";
            if (_sessionManager.IsClient) return "CLIENT";
            if (_sessionManager.IsServer) return "SERVER";
            return "DISCONNECTED";
        }
    }
}