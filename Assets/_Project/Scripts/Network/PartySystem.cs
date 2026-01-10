using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EtherDomes.Network
{
    /// <summary>
    /// Manages player parties with support for invites, leadership, and member management.
    /// Maximum party size is 10 players.
    /// </summary>
    public class PartySystem : IPartySystem
    {
        private readonly Dictionary<ulong, Party> _parties = new();
        private readonly Dictionary<ulong, ulong> _playerToParty = new();
        private readonly Dictionary<ulong, List<PartyInvite>> _pendingInvites = new();
        
        private const float INVITE_TIMEOUT_SECONDS = 60f;

        public int MaxPartySize => 10;

        public event Action<ulong, ulong> OnPlayerInvited;
        public event Action<ulong, ulong> OnPlayerJoined;
        public event Action<ulong, ulong> OnPlayerLeft;
        public event Action<ulong, ulong> OnLeaderChanged;
        public event Action<ulong> OnPartyDisbanded;

        private class Party
        {
            public ulong LeaderId;
            public List<ulong> Members = new();
        }

        public bool IsInParty(ulong playerId)
        {
            return _playerToParty.ContainsKey(playerId);
        }

        public ulong[] GetPartyMembers(ulong playerId)
        {
            if (!_playerToParty.TryGetValue(playerId, out ulong leaderId))
                return Array.Empty<ulong>();

            if (!_parties.TryGetValue(leaderId, out var party))
                return Array.Empty<ulong>();

            return party.Members.ToArray();
        }

        public ulong GetPartyLeader(ulong playerId)
        {
            if (!_playerToParty.TryGetValue(playerId, out ulong leaderId))
                return 0;

            return leaderId;
        }

        public int GetPartySize(ulong playerId)
        {
            if (!_playerToParty.TryGetValue(playerId, out ulong leaderId))
                return 0;

            if (!_parties.TryGetValue(leaderId, out var party))
                return 0;

            return party.Members.Count;
        }

        public bool TryInvitePlayer(ulong inviterId, ulong inviteeId)
        {
            // Cannot invite yourself
            if (inviterId == inviteeId)
            {
                Debug.LogWarning("[PartySystem] Cannot invite yourself");
                return false;
            }

            // Check if invitee is already in a party
            if (IsInParty(inviteeId))
            {
                Debug.LogWarning($"[PartySystem] Player {inviteeId} is already in a party");
                return false;
            }

            // Get or create party for inviter
            ulong partyLeaderId = inviterId;
            if (_playerToParty.TryGetValue(inviterId, out ulong existingLeaderId))
            {
                partyLeaderId = existingLeaderId;
                
                // Only leader can invite
                if (partyLeaderId != inviterId)
                {
                    Debug.LogWarning("[PartySystem] Only party leader can invite");
                    return false;
                }
            }

            // Check party size
            if (_parties.TryGetValue(partyLeaderId, out var party))
            {
                if (party.Members.Count >= MaxPartySize)
                {
                    Debug.LogWarning("[PartySystem] Party is full");
                    return false;
                }
            }

            // Create invite
            var invite = new PartyInvite
            {
                InviterId = inviterId,
                InviteeId = inviteeId,
                ExpiresAt = DateTime.UtcNow.AddSeconds(INVITE_TIMEOUT_SECONDS)
            };

            if (!_pendingInvites.ContainsKey(inviteeId))
            {
                _pendingInvites[inviteeId] = new List<PartyInvite>();
            }

            // Remove any existing invite from same party
            _pendingInvites[inviteeId].RemoveAll(i => i.InviterId == inviterId);
            _pendingInvites[inviteeId].Add(invite);

            Debug.Log($"[PartySystem] Player {inviterId} invited {inviteeId} to party");
            OnPlayerInvited?.Invoke(inviterId, inviteeId);
            return true;
        }

        public void AcceptInvite(ulong playerId, ulong partyLeaderId)
        {
            // Find and validate invite
            if (!_pendingInvites.TryGetValue(playerId, out var invites))
            {
                Debug.LogWarning($"[PartySystem] No pending invites for player {playerId}");
                return;
            }

            var invite = invites.FirstOrDefault(i => 
                i.InviterId == partyLeaderId || 
                (_playerToParty.TryGetValue(i.InviterId, out var lid) && lid == partyLeaderId));

            if (invite.InviterId == 0)
            {
                Debug.LogWarning($"[PartySystem] No invite from party {partyLeaderId}");
                return;
            }

            // Check if invite expired
            if (DateTime.UtcNow > invite.ExpiresAt)
            {
                invites.Remove(invite);
                Debug.LogWarning("[PartySystem] Invite has expired");
                return;
            }

            // Remove invite
            invites.Remove(invite);

            // Add player to party
            AddPlayerToParty(playerId, partyLeaderId);
        }

        public void DeclineInvite(ulong playerId, ulong partyLeaderId)
        {
            if (!_pendingInvites.TryGetValue(playerId, out var invites))
                return;

            invites.RemoveAll(i => 
                i.InviterId == partyLeaderId || 
                (_playerToParty.TryGetValue(i.InviterId, out var lid) && lid == partyLeaderId));

            Debug.Log($"[PartySystem] Player {playerId} declined invite from {partyLeaderId}");
        }

        public void LeaveParty(ulong playerId)
        {
            if (!_playerToParty.TryGetValue(playerId, out ulong leaderId))
            {
                Debug.LogWarning($"[PartySystem] Player {playerId} is not in a party");
                return;
            }

            RemovePlayerFromParty(playerId, leaderId);
        }

        public void KickPlayer(ulong leaderId, ulong targetId)
        {
            // Verify leader
            if (!_parties.TryGetValue(leaderId, out var party))
            {
                Debug.LogWarning($"[PartySystem] Player {leaderId} is not a party leader");
                return;
            }

            // Cannot kick yourself
            if (leaderId == targetId)
            {
                Debug.LogWarning("[PartySystem] Leader cannot kick themselves");
                return;
            }

            // Verify target is in party
            if (!party.Members.Contains(targetId))
            {
                Debug.LogWarning($"[PartySystem] Player {targetId} is not in this party");
                return;
            }

            RemovePlayerFromParty(targetId, leaderId);
            Debug.Log($"[PartySystem] Player {targetId} was kicked from party by {leaderId}");
        }

        public void PromoteToLeader(ulong currentLeaderId, ulong newLeaderId)
        {
            if (!_parties.TryGetValue(currentLeaderId, out var party))
            {
                Debug.LogWarning($"[PartySystem] Player {currentLeaderId} is not a party leader");
                return;
            }

            if (!party.Members.Contains(newLeaderId))
            {
                Debug.LogWarning($"[PartySystem] Player {newLeaderId} is not in this party");
                return;
            }

            // Transfer leadership
            party.LeaderId = newLeaderId;
            _parties.Remove(currentLeaderId);
            _parties[newLeaderId] = party;

            // Update all member mappings
            foreach (var member in party.Members)
            {
                _playerToParty[member] = newLeaderId;
            }

            Debug.Log($"[PartySystem] Leadership transferred from {currentLeaderId} to {newLeaderId}");
            OnLeaderChanged?.Invoke(currentLeaderId, newLeaderId);
        }

        /// <summary>
        /// Handles player disconnect - promotes new leader or disbands party.
        /// </summary>
        public void HandlePlayerDisconnect(ulong playerId)
        {
            if (!_playerToParty.TryGetValue(playerId, out ulong leaderId))
                return;

            if (!_parties.TryGetValue(leaderId, out var party))
                return;

            // If disconnecting player is leader, promote someone else
            if (playerId == leaderId)
            {
                var remainingMembers = party.Members.Where(m => m != playerId).ToList();
                
                if (remainingMembers.Count > 0)
                {
                    // Promote first remaining member
                    ulong newLeader = remainingMembers[0];
                    
                    // Remove old leader from party first
                    party.Members.Remove(playerId);
                    _playerToParty.Remove(playerId);
                    
                    // Transfer leadership
                    party.LeaderId = newLeader;
                    _parties.Remove(leaderId);
                    _parties[newLeader] = party;

                    // Update all member mappings
                    foreach (var member in party.Members)
                    {
                        _playerToParty[member] = newLeader;
                    }

                    Debug.Log($"[PartySystem] Leader {playerId} disconnected, promoted {newLeader}");
                    OnLeaderChanged?.Invoke(leaderId, newLeader);
                    OnPlayerLeft?.Invoke(newLeader, playerId);
                }
                else
                {
                    // No one left, disband
                    DisbandParty(leaderId);
                }
            }
            else
            {
                // Regular member disconnect
                RemovePlayerFromParty(playerId, leaderId);
            }
        }

        private void AddPlayerToParty(ulong playerId, ulong leaderId)
        {
            // Create party if it doesn't exist
            if (!_parties.TryGetValue(leaderId, out var party))
            {
                party = new Party
                {
                    LeaderId = leaderId,
                    Members = new List<ulong> { leaderId }
                };
                _parties[leaderId] = party;
                _playerToParty[leaderId] = leaderId;
            }

            // Check size limit
            if (party.Members.Count >= MaxPartySize)
            {
                Debug.LogWarning("[PartySystem] Party is full");
                return;
            }

            // Add member
            party.Members.Add(playerId);
            _playerToParty[playerId] = leaderId;

            Debug.Log($"[PartySystem] Player {playerId} joined party of {leaderId}");
            OnPlayerJoined?.Invoke(leaderId, playerId);
        }

        private void RemovePlayerFromParty(ulong playerId, ulong leaderId)
        {
            if (!_parties.TryGetValue(leaderId, out var party))
                return;

            party.Members.Remove(playerId);
            _playerToParty.Remove(playerId);

            OnPlayerLeft?.Invoke(leaderId, playerId);

            // Check if party should disband (less than 2 members)
            if (party.Members.Count < 2)
            {
                DisbandParty(leaderId);
            }
        }

        private void DisbandParty(ulong leaderId)
        {
            if (!_parties.TryGetValue(leaderId, out var party))
                return;

            // Remove all member mappings
            foreach (var member in party.Members)
            {
                _playerToParty.Remove(member);
            }

            _parties.Remove(leaderId);

            Debug.Log($"[PartySystem] Party {leaderId} disbanded");
            OnPartyDisbanded?.Invoke(leaderId);
        }

        /// <summary>
        /// Cleans up expired invites.
        /// </summary>
        public void CleanupExpiredInvites()
        {
            var now = DateTime.UtcNow;
            
            foreach (var kvp in _pendingInvites.ToList())
            {
                kvp.Value.RemoveAll(i => now > i.ExpiresAt);
                
                if (kvp.Value.Count == 0)
                {
                    _pendingInvites.Remove(kvp.Key);
                }
            }
        }

        /// <summary>
        /// Gets pending invites for a player.
        /// </summary>
        public PartyInvite[] GetPendingInvites(ulong playerId)
        {
            if (!_pendingInvites.TryGetValue(playerId, out var invites))
                return Array.Empty<PartyInvite>();

            return invites.Where(i => DateTime.UtcNow <= i.ExpiresAt).ToArray();
        }
    }
}
