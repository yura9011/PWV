using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

namespace EtherDomes.Network
{
    /// <summary>
    /// Manages integration with Unity Relay service.
    /// Handles allocation and join code generation.
    /// </summary>
    public class RelayManager : MonoBehaviour
    {
        public static RelayManager Instance { get; private set; }
        
        public string LastError { get; private set; }
        public string JoinCode { get; private set; } // Current relay join code
        
        public void SetLastError(string error) => LastError = error;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }



        /// <summary>
        /// Ensures Unity Services are initialized and user is signed in.
        /// </summary>
        public async Task<bool> EnsureAuthenticatedAsync()
        {
            LastError = string.Empty;
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                try
                {
                    await UnityServices.InitializeAsync();
                }
                catch (System.Exception e)
                {
                    LastError = $"Init Failed: {e.Message}";
                    Debug.LogError($"[RelayManager] Unity Services Initialization Failed: {e.Message}");
                    return false;
                }
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                try
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                    Debug.Log($"[RelayManager] Signed in as {AuthenticationService.Instance.PlayerId}");
                }
                catch (System.Exception e)
                {
                    LastError = $"Auth Failed: {e.Message}";
                    Debug.LogError($"[RelayManager] Authentication Failed: {e.Message}");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates a Relay allocation and returns the Join Code.
        /// Configures the NetworkManager's UnityTransport.
        /// </summary>
        public async Task<string> CreateRelay(int maxConnections)
        {
            if (!await EnsureAuthenticatedAsync()) return null;

            try
            {
                // Allocation
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
                
                // Get Join Code
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
                JoinCode = joinCode; // Store for UI access
                Debug.Log($"[RelayManager] Created Relay. Join Code: {joinCode}");

                // Configure Transport
                ConfigureTransport(allocation, "dtls"); // "dtls" is secure UDP

                return joinCode;
            }
            catch (System.Exception e)
            {
                LastError = $"Alloc Error: {e.Message}";
                Debug.LogError($"[RelayManager] Create Relay Failed: {e.ToString()}"); // Log full stack trace
                return null;
            }
        }

        /// <summary>
        /// Joins a Relay allocation using a Join Code.
        /// Configures the NetworkManager's UnityTransport.
        /// </summary>
        public async Task<bool> JoinRelay(string joinCode)
        {
            if (string.IsNullOrWhiteSpace(joinCode))
            {
                LastError = "Código relay vacío";
                Debug.LogError("[RelayManager] Join code is empty!");
                return false;
            }
            
            if (!await EnsureAuthenticatedAsync()) 
            {
                Debug.LogError($"[RelayManager] Authentication failed. LastError: {LastError}");
                return false;
            }

            try
            {
                Debug.Log($"[RelayManager] Attempting to join relay with code: {joinCode}");
                
                // Join Allocation
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                Debug.Log($"[RelayManager] Joined Relay successfully with code: {joinCode}");

                // Configure Transport
                ConfigureTransport(joinAllocation, "dtls");

                return true;
            }
            catch (Unity.Services.Relay.RelayServiceException relayEx)
            {
                LastError = $"Relay Error: {relayEx.Reason}";
                Debug.LogError($"[RelayManager] Relay Service Exception: {relayEx.Reason} - {relayEx.Message}");
                return false;
            }
            catch (System.Exception e)
            {
                LastError = $"Join Error: {e.Message}";
                Debug.LogError($"[RelayManager] Join Relay Failed: {e.ToString()}");
                return false;
            }
        }

        private void ConfigureTransport(Allocation allocation, string connectionType)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new Unity.Networking.Transport.Relay.RelayServerData(allocation, connectionType));
        }

        private void ConfigureTransport(JoinAllocation joinAllocation, string connectionType)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(new Unity.Networking.Transport.Relay.RelayServerData(joinAllocation, connectionType));
        }
    }
}
