using System;
using System.Collections.Generic;
using UnityEngine;

namespace EtherDomes.World
{
    /// <summary>
    /// Manages the guild base, furniture placement, and trophies.
    /// </summary>
    public class GuildBaseSystem : MonoBehaviour, IGuildBaseSystem
    {
        // Players in guild base
        private readonly HashSet<ulong> _playersInBase = new();
        
        // Placed furniture
        private readonly Dictionary<string, FurnitureInstance> _furniture = new();
        
        // Unlocked trophies
        private readonly HashSet<string> _unlockedTrophies = new();

        public event Action<string> OnTrophyUnlocked;
        public event Action<ulong> OnPlayerEntered;
        public event Action<ulong> OnPlayerLeft;
        public event Action<FurnitureInstance> OnFurniturePlaced;
        public event Action<string> OnFurnitureRemoved;

        public void Enter(ulong playerId)
        {
            if (_playersInBase.Add(playerId))
            {
                Debug.Log($"[GuildBaseSystem] Player {playerId} entered guild base");
                OnPlayerEntered?.Invoke(playerId);
            }
        }

        public void Leave(ulong playerId)
        {
            if (_playersInBase.Remove(playerId))
            {
                Debug.Log($"[GuildBaseSystem] Player {playerId} left guild base");
                OnPlayerLeft?.Invoke(playerId);
            }
        }

        public void PlaceFurniture(FurnitureData furniture, Vector3 position, Quaternion rotation)
        {
            if (furniture == null)
            {
                Debug.LogError("[GuildBaseSystem] Cannot place null furniture");
                return;
            }

            var instance = new FurnitureInstance
            {
                InstanceId = Guid.NewGuid().ToString(),
                Data = furniture,
                Position = position,
                Rotation = rotation
            };

            _furniture[instance.InstanceId] = instance;
            Debug.Log($"[GuildBaseSystem] Placed furniture: {furniture.FurnitureName} at {position}");
            OnFurniturePlaced?.Invoke(instance);
        }

        public void RemoveFurniture(string furnitureInstanceId)
        {
            if (_furniture.Remove(furnitureInstanceId))
            {
                Debug.Log($"[GuildBaseSystem] Removed furniture: {furnitureInstanceId}");
                OnFurnitureRemoved?.Invoke(furnitureInstanceId);
            }
        }

        public void UnlockTrophy(string bossId)
        {
            if (_unlockedTrophies.Add(bossId))
            {
                Debug.Log($"[GuildBaseSystem] Trophy unlocked: {bossId}");
                OnTrophyUnlocked?.Invoke(bossId);
            }
        }

        public GuildBaseState GetState()
        {
            var furnitureArray = new FurnitureInstance[_furniture.Count];
            int i = 0;
            foreach (var kvp in _furniture)
            {
                furnitureArray[i++] = kvp.Value;
            }

            var trophyArray = new string[_unlockedTrophies.Count];
            _unlockedTrophies.CopyTo(trophyArray);

            return new GuildBaseState
            {
                PlacedFurniture = furnitureArray,
                UnlockedTrophies = trophyArray
            };
        }

        public bool IsTrophyUnlocked(string bossId)
        {
            return _unlockedTrophies.Contains(bossId);
        }

        /// <summary>
        /// Load state from saved data.
        /// </summary>
        public void LoadState(GuildBaseState state)
        {
            if (state == null) return;

            _furniture.Clear();
            _unlockedTrophies.Clear();

            if (state.PlacedFurniture != null)
            {
                foreach (var furniture in state.PlacedFurniture)
                {
                    _furniture[furniture.InstanceId] = furniture;
                }
            }

            if (state.UnlockedTrophies != null)
            {
                foreach (var trophy in state.UnlockedTrophies)
                {
                    _unlockedTrophies.Add(trophy);
                }
            }

            Debug.Log($"[GuildBaseSystem] Loaded state: {_furniture.Count} furniture, {_unlockedTrophies.Count} trophies");
        }

        /// <summary>
        /// Get players currently in the guild base.
        /// </summary>
        public ulong[] GetPlayersInBase()
        {
            var players = new ulong[_playersInBase.Count];
            _playersInBase.CopyTo(players);
            return players;
        }

        /// <summary>
        /// Check if a player is in the guild base.
        /// </summary>
        public bool IsPlayerInBase(ulong playerId)
        {
            return _playersInBase.Contains(playerId);
        }
    }
}
