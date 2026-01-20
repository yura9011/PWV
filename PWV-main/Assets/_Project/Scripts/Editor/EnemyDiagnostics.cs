using UnityEngine;
using UnityEditor;
using System.Linq;
using EtherDomes.Enemy;

public class EnemyDiagnostics : MonoBehaviour
{
    [MenuItem("EtherDomes/Run Enemy Diagnostics")]
    public static void RunDiagnostics()
    {
        Debug.Log("========== ENEMY DIAGNOSTICS START ==========");

        var enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        Debug.Log($"Total Enemies found in Scene: {enemies.Length}");

        foreach (var enemy in enemies)
        {
            Debug.Log($"[ENEMY] '{enemy.name}'");
            var components = enemy.GetComponents<Component>();
            Debug.Log($"   -> Components: {string.Join(", ", components.Select(c => c.GetType().Name))}");
            
            var ai = enemy.GetComponent<EnemyAI>();
            if (ai == null)
            {
                 Debug.LogError($"   -> MISSING EnemyAI Component!");
            }
            else
            {
                 Debug.Log($"   -> EnemyAI Found. Enabled: {ai.enabled}");
            }
        }
        Debug.Log("========== ENEMY DIAGNOSTICS END ==========");
    }
}
