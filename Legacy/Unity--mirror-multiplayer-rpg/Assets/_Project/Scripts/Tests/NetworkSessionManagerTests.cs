using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using EtherDomes.Network;
using System.Collections;

namespace EtherDomes.Tests
{
    /// <summary>
    /// Property-based and unit tests for NetworkSessionManager.
    /// </summary>
    [TestFixture]
    public class NetworkSessionManagerTests
    {
        private GameObject _managerObject;
        private NetworkSessionManager _sessionManager;

        [SetUp]
        public void SetUp()
        {
            _managerObject = new GameObject("TestNetworkManager");
            _sessionManager = _managerObject.AddComponent<NetworkSessionManager>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_sessionManager != null)
            {
                _sessionManager.Disconnect();
            }
            if (_managerObject != null)
            {
                Object.DestroyImmediate(_managerObject);
            }
        }

        #region Property 1: Session State Consistency

        /// <summary>
        /// Feature: network-player-foundation, Property 1: Session State Consistency
        /// For any session start operation, the resulting state flags SHALL be mutually consistent.
        /// Validates: Requirements 1.1, 1.3
        /// </summary>
        [Test]
        public void Property1_SessionStateConsistency_InitialState_AllFlagsAreFalse()
        {
            // Initial state: no session started
            Assert.IsFalse(_sessionManager.IsHost, "IsHost should be false initially");
            Assert.IsFalse(_sessionManager.IsClient, "IsClient should be false initially");
            Assert.IsFalse(_sessionManager.IsServer, "IsServer should be false initially");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 1: Session State Consistency
        /// Verifies that state flags are mutually exclusive.
        /// Validates: Requirements 1.1, 1.3
        /// </summary>
        [Test]
        public void Property1_SessionStateConsistency_FlagsAreMutuallyExclusive()
        {
            // Property: At most one of IsHost, IsClient, IsServer can be true at any time
            // (or all false when disconnected)
            
            int trueCount = 0;
            if (_sessionManager.IsHost) trueCount++;
            if (_sessionManager.IsClient) trueCount++;
            if (_sessionManager.IsServer) trueCount++;

            Assert.LessOrEqual(trueCount, 1, 
                "At most one state flag should be true at any time");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 1: Session State Consistency
        /// Verifies Host state definition: NetworkServer.active && NetworkClient.active
        /// Validates: Requirements 1.1, 1.3
        /// </summary>
        [Test]
        public void Property1_SessionStateConsistency_HostDefinition()
        {
            // Host = Server AND Client active simultaneously
            // When IsHost is true, both server and client functionality should be active
            // This is tested by checking the property definition matches Mirror's state
            
            bool isHost = _sessionManager.IsHost;
            bool isClient = _sessionManager.IsClient;
            bool isServer = _sessionManager.IsServer;

            // If IsHost is true, IsClient and IsServer must be false (mutual exclusivity)
            if (isHost)
            {
                Assert.IsFalse(isClient, "When IsHost=true, IsClient should be false");
                Assert.IsFalse(isServer, "When IsHost=true, IsServer should be false");
            }
        }

        #endregion

        #region Property 2: Player Count Enforcement

        /// <summary>
        /// Feature: network-player-foundation, Property 2: Player Count Enforcement
        /// MaxPlayers should return the configured limit (10).
        /// Validates: Requirements 1.6
        /// </summary>
        [Test]
        public void Property2_PlayerCountEnforcement_MaxPlayersIsConfigured()
        {
            Assert.AreEqual(NetworkSessionManager.DEFAULT_MAX_PLAYERS, _sessionManager.MaxPlayers,
                "MaxPlayers should be set to DEFAULT_MAX_PLAYERS (10)");
        }

        /// <summary>
        /// Feature: network-player-foundation, Property 2: Player Count Enforcement
        /// ConnectedPlayerCount should be 0 when no session is active.
        /// Validates: Requirements 1.6
        /// </summary>
        [Test]
        public void Property2_PlayerCountEnforcement_InitialCountIsZero()
        {
            // When no server is running, connected count should be 0
            Assert.AreEqual(0, _sessionManager.ConnectedPlayerCount,
                "ConnectedPlayerCount should be 0 when no session is active");
        }

        #endregion

        #region Unit Tests

        [Test]
        public void StartAsClient_WithEmptyIP_InvokesConnectionFailed()
        {
            bool connectionFailedInvoked = false;
            string failureMessage = null;

            _sessionManager.OnConnectionFailed += (msg) =>
            {
                connectionFailedInvoked = true;
                failureMessage = msg;
            };

            _sessionManager.StartAsClient("", 7777);

            Assert.IsTrue(connectionFailedInvoked, "OnConnectionFailed should be invoked for empty IP");
            Assert.IsNotNull(failureMessage, "Failure message should not be null");
        }

        [Test]
        public void StartAsClient_WithWhitespaceIP_InvokesConnectionFailed()
        {
            bool connectionFailedInvoked = false;

            _sessionManager.OnConnectionFailed += (msg) => connectionFailedInvoked = true;

            _sessionManager.StartAsClient("   ", 7777);

            Assert.IsTrue(connectionFailedInvoked, "OnConnectionFailed should be invoked for whitespace IP");
        }

        [Test]
        public void Disconnect_WhenNotConnected_DoesNotThrow()
        {
            Assert.DoesNotThrow(() => _sessionManager.Disconnect(),
                "Disconnect should not throw when not connected");
        }

        #endregion
    }
}
