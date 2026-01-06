using System;
using NUnit.Framework;
using EtherDomes.Network;

namespace EtherDomes.Tests.PropertyTests
{
    /// <summary>
    /// Property-based tests for the Party System.
    /// </summary>
    [TestFixture]
    public class PartyPropertyTests
    {
        private PartySystem _partySystem;

        [SetUp]
        public void SetUp()
        {
            _partySystem = new PartySystem();
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 33: Party Size Limits
        /// For any party, GetPartySize SHALL return value between 2 and MaxPartySize (10).
        /// Validates: Requirements 14.1
        /// </summary>
        [Test]
        public void PartySizeLimits_SizeBetween2And10()
        {
            // Arrange
            ulong leaderId = 1;
            
            // Create party by inviting and accepting
            for (ulong i = 2; i <= 11; i++)
            {
                _partySystem.TryInvitePlayer(leaderId, i);
                _partySystem.AcceptInvite(i, leaderId);
            }

            // Act
            int partySize = _partySystem.GetPartySize(leaderId);

            // Assert
            Assert.That(partySize, Is.GreaterThanOrEqualTo(2), 
                "Party size should be at least 2 (leader + 1 member)");
            Assert.That(partySize, Is.LessThanOrEqualTo(_partySystem.MaxPartySize), 
                $"Party size should not exceed MaxPartySize ({_partySystem.MaxPartySize})");
            Assert.That(_partySystem.MaxPartySize, Is.EqualTo(10), 
                "MaxPartySize should be 10");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 33: Party Size Limits
        /// Party cannot exceed 10 members.
        /// Validates: Requirements 14.1
        /// </summary>
        [Test]
        public void PartySizeLimits_CannotExceed10Members()
        {
            // Arrange
            ulong leaderId = 1;
            
            // Fill party to max
            for (ulong i = 2; i <= 10; i++)
            {
                _partySystem.TryInvitePlayer(leaderId, i);
                _partySystem.AcceptInvite(i, leaderId);
            }

            // Act - Try to add 11th member
            bool inviteResult = _partySystem.TryInvitePlayer(leaderId, 11);

            // Assert
            Assert.That(_partySystem.GetPartySize(leaderId), Is.EqualTo(10), 
                "Party should have exactly 10 members");
            Assert.That(inviteResult, Is.False, 
                "Should not be able to invite when party is full");
        }

        /// <summary>
        /// Feature: mvp-10-features, Property 34: Party Leader Succession
        /// For any party where leader disconnects, GetPartyLeader SHALL return 
        /// a different connected member.
        /// Validates: Requirements 14.5
        /// </summary>
        [Test]
        public void PartyLeaderSuccession_NewLeaderOnDisconnect()
        {
            // Arrange
            ulong originalLeader = 1;
            ulong member1 = 2;
            ulong member2 = 3;
            
            _partySystem.TryInvitePlayer(originalLeader, member1);
            _partySystem.AcceptInvite(member1, originalLeader);
            _partySystem.TryInvitePlayer(originalLeader, member2);
            _partySystem.AcceptInvite(member2, originalLeader);

            ulong leaderChangedFrom = 0;
            ulong leaderChangedTo = 0;
            _partySystem.OnLeaderChanged += (from, to) =>
            {
                leaderChangedFrom = from;
                leaderChangedTo = to;
            };

            // Act - Leader disconnects
            _partySystem.HandlePlayerDisconnect(originalLeader);

            // Assert
            ulong newLeader = _partySystem.GetPartyLeader(member1);
            Assert.That(newLeader, Is.Not.EqualTo(originalLeader), 
                "New leader should be different from disconnected leader");
            Assert.That(newLeader, Is.Not.EqualTo(0UL), 
                "Party should have a new leader");
            Assert.That(_partySystem.IsInParty(member1), Is.True, 
                "Remaining members should still be in party");
            Assert.That(leaderChangedFrom, Is.EqualTo(originalLeader), 
                "OnLeaderChanged should report old leader");
            Assert.That(leaderChangedTo, Is.EqualTo(newLeader), 
                "OnLeaderChanged should report new leader");
        }

        /// <summary>
        /// Property: Party disbands when only one member remains
        /// </summary>
        [Test]
        public void PartyDisbands_WhenOnlyOneMemberRemains()
        {
            // Arrange
            ulong leaderId = 1;
            ulong memberId = 2;
            
            _partySystem.TryInvitePlayer(leaderId, memberId);
            _partySystem.AcceptInvite(memberId, leaderId);

            bool disbanded = false;
            _partySystem.OnPartyDisbanded += (id) => disbanded = true;

            // Act - Member leaves
            _partySystem.LeaveParty(memberId);

            // Assert
            Assert.That(disbanded, Is.True, "Party should disband when only leader remains");
            Assert.That(_partySystem.IsInParty(leaderId), Is.False, 
                "Leader should no longer be in a party");
        }

        /// <summary>
        /// Property: Cannot invite player already in a party
        /// </summary>
        [Test]
        public void CannotInvite_PlayerAlreadyInParty()
        {
            // Arrange
            ulong leader1 = 1;
            ulong leader2 = 10;
            ulong player = 2;
            
            // Player joins first party
            _partySystem.TryInvitePlayer(leader1, player);
            _partySystem.AcceptInvite(player, leader1);

            // Act - Try to invite to second party
            bool result = _partySystem.TryInvitePlayer(leader2, player);

            // Assert
            Assert.That(result, Is.False, 
                "Should not be able to invite player already in a party");
        }

        /// <summary>
        /// Property: Only leader can invite
        /// </summary>
        [Test]
        public void OnlyLeaderCanInvite()
        {
            // Arrange
            ulong leaderId = 1;
            ulong memberId = 2;
            ulong newPlayerId = 3;
            
            _partySystem.TryInvitePlayer(leaderId, memberId);
            _partySystem.AcceptInvite(memberId, leaderId);

            // Act - Non-leader tries to invite
            bool result = _partySystem.TryInvitePlayer(memberId, newPlayerId);

            // Assert
            Assert.That(result, Is.False, 
                "Non-leader should not be able to invite");
        }

        /// <summary>
        /// Property: GetPartyMembers returns all members including leader
        /// </summary>
        [Test]
        public void GetPartyMembers_ReturnsAllMembersIncludingLeader()
        {
            // Arrange
            ulong leaderId = 1;
            ulong member1 = 2;
            ulong member2 = 3;
            
            _partySystem.TryInvitePlayer(leaderId, member1);
            _partySystem.AcceptInvite(member1, leaderId);
            _partySystem.TryInvitePlayer(leaderId, member2);
            _partySystem.AcceptInvite(member2, leaderId);

            // Act
            var members = _partySystem.GetPartyMembers(leaderId);

            // Assert
            Assert.That(members, Contains.Item(leaderId), "Members should include leader");
            Assert.That(members, Contains.Item(member1), "Members should include member1");
            Assert.That(members, Contains.Item(member2), "Members should include member2");
            Assert.That(members.Length, Is.EqualTo(3), "Should have exactly 3 members");
        }

        /// <summary>
        /// Property: PromoteToLeader transfers leadership correctly
        /// </summary>
        [Test]
        public void PromoteToLeader_TransfersLeadershipCorrectly()
        {
            // Arrange
            ulong originalLeader = 1;
            ulong newLeader = 2;
            ulong member = 3;
            
            _partySystem.TryInvitePlayer(originalLeader, newLeader);
            _partySystem.AcceptInvite(newLeader, originalLeader);
            _partySystem.TryInvitePlayer(originalLeader, member);
            _partySystem.AcceptInvite(member, originalLeader);

            // Act
            _partySystem.PromoteToLeader(originalLeader, newLeader);

            // Assert
            Assert.That(_partySystem.GetPartyLeader(originalLeader), Is.EqualTo(newLeader), 
                "Old leader should report new leader");
            Assert.That(_partySystem.GetPartyLeader(newLeader), Is.EqualTo(newLeader), 
                "New leader should be party leader");
            Assert.That(_partySystem.GetPartyLeader(member), Is.EqualTo(newLeader), 
                "Member should report new leader");
        }

        /// <summary>
        /// Property: KickPlayer removes player from party
        /// </summary>
        [Test]
        public void KickPlayer_RemovesPlayerFromParty()
        {
            // Arrange
            ulong leaderId = 1;
            ulong memberId = 2;
            ulong member2Id = 3;
            
            _partySystem.TryInvitePlayer(leaderId, memberId);
            _partySystem.AcceptInvite(memberId, leaderId);
            _partySystem.TryInvitePlayer(leaderId, member2Id);
            _partySystem.AcceptInvite(member2Id, leaderId);

            // Act
            _partySystem.KickPlayer(leaderId, memberId);

            // Assert
            Assert.That(_partySystem.IsInParty(memberId), Is.False, 
                "Kicked player should not be in party");
            Assert.That(_partySystem.GetPartySize(leaderId), Is.EqualTo(2), 
                "Party size should decrease by 1");
        }

        /// <summary>
        /// Property: Cannot kick yourself
        /// </summary>
        [Test]
        public void CannotKickYourself()
        {
            // Arrange
            ulong leaderId = 1;
            ulong memberId = 2;
            
            _partySystem.TryInvitePlayer(leaderId, memberId);
            _partySystem.AcceptInvite(memberId, leaderId);

            int sizeBefore = _partySystem.GetPartySize(leaderId);

            // Act
            _partySystem.KickPlayer(leaderId, leaderId);

            // Assert
            Assert.That(_partySystem.GetPartySize(leaderId), Is.EqualTo(sizeBefore), 
                "Party size should not change when trying to kick self");
            Assert.That(_partySystem.IsInParty(leaderId), Is.True, 
                "Leader should still be in party");
        }

        /// <summary>
        /// Property: Events fire correctly on party changes
        /// </summary>
        [Test]
        public void Events_FireCorrectlyOnPartyChanges()
        {
            // Arrange
            ulong leaderId = 1;
            ulong memberId = 2;
            
            bool inviteFired = false;
            bool joinFired = false;
            bool leftFired = false;
            
            _partySystem.OnPlayerInvited += (_, _) => inviteFired = true;
            _partySystem.OnPlayerJoined += (_, _) => joinFired = true;
            _partySystem.OnPlayerLeft += (_, _) => leftFired = true;

            // Act
            _partySystem.TryInvitePlayer(leaderId, memberId);
            _partySystem.AcceptInvite(memberId, leaderId);
            _partySystem.LeaveParty(memberId);

            // Assert
            Assert.That(inviteFired, Is.True, "OnPlayerInvited should fire");
            Assert.That(joinFired, Is.True, "OnPlayerJoined should fire");
            Assert.That(leftFired, Is.True, "OnPlayerLeft should fire");
        }
    }
}
