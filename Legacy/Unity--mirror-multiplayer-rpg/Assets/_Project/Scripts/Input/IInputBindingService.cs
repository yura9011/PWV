using System.Collections.Generic;

namespace EtherDomes.Input
{
    /// <summary>
    /// Interface for managing input bindings with persistence and conflict detection.
    /// </summary>
    public interface IInputBindingService
    {
        /// <summary>
        /// Rebinds an action to a new binding path.
        /// </summary>
        /// <param name="actionName">Name of the action to rebind</param>
        /// <param name="bindingPath">New binding path (e.g., "<Keyboard>/w")</param>
        /// <returns>True if rebind was successful</returns>
        bool RebindAction(string actionName, string bindingPath);

        /// <summary>
        /// Gets the current binding path for an action.
        /// </summary>
        /// <param name="actionName">Name of the action</param>
        /// <returns>Current binding path or null if not found</returns>
        string GetCurrentBinding(string actionName);

        /// <summary>
        /// Resets all bindings to their default values.
        /// </summary>
        void ResetToDefaults();

        /// <summary>
        /// Saves current bindings to persistent storage.
        /// </summary>
        void SaveBindings();

        /// <summary>
        /// Loads bindings from persistent storage.
        /// </summary>
        void LoadBindings();

        /// <summary>
        /// Checks if a binding would conflict with existing bindings.
        /// </summary>
        /// <param name="actionName">Action to check</param>
        /// <param name="newBinding">Proposed new binding</param>
        /// <returns>True if there would be a conflict</returns>
        bool HasConflict(string actionName, string newBinding);

        /// <summary>
        /// Gets all actions that conflict with a given binding.
        /// </summary>
        /// <param name="binding">Binding path to check</param>
        /// <returns>List of action names that use this binding</returns>
        List<string> GetConflictingActions(string binding);
    }
}
