using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EtherDomes.Input
{
    /// <summary>
    /// Service for managing input bindings with persistence and conflict detection.
    /// Uses Unity Input System for rebinding and PlayerPrefs for persistence.
    /// </summary>
    public class InputBindingService : IInputBindingService
    {
        private const string BINDINGS_PREFS_KEY = "EtherDomes_InputBindings";

        private InputActionAsset _inputActions;
        private Dictionary<string, string> _bindingOverrides;

        public InputBindingService(InputActionAsset inputActions)
        {
            _inputActions = inputActions ?? throw new ArgumentNullException(nameof(inputActions));
            _bindingOverrides = new Dictionary<string, string>();
        }

        #region IInputBindingService Implementation

        public bool RebindAction(string actionName, string bindingPath)
        {
            if (string.IsNullOrEmpty(actionName) || string.IsNullOrEmpty(bindingPath))
                return false;

            var action = _inputActions.FindAction(actionName);
            if (action == null)
            {
                Debug.LogWarning($"[InputBindingService] Action not found: {actionName}");
                return false;
            }

            try
            {
                // Find the first non-composite binding
                int bindingIndex = -1;
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (!action.bindings[i].isComposite && !action.bindings[i].isPartOfComposite)
                    {
                        bindingIndex = i;
                        break;
                    }
                }

                if (bindingIndex < 0)
                {
                    // For composite bindings, we'd need more complex handling
                    Debug.LogWarning($"[InputBindingService] No simple binding found for: {actionName}");
                    return false;
                }

                // Apply the override
                action.ApplyBindingOverride(bindingIndex, bindingPath);
                _bindingOverrides[actionName] = bindingPath;

                Debug.Log($"[InputBindingService] Rebound {actionName} to {bindingPath}");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputBindingService] Failed to rebind {actionName}: {ex.Message}");
                return false;
            }
        }

        public string GetCurrentBinding(string actionName)
        {
            if (string.IsNullOrEmpty(actionName))
                return null;

            // Check overrides first
            if (_bindingOverrides.TryGetValue(actionName, out string overridePath))
                return overridePath;

            var action = _inputActions.FindAction(actionName);
            if (action == null)
                return null;

            // Find the first non-composite binding
            for (int i = 0; i < action.bindings.Count; i++)
            {
                var binding = action.bindings[i];
                if (!binding.isComposite && !binding.isPartOfComposite)
                {
                    return binding.effectivePath;
                }
            }

            return null;
        }

        public void ResetToDefaults()
        {
            _inputActions.RemoveAllBindingOverrides();
            _bindingOverrides.Clear();
            Debug.Log("[InputBindingService] Reset all bindings to defaults");
        }

        public void SaveBindings()
        {
            try
            {
                // Save as JSON
                string json = _inputActions.SaveBindingOverridesAsJson();
                PlayerPrefs.SetString(BINDINGS_PREFS_KEY, json);
                PlayerPrefs.Save();
                Debug.Log("[InputBindingService] Bindings saved");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputBindingService] Failed to save bindings: {ex.Message}");
            }
        }

        public void LoadBindings()
        {
            try
            {
                string json = PlayerPrefs.GetString(BINDINGS_PREFS_KEY, string.Empty);
                if (!string.IsNullOrEmpty(json))
                {
                    _inputActions.LoadBindingOverridesFromJson(json);
                    
                    // Rebuild our override dictionary
                    _bindingOverrides.Clear();
                    foreach (var actionMap in _inputActions.actionMaps)
                    {
                        foreach (var action in actionMap.actions)
                        {
                            for (int i = 0; i < action.bindings.Count; i++)
                            {
                                var binding = action.bindings[i];
                                if (!string.IsNullOrEmpty(binding.overridePath))
                                {
                                    _bindingOverrides[action.name] = binding.overridePath;
                                }
                            }
                        }
                    }
                    
                    Debug.Log("[InputBindingService] Bindings loaded");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[InputBindingService] Failed to load bindings: {ex.Message}");
            }
        }

        public bool HasConflict(string actionName, string newBinding)
        {
            var conflicting = GetConflictingActions(newBinding);
            
            // Remove the action we're rebinding from the conflict list
            conflicting.Remove(actionName);
            
            return conflicting.Count > 0;
        }

        public List<string> GetConflictingActions(string binding)
        {
            var conflicts = new List<string>();

            if (string.IsNullOrEmpty(binding))
                return conflicts;

            foreach (var actionMap in _inputActions.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    for (int i = 0; i < action.bindings.Count; i++)
                    {
                        var existingBinding = action.bindings[i];
                        
                        // Skip composite bindings themselves
                        if (existingBinding.isComposite)
                            continue;

                        string effectivePath = existingBinding.effectivePath;
                        
                        if (string.Equals(effectivePath, binding, StringComparison.OrdinalIgnoreCase))
                        {
                            if (!conflicts.Contains(action.name))
                            {
                                conflicts.Add(action.name);
                            }
                        }
                    }
                }
            }

            return conflicts;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Gets all action names in the input actions asset.
        /// </summary>
        public List<string> GetAllActionNames()
        {
            var names = new List<string>();
            
            foreach (var actionMap in _inputActions.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    names.Add(action.name);
                }
            }
            
            return names;
        }

        /// <summary>
        /// Gets all current bindings as a dictionary.
        /// </summary>
        public Dictionary<string, string> GetAllBindings()
        {
            var bindings = new Dictionary<string, string>();
            
            foreach (var actionMap in _inputActions.actionMaps)
            {
                foreach (var action in actionMap.actions)
                {
                    string binding = GetCurrentBinding(action.name);
                    if (!string.IsNullOrEmpty(binding))
                    {
                        bindings[action.name] = binding;
                    }
                }
            }
            
            return bindings;
        }

        #endregion
    }
}
