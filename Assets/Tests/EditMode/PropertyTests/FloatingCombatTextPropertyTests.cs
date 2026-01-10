using NUnit.Framework;
using EtherDomes.UI;
using UnityEngine;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Floating Combat Text system.
    /// </summary>
    [TestFixture]
    public class FloatingCombatTextPropertyTests
    {
        /// <summary>
        /// Feature: mvp-10-features, Property 19: FCT on Damage
        /// For any damage > 0, ShowDamage SHALL fire OnFCTCreated event.
        /// Validates: Requirements 6.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void FCTOnDamage_PositiveDamage_FiresEvent()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            bool eventFired = false;
            FCTData receivedData = default;
            
            fct.OnFCTCreated += (data) =>
            {
                eventFired = true;
                receivedData = data;
            };

            float damage = Random.Range(1f, 10000f);
            Vector3 position = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(0f, 50f),
                Random.Range(-100f, 100f)
            );

            // Act
            fct.ShowDamage(position, damage, false);

            // Assert
            Assert.That(eventFired, Is.True,
                "OnFCTCreated should fire for positive damage");
            Assert.That(receivedData.Value, Is.EqualTo(damage),
                "FCT data should contain correct damage value");
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.Damage),
                "FCT type should be Damage");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property 19: Critical damage fires event with correct type
        /// </summary>
        [Test]
        [Repeat(100)]
        public void FCTOnDamage_CriticalDamage_HasCorrectType()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            float damage = Random.Range(1f, 10000f);
            Vector3 position = Vector3.zero;

            // Act
            fct.ShowDamage(position, damage, true);

            // Assert
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.CriticalDamage),
                "Critical damage should have CriticalDamage type");
            Assert.That(receivedData.IsCritical, Is.True,
                "IsCritical should be true for critical damage");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Healing fires event with correct type
        /// </summary>
        [Test]
        [Repeat(100)]
        public void FCTOnHealing_PositiveHealing_FiresEvent()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            bool eventFired = false;
            FCTData receivedData = default;
            
            fct.OnFCTCreated += (data) =>
            {
                eventFired = true;
                receivedData = data;
            };

            float healing = Random.Range(1f, 10000f);
            Vector3 position = Vector3.zero;

            // Act
            fct.ShowHealing(position, healing, false);

            // Assert
            Assert.That(eventFired, Is.True,
                "OnFCTCreated should fire for healing");
            Assert.That(receivedData.Value, Is.EqualTo(healing),
                "FCT data should contain correct healing value");
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.Healing),
                "FCT type should be Healing");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Miss fires event
        /// </summary>
        [Test]
        public void FCTOnMiss_FiresEvent()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            bool eventFired = false;
            FCTData receivedData = default;
            
            fct.OnFCTCreated += (data) =>
            {
                eventFired = true;
                receivedData = data;
            };

            // Act
            fct.ShowMiss(Vector3.zero);

            // Assert
            Assert.That(eventFired, Is.True, "OnFCTCreated should fire for miss");
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.Miss), "Type should be Miss");
            Assert.That(receivedData.Text, Is.EqualTo("Miss"), "Text should be 'Miss'");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Status fires event with correct text
        /// </summary>
        [Test]
        public void FCTOnStatus_FiresEventWithCorrectText()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            string status = "Stunned";

            // Act
            fct.ShowStatus(Vector3.zero, status);

            // Assert
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.Status), "Type should be Status");
            Assert.That(receivedData.Text, Is.EqualTo(status), "Text should match status");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Dodge fires event
        /// </summary>
        [Test]
        public void FCTOnDodge_FiresEvent()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            // Act
            fct.ShowDodge(Vector3.zero);

            // Assert
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.Dodge), "Type should be Dodge");
            Assert.That(receivedData.Text, Is.EqualTo("Dodge"), "Text should be 'Dodge'");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Parry fires event
        /// </summary>
        [Test]
        public void FCTOnParry_FiresEvent()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            // Act
            fct.ShowParry(Vector3.zero);

            // Assert
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.Parry), "Type should be Parry");
            Assert.That(receivedData.Text, Is.EqualTo("Parry"), "Text should be 'Parry'");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Block fires event
        /// </summary>
        [Test]
        public void FCTOnBlock_FiresEvent()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            // Act
            fct.ShowBlock(Vector3.zero, 50f);

            // Assert
            Assert.That(receivedData.Type, Is.EqualTo(FCTType.Block), "Type should be Block");
            Assert.That(receivedData.Value, Is.EqualTo(50f), "Value should be blocked amount");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: FCT data contains correct world position
        /// </summary>
        [Test]
        [Repeat(100)]
        public void FCTData_ContainsCorrectPosition()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            Vector3 position = new Vector3(
                Random.Range(-100f, 100f),
                Random.Range(0f, 50f),
                Random.Range(-100f, 100f)
            );

            // Act
            fct.ShowDamage(position, 100f, false);

            // Assert
            Assert.That(receivedData.WorldPosition, Is.EqualTo(position),
                "FCT data should contain correct world position");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Damage text is rounded integer
        /// </summary>
        [Test]
        [Repeat(100)]
        public void DamageText_IsRoundedInteger()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            float damage = Random.Range(1f, 10000f);

            // Act
            fct.ShowDamage(Vector3.zero, damage, false);

            // Assert
            string expectedText = Mathf.RoundToInt(damage).ToString();
            Assert.That(receivedData.Text, Is.EqualTo(expectedText),
                "Damage text should be rounded integer");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }

        /// <summary>
        /// Property: Healing text has plus prefix
        /// </summary>
        [Test]
        [Repeat(100)]
        public void HealingText_HasPlusPrefix()
        {
            // Arrange
            var fctGO = new GameObject("FCT");
            var fct = fctGO.AddComponent<FloatingCombatText>();
            
            FCTData receivedData = default;
            fct.OnFCTCreated += (data) => receivedData = data;

            float healing = Random.Range(1f, 10000f);

            // Act
            fct.ShowHealing(Vector3.zero, healing, false);

            // Assert
            Assert.That(receivedData.Text.StartsWith("+"), Is.True,
                "Healing text should start with '+'");

            // Cleanup
            Object.DestroyImmediate(fctGO);
        }
    }
}
