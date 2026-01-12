using UnityEngine;
using UnityEngine.UI;
using EtherDomes.Combat;
using EtherDomes.Progression;
using EtherDomes.World;
using EtherDomes.Data;
using System.Collections.Generic;

namespace EtherDomes.UI.Debug
{
    /// <summary>
    /// Runtime UI for testing MVP features in Play Mode.
    /// Add this to a GameObject in your test scene.
    /// </summary>
    public class MVPFeatureTestUI : MonoBehaviour
    {
        [Header("Test Configuration")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;
        [SerializeField] private bool _showOnStart = true;

        private bool _isVisible;
        private Rect _windowRect = new Rect(10, 10, 350, 500);
        private Vector2 _scrollPosition;
        private string _logOutput = "";
        private int _maxLogLines = 10;
        private List<string> _logLines = new List<string>();

        // Systems for testing
        private ManaSystem _manaSystem;
        private SoftCapSystem _softCapSystem;
        private DurabilitySystem _durabilitySystem;
        private LootDistributionSystem _lootSystem;
        private SalvageSystem _salvageSystem;
        private DungeonDifficultySystem _difficultySystem;
        private WipeTracker _wipeTracker;
        private WeeklyLockoutSystem _lockoutSystem;

        // Test state
        private ulong _testPlayerId = 1;
        private string _testInstanceId = "test_dungeon";
        private ulong _testCharacterId = 1;
        private string _testBossId = "crypt_lord";

        private void Start()
        {
            _isVisible = _showOnStart;
            InitializeSystems();
            Log("MVP Feature Test UI initialized. Press F1 to toggle.");
        }

        private void InitializeSystems()
        {
            _manaSystem = new ManaSystem();
            _softCapSystem = new SoftCapSystem();
            _durabilitySystem = new DurabilitySystem();
            _lootSystem = new LootDistributionSystem();
            _salvageSystem = new SalvageSystem();
            _difficultySystem = new DungeonDifficultySystem();
            _wipeTracker = new WipeTracker();
            _lockoutSystem = new WeeklyLockoutSystem();

            // Initialize test player
            _manaSystem.RegisterPlayer(_testPlayerId, 100f);
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                _isVisible = !_isVisible;
            }
        }

        private void OnGUI()
        {
            if (!_isVisible) return;

            _windowRect = GUI.Window(0, _windowRect, DrawWindow, "MVP Feature Tests (F1 to hide)");
        }

        private void DrawWindow(int windowId)
        {
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            // Combat Section
            GUILayout.Label("=== COMBAT ===", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Spend 20 Mana"))
            {
                bool success = _manaSystem.TrySpendMana(_testPlayerId, 20f);
                float current = _manaSystem.GetCurrentMana(_testPlayerId);
                Log($"Mana: {(success ? "Spent 20" : "Failed")} - Current: {current:F0}");
            }
            if (GUILayout.Button("Restore Mana"))
            {
                _manaSystem.RestoreMana(_testPlayerId, 50f);
                Log($"Mana restored to: {_manaSystem.GetCurrentMana(_testPlayerId):F0}");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Progression Section
            GUILayout.Label("=== PROGRESSION ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Soft Cap 40%"))
            {
                float effective = _softCapSystem.ApplyDiminishingReturns(40f);
                Log($"Soft Cap: 40% raw -> {effective:F1}% effective");
            }
            if (GUILayout.Button("Test Soft Cap 60%"))
            {
                float effective = _softCapSystem.ApplyDiminishingReturns(60f);
                Log($"Soft Cap: 60% raw -> {effective:F1}% effective");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Degrade Item"))
            {
                var item = new ItemData { MaxDurability = 100, CurrentDurability = 100 };
                _durabilitySystem.DegradeDurability(item, 30);
                float penalty = _durabilitySystem.GetStatPenalty(item);
                Log($"Durability: {item.CurrentDurability}/{item.MaxDurability}, Penalty: {penalty:P0}");
            }
            if (GUILayout.Button("Break Item"))
            {
                var item = new ItemData { MaxDurability = 100, CurrentDurability = 0 };
                float penalty = _durabilitySystem.GetStatPenalty(item);
                Log($"Broken item penalty: {penalty:P0} (50% expected)");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("BLP Bonus (15 tries)"))
            {
                float bonus = _lootSystem.GetBadLuckProtectionBonus(15, ItemRarity.Epic);
                Log($"Bad Luck Protection: {bonus:P0} bonus after 15 tries");
            }
            if (GUILayout.Button("Salvage Epic"))
            {
                var item = new ItemData { Rarity = ItemRarity.Epic };
                var mats = _salvageSystem.PreviewSalvage(item);
                Log($"Salvage Epic: {mats.Count} materials");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // World Section
            GUILayout.Label("=== WORLD ===", GUI.skin.box);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Normal Diff"))
            {
                var mods = _difficultySystem.GetModifiers(World.DungeonDifficulty.Normal);
                Log($"Normal: HP {mods.HealthMultiplier}x, DMG {mods.DamageMultiplier}x");
            }
            if (GUILayout.Button("Mythic Diff"))
            {
                var mods = _difficultySystem.GetModifiers(World.DungeonDifficulty.Mythic);
                Log($"Mythic: HP {mods.HealthMultiplier}x, DMG {mods.DamageMultiplier}x");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Record Wipe"))
            {
                _wipeTracker.RecordWipe(_testInstanceId);
                int count = _wipeTracker.GetWipeCount(_testInstanceId);
                bool expel = _wipeTracker.ShouldExpelGroup(_testInstanceId);
                Log($"Wipes: {count}/3, Expel: {expel}");
            }
            if (GUILayout.Button("Reset Wipes"))
            {
                _wipeTracker = new WipeTracker();
                Log("Wipe tracker reset");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kill Boss"))
            {
                _lockoutSystem.RecordKill(_testCharacterId, _testBossId);
                bool locked = _lockoutSystem.IsLockedOut(_testCharacterId, _testBossId);
                Log($"Boss killed. Locked out: {locked}");
            }
            if (GUILayout.Button("Check Lockout"))
            {
                bool locked = _lockoutSystem.IsLockedOut(_testCharacterId, _testBossId);
                var reset = _lockoutSystem.GetResetTime();
                Log($"Lockout: {locked}, Reset: {reset:ddd HH:mm}");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            // Log Output
            GUILayout.Label("=== LOG ===", GUI.skin.box);
            GUILayout.TextArea(_logOutput, GUILayout.Height(120));

            if (GUILayout.Button("Clear Log"))
            {
                _logLines.Clear();
                _logOutput = "";
            }

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        private void Log(string message)
        {
            _logLines.Add($"[{Time.time:F1}] {message}");
            if (_logLines.Count > _maxLogLines)
                _logLines.RemoveAt(0);
            
            _logOutput = string.Join("\n", _logLines);
            UnityEngine.Debug.Log($"[MVPTest] {message}");
        }
    }
}
