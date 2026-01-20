using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace EtherDomes.Enemy
{
    /// <summary>
    /// Configura automáticamente el NavMesh para pathfinding 3D.
    /// Debe ser colocado en un GameObject en la escena.
    /// </summary>
    [System.Serializable]
    public class NavMeshSetup : MonoBehaviour
    {
        [Header("NavMesh Settings")]
        [SerializeField] private bool _autoSetupOnStart = true;
        [SerializeField] private LayerMask _walkableLayerMask = -1; // Default: all layers
        [SerializeField] private LayerMask _obstacleLayerMask = 1; // Default layer (walls)
        
        [Header("Agent Settings")]
        [SerializeField] private float _agentRadius = 0.5f;
        [SerializeField] private float _agentHeight = 2f;
        [SerializeField] private float _maxSlope = 45f;
        [SerializeField] private float _stepHeight = 0.4f;
        
        [Header("Debug")]
        [SerializeField] private bool _showNavMeshInScene = true;
        
        private void Start()
        {
            if (_autoSetupOnStart)
            {
                SetupNavMesh();
            }
        }
        
        /// <summary>
        /// Configura el NavMesh para la escena actual
        /// </summary>
        [ContextMenu("Setup NavMesh")]
        public void SetupNavMesh()
        {
            Debug.Log("[NavMeshSetup] Setting up NavMesh for 3D pathfinding...");
            
            // Verificar que el NavMesh se construyó correctamente
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            Debug.Log($"[NavMeshSetup] NavMesh contains {triangulation.vertices.Length} vertices and {triangulation.indices.Length / 3} triangles");
            
            if (triangulation.vertices.Length == 0)
            {
                Debug.LogWarning("[NavMeshSetup] NavMesh is empty! Please bake NavMesh manually in Window > AI > Navigation");
                Debug.LogWarning("[NavMeshSetup] 1. Mark floor objects as 'Navigation Static'");
                Debug.LogWarning("[NavMeshSetup] 2. Go to Window > AI > Navigation");
                Debug.LogWarning("[NavMeshSetup] 3. Click 'Bake' button");
            }
            else
            {
                Debug.Log("[NavMeshSetup] NavMesh is ready!");
            }
        }
        
        /// <summary>
        /// Configura un GameObject para ser navegable
        /// </summary>
        public static void MakeNavigationStatic(GameObject obj)
        {
#if UNITY_EDITOR
            // Marcar como Navigation Static
            var flags = GameObjectUtility.GetStaticEditorFlags(obj);
            flags |= StaticEditorFlags.NavigationStatic;
            GameObjectUtility.SetStaticEditorFlags(obj, flags);
            
            Debug.Log($"[NavMeshSetup] Marked {obj.name} as Navigation Static");
#else
            Debug.LogWarning($"[NavMeshSetup] Cannot mark {obj.name} as Navigation Static in runtime");
#endif
        }
        
        /// <summary>
        /// Configura un enemigo para usar pathfinding
        /// </summary>
        public static void SetupEnemyForPathfinding(GameObject enemy)
        {
            // Verificar NavMeshAgent
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
            if (agent == null)
            {
                agent = enemy.AddComponent<NavMeshAgent>();
                Debug.Log($"[NavMeshSetup] Added NavMeshAgent to {enemy.name}");
            }
            
            // Verificar SmartPathfinding3D usando reflection para evitar dependencia de namespace
            var pathfindingType = System.Type.GetType("EtherDomes.Testing.SmartPathfinding3D");
            if (pathfindingType != null)
            {
                var pathfinding = enemy.GetComponent(pathfindingType);
                if (pathfinding == null)
                {
                    pathfinding = enemy.AddComponent(pathfindingType);
                    Debug.Log($"[NavMeshSetup] Added SmartPathfinding3D to {enemy.name}");
                }
            }
            else
            {
                Debug.LogWarning("[NavMeshSetup] SmartPathfinding3D type not found");
            }
            
            // Configurar agente básico
            agent.speed = 4f;
            agent.stoppingDistance = 2f;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
            
            Debug.Log($"[NavMeshSetup] Configured {enemy.name} for pathfinding");
        }
        
        private void OnDrawGizmos()
        {
            if (!_showNavMeshInScene) return;
            
            // Dibujar NavMesh en la escena
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            
            if (triangulation.vertices.Length > 0)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
                
                for (int i = 0; i < triangulation.indices.Length; i += 3)
                {
                    Vector3 v1 = triangulation.vertices[triangulation.indices[i]];
                    Vector3 v2 = triangulation.vertices[triangulation.indices[i + 1]];
                    Vector3 v3 = triangulation.vertices[triangulation.indices[i + 2]];
                    
                    // Dibujar triángulo
                    Gizmos.DrawLine(v1, v2);
                    Gizmos.DrawLine(v2, v3);
                    Gizmos.DrawLine(v3, v1);
                }
            }
        }
        
        [ContextMenu("Setup All Enemies in Scene")]
        public void SetupAllEnemiesInScene()
        {
            // Buscar TestEnemy por componente en lugar de tipo específico
            MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            int enemyCount = 0;
            
            foreach (var component in allComponents)
            {
                if (component.GetType().Name == "TestEnemy")
                {
                    SetupEnemyForPathfinding(component.gameObject);
                    enemyCount++;
                }
            }
            
            Debug.Log($"[NavMeshSetup] Configured {enemyCount} enemies for pathfinding");
        }
        
        [ContextMenu("Mark All Walls as Navigation Static")]
        public void MarkWallsAsNavigationStatic()
        {
            // Buscar todos los objetos en la capa Default (paredes)
            GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            int markedCount = 0;
            
            foreach (var obj in allObjects)
            {
                if (obj.layer == 0) // Default layer
                {
                    var collider = obj.GetComponent<Collider>();
                    if (collider != null && !collider.isTrigger)
                    {
                        MakeNavigationStatic(obj);
                        markedCount++;
                    }
                }
            }
            
            Debug.Log($"[NavMeshSetup] Marked {markedCount} wall objects as Navigation Static");
        }
    }
}