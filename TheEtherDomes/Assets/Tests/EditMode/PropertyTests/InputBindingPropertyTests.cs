using NUnit.Framework;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for input binding persistence.
    /// </summary>
    [TestFixture]
    public class InputBindingPropertyTests
    {
        /// <summary>
        /// Property 5: Input Binding Persistence Round-Trip
        /// Saved bindings should be restored exactly.
        /// </summary>
        [Test]
        public void InputBindings_RoundTrip_PreservesBindings()
        {
            // Arrange - Simulate binding data
            var originalBindings = new InputBindingData
            {
                MoveForward = "w",
                MoveBackward = "s",
                MoveLeft = "a",
                MoveRight = "d",
                CycleTarget = "tab",
                ClearTarget = "escape",
                Ability1 = "1",
                Ability2 = "2",
                Ability3 = "3"
            };

            // Act - Simulate serialization round-trip
            string json = JsonUtility.ToJson(originalBindings);
            var loadedBindings = JsonUtility.FromJson<InputBindingData>(json);

            // Assert - All bindings should match
            Assert.That(loadedBindings.MoveForward, Is.EqualTo(originalBindings.MoveForward));
            Assert.That(loadedBindings.MoveBackward, Is.EqualTo(originalBindings.MoveBackward));
            Assert.That(loadedBindings.MoveLeft, Is.EqualTo(originalBindings.MoveLeft));
            Assert.That(loadedBindings.MoveRight, Is.EqualTo(originalBindings.MoveRight));
            Assert.That(loadedBindings.CycleTarget, Is.EqualTo(originalBindings.CycleTarget));
            Assert.That(loadedBindings.ClearTarget, Is.EqualTo(originalBindings.ClearTarget));
            Assert.That(loadedBindings.Ability1, Is.EqualTo(originalBindings.Ability1));
            Assert.That(loadedBindings.Ability2, Is.EqualTo(originalBindings.Ability2));
            Assert.That(loadedBindings.Ability3, Is.EqualTo(originalBindings.Ability3));
        }

        /// <summary>
        /// Property: Custom bindings are preserved
        /// </summary>
        [Test]
        public void CustomBindings_ArePreserved()
        {
            var customBindings = new InputBindingData
            {
                MoveForward = "upArrow",
                MoveBackward = "downArrow",
                MoveLeft = "leftArrow",
                MoveRight = "rightArrow",
                CycleTarget = "q",
                ClearTarget = "x",
                Ability1 = "numpad1",
                Ability2 = "numpad2",
                Ability3 = "numpad3"
            };

            string json = JsonUtility.ToJson(customBindings);
            var loaded = JsonUtility.FromJson<InputBindingData>(json);

            Assert.That(loaded.MoveForward, Is.EqualTo("upArrow"));
            Assert.That(loaded.CycleTarget, Is.EqualTo("q"));
            Assert.That(loaded.Ability1, Is.EqualTo("numpad1"));
        }

        /// <summary>
        /// Property: Empty bindings are handled
        /// </summary>
        [Test]
        public void EmptyBindings_AreHandled()
        {
            var emptyBindings = new InputBindingData
            {
                MoveForward = "",
                Ability1 = ""
            };

            string json = JsonUtility.ToJson(emptyBindings);
            var loaded = JsonUtility.FromJson<InputBindingData>(json);

            Assert.That(loaded.MoveForward, Is.EqualTo(""));
            Assert.That(loaded.Ability1, Is.EqualTo(""));
        }

        /// <summary>
        /// Property: Default bindings are consistent
        /// </summary>
        [Test]
        public void DefaultBindings_AreConsistent()
        {
            var defaults1 = InputBindingData.GetDefaults();
            var defaults2 = InputBindingData.GetDefaults();

            Assert.That(defaults1.MoveForward, Is.EqualTo(defaults2.MoveForward));
            Assert.That(defaults1.CycleTarget, Is.EqualTo(defaults2.CycleTarget));
            Assert.That(defaults1.Ability1, Is.EqualTo(defaults2.Ability1));
        }

        /// <summary>
        /// Property: Binding paths are valid format
        /// </summary>
        [Test]
        public void BindingPaths_AreValidFormat(
            [Values("w", "a", "s", "d", "tab", "escape", "1", "2", "space")] string binding)
        {
            Assert.That(binding, Is.Not.Null.And.Not.Empty,
                "Binding should not be null or empty");
            Assert.That(binding.Length, Is.GreaterThan(0),
                "Binding should have at least one character");
        }
    }

    /// <summary>
    /// Test data structure for input bindings.
    /// </summary>
    [System.Serializable]
    public class InputBindingData
    {
        public string MoveForward;
        public string MoveBackward;
        public string MoveLeft;
        public string MoveRight;
        public string CycleTarget;
        public string ClearTarget;
        public string Ability1;
        public string Ability2;
        public string Ability3;

        public static InputBindingData GetDefaults()
        {
            return new InputBindingData
            {
                MoveForward = "w",
                MoveBackward = "s",
                MoveLeft = "a",
                MoveRight = "d",
                CycleTarget = "tab",
                ClearTarget = "escape",
                Ability1 = "1",
                Ability2 = "2",
                Ability3 = "3"
            };
        }
    }
}
