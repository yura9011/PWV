using UnityEngine;
using UnityEditor;
using EtherDomes.Testing;

namespace EtherDomes.Editor
{
    public static class DiagnosticsSetup
    {
        [MenuItem("EtherDomes/Setup Diagnostics")]
        public static void Setup()
        {
            // Check if already exists
            var existing = Object.FindFirstObjectByType<PlayerRuntimeDiagnostics>();
            if (existing != null)
            {
                Debug.Log($"[Diagnostics] Already exists on '{existing.name}'. Selected it.");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            GameObject go = new GameObject("Diagnostics_Auto");
            go.AddComponent<PlayerRuntimeDiagnostics>();
            Undo.RegisterCreatedObjectUndo(go, "Create Diagnostics");
            Selection.activeGameObject = go;
            
            Debug.Log("[Diagnostics] Created 'Diagnostics_Auto' object. Logs will appear in Console every 2 seconds during Play.");
            EditorUtility.DisplayDialog("Diagnostics Ready", 
                "Diagnostics tool created!\n\n1. Press Play.\n2. Watch the Console for '[Diagnostics] REPORT'.\n3. Copy that log to the chat.", "OK");
        }
    }
}
