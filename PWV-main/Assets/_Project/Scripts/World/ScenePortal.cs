using UnityEngine;
using UnityEngine.SceneManagement;

namespace EtherDomes.World
{
    /// <summary>
    /// Portal that teleports player to another scene when triggered.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ScenePortal : MonoBehaviour
    {
        [Header("Destination")]
        [SerializeField] private string _targetSceneName;
        [SerializeField] private string _displayName;
        
        [Header("Visual")]
        [SerializeField] private Color _portalColor = new Color(0.5f, 0f, 1f, 0.5f);
        [SerializeField] private float _labelHeight = 3f;
        
        private TextMesh _label;
        private MeshRenderer _renderer;
        
        public string TargetSceneName => _targetSceneName;
        public string DisplayName => _displayName;

        private void Awake()
        {
            // Ensure collider is trigger
            var collider = GetComponent<BoxCollider>();
            collider.isTrigger = true;
            
            // Create visual if not exists
            if (_renderer == null)
            {
                _renderer = GetComponent<MeshRenderer>();
            }
            
            // Create floating label
            CreateLabel();
        }

        private void CreateLabel()
        {
            // Check if label already exists
            var existingLabel = transform.Find("PortalLabel");
            if (existingLabel != null)
            {
                _label = existingLabel.GetComponent<TextMesh>();
                UpdateLabel();
                return;
            }
            
            // Create label GameObject
            var labelGO = new GameObject("PortalLabel");
            labelGO.transform.SetParent(transform);
            labelGO.transform.localPosition = new Vector3(0, _labelHeight, 0);
            labelGO.transform.localRotation = Quaternion.identity;
            
            _label = labelGO.AddComponent<TextMesh>();
            _label.alignment = TextAlignment.Center;
            _label.anchor = TextAnchor.MiddleCenter;
            _label.characterSize = 0.2f;
            _label.fontSize = 48;
            _label.color = Color.white;
            
            UpdateLabel();
        }

        private void UpdateLabel()
        {
            if (_label != null)
            {
                string text = string.IsNullOrEmpty(_displayName) ? _targetSceneName : _displayName;
                _label.text = $"→ {text}";
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[ScenePortal] OnTriggerEnter: {other.name}, Tag: {other.tag}");
            
            // Check if player entered - use tag or component name
            bool isPlayer = other.CompareTag("Player") || 
                other.GetComponent("PlayerController") != null ||
                other.GetComponent("TestPlayer") != null ||
                other.GetComponentInParent<CharacterController>() != null;
                
            if (isPlayer)
            {
                TeleportToScene();
            }
        }

        private void TeleportToScene()
        {
            if (string.IsNullOrEmpty(_targetSceneName))
            {
                Debug.LogWarning($"[ScenePortal] No target scene configured for portal {name}");
                return;
            }
            
            Debug.Log($"[ScenePortal] Teleporting to: {_targetSceneName}");
            SceneManager.LoadScene(_targetSceneName);
        }

        private void OnDrawGizmos()
        {
            // Draw portal area
            Gizmos.color = _portalColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one);
            
            // Draw wireframe
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }

        private void OnValidate()
        {
            // Only update in play mode when label exists
            if (Application.isPlaying && _label != null)
            {
                UpdateLabel();
            }
        }
        
        /// <summary>
        /// Configure portal at runtime
        /// </summary>
        public void Configure(string targetScene, string displayName)
        {
            _targetSceneName = targetScene;
            _displayName = displayName;
            UpdateLabel();
        }
    }
}
