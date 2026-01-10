using UnityEngine;
using UnityEditor;
using EtherDomes.Enemy;

namespace EtherDomes.Editor
{
    public class EnemySceneFixer : MonoBehaviour
    {
        [MenuItem("EtherDomes/Fix Enemy Components")]
        public static void FixEnemies()
        {
            var enemies = FindObjectsByType<EtherDomes.Enemy.Enemy>(FindObjectsSortMode.None);
            int fixedCount = 0;

            foreach (var enemy in enemies)
            {
                if (enemy.GetComponent<EnemyAI>() == null)
                {
                    Undo.AddComponent<EnemyAI>(enemy.gameObject);
                    Debug.Log($"[EnemyFixer] Added EnemyAI to '{enemy.name}'");
                    fixedCount++;
                }
            }

            if (fixedCount > 0)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                    UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log($"[EnemyFixer] Fixed {fixedCount} enemies. Scene saved.");
                EditorUtility.DisplayDialog("Fix Complete", $"Added EnemyAI to {fixedCount} enemies.\n\nReady to test Combat!", "OK");
            }
            else
            {
                Debug.Log("[EnemyFixer] All enemies already have EnemyAI.");
                EditorUtility.DisplayDialog("Fix Complete", "All enemies already have the AI component.", "OK");
            }
        }
    }
}
