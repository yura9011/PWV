using NUnit.Framework;
using EtherDomes.Network;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Latency Monitor.
    /// </summary>
    [TestFixture]
    public class LatencyPropertyTests
    {
        private LatencyMonitor _monitor;

        [SetUp]
        public void SetUp()
        {
            _monitor = new LatencyMonitor();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 35: Latency State Transitions
        /// For any latency > PauseThreshold (500ms), CurrentState SHALL be Paused.
        /// Validates: Requirements 21.3
        /// </summary>
        [Test]
        [Repeat(100)]
        public void LatencyStateTransitions_AbovePauseThreshold_IsPaused()
        {
            // Arrange
            float latency = UnityEngine.Random.Range(500f, 2000f);

            // Act
            _monitor.UpdateLatency(latency);

            // Assert
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Paused),
                $"Latency {latency}ms should result in Paused state");
            Assert.That(_monitor.IsPaused, Is.True,
                "IsPaused should be true when state is Paused");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 35: Latency State Transitions
        /// For any latency < ResumeThreshold (400ms) after being Paused, 
        /// CurrentState SHALL transition to Normal or Warning.
        /// Validates: Requirements 21.4
        /// </summary>
        [Test]
        public void LatencyStateTransitions_BelowResumeThreshold_Resumes()
        {
            // Arrange - First enter paused state
            _monitor.UpdateLatency(600f);
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Paused));

            // Act - Drop below resume threshold
            _monitor.UpdateLatency(350f);

            // Assert
            Assert.That(_monitor.CurrentState, Is.Not.EqualTo(LatencyState.Paused),
                "Should exit Paused state when latency drops below ResumeThreshold");
            Assert.That(_monitor.IsPaused, Is.False,
                "IsPaused should be false after resuming");
        }

        /// <summary>
        /// Property 35: Latency between resume and pause thresholds stays paused
        /// </summary>
        [Test]
        public void LatencyStateTransitions_BetweenResumeAndPause_StaysPaused()
        {
            // Arrange - Enter paused state
            _monitor.UpdateLatency(600f);
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Paused));

            // Act - Latency drops but stays above resume threshold
            _monitor.UpdateLatency(450f); // Between 400 (resume) and 500 (pause)

            // Assert - Should stay paused (hysteresis)
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Paused),
                "Should stay Paused when latency is between resume and pause thresholds");
        }

        /// <summary>
        /// Property: Warning state for latency between 200-500ms
        /// </summary>
        [Test]
        [Repeat(100)]
        public void LatencyState_BetweenWarningAndPause_IsWarning()
        {
            // Arrange
            float latency = UnityEngine.Random.Range(200f, 499f);

            // Act
            _monitor.UpdateLatency(latency);

            // Assert
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Warning),
                $"Latency {latency}ms should result in Warning state");
            Assert.That(_monitor.IsHighLatency, Is.True,
                "IsHighLatency should be true in Warning state");
        }

        /// <summary>
        /// Property: Normal state for latency below 200ms
        /// </summary>
        [Test]
        [Repeat(100)]
        public void LatencyState_BelowWarning_IsNormal()
        {
            // Arrange
            float latency = UnityEngine.Random.Range(0f, 199f);

            // Act
            _monitor.UpdateLatency(latency);

            // Assert
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Normal),
                $"Latency {latency}ms should result in Normal state");
            Assert.That(_monitor.IsHighLatency, Is.False,
                "IsHighLatency should be false in Normal state");
            Assert.That(_monitor.IsPaused, Is.False,
                "IsPaused should be false in Normal state");
        }

        /// <summary>
        /// Property: Thresholds have correct values
        /// </summary>
        [Test]
        public void Thresholds_HaveCorrectValues()
        {
            Assert.That(_monitor.WarningThreshold, Is.EqualTo(200f),
                "WarningThreshold should be 200ms");
            Assert.That(_monitor.PauseThreshold, Is.EqualTo(500f),
                "PauseThreshold should be 500ms");
            Assert.That(_monitor.ResumeThreshold, Is.EqualTo(400f),
                "ResumeThreshold should be 400ms");
        }

        /// <summary>
        /// Property: OnLatencyStateChanged fires on state changes
        /// </summary>
        [Test]
        public void OnLatencyStateChanged_FiresOnStateChanges()
        {
            // Arrange
            int changeCount = 0;
            LatencyState lastState = LatencyState.Normal;
            
            _monitor.OnLatencyStateChanged += (state) =>
            {
                changeCount++;
                lastState = state;
            };

            // Act - Transition through states
            _monitor.UpdateLatency(100f); // Normal (no change from initial)
            _monitor.UpdateLatency(300f); // Warning
            _monitor.UpdateLatency(600f); // Paused
            _monitor.UpdateLatency(350f); // Back to Warning

            // Assert
            Assert.That(changeCount, Is.EqualTo(3),
                "Should fire 3 times for Normal->Warning->Paused->Warning");
        }

        /// <summary>
        /// Property: OnActionsPaused fires when entering Paused state
        /// </summary>
        [Test]
        public void OnActionsPaused_FiresWhenEnteringPaused()
        {
            // Arrange
            bool pausedFired = false;
            _monitor.OnActionsPaused += () => pausedFired = true;

            // Act
            _monitor.UpdateLatency(600f);

            // Assert
            Assert.That(pausedFired, Is.True,
                "OnActionsPaused should fire when entering Paused state");
        }

        /// <summary>
        /// Property: OnActionsResumed fires when leaving Paused state
        /// </summary>
        [Test]
        public void OnActionsResumed_FiresWhenLeavingPaused()
        {
            // Arrange
            bool resumedFired = false;
            _monitor.OnActionsResumed += () => resumedFired = true;
            
            _monitor.UpdateLatency(600f); // Enter paused

            // Act
            _monitor.UpdateLatency(350f); // Exit paused

            // Assert
            Assert.That(resumedFired, Is.True,
                "OnActionsResumed should fire when leaving Paused state");
        }

        /// <summary>
        /// Property: CurrentLatency reflects last update
        /// </summary>
        [Test]
        [Repeat(100)]
        public void CurrentLatency_ReflectsLastUpdate()
        {
            // Arrange
            float latency = UnityEngine.Random.Range(0f, 1000f);

            // Act
            _monitor.UpdateLatency(latency);

            // Assert
            Assert.That(_monitor.CurrentLatency, Is.EqualTo(latency),
                "CurrentLatency should equal the last updated value");
        }

        /// <summary>
        /// Property: AverageLatency calculates correctly
        /// </summary>
        [Test]
        public void AverageLatency_CalculatesCorrectly()
        {
            // Arrange & Act
            _monitor.UpdateLatency(100f);
            _monitor.UpdateLatency(200f);
            _monitor.UpdateLatency(300f);

            // Assert
            float expectedAverage = (100f + 200f + 300f) / 3f;
            Assert.That(_monitor.AverageLatency, Is.EqualTo(expectedAverage).Within(0.01f),
                "AverageLatency should be the mean of samples");
        }

        /// <summary>
        /// Property: Reset returns to Normal state
        /// </summary>
        [Test]
        public void Reset_ReturnsToNormalState()
        {
            // Arrange
            _monitor.UpdateLatency(600f);
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Paused));

            bool resumedFired = false;
            _monitor.OnActionsResumed += () => resumedFired = true;

            // Act
            _monitor.Reset();

            // Assert
            Assert.That(_monitor.CurrentState, Is.EqualTo(LatencyState.Normal),
                "Reset should return to Normal state");
            Assert.That(_monitor.CurrentLatency, Is.EqualTo(0f),
                "Reset should clear current latency");
            Assert.That(resumedFired, Is.True,
                "Reset from Paused should fire OnActionsResumed");
        }

        /// <summary>
        /// Property: Custom thresholds work correctly
        /// </summary>
        [Test]
        public void CustomThresholds_WorkCorrectly()
        {
            // Arrange
            var customMonitor = new LatencyMonitor(100f, 300f, 200f);

            // Act & Assert
            customMonitor.UpdateLatency(50f);
            Assert.That(customMonitor.CurrentState, Is.EqualTo(LatencyState.Normal));

            customMonitor.UpdateLatency(150f);
            Assert.That(customMonitor.CurrentState, Is.EqualTo(LatencyState.Warning));

            customMonitor.UpdateLatency(350f);
            Assert.That(customMonitor.CurrentState, Is.EqualTo(LatencyState.Paused));

            customMonitor.UpdateLatency(150f); // Below custom resume threshold
            Assert.That(customMonitor.CurrentState, Is.EqualTo(LatencyState.Warning));
        }
    }
}
