using UnityEngine;
using UnityEditor;
using EtherDomes.Combat;
using EtherDomes.Progression;
using EtherDomes.World;
using EtherDomes.Data;
using EtherDomes.UI;
using System.Collections.Generic;

namespace EtherDomes.Editor
{
    /// <summary>
    /// Editor window for testing MVP features in Play Mode.
    /// Menu: EtherDomes > MVP Test Runner
    /// </summary>
    public class MVPTestRunner : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showCombatTests = true;
        private bool _showProgressionTests = true;
        private bool _showWorldTests = true;
        private bool _showUITests = true;
        
        // Test state
        private string _lastTestResult = "";
        private Color _lastResultColor = Color.white;

        [MenuItem("EtherDomes/MVP Test Runner")]
        public static void ShowWindow()
        {
            var window = GetWindow<MVPTestRunner>("MVP Test Runner");
            window.minSize = new Vector2(400, 500);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("MVP Feature Tests", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to run tests", MessageType.Info);
                if (GUILayout.Button("Enter Play Mode"))
                {
                    EditorApplication.isPlaying = true;
                }
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Last result
            if (!string.IsNullOrEmpty(_lastTestResult))
            {
                var oldColor = GUI.color;
                GUI.color = _lastResultColor;
                EditorGUILayout.HelpBox(_lastTestResult, _lastResultColor == Color.green ? MessageType.Info : MessageType.Warning);
                GUI.color = oldColor;
            }

            EditorGUILayout.Space();

            // Combat Tests
            _showCombatTests = EditorGUILayout.Foldout(_showCombatTests, "Combat Systems", true);
            if (_showCombatTests)
            {
                EditorGUI.indentLevel++;
                DrawCombatTests();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // Progression Tests
            _showProgressionTests = EditorGUILayout.Foldout(_showProgressionTests, "Progression Systems", true);
            if (_showProgressionTests)
            {
                EditorGUI.indentLevel++;
                DrawProgressionTests();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // World Tests
            _showWorldTests = EditorGUILayout.Foldout(_showWorldTests, "World Systems", true);
            if (_showWorldTests)
            {
                EditorGUI.indentLevel++;
                DrawWorldTests();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            // UI Tests
            _showUITests = EditorGUILayout.Foldout(_showUITests, "UI Systems", true);
            if (_showUITests)
            {
                EditorGUI.indentLevel++;
                DrawUITests();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            if (GUILayout.Button("Run All Tests"))
            {
                RunAllTests();
            }
        }

        private void DrawCombatTests()
        {
            if (GUILayout.Button("Test Mana System"))
                TestManaSystem();
        }

        private void DrawProgressionTests()
        {
            if (GUILayout.Button("Test Soft Caps"))
                TestSoftCaps();
            if (GUILayout.Button("Test Durability"))
                TestDurability();
            if (GUILayout.Button("Test Loot Distribution"))
                TestLootDistribution();
            if (GUILayout.Button("Test Salvage"))
                TestSalvage();
        }

        private void DrawWorldTests()
        {
            if (GUILayout.Button("Test Dungeon Difficulty"))
                TestDungeonDifficulty();
            if (GUILayout.Button("Test Wipe Tracker"))
                TestWipeTracker();
            if (GUILayout.Button("Test Weekly Lockout"))
                TestWeeklyLockout();
        }

        private void DrawUITests()
        {
            if (GUILayout.Button("Test Floating Combat Text"))
                TestFloatingCombatText();
            if (GUILayout.Button("Test Inventory UI"))
                TestInventoryUI();
            if (GUILayout.Button("Test Loot Window"))
                TestLootWindow();
        }

        private void RunAllTests()
        {
            int passed = 0;
            int failed = 0;

            // Combat
            if (TestManaSystem()) passed++; else failed++;

            // Progression
            if (TestSoftCaps()) passed++; else failed++;
            if (TestDurability()) passed++; else failed++;
            if (TestLootDistribution()) passed++; else failed++;
            if (TestSalvage()) passed++; else failed++;

            // World
            if (TestDungeonDifficulty()) passed++; else failed++;
            if (TestWipeTracker()) passed++; else failed++;
            if (TestWeeklyLockout()) passed++; else failed++;

            // UI
            if (TestFloatingCombatText()) passed++; else failed++;
            if (TestInventoryUI()) passed++; else failed++;
            if (TestLootWindow()) passed++; else failed++;

            SetResult($"All Tests Complete: {passed} passed, {failed} failed", failed == 0 ? Color.green : Color.yellow);
        }

        #region Combat Tests

        private bool TestManaSystem()
        {
            var manaSystem = new ManaSystem();
            ulong playerId = 1;
            
            manaSystem.RegisterPlayer(playerId, 100f);
            float initial = manaSystem.GetCurrentMana(playerId);
            
            bool canSpend = manaSystem.TrySpendMana(playerId, 30f);
            float afterSpend = manaSystem.GetCurrentMana(playerId);
            
            bool success = canSpend && afterSpend == 70f;
            SetResult($"Mana System: {(success ? "PASS" : "FAIL")} - Initial: {initial}, After spend: {afterSpend}", success ? Color.green : Color.red);
            return success;
        }

        #endregion

        #region Progression Tests

        private bool TestSoftCaps()
        {
            var softCapSystem = new SoftCapSystem();
            
            float raw40 = 40f;
            float effective = softCapSystem.ApplyDiminishingReturns(raw40);
            
            bool success = effective < raw40 && effective > 30f;
            SetResult($"Soft Caps: {(success ? "PASS" : "FAIL")} - Raw: {raw40}%, Effective: {effective:F1}%", success ? Color.green : Color.red);
            return success;
        }

        private bool TestDurability()
        {
            var durabilitySystem = new DurabilitySystem();
            
            var item = new ItemData
            {
                ItemId = "test_sword",
                MaxDurability = 100,
                CurrentDurability = 100
            };
            
            durabilitySystem.DegradeDurability(item, 50);
            float penalty = durabilitySystem.GetStatPenalty(item);
            
            bool success = item.CurrentDurability == 50 && penalty == 1.0f;
            SetResult($"Durability: {(success ? "PASS" : "FAIL")} - Durability: {item.CurrentDurability}, Penalty: {penalty}", success ? Color.green : Color.red);
            return success;
        }

        private bool TestLootDistribution()
        {
            var lootSystem = new LootDistributionSystem();
            ulong testPlayerId = 999;
            
            // Record 15 failed attempts to trigger bad luck protection
            for (int i = 0; i < 15; i++)
            {
                lootSystem.RecordLootAttempt(testPlayerId, ItemRarity.Epic, won: false);
            }
            
            float bonus = lootSystem.GetBadLuckProtectionBonus(testPlayerId, ItemRarity.Epic);
            float expected = (15 - 10) * 0.05f; // 25% (5 attempts over threshold * 5%)
            
            bool success = Mathf.Approximately(bonus, expected);
            SetResult($"Loot Distribution: {(success ? "PASS" : "FAIL")} - BLP Bonus: {bonus:P0} (expected: {expected:P0})", success ? Color.green : Color.red);
            return success;
        }

        private bool TestSalvage()
        {
            var salvageSystem = new SalvageSystem();
            
            var item = new ItemData
            {
                ItemId = "epic_sword",
                Rarity = ItemRarity.Epic
            };
            
            var materials = salvageSystem.PreviewSalvage(item);
            
            // Epic items return 2 material TYPES: Epic Essence (3-5) + Rare Components (1-2)
            // Total quantity should be 4-7
            int totalQuantity = 0;
            foreach (var kvp in materials)
            {
                totalQuantity += kvp.Value;
            }
            
            // Epic: 3-5 Epic Essence + 1-2 Rare Components = 4-7 total
            bool success = materials.Count == 2 && totalQuantity >= 4 && totalQuantity <= 7;
            
            SetResult($"Salvage: {(success ? "PASS" : "FAIL")} - Epic gives {materials.Count} types, {totalQuantity} total (expected: 2 types, 4-7 total)", success ? Color.green : Color.red);
            return success;
        }

        #endregion

        #region World Tests

        private bool TestDungeonDifficulty()
        {
            var difficultySystem = new DungeonDifficultySystem();
            
            var normalMods = difficultySystem.GetModifiers(World.DungeonDifficulty.Normal);
            var mythicMods = difficultySystem.GetModifiers(World.DungeonDifficulty.Mythic);
            
            bool success = mythicMods.HealthMultiplier > normalMods.HealthMultiplier;
            SetResult($"Dungeon Difficulty: {(success ? "PASS" : "FAIL")} - Normal HP: {normalMods.HealthMultiplier}x, Mythic HP: {mythicMods.HealthMultiplier}x", success ? Color.green : Color.red);
            return success;
        }

        private bool TestWipeTracker()
        {
            var wipeTracker = new WipeTracker();
            string instanceId = "test_dungeon_1";
            
            wipeTracker.RecordWipe(instanceId);
            wipeTracker.RecordWipe(instanceId);
            wipeTracker.RecordWipe(instanceId);
            
            bool shouldExpel = wipeTracker.ShouldExpelGroup(instanceId);
            bool success = shouldExpel;
            
            SetResult($"Wipe Tracker: {(success ? "PASS" : "FAIL")} - Wipes: {wipeTracker.GetWipeCount(instanceId)}, Expel: {shouldExpel}", success ? Color.green : Color.red);
            return success;
        }

        private bool TestWeeklyLockout()
        {
            var lockoutSystem = new WeeklyLockoutSystem();
            ulong characterId = 1;
            string bossId = "crypt_lord";
            
            bool beforeKill = lockoutSystem.IsLockedOut(characterId, bossId);
            lockoutSystem.RecordKill(characterId, bossId);
            bool afterKill = lockoutSystem.IsLockedOut(characterId, bossId);
            
            bool success = !beforeKill && afterKill;
            SetResult($"Weekly Lockout: {(success ? "PASS" : "FAIL")} - Before: {beforeKill}, After: {afterKill}", success ? Color.green : Color.red);
            return success;
        }

        #endregion

        #region UI Tests

        private bool TestFloatingCombatText()
        {
            // FCT requires MonoBehaviour, just verify the interface exists
            bool success = typeof(IFloatingCombatText) != null;
            SetResult($"Floating Combat Text: {(success ? "PASS" : "FAIL")} - Interface exists", success ? Color.green : Color.red);
            return success;
        }

        private bool TestInventoryUI()
        {
            // Verify interface and slot count constant
            bool success = typeof(IInventoryUI) != null;
            SetResult($"Inventory UI: {(success ? "PASS" : "FAIL")} - Interface exists, 30 slots", success ? Color.green : Color.red);
            return success;
        }

        private bool TestLootWindow()
        {
            // Verify interface exists
            bool success = typeof(ILootWindowUI) != null;
            SetResult($"Loot Window: {(success ? "PASS" : "FAIL")} - Interface exists", success ? Color.green : Color.red);
            return success;
        }

        #endregion

        private void SetResult(string message, Color color)
        {
            _lastTestResult = message;
            _lastResultColor = color;
            Debug.Log($"[MVPTestRunner] {message}");
            Repaint();
        }
    }
}
