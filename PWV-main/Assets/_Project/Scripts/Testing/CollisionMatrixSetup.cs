using UnityEngine;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Configura automáticamente la matriz de colisiones para el juego.
    /// Se ejecuta al inicio para asegurar que las colisiones estén correctamente configuradas.
    /// </summary>
    public class CollisionMatrixSetup : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetupCollisionMatrix()
        {
            // Obtener los índices de las capas
            int playerLayer = LayerMask.NameToLayer("Player");
            int enemyLayer = LayerMask.NameToLayer("Enemy");
            int defaultLayer = 0; // Default layer
            
            if (playerLayer == -1)
            {
                Debug.LogWarning("[CollisionMatrixSetup] Player layer not found!");
                return;
            }
            
            if (enemyLayer == -1)
            {
                Debug.LogWarning("[CollisionMatrixSetup] Enemy layer not found!");
                return;
            }
            
            // Configurar matriz de colisiones
            // Enemy NO colisiona con Player
            Physics.IgnoreLayerCollision(enemyLayer, playerLayer, true);
            
            // Enemy NO colisiona con Enemy
            Physics.IgnoreLayerCollision(enemyLayer, enemyLayer, true);
            
            // Enemy SÍ colisiona con Default (paredes/estructuras) - esto es el comportamiento por defecto
            Physics.IgnoreLayerCollision(enemyLayer, defaultLayer, false);
            
            Debug.Log($"[CollisionMatrixSetup] Collision matrix configured:");
            Debug.Log($"  - Enemy (layer {enemyLayer}) ignores Player (layer {playerLayer})");
            Debug.Log($"  - Enemy (layer {enemyLayer}) ignores Enemy (layer {enemyLayer})");
            Debug.Log($"  - Enemy (layer {enemyLayer}) collides with Default (layer {defaultLayer})");
        }
    }
}