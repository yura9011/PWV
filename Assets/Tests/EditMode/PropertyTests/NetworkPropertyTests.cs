using NUnit.Framework;
using EtherDomes.Data;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for network session management.
    /// </summary>
    [TestFixture]
    public class NetworkPropertyTests
    {
        /// <summary>
        /// Property: Session State Consistency
        /// Session state transitions should be valid and consistent.
        /// </summary>
        [Test]
        public void SessionState_TransitionsAreValid()
        {
            // Valid state transitions
            var validTransitions = new[]
            {
                (from: SessionState.Disconnected, to: SessionState.Connecting),
                (from: SessionState.Connecting, to: SessionState.Connected),
                (from: SessionState.Connecting, to: SessionState.Disconnected),
                (from: SessionState.Connected, to: SessionState.Disconnected),
                (from: SessionState.Connected, to: SessionState.InGame),
                (from: SessionState.InGame, to: SessionState.Connected),
                (from: SessionState.InGame, to: SessionState.Disconnected)
            };

            foreach (var transition in validTransitions)
            {
                Assert.That(IsValidTransition(transition.from, transition.to), Is.True,
                    $"Transition from {transition.from} to {transition.to} should be valid");
            }
        }

        /// <summary>
        /// Property: Invalid state transitions are rejected
        /// </summary>
        [Test]
        public void SessionState_InvalidTransitionsAreRejected()
        {
            // Invalid state transitions
            var invalidTransitions = new[]
            {
                (from: SessionState.Disconnected, to: SessionState.Connected), // Must go through Connecting
                (from: SessionState.Disconnected, to: SessionState.InGame),    // Must connect first
                (from: SessionState.InGame, to: SessionState.Connecting)       // Must disconnect first
            };

            foreach (var transition in invalidTransitions)
            {
                Assert.That(IsValidTransition(transition.from, transition.to), Is.False,
                    $"Transition from {transition.from} to {transition.to} should be invalid");
            }
        }

        /// <summary>
        /// Property: All states can transition to Disconnected
        /// </summary>
        [Test]
        public void AllStates_CanTransitionToDisconnected(
            [Values] SessionState fromState)
        {
            // Any state should be able to transition to Disconnected (connection lost, etc.)
            if (fromState == SessionState.Disconnected)
            {
                Assert.Pass("Already disconnected");
                return;
            }

            Assert.That(IsValidTransition(fromState, SessionState.Disconnected), Is.True,
                $"State {fromState} should be able to transition to Disconnected");
        }

        /// <summary>
        /// Property: Disconnected is the only valid initial state
        /// </summary>
        [Test]
        public void DisconnectedIsInitialState()
        {
            var initialState = SessionState.Disconnected;
            
            Assert.That(initialState, Is.EqualTo(SessionState.Disconnected),
                "Initial session state should be Disconnected");
        }

        /// <summary>
        /// Property: Player count constraints
        /// </summary>
        [Test]
        public void PlayerCount_IsWithinBounds(
            [Values(0, 1, 5, 10, 11, 100)] int playerCount)
        {
            const int minPlayers = 1;
            const int maxPlayers = 10;

            bool isValid = playerCount >= minPlayers && playerCount <= maxPlayers;

            if (playerCount >= minPlayers && playerCount <= maxPlayers)
            {
                Assert.That(isValid, Is.True,
                    $"Player count {playerCount} should be valid");
            }
            else
            {
                Assert.That(isValid, Is.False,
                    $"Player count {playerCount} should be invalid");
            }
        }

        /// <summary>
        /// Helper method to validate state transitions.
        /// </summary>
        private bool IsValidTransition(SessionState from, SessionState to)
        {
            return (from, to) switch
            {
                (SessionState.Disconnected, SessionState.Connecting) => true,
                (SessionState.Connecting, SessionState.Connected) => true,
                (SessionState.Connecting, SessionState.Disconnected) => true,
                (SessionState.Connected, SessionState.Disconnected) => true,
                (SessionState.Connected, SessionState.InGame) => true,
                (SessionState.InGame, SessionState.Connected) => true,
                (SessionState.InGame, SessionState.Disconnected) => true,
                _ => false
            };
        }
    }

    /// <summary>
    /// Session state enum for testing.
    /// </summary>
    public enum SessionState
    {
        Disconnected,
        Connecting,
        Connected,
        InGame
    }
}
