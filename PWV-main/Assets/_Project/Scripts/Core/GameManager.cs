using System;
using UnityEngine;

namespace EtherDomes.Core
{
    /// <summary>
    /// Central game manager that initializes and coordinates all game systems.
    /// Singleton pattern for easy access across the game.
    /// Note: System references are set via Inspector or found at runtime to avoid cyclic dependencies.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public bool IsInitialized { get; private set; }

        public event Action OnGameInitialized;
        public event Action<ulong> OnPlayerJoined;
        public event Action<ulong> OnPlayerLeft;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        private void InitializeSystems()
        {
            Debug.Log("[GameManager] Initializing game systems...");

            IsInitialized = true;
            Debug.Log("[GameManager] All systems initialized");
            OnGameInitialized?.Invoke();
        }

        public void NotifyPlayerJoined(ulong clientId)
        {
            Debug.Log($"[GameManager] Player connected: {clientId}");
            OnPlayerJoined?.Invoke(clientId);
        }

        public void NotifyPlayerLeft(ulong clientId)
        {
            Debug.Log($"[GameManager] Player disconnected: {clientId}");
            OnPlayerLeft?.Invoke(clientId);
        }
    }
}
