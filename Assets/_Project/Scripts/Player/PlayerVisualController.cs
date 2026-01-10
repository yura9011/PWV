using Unity.Netcode;
using UnityEngine;
using EtherDomes.Core;

namespace EtherDomes.Player
{
    /// <summary>
    /// Controls player visual appearance based on class selection.
    /// Syncs class color across network using NetworkVariable.
    /// </summary>
    public class PlayerVisualController : NetworkBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private string _colorPropertyName = "_BaseColor";
        
        [Header("Class Colors")]
        [SerializeField] private Color _guerreroColor = Color.red;
        [SerializeField] private Color _magoColor = Color.blue;
        
        // NGO NetworkVariable
        private NetworkVariable<int> _classID = new NetworkVariable<int>(0);
        
        private MaterialPropertyBlock _propertyBlock;

        public int ClassID => _classID.Value;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            
            // Auto-find renderer if not assigned
            if (_bodyRenderer == null)
            {
                _bodyRenderer = GetComponentInChildren<Renderer>();
            }
        }

        public override void OnNetworkSpawn()
        {
            // Apply initial visual
            ApplyClassVisual(_classID.Value);

            // Subscribe to changess
            _classID.OnValueChanged += OnClassChanged;

            if (IsOwner)
            {
                // Request server to set our class from local selection
                // Use ServerRpc
                SetClassServerRpc(ClassSelectionData.GetClassID());
            }
        }

        public override void OnNetworkDespawn()
        {
            _classID.OnValueChanged -= OnClassChanged;
        }

        /// <summary>
        /// Called by server to set class during spawn.
        /// </summary>
        public void ServerSetClass(int classID)
        {
            if (IsServer)
            {
                _classID.Value = classID;
                Debug.Log($"[PlayerVisual] Server set class to {classID}");
            }
        }

        [ServerRpc]
        private void SetClassServerRpc(int classID)
        {
            _classID.Value = classID;
            Debug.Log($"[PlayerVisual] Class set via ServerRpc: {classID}");
        }

        private void OnClassChanged(int oldValue, int newValue)
        {
            ApplyClassVisual(newValue);
            Debug.Log($"[PlayerVisual] Class changed from {oldValue} to {newValue}");
        }

        private void ApplyClassVisual(int classID)
        {
            if (_bodyRenderer == null) return;
            
            Color targetColor = classID == (int)PlayerClass.Guerrero ? _guerreroColor : _magoColor;
            
            // Use property block to avoid material instancing
            _bodyRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(_colorPropertyName, targetColor);
            _bodyRenderer.SetPropertyBlock(_propertyBlock);
            
            // Also set material color directly as fallback
            if (_bodyRenderer.material != null)
            {
                _bodyRenderer.material.color = targetColor;
            }
            
            Debug.Log($"[PlayerVisual] Applied color: {targetColor} for class {(PlayerClass)classID}");
        }

        /// <summary>
        /// Gets the color for the current class.
        /// </summary>
        public Color GetCurrentColor()
        {
            return _classID.Value == (int)PlayerClass.Guerrero ? _guerreroColor : _magoColor;
        }
    }
}
