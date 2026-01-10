using UnityEngine;
using UnityEditor;
using System.Linq;

public class CameraDiagnostics : MonoBehaviour
{
    [MenuItem("EtherDomes/Run Camera Diagnostics")]
    public static void RunDiagnostics()
    {
        Debug.Log("========== CAMERA DIAGNOSTICS START ==========");

        // 1. Count Cameras
        var cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);
        Debug.Log($"Total Cameras in Scene: {cameras.Length}");
        foreach (var cam in cameras)
        {
            Debug.Log($"[CAMERA] Name: '{cam.name}' | Active: {cam.gameObject.activeInHierarchy} | Tag: {cam.tag}");
            Debug.Log($"   -> Parent: {(cam.transform.parent != null ? cam.transform.parent.name : "ROOT (No Parent)")}");
            
            // 2. List Components
            var components = cam.GetComponents<Component>();
            Debug.Log($"   -> Components ({components.Length}): " + string.Join(", ", components.Select(c => c.GetType().Name)));
        }

        // 3. Check for Ghost Objects
        var brains = FindObjectsByType<Unity.Cinemachine.CinemachineBrain>(FindObjectsSortMode.None);
        Debug.Log($"CinemachineBrains found: {brains.Length}");
        
        // 4. Check for AudioListeners (should be 1)
        var listeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        Debug.Log($"AudioListeners found: {listeners.Length}");

        Debug.Log("========== CAMERA DIAGNOSTICS END ==========");
    }
}
