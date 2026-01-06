using Mirror;
using UnityEngine;
using EtherDomes.Core;

namespace EtherDomes.Player
{
    /// <summary>
    /// Controls player visual appearance based on class selection.
    /// Syncs class color across network using SyncVar.
    /// </summary>
    public class PlayerVisualController : NetworkBehaviour
    {
        [Header("Visual References")]
        [SerializeField] private Renderer _bodyRenderer;
        [SerializeField] private string _colorPropertyName = "_BaseColor";
        
        [Header("Class Colors")]
        [SerializeField] private Color _guerreroColor = Color.red;
        [SerializeField] private Color _magoColor = Color.blue;
        
        [SyncVar(hook = nameof(OnClassChanged))]
        private int _classID;
        
        private MaterialPropertyBlock _propertyBlock;

        public int ClassID => _classID;

        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            
            // Auto-find renderer if not assigned
            if (_bodyRenderer == null)
            {
                _bodyRenderer = GetComponentInChildren<Renderer>();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            // Apply initial visual
            ApplyClassVisual(_classID);
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            
            // Request server to set our class from local selection
            CmdSetClass(ClassSelectionData.GetClassID());
        }

        /// <summary>
        /// Called by server to set class during spawn.
        /// </summary>
        [Server]
        public void ServerSetClass(int classID)
        {
            _classID = classID;
            Debug.Log($"[PlayerVisual] Server set class to {classID}");
        }

        [Command]
        private void CmdSetClass(int classID)
        {
            _classID = classID;
            Debug.Log($"[PlayerVisual] Class set via Command: {classID}");
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
            return _classID == (int)PlayerClass.Guerrero ? _guerreroColor : _magoColor;
        }
    }
}
