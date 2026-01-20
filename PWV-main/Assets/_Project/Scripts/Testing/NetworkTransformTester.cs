using UnityEngine;
using Unity.Netcode;
using EtherDomes.Network;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Tester to verify NetworkTransform synchronization works correctly
    /// </summary>
    public class NetworkTransformTester : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private bool _autoTestOnStart = false;
        [SerializeField] private GameObject _networkPlayerPrefab;
        
        private NetworkSessionManager _sessionManager;
        private GameObject _spawnedPlayer1;
        private GameObject _spawnedPlayer2;
        
        private void Start()
        {
            _sessionManager = FindObjectOfType<NetworkSessionManager>();
            
            if (_sessionManager == null)
            {
                Debug.LogError("[NetworkTransformTester] NetworkSessionManager not found!");
                return;
            }
            
            // Load NetworkPlayer prefab if not assigned
            if (_networkPlayerPrefab == null)
            {
                _networkPlayerPrefab = Resources.Load<GameObject>("NetworkPlayer");
                if (_networkPlayerPrefab == null)
                {
                    Debug.LogError("[NetworkTransformTester] NetworkPlayer prefab not found in Resources!");
                }
            }
            
            if (_autoTestOnStart)
            {
                StartNetworkTransformTest();
            }
        }
        
        [ContextMenu("Test NetworkTransform Sync")]
        public void StartNetworkTransformTest()
        {
            Debug.Log("[NetworkTransformTester] Starting NetworkTransform synchronization test...");
            
            if (!_sessionManager.IsHost && !_sessionManager.IsClient)
            {
                Debug.LogWarning("[NetworkTransformTester] Not connected to network. Starting as Host...");
                _sessionManager.StartAsHost(7777);
            }
            
            // Wait a frame for network to initialize
            StartCoroutine(SpawnTestPlayersCoroutine());
        }
        
        private System.Collections.IEnumerator SpawnTestPlayersCoroutine()
        {
            yield return new WaitForSeconds(1f);
            
            if (_sessionManager.IsHost)
            {
                SpawnTestPlayers();
            }
        }
        
        private void SpawnTestPlayers()
        {
            if (_networkPlayerPrefab == null)
            {
                Debug.LogError("[NetworkTransformTester] NetworkPlayer prefab is null!");
                return;
            }
            
            // Spawn two test players at different positions
            Vector3 pos1 = new Vector3(-2f, 0f, 0f);
            Vector3 pos2 = new Vector3(2f, 0f, 0f);
            
            _spawnedPlayer1 = Instantiate(_networkPlayerPrefab, pos1, Quaternion.identity);
            _spawnedPlayer2 = Instantiate(_networkPlayerPrefab, pos2, Quaternion.identity);
            
            _spawnedPlayer1.name = "TestPlayer1";
            _spawnedPlayer2.name = "TestPlayer2";
            
            // Get NetworkObjects and spawn them
            var netObj1 = _spawnedPlayer1.GetComponent<NetworkObject>();
            var netObj2 = _spawnedPlayer2.GetComponent<NetworkObject>();
            
            if (netObj1 != null) netObj1.Spawn();
            if (netObj2 != null) netObj2.Spawn();
            
            Debug.Log("[NetworkTransformTester] ✅ Test players spawned with NetworkTransform");
            
            // Start movement test
            StartCoroutine(MovePlayersTest());
        }
        
        private System.Collections.IEnumerator MovePlayersTest()
        {
            float testDuration = 10f;
            float elapsed = 0f;
            
            Debug.Log("[NetworkTransformTester] Starting movement test for NetworkTransform sync...");
            
            while (elapsed < testDuration && _spawnedPlayer1 != null && _spawnedPlayer2 != null)
            {
                // Move players in circles
                float angle1 = elapsed * 2f;
                float angle2 = elapsed * -1.5f;
                
                Vector3 newPos1 = new Vector3(Mathf.Cos(angle1) * 2f, 0f, Mathf.Sin(angle1) * 2f);
                Vector3 newPos2 = new Vector3(Mathf.Cos(angle2) * 3f, 0f, Mathf.Sin(angle2) * 3f);
                
                _spawnedPlayer1.transform.position = newPos1;
                _spawnedPlayer2.transform.position = newPos2;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log("[NetworkTransformTester] ✅ Movement test completed - NetworkTransform should have synced positions");
        }
        
        [ContextMenu("Clean Up Test")]
        public void CleanUpTest()
        {
            if (_spawnedPlayer1 != null)
            {
                var netObj1 = _spawnedPlayer1.GetComponent<NetworkObject>();
                if (netObj1 != null && netObj1.IsSpawned) netObj1.Despawn();
                Destroy(_spawnedPlayer1);
            }
            
            if (_spawnedPlayer2 != null)
            {
                var netObj2 = _spawnedPlayer2.GetComponent<NetworkObject>();
                if (netObj2 != null && netObj2.IsSpawned) netObj2.Despawn();
                Destroy(_spawnedPlayer2);
            }
            
            Debug.Log("[NetworkTransformTester] Test cleanup completed");
        }
        
        private void OnGUI()
        {
            if (_sessionManager == null) return;
            
            GUILayout.BeginArea(new Rect(320, 10, 300, 200));
            GUILayout.Label("=== NetworkTransform Tester ===");
            GUILayout.Label($"Network Status: {GetNetworkStatus()}");
            
            if (_spawnedPlayer1 != null && _spawnedPlayer2 != null)
            {
                GUILayout.Label($"Player1 Pos: {_spawnedPlayer1.transform.position}");
                GUILayout.Label($"Player2 Pos: {_spawnedPlayer2.transform.position}");
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Test NetworkTransform"))
            {
                StartNetworkTransformTest();
            }
            
            if (GUILayout.Button("Clean Up"))
            {
                CleanUpTest();
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