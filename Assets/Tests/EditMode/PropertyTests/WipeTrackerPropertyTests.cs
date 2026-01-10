using NUnit.Framework;
using EtherDomes.World;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Wipe Tracker.
    /// </summary>
    [TestFixture]
    public class WipeTrackerPropertyTests
    {
        private WipeTracker _wipeTracker;

        [SetUp]
        public void SetUp()
        {
            _wipeTracker = new WipeTracker();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 17: Three Wipe Expulsion
        /// After 3 wipes, ShouldExpelGroup SHALL return true.
        /// Validates: Requirements 5.5
        /// </summary>
        [Test]
        public void ThreeWipeExpulsion_After3Wipes_ShouldExpelGroupReturnsTrue()
        {
            // Arrange
            string instanceId = "test-instance";

            // Act
            _wipeTracker.RecordWipe(instanceId);
            _wipeTracker.RecordWipe(instanceId);
            _wipeTracker.RecordWipe(instanceId);

            // Assert
            Assert.That(_wipeTracker.ShouldExpelGroup(instanceId), Is.True,
                "ShouldExpelGroup should return true after 3 wipes");
        }

        /// <summary>
        /// Property 17: Before 3 wipes, ShouldExpelGroup returns false
        /// </summary>
        [Test]
        [Repeat(100)]
        public void ThreeWipeExpulsion_Before3Wipes_ShouldExpelGroupReturnsFalse()
        {
            // Arrange
            string instanceId = $"test-instance-{UnityEngine.Random.Range(1, 1000)}";
            int wipeCount = UnityEngine.Random.Range(0, 3);

            // Act
            for (int i = 0; i < wipeCount; i++)
            {
                _wipeTracker.RecordWipe(instanceId);
            }

            // Assert
            Assert.That(_wipeTracker.ShouldExpelGroup(instanceId), Is.False,
                $"ShouldExpelGroup should return false with {wipeCount} wipes");
        }

        /// <summary>
        /// Property: MaxWipesBeforeExpulsion is 3
        /// </summary>
        [Test]
        public void MaxWipesBeforeExpulsion_Is3()
        {
            Assert.That(_wipeTracker.MaxWipesBeforeExpulsion, Is.EqualTo(3),
                "MaxWipesBeforeExpulsion should be 3");
        }

        /// <summary>
        /// Property: GetWipeCount returns correct count
        /// </summary>
        [Test]
        [Repeat(100)]
        public void GetWipeCount_ReturnsCorrectCount()
        {
            // Arrange
            string instanceId = $"test-instance-{UnityEngine.Random.Range(1, 1000)}";
            int expectedWipes = UnityEngine.Random.Range(0, 10);

            // Act
            for (int i = 0; i < expectedWipes; i++)
            {
                _wipeTracker.RecordWipe(instanceId);
            }

            // Assert
            Assert.That(_wipeTracker.GetWipeCount(instanceId), Is.EqualTo(expectedWipes),
                $"GetWipeCount should return {expectedWipes}");
        }

        /// <summary>
        /// Property: ResetWipeCount sets count to 0
        /// </summary>
        [Test]
        public void ResetWipeCount_SetsCountToZero()
        {
            // Arrange
            string instanceId = "test-instance";
            _wipeTracker.RecordWipe(instanceId);
            _wipeTracker.RecordWipe(instanceId);

            // Act
            _wipeTracker.ResetWipeCount(instanceId);

            // Assert
            Assert.That(_wipeTracker.GetWipeCount(instanceId), Is.EqualTo(0),
                "ResetWipeCount should set count to 0");
            Assert.That(_wipeTracker.ShouldExpelGroup(instanceId), Is.False,
                "ShouldExpelGroup should return false after reset");
        }

        /// <summary>
        /// Property: OnWipeRecorded fires on each wipe
        /// </summary>
        [Test]
        public void OnWipeRecorded_FiresOnEachWipe()
        {
            // Arrange
            string instanceId = "test-instance";
            int eventCount = 0;
            int lastReportedCount = 0;
            
            _wipeTracker.OnWipeRecorded += (id, count) =>
            {
                eventCount++;
                lastReportedCount = count;
            };

            // Act
            _wipeTracker.RecordWipe(instanceId);
            _wipeTracker.RecordWipe(instanceId);

            // Assert
            Assert.That(eventCount, Is.EqualTo(2), "OnWipeRecorded should fire twice");
            Assert.That(lastReportedCount, Is.EqualTo(2), "Last reported count should be 2");
        }

        /// <summary>
        /// Property: OnGroupExpelled fires when reaching max wipes
        /// </summary>
        [Test]
        public void OnGroupExpelled_FiresAtMaxWipes()
        {
            // Arrange
            string instanceId = "test-instance";
            bool eventFired = false;
            string expelledInstanceId = null;
            
            _wipeTracker.OnGroupExpelled += (id) =>
            {
                eventFired = true;
                expelledInstanceId = id;
            };

            // Act
            _wipeTracker.RecordWipe(instanceId);
            _wipeTracker.RecordWipe(instanceId);
            Assert.That(eventFired, Is.False, "Should not fire before 3 wipes");
            
            _wipeTracker.RecordWipe(instanceId);

            // Assert
            Assert.That(eventFired, Is.True, "OnGroupExpelled should fire at 3 wipes");
            Assert.That(expelledInstanceId, Is.EqualTo(instanceId), "Should report correct instance");
        }

        /// <summary>
        /// Property: Wipe counts are per-instance
        /// </summary>
        [Test]
        public void WipeCounts_ArePerInstance()
        {
            // Arrange
            string instance1 = "instance-1";
            string instance2 = "instance-2";

            // Act
            _wipeTracker.RecordWipe(instance1);
            _wipeTracker.RecordWipe(instance1);
            _wipeTracker.RecordWipe(instance2);

            // Assert
            Assert.That(_wipeTracker.GetWipeCount(instance1), Is.EqualTo(2),
                "Instance 1 should have 2 wipes");
            Assert.That(_wipeTracker.GetWipeCount(instance2), Is.EqualTo(1),
                "Instance 2 should have 1 wipe");
        }

        /// <summary>
        /// Property: New instance starts with 0 wipes
        /// </summary>
        [Test]
        public void NewInstance_StartsWithZeroWipes()
        {
            // Arrange
            string instanceId = "new-instance";

            // Assert
            Assert.That(_wipeTracker.GetWipeCount(instanceId), Is.EqualTo(0),
                "New instance should have 0 wipes");
            Assert.That(_wipeTracker.ShouldExpelGroup(instanceId), Is.False,
                "New instance should not trigger expulsion");
        }

        /// <summary>
        /// Property: Custom max wipes works correctly
        /// </summary>
        [Test]
        public void CustomMaxWipes_WorksCorrectly()
        {
            // Arrange
            var customTracker = new WipeTracker(5);
            string instanceId = "test-instance";

            // Act
            for (int i = 0; i < 4; i++)
            {
                customTracker.RecordWipe(instanceId);
            }

            // Assert
            Assert.That(customTracker.MaxWipesBeforeExpulsion, Is.EqualTo(5));
            Assert.That(customTracker.ShouldExpelGroup(instanceId), Is.False,
                "Should not expel at 4 wipes with max 5");

            customTracker.RecordWipe(instanceId);
            Assert.That(customTracker.ShouldExpelGroup(instanceId), Is.True,
                "Should expel at 5 wipes with max 5");
        }
    }
}
