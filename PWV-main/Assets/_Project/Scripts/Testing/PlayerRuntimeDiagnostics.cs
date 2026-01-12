using UnityEngine;
using Unity.Netcode;
using EtherDomes.Movement;

namespace EtherDomes.Testing
{
    public class PlayerRuntimeDiagnostics : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("[Diagnostics] System STARTED. DontDestroyOnLoad applied.");
            DontDestroyOnLoad(gameObject);
            InvokeRepeating(nameof(ReportStatus), 1f, 2f); // Start sooner
        }

        private void ReportStatus()
        {
            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("[Diagnostics] NetworkManager is NULL");
                return;
            }
            if (!NetworkManager.Singleton.IsListening)
            {
               // Still in menu or not started, silent is fine, or print once
               return; 
            }

            var localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (localPlayer == null)
            {
                Debug.LogWarning($"[Diagnostics] Network Active but NO PlayerObject. LocalId: {NetworkManager.Singleton.LocalClientId}");
                return;
            }

            var wow = localPlayer.GetComponent<WoWMovementController>();
            var cc = localPlayer.GetComponent<CharacterController>();
            
            string report = $"[Diagnostics] REPORT for {localPlayer.name}:\n" +
                            $"- Position: {localPlayer.transform.position}\n" +
                            $"- IsOwner: {localPlayer.IsOwner}\n" +
                            $"- ActiveSelf: {localPlayer.gameObject.activeSelf}\n" +
                            $"- Time.timeScale: {Time.timeScale}\n";

            if (wow != null)
            {
                report += $"- WoWMovement: ENABLED={wow.enabled}\n";
            }
            else
            {
                report += "- WoWMovement: MISSING!\n";
            }

            if (cc != null)
            {
                report += $"- CharacterController: ENABLED={cc.enabled}, Grounded={cc.isGrounded}\n";
            }
            else
            {
                report += "- CharacterController: MISSING!\n";
            }

            Debug.Log(report);
        }
    }
}
