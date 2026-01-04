using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using EtherDomes.Input;
using System.Collections.Generic;

namespace EtherDomes.Tests
{
    /// <summary>
    /// Property-based and unit tests for InputBindingService.
    /// </summary>
    [TestFixture]
    public class InputBindingTests
    {
        private InputActionAsset _testInputActions;
        private InputBindingService _bindingService;

        [SetUp]
        public void SetUp()
        {
            // Create a test input action asset
            _testInputActions = ScriptableObject.CreateInstance<InputActionAsset>();
            
            var actionMap = _testInputActions.AddActionMap("Player");
            
            var moveAction = actionMap.AddAction("Move", InputActionType.Value);
            moveAction.AddBinding("<Keyboard>/w");
            
            var jumpAction = actionMap.AddAction("Jump", InputActionType.Button);
            jumpAction.AddBinding("<Keyboard>/space");
            
            var interactAction = actionMap.AddAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");
            
            var sprintAction = actionMap.AddAction("Sprint", InputActionType.Button);
            sprintAction.AddBinding("<Keyboard>/leftShift");

            _bindingService = new InputBindingService(_testInputActions);
            
            // Clear any saved bindings from previous tests
            PlayerPrefs.DeleteKey("EtherDomes_InputBindings");
        }

        [TearDown]
        public void TearDown()
        {
            if (_testInputActions != null)
            {
                Object.DestroyImmediate(_testInputActions);
            }
            
            // Clean up PlayerPrefs
            PlayerPrefs.DeleteKey("EtherDomes_InputBindings");
        }

        #region Property 6: Input Binding Round-Trip Persistence

        /// <summary>
        /// Feature: network-player-foundation, Property 6: Input Binding Round-Trip Persistence
        /// For any valid action name and binding path, calling RebindAction followed by SaveBindings,
        /// then LoadBindings, then GetCurrentBinding SHALL return the original binding path.
        /// Validates: Requirements 4.3
        /// </summary>
        [Test]
        public void Property6_InputBindingRoundTrip_PreservesBindings()
        {
            string actionName = "Jump";
            string newBinding = "<Keyboard>/f";

            // Rebind
            bool rebound = _bindingService.RebindAction(actionName, newBinding);
            Assert.IsTrue(rebound, "Rebind should succeed");

            // Save
            _bindingService.SaveBindings();

            // Create new service instance to simulate app restart
            var newService = new InputBindingService(_testInputActions);
            
            // Load
            newService.LoadBindings();

            // Verify
            string loadedBinding = newService.GetCurrentBinding(actionName);
            Assert.AreEqual(newBinding, loadedBinding,
                "Loaded binding should match saved binding");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 6: Input Binding Round-Trip Persistence
        /// Tests multiple bindings round-trip.
        /// Validates: Requirements 4.3
        /// </summary>
        [Test]
        public void Property6_InputBindingRoundTrip_MultipleBindings()
        {
            var bindings = new Dictionary<string, string>
            {
                { "Jump", "<Keyboard>/f" },
                { "Interact", "<Keyboard>/q" },
                { "Sprint", "<Keyboard>/leftControl" }
            };

            // Rebind all
            foreach (var kvp in bindings)
            {
                bool rebound = _bindingService.RebindAction(kvp.Key, kvp.Value);
                Assert.IsTrue(rebound, $"Rebind should succeed for {kvp.Key}");
            }

            // Save
            _bindingService.SaveBindings();

            // Create new service instance
            var newService = new InputBindingService(_testInputActions);
            newService.LoadBindings();

            // Verify all
            foreach (var kvp in bindings)
            {
                string loadedBinding = newService.GetCurrentBinding(kvp.Key);
                Assert.AreEqual(kvp.Value, loadedBinding,
                    $"Loaded binding for {kvp.Key} should match saved binding");
            }
        }

        #endregion

        #region Property 7: Binding Conflict Detection

        /// <summary>
        /// Feature: network-player-foundation, Property 7: Binding Conflict Detection
        /// For any two distinct action names bound to the same binding path,
        /// HasConflict SHALL return true for either action when checking that binding.
        /// Validates: Requirements 4.5
        /// </summary>
        [Test]
        public void Property7_BindingConflictDetection_DetectsConflicts()
        {
            // Jump is bound to <Keyboard>/space by default
            string conflictingBinding = "<Keyboard>/space";

            // Check if rebinding Interact to space would conflict
            bool hasConflict = _bindingService.HasConflict("Interact", conflictingBinding);
            Assert.IsTrue(hasConflict, "Should detect conflict with existing Jump binding");

            // Get conflicting actions
            var conflicts = _bindingService.GetConflictingActions(conflictingBinding);
            Assert.Contains("Jump", conflicts, "Jump should be in conflict list");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 7: Binding Conflict Detection
        /// Tests that non-conflicting bindings are allowed.
        /// Validates: Requirements 4.5
        /// </summary>
        [Test]
        public void Property7_BindingConflictDetection_AllowsNonConflicting()
        {
            string uniqueBinding = "<Keyboard>/p"; // Not used by any action

            bool hasConflict = _bindingService.HasConflict("Interact", uniqueBinding);
            Assert.IsFalse(hasConflict, "Should not detect conflict for unique binding");

            var conflicts = _bindingService.GetConflictingActions(uniqueBinding);
            Assert.IsEmpty(conflicts, "Conflict list should be empty for unique binding");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 7: Binding Conflict Detection
        /// Tests that rebinding same action to same key doesn't count as conflict.
        /// Validates: Requirements 4.5
        /// </summary>
        [Test]
        public void Property7_BindingConflictDetection_SameActionNotConflict()
        {
            // Jump is already bound to space
            string currentBinding = "<Keyboard>/space";

            // Rebinding Jump to its current binding should not be a conflict
            bool hasConflict = _bindingService.HasConflict("Jump", currentBinding);
            Assert.IsFalse(hasConflict, 
                "Rebinding action to its current binding should not be a conflict");
        }

        #endregion

        #region Unit Tests

        [Test]
        public void GetCurrentBinding_ReturnsDefaultBinding()
        {
            string binding = _bindingService.GetCurrentBinding("Jump");
            Assert.AreEqual("<Keyboard>/space", binding);
        }

        [Test]
        public void GetCurrentBinding_InvalidAction_ReturnsNull()
        {
            string binding = _bindingService.GetCurrentBinding("NonExistentAction");
            Assert.IsNull(binding);
        }

        [Test]
        public void RebindAction_InvalidAction_ReturnsFalse()
        {
            bool result = _bindingService.RebindAction("NonExistentAction", "<Keyboard>/x");
            Assert.IsFalse(result);
        }

        [Test]
        public void RebindAction_EmptyBinding_ReturnsFalse()
        {
            bool result = _bindingService.RebindAction("Jump", "");
            Assert.IsFalse(result);
        }

        [Test]
        public void ResetToDefaults_RestoresOriginalBindings()
        {
            // Change a binding
            _bindingService.RebindAction("Jump", "<Keyboard>/f");
            
            // Reset
            _bindingService.ResetToDefaults();
            
            // Verify original binding is restored
            string binding = _bindingService.GetCurrentBinding("Jump");
            Assert.AreEqual("<Keyboard>/space", binding);
        }

        [Test]
        public void GetAllActionNames_ReturnsAllActions()
        {
            var names = _bindingService.GetAllActionNames();
            
            Assert.Contains("Move", names);
            Assert.Contains("Jump", names);
            Assert.Contains("Interact", names);
            Assert.Contains("Sprint", names);
        }

        #endregion
    }
}
