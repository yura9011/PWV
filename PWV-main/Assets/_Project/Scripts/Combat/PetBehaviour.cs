using EtherDomes.Data;
using UnityEngine;
using UnityEngine.AI;

namespace EtherDomes.Combat
{
    /// <summary>
    /// MonoBehaviour component attached to pet GameObjects.
    /// Handles visual representation and NavMeshAgent configuration.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PetBehaviour : MonoBehaviour
    {
        #region Serialized Fields
        
        [Header("References")]
        [SerializeField] private NavMeshAgent _navMeshAgent;
        
        [Header("Configuration")]
        [SerializeField] private float _defaultSpeed = 5f;
        [SerializeField] private float _defaultStoppingDistance = 2.5f;
        [SerializeField] private float _defaultAcceleration = 8f;
        [SerializeField] private float _defaultAngularSpeed = 120f;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// The NavMeshAgent component for pathfinding.
        /// </summary>
        public NavMeshAgent NavMeshAgent => _navMeshAgent;
        
        /// <summary>
        /// Owner ID of this pet (set by PetSystem).
        /// </summary>
        public ulong OwnerId { get; set; }
        
        /// <summary>
        /// Pet entity ID (set by PetSystem).
        /// </summary>
        public ulong PetEntityId { get; set; }
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_navMeshAgent == null)
            {
                _navMeshAgent = GetComponent<NavMeshAgent>();
            }
            
            ConfigureNavMeshAgent();
        }

        private void Reset()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            ConfigureNavMeshAgent();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Initialize the pet behaviour with pet data.
        /// </summary>
        /// <param name="petData">Pet data containing speed and other configuration</param>
        public void Initialize(PetData petData)
        {
            if (petData == null) return;
            
            if (_navMeshAgent != null)
            {
                _navMeshAgent.speed = petData.MoveSpeed;
                _navMeshAgent.stoppingDistance = petData.AttackRange - 0.5f;
            }
        }

        /// <summary>
        /// Move to a target position.
        /// </summary>
        /// <param name="position">Target position to move to</param>
        public void MoveTo(Vector3 position)
        {
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.SetDestination(position);
            }
        }

        /// <summary>
        /// Stop all movement.
        /// </summary>
        public void StopMovement()
        {
            if (_navMeshAgent != null && _navMeshAgent.isOnNavMesh)
            {
                _navMeshAgent.ResetPath();
            }
        }

        /// <summary>
        /// Teleport to a position.
        /// </summary>
        /// <param name="position">Position to teleport to</param>
        public void Teleport(Vector3 position)
        {
            if (_navMeshAgent != null)
            {
                _navMeshAgent.Warp(position);
            }
            else
            {
                transform.position = position;
            }
        }

        /// <summary>
        /// Look at a target position.
        /// </summary>
        /// <param name="position">Position to look at</param>
        public void LookAt(Vector3 position)
        {
            Vector3 direction = position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
        
        #endregion

        #region Private Methods
        
        private void ConfigureNavMeshAgent()
        {
            if (_navMeshAgent == null) return;
            
            _navMeshAgent.speed = _defaultSpeed;
            _navMeshAgent.stoppingDistance = _defaultStoppingDistance;
            _navMeshAgent.acceleration = _defaultAcceleration;
            _navMeshAgent.angularSpeed = _defaultAngularSpeed;
            _navMeshAgent.autoBraking = true;
            _navMeshAgent.autoRepath = true;
        }
        
        #endregion

        #region Editor
        
#if UNITY_EDITOR
        /// <summary>
        /// Create a basic pet prefab structure in the editor.
        /// </summary>
        [UnityEditor.MenuItem("GameObject/EtherDomes/Create Pet Prefab", false, 10)]
        public static void CreatePetPrefab()
        {
            // Create root object
            GameObject petRoot = new GameObject("BasePet");
            
            // Add capsule mesh as placeholder
            GameObject meshObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            meshObject.name = "Mesh";
            meshObject.transform.SetParent(petRoot.transform);
            meshObject.transform.localPosition = Vector3.zero;
            meshObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Remove collider from mesh (we'll add one to root)
            var meshCollider = meshObject.GetComponent<Collider>();
            if (meshCollider != null)
            {
                DestroyImmediate(meshCollider);
            }
            
            // Add components to root
            var navAgent = petRoot.AddComponent<NavMeshAgent>();
            navAgent.radius = 0.3f;
            navAgent.height = 1f;
            navAgent.speed = 5f;
            navAgent.acceleration = 8f;
            navAgent.angularSpeed = 120f;
            navAgent.stoppingDistance = 2.5f;
            navAgent.autoBraking = true;
            
            // Add capsule collider
            var capsuleCollider = petRoot.AddComponent<CapsuleCollider>();
            capsuleCollider.radius = 0.3f;
            capsuleCollider.height = 1f;
            capsuleCollider.center = new Vector3(0, 0.5f, 0);
            
            // Add PetBehaviour
            var petBehaviour = petRoot.AddComponent<PetBehaviour>();
            
            // Select the created object
            UnityEditor.Selection.activeGameObject = petRoot;
            
            Debug.Log("[PetBehaviour] Created pet prefab structure. Save as prefab in Assets/_Project/Prefabs/Pets/");
        }
#endif
        
        #endregion
    }
}
