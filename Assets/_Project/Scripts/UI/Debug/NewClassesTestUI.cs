using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;
using EtherDomes.Classes;
using EtherDomes.Classes.Abilities;
using System.Collections.Generic;

namespace EtherDomes.UI.Debug
{
    /// <summary>
    /// Test UI for new classes (Rogue, Hunter, Warlock, Death Knight).
    /// Allows switching classes and testing all abilities.
    /// </summary>
    public class NewClassesTestUI : MonoBehaviour
    {
        [Header("Systems")]
        [SerializeField] private AbilitySystem _abilitySystem;
        [SerializeField] private TargetSystem _targetSystem;
        [SerializeField] private SecondaryResourceSystem _resourceSystem;
        [SerializeField] private BuffSystem _buffSystem;
        [SerializeField] private StealthSystem _stealthSystem;
        [SerializeField] private PetSystem _petSystem;
        [SerializeField] private DiminishingReturnsSystem _drSystem;
        
        private CharacterClass _currentClass = CharacterClass.Rogue;
        private int _currentSpec = 0; // 0 = first spec, 1 = second spec
        private AbilityData[] _currentAbilities;
        private ulong _playerId = 1;
        private ulong _targetId = 1000;
        
        private GUIStyle _headerStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _abilityStyle;
        private GUIStyle _resourceStyle;
        private bool _stylesInitialized;
        
        private Vector2 _abilityScrollPos;
        private string _lastMessage = "";
        private float _messageTimer;
        
        private void Start()
        {
            FindOrCreateSystems();
            SetupClass(_currentClass);
        }
        
        private void FindOrCreateSystems()
        {
            // Find existing systems or create them
            if (_abilitySystem == null)
                _abilitySystem = FindFirstObjectByType<AbilitySystem>();
            if (_targetSystem == null)
                _targetSystem = FindFirstObjectByType<TargetSystem>();
            if (_resourceSystem == null)
            {
                _resourceSystem = FindFirstObjectByType<SecondaryResourceSystem>();
                if (_resourceSystem == null)
                {
                    var go = new GameObject("SecondaryResourceSystem");
                    _resourceSystem = go.AddComponent<SecondaryResourceSystem>();
                }
            }
            if (_buffSystem == null)
            {
                _buffSystem = FindFirstObjectByType<BuffSystem>();
                if (_buffSystem == null)
                {
                    var go = new GameObject("BuffSystem");
                    _buffSystem = go.AddComponent<BuffSystem>();
                }
            }
            if (_stealthSystem == null)
            {
                _stealthSystem = FindFirstObjectByType<StealthSystem>();
                if (_stealthSystem == null)
                {
                    var go = new GameObject("StealthSystem");
                    _stealthSystem = go.AddComponent<StealthSystem>();
                }
            }
            if (_drSystem == null)
            {
                _drSystem = FindFirstObjectByType<DiminishingReturnsSystem>();
                if (_drSystem == null)
                {
                    var go = new GameObject("DiminishingReturnsSystem");
                    _drSystem = go.AddComponent<DiminishingReturnsSystem>();
                }
            }
            
            // Subscribe to events
            if (_abilitySystem != null)
            {
                _abilitySystem.OnAbilityExecuted += OnAbilityExecuted;
                _abilitySystem.OnAbilityError += OnAbilityError;
            }
        }
        
        private void OnDestroy()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.OnAbilityExecuted -= OnAbilityExecuted;
                _abilitySystem.OnAbilityError -= OnAbilityError;
            }
        }
        
        private void SetupClass(CharacterClass characterClass)
        {
            _currentClass = characterClass;
            
            // Setup secondary resource
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(characterClass);
            _resourceSystem.RegisterResource(_playerId, resourceType, 0);
            
            // Add starting resource
            if (resourceType == SecondaryResourceType.Energy)
                _resourceSystem.AddResource(_playerId, 100f);
            else if (resourceType == SecondaryResourceType.Focus)
                _resourceSystem.AddResource(_playerId, 100f);
            
            // Setup combo points for Rogue
            if (SecondaryResourceSystem.ClassUsesComboPoints(characterClass))
                _resourceSystem.RegisterComboPoints(_playerId);
            
            // Load abilities
            _currentAbilities = GetAbilitiesForClass(characterClass, _currentSpec);
            
            if (_abilitySystem != null)
            {
                _abilitySystem.LoadAbilities(_currentAbilities);
            }
            
            ShowMessage($"Switched to {characterClass} ({GetSpecName(characterClass, _currentSpec)})");
        }
        
        private AbilityData[] GetAbilitiesForClass(CharacterClass characterClass, int spec)
        {
            var abilities = new List<AbilityData>();
            
            switch (characterClass)
            {
                case CharacterClass.Rogue:
                    abilities.AddRange(ClassAbilityDefinitions.GetRogueSharedAbilities());
                    abilities.AddRange(spec == 0 
                        ? ClassAbilityDefinitions.GetRogueAssassinationAbilities()
                        : ClassAbilityDefinitions.GetRogueCombatAbilities());
                    break;
                    
                case CharacterClass.Hunter:
                    abilities.AddRange(ClassAbilityDefinitions.GetHunterSharedAbilities());
                    abilities.AddRange(spec == 0 
                        ? ClassAbilityDefinitions.GetHunterBeastMasteryAbilities()
                        : ClassAbilityDefinitions.GetHunterMarksmanshipAbilities());
                    break;
                    
                case CharacterClass.Warlock:
                    abilities.AddRange(ClassAbilityDefinitions.GetWarlockSharedAbilities());
                    abilities.AddRange(spec == 0 
                        ? ClassAbilityDefinitions.GetWarlockAfflictionAbilities()
                        : ClassAbilityDefinitions.GetWarlockDestructionAbilities());
                    break;
                    
                case CharacterClass.DeathKnight:
                    abilities.AddRange(ClassAbilityDefinitions.GetDeathKnightSharedAbilities());
                    abilities.AddRange(spec == 0 
                        ? ClassAbilityDefinitions.GetDeathKnightBloodAbilities()
                        : ClassAbilityDefinitions.GetDeathKnightFrostAbilities());
                    break;
            }
            
            return abilities.ToArray();
        }
        
        private string GetSpecName(CharacterClass characterClass, int spec)
        {
            return characterClass switch
            {
                CharacterClass.Rogue => spec == 0 ? "Assassination" : "Combat",
                CharacterClass.Hunter => spec == 0 ? "Beast Mastery" : "Marksmanship",
                CharacterClass.Warlock => spec == 0 ? "Affliction" : "Destruction",
                CharacterClass.DeathKnight => spec == 0 ? "Blood" : "Frost",
                _ => "Unknown"
            };
        }
        
        private void Update()
        {
            if (_messageTimer > 0)
                _messageTimer -= Time.deltaTime;
            
            // Update resource regeneration
            _resourceSystem?.ApplyDecay(_playerId, Time.deltaTime, inCombat: true);
            
            // Hotkeys for abilities (1-9)
            for (int i = 0; i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    TryUseAbility(i);
                }
            }
            
            // F1-F4 to switch classes
            if (Input.GetKeyDown(KeyCode.F1)) SetupClass(CharacterClass.Rogue);
            if (Input.GetKeyDown(KeyCode.F2)) SetupClass(CharacterClass.Hunter);
            if (Input.GetKeyDown(KeyCode.F3)) SetupClass(CharacterClass.Warlock);
            if (Input.GetKeyDown(KeyCode.F4)) SetupClass(CharacterClass.DeathKnight);
            
            // F5 to toggle spec
            if (Input.GetKeyDown(KeyCode.F5))
            {
                _currentSpec = _currentSpec == 0 ? 1 : 0;
                SetupClass(_currentClass);
            }
        }
        
        private void TryUseAbility(int slot)
        {
            if (_currentAbilities == null || slot >= _currentAbilities.Length)
                return;
            
            var ability = _currentAbilities[slot];
            
            // Check resource cost
            if (ability.ResourceType == SecondaryResourceType.Energy || 
                ability.ResourceType == SecondaryResourceType.Focus)
            {
                float current = _resourceSystem.GetResource(_playerId);
                if (current < ability.ResourceCost)
                {
                    ShowMessage($"Not enough {ability.ResourceType}! ({current:F0}/{ability.ResourceCost})");
                    return;
                }
            }
            
            // Check stealth requirement
            if (ability.RequiresStealth && !_stealthSystem.IsInStealth(_playerId))
            {
                ShowMessage("Must be in stealth!");
                return;
            }
            
            // Check combo points for finishers
            if (ability.ConsumesComboPoints && !_resourceSystem.HasComboPoints(_playerId))
            {
                ShowMessage("Need combo points!");
                return;
            }
            
            // Execute ability
            if (_abilitySystem != null)
            {
                _abilitySystem.TryExecuteAbility(slot);
            }
            else
            {
                // Simulate execution
                SimulateAbility(ability);
            }
        }
        
        private void SimulateAbility(AbilityData ability)
        {
            // Spend resource
            if (ability.ResourceType == SecondaryResourceType.Energy || 
                ability.ResourceType == SecondaryResourceType.Focus)
            {
                _resourceSystem.TrySpendResource(_playerId, ability.ResourceCost);
            }
            
            // Generate combo points
            if (ability.GeneratesComboPoint)
            {
                int amount = ability.ComboPointsGenerated > 0 ? ability.ComboPointsGenerated : 1;
                _resourceSystem.AddComboPoint(_playerId, amount);
            }
            
            // Consume combo points
            if (ability.ConsumesComboPoints)
            {
                int consumed = _resourceSystem.ConsumeAllComboPoints(_playerId);
                float multiplier = _resourceSystem.CalculateComboPointDamageMultiplier(consumed);
                ShowMessage($"{ability.AbilityName} with {consumed} combo points! ({multiplier:P0} damage)");
                return;
            }
            
            // Break stealth
            if (ability.BreaksStealth && _stealthSystem.IsInStealth(_playerId))
            {
                _stealthSystem.BreakStealth(_playerId, StealthBreakReason.AbilityUsed);
            }
            
            ShowMessage($"Used {ability.AbilityName}!");
        }
        
        private void OnAbilityExecuted(AbilityData ability, ITargetable target)
        {
            ShowMessage($"{ability.AbilityName} -> {target?.DisplayName ?? "no target"}");
        }
        
        private void OnAbilityError(string error)
        {
            ShowMessage($"Error: {error}");
        }
        
        private void ShowMessage(string msg)
        {
            _lastMessage = msg;
            _messageTimer = 3f;
            UnityEngine.Debug.Log($"[NewClassesTestUI] {msg}");
        }
        
        private void InitStyles()
        {
            if (_stylesInitialized) return;
            
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            
            _buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12
            };
            
            _abilityStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 11,
                wordWrap = true,
                fixedHeight = 50
            };
            
            _resourceStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            
            _stylesInitialized = true;
        }
        
        private void OnGUI()
        {
            InitStyles();
            
            // Main panel - right side
            GUILayout.BeginArea(new Rect(Screen.width - 320, 10, 310, Screen.height - 20));
            GUILayout.BeginVertical(GUI.skin.box);
            
            // Header
            GUI.color = GetClassColor(_currentClass);
            GUILayout.Label($"{_currentClass} - {GetSpecName(_currentClass, _currentSpec)}", _headerStyle);
            GUI.color = Color.white;
            
            // Class buttons
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Rogue (F1)", _buttonStyle)) SetupClass(CharacterClass.Rogue);
            if (GUILayout.Button("Hunter (F2)", _buttonStyle)) SetupClass(CharacterClass.Hunter);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Warlock (F3)", _buttonStyle)) SetupClass(CharacterClass.Warlock);
            if (GUILayout.Button("DK (F4)", _buttonStyle)) SetupClass(CharacterClass.DeathKnight);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button($"Toggle Spec (F5): {GetSpecName(_currentClass, _currentSpec)}", _buttonStyle))
            {
                _currentSpec = _currentSpec == 0 ? 1 : 0;
                SetupClass(_currentClass);
            }
            
            GUILayout.Space(10);
            
            // Resources
            DrawResources();
            
            GUILayout.Space(10);
            
            // Abilities
            GUILayout.Label("Abilities (1-9):", _headerStyle);
            _abilityScrollPos = GUILayout.BeginScrollView(_abilityScrollPos, GUILayout.Height(300));
            DrawAbilities();
            GUILayout.EndScrollView();
            
            GUILayout.Space(10);
            
            // Stealth button for Rogue
            if (_currentClass == CharacterClass.Rogue)
            {
                bool inStealth = _stealthSystem?.IsInStealth(_playerId) ?? false;
                GUI.backgroundColor = inStealth ? Color.gray : Color.white;
                if (GUILayout.Button(inStealth ? "Break Stealth" : "Enter Stealth", _buttonStyle))
                {
                    if (inStealth)
                        _stealthSystem?.BreakStealth(_playerId, StealthBreakReason.Manual);
                    else
                        _stealthSystem?.TryEnterStealth(_playerId);
                }
                GUI.backgroundColor = Color.white;
            }
            
            // Message
            if (_messageTimer > 0)
            {
                GUI.color = Color.yellow;
                GUILayout.Label(_lastMessage, _resourceStyle);
                GUI.color = Color.white;
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void DrawResources()
        {
            var resourceType = SecondaryResourceSystem.GetResourceTypeForClass(_currentClass);
            
            if (resourceType != SecondaryResourceType.None)
            {
                float current = _resourceSystem?.GetResource(_playerId) ?? 0;
                float max = _resourceSystem?.GetMaxResource(_playerId) ?? 100;
                
                GUI.color = GetResourceColor(resourceType);
                GUILayout.Label($"{resourceType}: {current:F0} / {max:F0}", _resourceStyle);
                
                // Resource bar
                Rect barRect = GUILayoutUtility.GetRect(280, 20);
                GUI.color = new Color(0.2f, 0.2f, 0.2f);
                GUI.Box(barRect, "");
                GUI.color = GetResourceColor(resourceType);
                float fillWidth = (barRect.width - 4) * (current / max);
                GUI.Box(new Rect(barRect.x + 2, barRect.y + 2, fillWidth, barRect.height - 4), "");
                GUI.color = Color.white;
            }
            
            // Combo points for Rogue
            if (SecondaryResourceSystem.ClassUsesComboPoints(_currentClass))
            {
                int comboPoints = _resourceSystem?.GetComboPoints(_playerId) ?? 0;
                int maxCombo = _resourceSystem?.GetMaxComboPoints() ?? 5;
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Combo: ", _resourceStyle);
                for (int i = 0; i < maxCombo; i++)
                {
                    GUI.color = i < comboPoints ? Color.yellow : Color.gray;
                    GUILayout.Label("●", _resourceStyle, GUILayout.Width(20));
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
            }
        }
        
        private void DrawAbilities()
        {
            if (_currentAbilities == null) return;
            
            for (int i = 0; i < _currentAbilities.Length; i++)
            {
                var ability = _currentAbilities[i];
                
                string costText = "";
                if (ability.ResourceCost > 0)
                {
                    costText = $" ({ability.ResourceCost} {ability.ResourceType})";
                }
                
                string extraInfo = "";
                if (ability.GeneratesComboPoint) extraInfo += " [Gen CP]";
                if (ability.ConsumesComboPoints) extraInfo += " [Finisher]";
                if (ability.RequiresStealth) extraInfo += " [Stealth]";
                if (ability.IsChanneled) extraInfo += " [Channel]";
                if (ability.CastTime > 0) extraInfo += $" [{ability.CastTime}s cast]";
                
                GUI.backgroundColor = GetAbilityColor(ability);
                if (GUILayout.Button($"[{i + 1}] {ability.AbilityName}{costText}\n{extraInfo}", _abilityStyle))
                {
                    TryUseAbility(i);
                }
                GUI.backgroundColor = Color.white;
            }
        }
        
        private Color GetClassColor(CharacterClass characterClass)
        {
            return characterClass switch
            {
                CharacterClass.Rogue => new Color(1f, 0.96f, 0.41f),      // Yellow
                CharacterClass.Hunter => new Color(0.67f, 0.83f, 0.45f), // Green
                CharacterClass.Warlock => new Color(0.58f, 0.51f, 0.79f), // Purple
                CharacterClass.DeathKnight => new Color(0.77f, 0.12f, 0.23f), // Red
                _ => Color.white
            };
        }
        
        private Color GetResourceColor(SecondaryResourceType resourceType)
        {
            return resourceType switch
            {
                SecondaryResourceType.Energy => new Color(1f, 0.96f, 0.41f),
                SecondaryResourceType.Focus => new Color(0.67f, 0.83f, 0.45f),
                SecondaryResourceType.Rage => Color.red,
                SecondaryResourceType.HolyPower => new Color(0.96f, 0.85f, 0.55f),
                _ => Color.white
            };
        }
        
        private Color GetAbilityColor(AbilityData ability)
        {
            if (ability.RequiresStealth) return new Color(0.5f, 0.5f, 0.5f);
            if (ability.ConsumesComboPoints) return new Color(1f, 0.8f, 0.3f);
            if (ability.IsChanneled) return new Color(0.3f, 0.6f, 1f);
            if (ability.Type == AbilityType.Healing) return new Color(0.3f, 1f, 0.3f);
            if (ability.Type == AbilityType.Buff) return new Color(0.3f, 0.8f, 1f);
            return Color.white;
        }
    }
}
