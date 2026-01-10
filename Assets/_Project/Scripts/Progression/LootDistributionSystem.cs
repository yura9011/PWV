using System;
using System.Collections.Generic;
using EtherDomes.Data;
using UnityEngine;

namespace EtherDomes.Progression
{
    /// <summary>
    /// Manages loot distribution using Need/Greed/Pass system.
    /// 
    /// Priority: Need > Greed > Pass
    /// Tiebreaker: Random roll 1-100
    /// Timeout: 30 seconds (auto-pass)
    /// 
    /// Bad Luck Protection:
    /// - Activates after 10 failed attempts per rarity
    /// - +5% bonus per additional attempt
    /// 
    /// Requirements: 4.1, 4.2, 4.3, 4.4, 4.5
    /// </summary>
    public class LootDistributionSystem : ILootDistributionSystem
    {
        public const float DEFAULT_ROLL_TIMEOUT = 30f;
        public const int BAD_LUCK_PROTECTION_THRESHOLD = 10;
        public const float BAD_LUCK_PROTECTION_BONUS_PER_ATTEMPT = 0.05f;

        private readonly Dictionary<string, LootRollSession> _activeSessions = new();
        private readonly Dictionary<ulong, Dictionary<ItemRarity, int>> _failedAttempts = new();

        public float RollTimeout => DEFAULT_ROLL_TIMEOUT;

        public event Action<LootRollSession> OnRollStarted;
        public event Action<string, ulong, LootRollResult> OnRollSubmitted;
        public event Action<string, ulong?, ItemData> OnRollFinalized;

        public string StartNeedGreedRoll(ItemData item, List<ulong> eligiblePlayers)
        {
            if (item == null || eligiblePlayers == null || eligiblePlayers.Count == 0)
                return null;

            var session = new LootRollSession
            {
                SessionId = Guid.NewGuid().ToString(),
                Item = item,
                EligiblePlayers = new List<ulong>(eligiblePlayers),
                Rolls = new Dictionary<ulong, LootRollResult>(),
                StartTime = DateTime.UtcNow,
                TimeoutSeconds = DEFAULT_ROLL_TIMEOUT,
                IsFinalized = false,
                WinnerId = null
            };

            _activeSessions[session.SessionId] = session;
            
            Debug.Log($"[LootDistribution] Started roll for {item.ItemName} with {eligiblePlayers.Count} players");
            OnRollStarted?.Invoke(session);

            return session.SessionId;
        }

        public void SubmitRoll(string sessionId, ulong playerId, LootRollType rollType)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                Debug.LogWarning($"[LootDistribution] Session {sessionId} not found");
                return;
            }

            if (session.IsFinalized)
            {
                Debug.LogWarning($"[LootDistribution] Session {sessionId} already finalized");
                return;
            }

            if (!session.EligiblePlayers.Contains(playerId))
            {
                Debug.LogWarning($"[LootDistribution] Player {playerId} not eligible for session {sessionId}");
                return;
            }

            if (session.Rolls.ContainsKey(playerId))
            {
                Debug.LogWarning($"[LootDistribution] Player {playerId} already rolled");
                return;
            }

            // Generate random roll value (1-100)
            int rollValue = UnityEngine.Random.Range(1, 101);

            // Apply bad luck protection bonus
            float blpBonus = GetBadLuckProtectionBonus(playerId, session.Item.Rarity);
            if (blpBonus > 0 && rollType != LootRollType.Pass)
            {
                rollValue = Mathf.Min(100, rollValue + (int)(blpBonus * 100));
            }

            var result = new LootRollResult
            {
                PlayerId = playerId,
                RollType = rollType,
                RollValue = rollValue,
                Won = false
            };

            session.Rolls[playerId] = result;
            
            Debug.Log($"[LootDistribution] Player {playerId} rolled {rollType} ({rollValue})");
            OnRollSubmitted?.Invoke(sessionId, playerId, result);

            // Check if all players have rolled
            if (session.Rolls.Count >= session.EligiblePlayers.Count)
            {
                FinalizeRolls(sessionId);
            }
        }

        public ulong? FinalizeRolls(string sessionId)
        {
            if (!_activeSessions.TryGetValue(sessionId, out var session))
                return null;

            if (session.IsFinalized)
                return session.WinnerId;

            session.IsFinalized = true;

            // Auto-pass for players who didn't roll
            foreach (var playerId in session.EligiblePlayers)
            {
                if (!session.Rolls.ContainsKey(playerId))
                {
                    session.Rolls[playerId] = new LootRollResult
                    {
                        PlayerId = playerId,
                        RollType = LootRollType.Pass,
                        RollValue = 0,
                        Won = false
                    };
                }
            }

            // Determine winner: Need > Greed > Pass, then by roll value
            ulong? winnerId = null;
            LootRollType winningRollType = LootRollType.Pass;
            int winningRollValue = -1;

            foreach (var roll in session.Rolls.Values)
            {
                if (roll.RollType == LootRollType.Pass)
                    continue;

                bool isHigherPriority = (int)roll.RollType > (int)winningRollType;
                bool isSamePriorityHigherRoll = roll.RollType == winningRollType && roll.RollValue > winningRollValue;

                if (isHigherPriority || isSamePriorityHigherRoll)
                {
                    winnerId = roll.PlayerId;
                    winningRollType = roll.RollType;
                    winningRollValue = roll.RollValue;
                }
            }

            session.WinnerId = winnerId;

            // Update roll results and bad luck protection
            foreach (var playerId in session.EligiblePlayers)
            {
                bool won = winnerId.HasValue && winnerId.Value == playerId;
                
                if (session.Rolls.TryGetValue(playerId, out var roll))
                {
                    var updatedRoll = roll;
                    updatedRoll.Won = won;
                    session.Rolls[playerId] = updatedRoll;
                }

                // Record attempt for bad luck protection
                if (roll.RollType != LootRollType.Pass)
                {
                    RecordLootAttempt(playerId, session.Item.Rarity, won);
                }
            }

            Debug.Log($"[LootDistribution] Finalized roll for {session.Item.ItemName}. Winner: {winnerId?.ToString() ?? "None"}");
            OnRollFinalized?.Invoke(sessionId, winnerId, session.Item);

            return winnerId;
        }

        public float GetBadLuckProtectionBonus(ulong playerId, ItemRarity rarity)
        {
            if (!_failedAttempts.TryGetValue(playerId, out var rarityAttempts))
                return 0f;

            if (!rarityAttempts.TryGetValue(rarity, out int attempts))
                return 0f;

            // Bad luck protection activates after 10 attempts
            if (attempts <= BAD_LUCK_PROTECTION_THRESHOLD)
                return 0f;

            // +5% per attempt after threshold
            int bonusAttempts = attempts - BAD_LUCK_PROTECTION_THRESHOLD;
            return bonusAttempts * BAD_LUCK_PROTECTION_BONUS_PER_ATTEMPT;
        }

        public void RecordLootAttempt(ulong playerId, ItemRarity rarity, bool won)
        {
            if (!_failedAttempts.ContainsKey(playerId))
            {
                _failedAttempts[playerId] = new Dictionary<ItemRarity, int>();
            }

            if (won)
            {
                // Reset counter on win
                _failedAttempts[playerId][rarity] = 0;
            }
            else
            {
                // Increment failed attempts
                if (!_failedAttempts[playerId].ContainsKey(rarity))
                {
                    _failedAttempts[playerId][rarity] = 0;
                }
                _failedAttempts[playerId][rarity]++;
            }
        }

        public LootRollSession GetSession(string sessionId)
        {
            return _activeSessions.TryGetValue(sessionId, out var session) ? session : null;
        }

        /// <summary>
        /// Gets the number of failed attempts for a player and rarity.
        /// Used for testing.
        /// </summary>
        public int GetFailedAttempts(ulong playerId, ItemRarity rarity)
        {
            if (!_failedAttempts.TryGetValue(playerId, out var rarityAttempts))
                return 0;

            return rarityAttempts.TryGetValue(rarity, out int attempts) ? attempts : 0;
        }

        /// <summary>
        /// Cleans up expired sessions.
        /// </summary>
        public void CleanupExpiredSessions()
        {
            var expiredSessions = new List<string>();
            var now = DateTime.UtcNow;

            foreach (var kvp in _activeSessions)
            {
                if (!kvp.Value.IsFinalized)
                {
                    var elapsed = (now - kvp.Value.StartTime).TotalSeconds;
                    if (elapsed >= kvp.Value.TimeoutSeconds)
                    {
                        FinalizeRolls(kvp.Key);
                        expiredSessions.Add(kvp.Key);
                    }
                }
            }
        }
    }
}
