using UnityEngine;
using EtherDomes.Combat;
using EtherDomes.Data;
using EtherDomes.Enemy;

namespace EtherDomes.UI.Debug
{
    /// <summary>
    /// OnGUI-based combat UI for testing.
    /// Shows Target Frame, Action Bar, and ability feedback.
    /// </summary>
    public class CombatTestUI : MonoBehaviour
    {
        private TargetSystem _targetSystem;
        private AbilitySystem _abilitySystem;
        
        private GUIStyle _boxStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _healthBarStyle;
        private GUIStyle _healthBarBgStyle;
        private GUIStyle _abilityButtonStyle;
        private GUIStyle _errorStyle;
        
        private string _lastError = "";
        private float _errorTimer = 0f;
        
        // Test abilities
        private AbilityData[] _testAbilities;
        
        private void Start()
        {
            _targetSystem = FindFirstObjectByType<TargetSystem>();
            _abilitySystem = FindFirstObjectByType<AbilitySystem>();
            
            UnityEngine.Debug.Log($"[CombatTestUI] TargetSystem found: {_targetSystem != null}");
            UnityEngine.Debug.Log($"[CombatTestUI] AbilitySystem found: {_abilitySystem != null}");
            
            if (_abilitySystem != null)
            {
                _abilitySystem.OnAbilityError += OnAbilityError;
                _abilitySystem.OnAbilityExecuted += OnAbilityExecuted;
                _abilitySystem.Initialize(_targetSystem);
                
                // Load test abilities
                CreateTestAbilities();
                _abilitySystem.LoadAbilities(_testAbilities);
                UnityEngine.Debug.Log($"[CombatTestUI] Loaded {_testAbilities.Length} test abilities");
            }
            else
            {
                UnityEngine.Debug.LogError("[CombatTestUI] AbilitySystem not found! Make sure GameManager is in the scene.");
            }
            
            if (_targetSystem != null)
            {
                _targetSystem.OnTargetChanged += OnTargetChanged;
            }
            else
            {
                UnityEngine.Debug.LogError("[CombatTestUI] TargetSystem not found! Make sure GameManager is in the scene.");
            }
        }
        
        private void OnDestroy()
        {
            if (_abilitySystem != null)
            {
                _abilitySystem.OnAbilityError -= OnAbilityError;
                _abilitySystem.OnAbilityExecuted -= OnAbilityExecuted;
            }
            
            if (_targetSystem != null)
            {
                _targetSystem.OnTargetChanged -= OnTargetChanged;
            }
        }
        
        private void CreateTestAbilities()
        {
            _testAbilities = new AbilityData[]
            {
                // Slot 1: Basic Attack (instant)
                new AbilityData
                {
                    AbilityName = "Strike",
                    Description = "Basic melee attack",
                    CastTime = 0f,
                    Cooldown = 0f,
                    Range = 5f,
                    RequiresTarget = true,
                    AffectedByGCD = true,
                    BaseDamage = 15f,
                    Type = AbilityType.Damage
                },
                // Slot 2: Heavy Strike (instant, cooldown)
                new AbilityData
                {
                    AbilityName = "Heavy Strike",
                    Description = "Powerful attack",
                    CastTime = 0f,
                    Cooldown = 6f,
                    Range = 5f,
                    RequiresTarget = true,
                    AffectedByGCD = true,
                    BaseDamage = 35f,
                    Type = AbilityType.Damage
                },
                // Slot 3: Fireball (cast time)
                new AbilityData
                {
                    AbilityName = "Fireball",
                    Description = "Ranged fire spell",
                    CastTime = 2f,
                    Cooldown = 0f,
                    Range = 30f,
                    RequiresTarget = true,
                    AffectedByGCD = true,
                    BaseDamage = 50f,
                    Type = AbilityType.Damage,
                    DamageType = DamageType.Fire
                },
                // Slot 4: Execute (instant, long cooldown)
                new AbilityData
                {
                    AbilityName = "Execute",
                    Description = "Finishing blow",
                    CastTime = 0f,
                    Cooldown = 15f,
                    Range = 5f,
                    RequiresTarget = true,
                    AffectedByGCD = true,
                    BaseDamage = 100f,
                    Type = AbilityType.Damage
                }
            };
        }
        
        private void OnAbilityError(string error)
        {
            _lastError = error;
            _errorTimer = 2f;
            UnityEngine.Debug.Log($"[CombatTestUI] Ability Error: {error}");
        }
        
        private void OnAbilityExecuted(AbilityData ability, ITargetable target)
        {
            UnityEngine.Debug.Log($"[CombatTestUI] OnAbilityExecuted: {ability.AbilityName} on {target?.DisplayName ?? "null"}");
            
            // Apply damage to target
            if (target != null && ability.BaseDamage > 0)
            {
                // Find the Enemy component and apply damage
                var enemy = target.Transform?.GetComponent<Enemy.Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(ability.BaseDamage);
                    UnityEngine.Debug.Log($"[CombatTestUI] {ability.AbilityName} dealt {ability.BaseDamage} damage to {target.DisplayName}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[CombatTestUI] Target {target.DisplayName} has no Enemy component!");
                }
            }
        }
        
        private void OnTargetChanged(ITargetable target)
        {
            // Could play sound or show animation
        }
        
        private void Update()
        {
            if (_errorTimer > 0)
            {
                _errorTimer -= Time.deltaTime;
            }
            
            // Debug: Manual key detection as fallback
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                UnityEngine.Debug.Log("[CombatTestUI] Legacy Input: Key 1 detected");
            }
        }
        
        private void InitStyles()
        {
            if (_boxStyle == null)
            {
                _boxStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 12
                };
                
                _labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleLeft
                };
                
                _healthBarBgStyle = new GUIStyle(GUI.skin.box);
                _healthBarStyle = new GUIStyle(GUI.skin.box);
                
                _abilityButtonStyle = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    fixedWidth = 60,
                    fixedHeight = 40
                };
                
                _errorStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 16,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }
        }
        
        private void OnGUI()
        {
            InitStyles();
            
            DrawTargetFrame();
            DrawActionBar();
            DrawCastBar();
            DrawErrorMessage();
        }

        private void DrawTargetFrame()
        {
            if (_targetSystem == null || !_targetSystem.HasTarget)
                return;
            
            var target = _targetSystem.CurrentTarget;
            
            // Position: top center of screen
            float frameWidth = 250f;
            float frameHeight = 80f;
            float x = (Screen.width - frameWidth) / 2f;
            float y = 10f;
            
            GUILayout.BeginArea(new Rect(x, y, frameWidth, frameHeight));
            GUILayout.BeginVertical(_boxStyle);
            
            // Target name and level
            GUI.color = GetTargetColor(target.Type);
            GUILayout.Label($"[{target.Level}] {target.DisplayName}", _labelStyle);
            GUI.color = Color.white;
            
            // Health bar
            DrawHealthBar(target.HealthPercent, target.CurrentHealth, target.MaxHealth);
            
            // Distance
            float distance = _targetSystem.TargetDistance;
            string rangeText = _targetSystem.IsTargetInRange ? "In Range" : "Out of Range";
            GUI.color = _targetSystem.IsTargetInRange ? Color.green : Color.red;
            GUILayout.Label($"{distance:F1}m - {rangeText}", _labelStyle);
            GUI.color = Color.white;
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void DrawHealthBar(float percent, float current, float max)
        {
            Rect barRect = GUILayoutUtility.GetRect(230, 20);
            
            // Background
            GUI.color = new Color(0.2f, 0.2f, 0.2f);
            GUI.Box(barRect, "");
            
            // Health fill
            GUI.color = GetHealthColor(percent);
            Rect fillRect = new Rect(barRect.x + 2, barRect.y + 2, (barRect.width - 4) * percent, barRect.height - 4);
            GUI.Box(fillRect, "");
            
            // Text
            GUI.color = Color.white;
            GUI.Label(barRect, $"{current:F0} / {max:F0}", new GUIStyle(GUI.skin.label) 
            { 
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            });
        }
        
        private Color GetHealthColor(float percent)
        {
            if (percent > 0.5f) return new Color(0.2f, 0.8f, 0.2f);
            if (percent > 0.25f) return new Color(0.9f, 0.7f, 0.1f);
            return new Color(0.9f, 0.2f, 0.2f);
        }
        
        private Color GetTargetColor(TargetType type)
        {
            return type switch
            {
                TargetType.Enemy => new Color(1f, 0.3f, 0.3f),
                TargetType.Friendly => new Color(0.3f, 1f, 0.3f),
                TargetType.Neutral => new Color(1f, 1f, 0.3f),
                _ => Color.white
            };
        }
        
        private void DrawActionBar()
        {
            if (_abilitySystem == null || _testAbilities == null)
                return;
            
            // Position: bottom center of screen
            float barWidth = 400f;
            float barHeight = 60f;
            float x = (Screen.width - barWidth) / 2f;
            float y = Screen.height - barHeight - 10f;
            
            GUILayout.BeginArea(new Rect(x, y, barWidth, barHeight));
            GUILayout.BeginHorizontal(_boxStyle);
            
            for (int i = 0; i < _testAbilities.Length && i < 9; i++)
            {
                var ability = _testAbilities[i];
                var state = _abilitySystem.GetAbility(i);
                
                DrawAbilityButton(i, ability, state);
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            
            // Draw keybind hints below
            GUILayout.BeginArea(new Rect(x, y + barHeight, barWidth, 20f));
            GUILayout.BeginHorizontal();
            for (int i = 0; i < _testAbilities.Length && i < 9; i++)
            {
                GUILayout.Label($"  {i + 1}  ", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fixedWidth = 60 });
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
        
        private void DrawAbilityButton(int slot, AbilityData ability, AbilityState state)
        {
            bool onCooldown = state != null && state.IsOnCooldown;
            bool onGCD = _abilitySystem.IsOnGCD && ability.AffectedByGCD;
            bool disabled = onCooldown || onGCD;
            
            GUI.enabled = !disabled;
            GUI.backgroundColor = disabled ? Color.gray : GetAbilityColor(ability.Type);
            
            string buttonText = ability.AbilityName.Length > 8 
                ? ability.AbilityName.Substring(0, 8) 
                : ability.AbilityName;
            
            if (onCooldown)
            {
                buttonText = $"{state.CooldownRemaining:F1}s";
            }
            
            if (GUILayout.Button(buttonText, _abilityButtonStyle))
            {
                _abilitySystem.TryExecuteAbility(slot);
            }
            
            GUI.enabled = true;
            GUI.backgroundColor = Color.white;
        }
        
        private Color GetAbilityColor(AbilityType type)
        {
            return type switch
            {
                AbilityType.Damage => new Color(1f, 0.4f, 0.4f),
                AbilityType.Healing => new Color(0.4f, 1f, 0.4f),
                AbilityType.Buff => new Color(0.4f, 0.6f, 1f),
                AbilityType.Debuff => new Color(0.8f, 0.4f, 0.8f),
                _ => Color.white
            };
        }
        
        private void DrawCastBar()
        {
            if (_abilitySystem == null || !_abilitySystem.IsCasting)
                return;
            
            var ability = _abilitySystem.CurrentCastAbility;
            float progress = _abilitySystem.CastProgress;
            
            // Position: above action bar
            float barWidth = 300f;
            float barHeight = 30f;
            float x = (Screen.width - barWidth) / 2f;
            float y = Screen.height - 120f;
            
            GUILayout.BeginArea(new Rect(x, y, barWidth, barHeight));
            
            // Background
            GUI.color = new Color(0.2f, 0.2f, 0.2f);
            GUI.Box(new Rect(0, 0, barWidth, barHeight), "");
            
            // Progress fill
            GUI.color = new Color(0.3f, 0.6f, 1f);
            GUI.Box(new Rect(2, 2, (barWidth - 4) * progress, barHeight - 4), "");
            
            // Text
            GUI.color = Color.white;
            GUI.Label(new Rect(0, 0, barWidth, barHeight), ability.AbilityName, 
                new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
            
            GUILayout.EndArea();
        }
        
        private void DrawErrorMessage()
        {
            if (_errorTimer <= 0 || string.IsNullOrEmpty(_lastError))
                return;
            
            // Position: center of screen
            float msgWidth = 300f;
            float msgHeight = 40f;
            float x = (Screen.width - msgWidth) / 2f;
            float y = Screen.height / 2f - 100f;
            
            GUI.color = new Color(1f, 0.3f, 0.3f, _errorTimer / 2f);
            GUI.Label(new Rect(x, y, msgWidth, msgHeight), _lastError, _errorStyle);
            GUI.color = Color.white;
        }
    }
}
