using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.Combat
{
    /// <summary>
    /// Handles visual representation of stealth state.
    /// Reduces opacity for local player (30%) and hides completely for enemies (0%).
    /// Requirements: 4.7
    /// </summary>
    public class StealthVisual : MonoBehaviour
    {
        #region Constants
        
        private const float LOCAL_PLAYER_OPACITY = 0.3f;
        private const float ENEMY_VIEW_OPACITY = 0f;
        private const float NORMAL_OPACITY = 1f;
        private const float FADE_DURATION = 0.3f;
        
        #endregion

        #region Serialized Fields
        
        [SerializeField] private Renderer[] _renderers;
        [SerializeField] private bool _isLocalPlayer;
        
        #endregion

        #region Private State
        
        private IStealthSystem _stealthSystem;
        private ulong _playerId;
        private bool _isStealthed;
        private float _currentOpacity = NORMAL_OPACITY;
        private float _targetOpacity = NORMAL_OPACITY;
        private readonly Dictionary<Renderer, Material[]> _originalMaterials = new();
        private readonly Dictionary<Renderer, Material[]> _instanceMaterials = new();
        
        #endregion

        #region Initialization
        
        public void Initialize(IStealthSystem stealthSystem, ulong playerId, bool isLocalPlayer)
        {
            _stealthSystem = stealthSystem;
            _playerId = playerId;
            _isLocalPlayer = isLocalPlayer;
            
            CacheRenderers();
            SubscribeToEvents();
            
            // Check initial state
            UpdateStealthVisual(_stealthSystem.IsInStealth(_playerId));
        }
        
        private void CacheRenderers()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>();
            }
            
            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;
                
                _originalMaterials[renderer] = renderer.sharedMaterials;
                _instanceMaterials[renderer] = renderer.materials; // Creates instances
            }
        }
        
        private void SubscribeToEvents()
        {
            if (_stealthSystem == null) return;
            
            _stealthSystem.OnStealthEntered += OnStealthEntered;
            _stealthSystem.OnStealthBroken += OnStealthBroken;
        }
        
        private void UnsubscribeFromEvents()
        {
            if (_stealthSystem == null) return;
            
            _stealthSystem.OnStealthEntered -= OnStealthEntered;
            _stealthSystem.OnStealthBroken -= OnStealthBroken;
        }
        
        #endregion

        #region Event Handlers
        
        private void OnStealthEntered(ulong playerId)
        {
            if (playerId != _playerId) return;
            UpdateStealthVisual(true);
        }
        
        private void OnStealthBroken(ulong playerId, StealthBreakReason reason)
        {
            if (playerId != _playerId) return;
            UpdateStealthVisual(false);
        }
        
        #endregion

        #region Visual Updates
        
        private void UpdateStealthVisual(bool isStealthed)
        {
            _isStealthed = isStealthed;
            
            if (isStealthed)
            {
                _targetOpacity = _isLocalPlayer ? LOCAL_PLAYER_OPACITY : ENEMY_VIEW_OPACITY;
            }
            else
            {
                _targetOpacity = NORMAL_OPACITY;
            }
        }
        
        private void Update()
        {
            if (Mathf.Approximately(_currentOpacity, _targetOpacity)) return;
            
            _currentOpacity = Mathf.MoveTowards(_currentOpacity, _targetOpacity, Time.deltaTime / FADE_DURATION);
            ApplyOpacity(_currentOpacity);
        }
        
        private void ApplyOpacity(float opacity)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer == null) continue;
                if (!_instanceMaterials.TryGetValue(renderer, out var materials)) continue;
                
                foreach (var material in materials)
                {
                    if (material == null) continue;
                    
                    // Handle transparency
                    if (opacity < 1f)
                    {
                        SetMaterialTransparent(material);
                    }
                    else
                    {
                        SetMaterialOpaque(material);
                    }
                    
                    // Set alpha
                    if (material.HasProperty("_Color"))
                    {
                        var color = material.color;
                        color.a = opacity;
                        material.color = color;
                    }
                    else if (material.HasProperty("_BaseColor"))
                    {
                        var color = material.GetColor("_BaseColor");
                        color.a = opacity;
                        material.SetColor("_BaseColor", color);
                    }
                }
                
                // Hide completely if opacity is 0
                renderer.enabled = opacity > 0.001f;
            }
        }
        
        private void SetMaterialTransparent(Material material)
        {
            material.SetFloat("_Surface", 1); // Transparent
            material.SetFloat("_Blend", 0); // Alpha
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
        }
        
        private void SetMaterialOpaque(Material material)
        {
            material.SetFloat("_Surface", 0); // Opaque
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            material.SetInt("_ZWrite", 1);
            material.DisableKeyword("_ALPHATEST_ON");
            material.DisableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = -1;
        }
        
        #endregion

        #region Public API
        
        /// <summary>
        /// Get the current visual opacity.
        /// </summary>
        public float GetCurrentOpacity() => _currentOpacity;
        
        /// <summary>
        /// Get the target opacity based on stealth state.
        /// </summary>
        public float GetTargetOpacity() => _targetOpacity;
        
        /// <summary>
        /// Check if currently in stealth visual state.
        /// </summary>
        public bool IsStealthed => _isStealthed;
        
        /// <summary>
        /// Force immediate opacity update (for testing).
        /// </summary>
        public void ForceOpacity(float opacity)
        {
            _currentOpacity = opacity;
            _targetOpacity = opacity;
            ApplyOpacity(opacity);
        }
        
        #endregion

        #region Cleanup
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
            
            // Clean up instanced materials
            foreach (var kvp in _instanceMaterials)
            {
                if (kvp.Value == null) continue;
                foreach (var material in kvp.Value)
                {
                    if (material != null)
                    {
                        Destroy(material);
                    }
                }
            }
            
            _instanceMaterials.Clear();
            _originalMaterials.Clear();
        }
        
        #endregion
    }
}
