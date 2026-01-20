using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace EtherDomes.Testing
{
    /// <summary>
    /// Sistema de pathfinding inteligente 3D basado en NavMesh de Unity.
    /// Adaptado de NavMeshPlus para funcionar en 3D (XZ plane en lugar de XY).
    /// </summary>
    public class SmartPathfinding3D : MonoBehaviour
    {
        [Header("Pathfinding Settings")]
        [SerializeField] private float _pathUpdateInterval = 0.2f; // Actualizar path cada 200ms
        [SerializeField] private float _stoppingDistance = 0.5f;
        [SerializeField] private float _pathEndThreshold = 1f;
        [SerializeField] private bool _debugPath = true;
        
        private NavMeshAgent _agent;
        private Transform _target;
        private Vector3 _lastTargetPosition;
        private float _lastPathUpdate;
        private bool _hasPath;
        private bool _isPathfinding;
        
        // Debug
        private Vector3[] _currentPath;
        
        public bool HasPath => _hasPath && _agent.hasPath;
        public bool IsPathfinding => _isPathfinding;
        public float RemainingDistance => _agent.remainingDistance;
        public bool HasReachedDestination => _agent.remainingDistance <= _stoppingDistance;
        
        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            if (_agent == null)
            {
                Debug.LogError($"[SmartPathfinding3D] {name} requires NavMeshAgent component!");
                enabled = false;
                return;
            }
        }
        
        private void Start()
        {
            // Configurar NavMeshAgent para 3D
            _agent.updateRotation = false; // Manejamos rotación manualmente
            _agent.updateUpAxis = false;   // No necesitamos actualizar Y axis
            _agent.stoppingDistance = _stoppingDistance;
            
            Debug.Log($"[SmartPathfinding3D] {name} initialized with agent type: {_agent.agentTypeID}");
        }
        
        /// <summary>
        /// Establece el objetivo para el pathfinding
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                _lastTargetPosition = _target.position;
                RequestPath(_target.position);
            }
            else
            {
                StopPathfinding();
            }
        }
        
        /// <summary>
        /// Solicita un path hacia una posición específica
        /// </summary>
        public void RequestPath(Vector3 destination)
        {
            if (!_agent.isOnNavMesh)
            {
                Debug.LogWarning($"[SmartPathfinding3D] {name} is not on NavMesh! Position: {transform.position}");
                return;
            }
            
            _isPathfinding = true;
            
            // Verificar si el destino es alcanzable
            NavMeshHit hit;
            if (NavMesh.SamplePosition(destination, out hit, 5f, NavMesh.AllAreas))
            {
                destination = hit.position;
            }
            else
            {
                Debug.LogWarning($"[SmartPathfinding3D] {name} destination not on NavMesh: {destination}");
                _isPathfinding = false;
                return;
            }
            
            // Calcular path
            NavMeshPath path = new NavMeshPath();
            if (_agent.CalculatePath(destination, path))
            {
                if (path.status == NavMeshPathStatus.PathComplete)
                {
                    _agent.SetPath(path);
                    _hasPath = true;
                    _currentPath = path.corners;
                    _lastPathUpdate = Time.time;
                    
                    if (_debugPath)
                    {
                        Debug.Log($"[SmartPathfinding3D] {name} found complete path with {path.corners.Length} corners");
                    }
                }
                else if (path.status == NavMeshPathStatus.PathPartial)
                {
                    // Path parcial - mejor que nada
                    _agent.SetPath(path);
                    _hasPath = true;
                    _currentPath = path.corners;
                    _lastPathUpdate = Time.time;
                    
                    Debug.LogWarning($"[SmartPathfinding3D] {name} found partial path with {path.corners.Length} corners");
                }
                else
                {
                    Debug.LogWarning($"[SmartPathfinding3D] {name} path calculation failed: {path.status}");
                    _hasPath = false;
                }
            }
            else
            {
                Debug.LogWarning($"[SmartPathfinding3D] {name} failed to calculate path to {destination}");
                _hasPath = false;
            }
            
            _isPathfinding = false;
        }
        
        /// <summary>
        /// Detiene el pathfinding
        /// </summary>
        public void StopPathfinding()
        {
            _agent.ResetPath();
            _hasPath = false;
            _isPathfinding = false;
            _target = null;
            _currentPath = null;
        }
        
        /// <summary>
        /// Pausa/reanuda el movimiento
        /// </summary>
        public void SetMovementEnabled(bool enabled)
        {
            _agent.isStopped = !enabled;
        }
        
        private void Update()
        {
            if (_target == null) return;
            
            // Verificar si el objetivo se ha movido significativamente
            float targetMovement = Vector3.Distance(_target.position, _lastTargetPosition);
            bool shouldUpdatePath = targetMovement > 2f || // Objetivo se movió más de 2m
                                  Time.time - _lastPathUpdate > _pathUpdateInterval || // Tiempo de actualización
                                  (!_hasPath && !_isPathfinding); // No tenemos path válido
            
            if (shouldUpdatePath)
            {
                _lastTargetPosition = _target.position;
                RequestPath(_target.position);
            }
            
            // Verificar si hemos llegado al destino
            if (_hasPath && _agent.remainingDistance <= _stoppingDistance)
            {
                if (_debugPath)
                {
                    Debug.Log($"[SmartPathfinding3D] {name} reached destination");
                }
            }
        }
        
        /// <summary>
        /// Obtiene la dirección de movimiento actual
        /// </summary>
        public Vector3 GetMovementDirection()
        {
            if (!_hasPath || !_agent.hasPath) return Vector3.zero;
            
            return _agent.desiredVelocity.normalized;
        }
        
        /// <summary>
        /// Obtiene la velocidad deseada del agente
        /// </summary>
        public Vector3 GetDesiredVelocity()
        {
            if (!_hasPath || !_agent.hasPath) return Vector3.zero;
            
            return _agent.desiredVelocity;
        }
        
        /// <summary>
        /// Verifica si hay línea de visión hacia el objetivo
        /// </summary>
        public bool HasLineOfSightToTarget()
        {
            if (_target == null) return false;
            
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Vector3 direction = (_target.position - origin).normalized;
            float distance = Vector3.Distance(transform.position, _target.position);
            
            // Solo verificar contra paredes (Default layer)
            int wallLayerMask = LayerMask.GetMask("Default");
            
            return !Physics.Raycast(origin, direction, distance, wallLayerMask);
        }
        
        private void OnDrawGizmos()
        {
            if (!_debugPath || _currentPath == null || _currentPath.Length < 2) return;
            
            // Dibujar el path
            Gizmos.color = Color.blue;
            for (int i = 0; i < _currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine(_currentPath[i], _currentPath[i + 1]);
            }
            
            // Dibujar puntos del path
            Gizmos.color = Color.cyan;
            foreach (var point in _currentPath)
            {
                Gizmos.DrawSphere(point, 0.2f);
            }
            
            // Dibujar destino
            if (_target != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(_target.position, 0.3f);
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_agent == null) return;
            
            // Dibujar stopping distance
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
            Gizmos.DrawSphere(transform.position, _stoppingDistance);
        }
    }
}