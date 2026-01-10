using System;

namespace EtherDomes.Network
{
    /// <summary>
    /// Interface for managing player parties.
    /// Handles party creation, invites, and member management.
    /// </summary>
    public interface IPartySystem
    {
        /// <summary>
        /// Checks if a player is currently in a party.
        /// </summary>
        bool IsInParty(ulong playerId);

        /// <summary>
        /// Gets all members of a player's party.
        /// </summary>
        ulong[] GetPartyMembers(ulong playerId);

        /// <summary>
        /// Gets the leader of a player's party.
        /// </summary>
        ulong GetPartyLeader(ulong playerId);

        /// <summary>
        /// Gets the current size of a player's party.
        /// </summary>
        int GetPartySize(ulong playerId);

        /// <summary>
        /// Attempts to invite a player to the inviter's party.
        /// </summary>
        /// <returns>True if invite was sent successfully.</returns>
        bool TryInvitePlayer(ulong inviterId, ulong inviteeId);

        /// <summary>
        /// Accepts a pending party invite.
        /// </summary>
        void AcceptInvite(ulong playerId, ulong partyLeaderId);

        /// <summary>
        /// Declines a pending party invite.
        /// </summary>
        void DeclineInvite(ulong playerId, ulong partyLeaderId);

        /// <summary>
        /// Removes a player from their current party.
        /// </summary>
        void LeaveParty(ulong playerId);

        /// <summary>
        /// Kicks a player from the party (leader only).
        /// </summary>
        void KickPlayer(ulong leaderId, ulong targetId);

        /// <summary>
        /// Promotes a party member to leader.
        /// </summary>
        void PromoteToLeader(ulong currentLeaderId, ulong newLeaderId);

        /// <summary>
        /// Event fired when a player is invited to a party.
        /// Args: inviterId, inviteeId
        /// </summary>
        event Action<ulong, ulong> OnPlayerInvited;

        /// <summary>
        /// Event fired when a player joins a party.
        /// Args: partyLeaderId, playerId
        /// </summary>
        event Action<ulong, ulong> OnPlayerJoined;

        /// <summary>
        /// Event fired when a player leaves a party.
        /// Args: partyLeaderId, playerId
        /// </summary>
        event Action<ulong, ulong> OnPlayerLeft;

        /// <summary>
        /// Event fired when party leadership changes.
        /// Args: partyLeaderId, newLeaderId
        /// </summary>
        event Action<ulong, ulong> OnLeaderChanged;

        /// <summary>
        /// Event fired when a party is disbanded.
        /// Args: partyLeaderId
        /// </summary>
        event Action<ulong> OnPartyDisbanded;

        /// <summary>
        /// Maximum number of players in a party (10).
        /// </summary>
        int MaxPartySize { get; }
    }

    /// <summary>
    /// Represents a pending party invite.
    /// </summary>
    public struct PartyInvite
    {
        public ulong InviterId;
        public ulong InviteeId;
        public DateTime ExpiresAt;
    }
}
