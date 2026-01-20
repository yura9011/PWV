# Design Document - Phase 3: Combat System and Abilities

## Overview

This design implements a professional Tab-Targeting combat system for EtherDomes. The architecture prioritizes responsiveness through input buffering, server-authoritative validation for fairness, and flexible ability configuration via ScriptableObjects. The system supports class-specific mechanics (stationary casters vs mobile archers) and integrates with Netcode for GameObjects for multiplayer synchronization.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Combat System                             │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │  Targeting  │  │   Combat    │  │    Ability System       │  │
│  │   System    │  │   State     │  │                         │  │
│  │             │  │  Machine    │  │  ┌─────────────────┐    │  │
│  │ - Tab Logic │  │             │  │  │ AbilityDefinition│    │  │
│  │ - LoS Check │  │ - Idle      │  │  │ ScriptableObject│    │  │
│  │ - AutoSwitch│  │ - Casting   │  │  └─────────────────┘    │  │
│  └──────┬──────┘  │ - GCD       │  │                         │  │
│         │         │ - Locked    │  │  ┌─────────────────┐    │  │
│         │         └──────┬──────┘  │  │  Spell Queue    │    │  │
│         │                │         │  │  (400ms buffer) │    │  │
│         │                │         │  └─────────────────┘    │  │
│         └────────────────┴─────────┴─────────────────────────┘  │
│                              │                                   │
│  ┌───────────────────────────┴───────────────────────────────┐  │
│  │                    Network Layer (NGO)                     │  │
│  │  - RequestCastServerRPC                                    │  │
│  │  - BroadcastVisualsClientRPC                               │  │
│  │  - NetworkVariable<Health>                                 │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Components and Interfaces

### 1. TargetingSystem

```csharp
public class TargetingSystem : NetworkBehaviour
{
    // Configuration
    public float MaxTargetRange = 40f;
    public float AutoSwitchRange = 10f;
    public LayerMask EnemyLayer;
    
    // State
    public ITargetable CurrentTarget { get; private set; }
    
    // Events
    public event Action<ITargetable> OnTargetChanged;
    public event Action OnTargetCleared;
    
    // Methods
    public void TabTarget();           // Cycle to next valid target
    public void SetTarget(ITargetable target);
    public void ClearTarget();
    public bool HasValidTarget();
    public bool IsTargetInRange(float range);
    public bool HasLineOfSight();
    
    // Internal
    private List<ITargetable> GetValidTargets();
    private float CalculateTargetPriority(ITargetable target);
    private void OnEnemyDeath(ITargetable enemy);
}

public interface ITargetable
{
    NetworkObject NetworkObject { get; }
    string DisplayName { get; }
    float CurrentHealth { get; }
    float MaxHealth { get; }
    bool IsAlive { get; }
    Vector3 Position { get; }
    float GetThreatTo(ulong playerId);
}
```

### 2. CombatStateMachine

```csharp
public enum CombatState
{
    Idle,
    Casting,
    GlobalCooldown,
    Locked
}

public class CombatStateMachine : NetworkBehaviour
{
    // State
    public CombatState CurrentState { get; private set; }
    public float StateTimeRemaining { get; private set; }
    public AbilityDefinitionSO CurrentCastingAbility { get; private set; }
    
    // Configuration
    public float GlobalCooldownDuration = 1.5f;
    
    // Events
    public event Action<CombatState> OnStateChanged;
    public event Action<float, float> OnCastProgress; // current, total
    public event Action OnCastInterrupted;
    public event Action OnCastCompleted;
    
    // Methods
    public bool CanUseAbility(AbilityDefinitionSO ability);
    public void StartCasting(AbilityDefinitionSO ability);
    public void InterruptCast();
    public void StartGCD();
    public void Lock(float duration);
    public void Unlock();
    
    // Internal
    private void UpdateState(float deltaTime);
    private void TransitionTo(CombatState newState);
}
```

### 3. SpellQueue (Input Buffering)

```csharp
public class SpellQueue : MonoBehaviour
{
    // Configuration
    public float BufferWindow = 0.4f; // 400ms
    
    // State
    public AbilityDefinitionSO QueuedAbility { get; private set; }
    public int QueuedAbilitySlot { get; private set; }
    
    // Methods
    public bool TryQueueAbility(AbilityDefinitionSO ability, int slot);
    public AbilityDefinitionSO ConsumeQueuedAbility();
    public void ClearQueue();
    public bool HasQueuedAbility();
    
    // Internal
    private bool IsWithinBufferWindow();
}
```

### 4. AbilityDefinitionSO

```csharp
[CreateAssetMenu(fileName = "NewAbility", menuName = "EtherDomes/Ability Definition")]
public class AbilityDefinitionSO : ScriptableObject
{
    [Header("Identity")]
    public int AbilityID;
    public string AbilityName;
    public string Description;
    public Sprite Icon;
    
    [Header("Costs and Timing")]
    public float CastTime;           // 0 = Instant
    public float Cooldown;
    public float Range;
    public int ResourceCost;
    public SecondaryResourceType ResourceType;
    
    [Header("Behavior")]
    public bool RequiresStationary;  // TRUE: Cancel on move
    public bool TriggersGCD;         // TRUE: Activates GCD
    public bool IsOffensive;         // TRUE: Requires enemy target
    public bool IsSelfCast;          // TRUE: Targets self
    
    [Header("Damage/Healing")]
    public float BaseDamage;
    public float BaseHealing;
    public DamageType DamageType;
    
    [Header("Visual")]
    public GameObject ProjectilePrefab;  // null = instant/hitscan
    public float ProjectileSpeed;
    public GameObject ImpactEffectPrefab;
    public AudioClip CastSound;
    public AudioClip ImpactSound;
    
    // Validation
    public bool IsInstant => CastTime <= 0;
    public bool IsProjectile => ProjectilePrefab != null;
}
```

### 5. AbilityExecutor (Network)

```csharp
public class AbilityExecutor : NetworkBehaviour
{
    // Dependencies
    private TargetingSystem _targeting;
    private CombatStateMachine _stateMachine;
    private SpellQueue _spellQueue;
    private SecondaryResourceSystem _resourceSystem;
    
    // Ability Slots
    public AbilityDefinitionSO[] AbilitySlots = new AbilityDefinitionSO[10];
    private float[] _cooldowns = new float[10];
    
    // Events
    public event Action<int, float> OnCooldownStarted; // slot, duration
    public event Action<AbilityDefinitionSO, ITargetable> OnAbilityExecuted;
    public event Action<string> OnAbilityError;
    
    // Client Methods
    public void TryUseAbility(int slot);
    public bool IsAbilityReady(int slot);
    public float GetCooldownRemaining(int slot);
    
    // RPCs
    [ServerRpc]
    private void RequestCastServerRpc(int abilitySlot, NetworkObjectReference targetRef);
    
    [ClientRpc]
    private void BroadcastVisualsClientRpc(int abilitySlot, NetworkObjectReference targetRef);
    
    [ClientRpc]
    private void AbilityErrorClientRpc(string errorMessage);
    
    // Server Validation
    private bool ValidateAbilityServer(int slot, ITargetable target, out string error);
}
```

### 6. HealthSystem

```csharp
public class HealthSystem : NetworkBehaviour, IDamageable
{
    // Network State
    public NetworkVariable<float> CurrentHealth = new();
    public NetworkVariable<float> MaxHealth = new();
    public NetworkVariable<bool> IsDead = new();
    
    // Events
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action<float, DamageType> OnDamageTaken;
    public event Action OnDeath;
    
    // Server Methods
    [Server]
    public void TakeDamage(float amount, DamageType type, ulong attackerId);
    
    [Server]
    public void Heal(float amount, ulong healerId);
    
    [Server]
    public void SetMaxHealth(float max);
    
    // Client Methods
    public float GetHealthPercent();
    
    // Internal
    private void OnHealthValueChanged(float previous, float current);
}

public interface IDamageable
{
    void TakeDamage(float amount, DamageType type, ulong attackerId);
    bool IsDead { get; }
}
```

### 7. ProjectileController

```csharp
public class ProjectileController : NetworkBehaviour
{
    // Configuration
    public float Speed;
    public float MaxLifetime = 10f;
    
    // State
    private NetworkObjectReference _targetRef;
    private AbilityDefinitionSO _ability;
    private ulong _casterId;
    
    // Methods
    public void Initialize(AbilityDefinitionSO ability, NetworkObjectReference target, ulong caster);
    
    // Internal
    private void Update(); // Home toward target
    private void OnReachTarget();
    
    [ClientRpc]
    private void PlayImpactEffectClientRpc();
}
```

### 8. FloatingCombatText

```csharp
public class FloatingCombatText : MonoBehaviour
{
    public static void Spawn(Vector3 position, float amount, DamageType type, bool isCrit);
    public static void SpawnHeal(Vector3 position, float amount, bool isCrit);
    
    // Animation
    private void Animate(); // Float up and fade
}
```

## Data Models

### Combat Configuration

```csharp
[CreateAssetMenu(fileName = "CombatConfig", menuName = "EtherDomes/Combat Configuration")]
public class CombatConfigSO : ScriptableObject
{
    [Header("Targeting")]
    public float MaxTargetRange = 40f;
    public float AutoSwitchRange = 10f;
    public float TabConeAngle = 90f;
    
    [Header("Combat Timing")]
    public float GlobalCooldownDuration = 1.5f;
    public float SpellQueueWindow = 0.4f;
    
    [Header("Layers")]
    public LayerMask EnemyLayer;
    public LayerMask ObstacleLayer;
}
```

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do.*

### Property 1: Tab Target Range Filter
*For any* set of enemies in the scene, the Tab algorithm SHALL only consider enemies within the configured MaxTargetRange (40m), excluding all enemies beyond this distance.
**Validates: Requirements 1.1**

### Property 2: Tab Target Priority by Dot Product
*For any* set of valid targets within range and frustum, the Tab algorithm SHALL select the target with the highest dot product value between Camera.forward and direction to enemy.
**Validates: Requirements 1.3**

### Property 3: Line of Sight Validation
*For any* target selection attempt, the system SHALL only select targets that pass a raycast check from player to target without obstacle collision on the ObstacleLayer.
**Validates: Requirements 1.4**

### Property 4: Tab Cycling
*For any* sequence of Tab presses, the system SHALL cycle through all valid targets without repeating until all have been visited.
**Validates: Requirements 1.6**

### Property 5: Auto-Switch Priority
*For any* auto-switch scenario when current target dies, the system SHALL: (1) only consider enemies within AutoSwitchRange (10m), (2) prioritize enemies with Threat > 0, (3) if no threatening enemies, select closest.
**Validates: Requirements 2.1, 2.2, 2.3**

### Property 6: Instant Cast Execution
*For any* ability with CastTime = 0, the ability SHALL execute immediately without entering Casting state.
**Validates: Requirements 3.5**

### Property 7: Stationary Cast Interruption
*For any* ability with RequiresStationary=true being cast, if player movement input is detected, the cast SHALL be interrupted immediately and OnCastInterrupted event SHALL fire.
**Validates: Requirements 3.6, 4.4**

### Property 8: Off-GCD During GCD
*For any* ability with TriggersGCD=false, the ability SHALL be usable during GlobalCooldown state (assuming cooldown and resources are available).
**Validates: Requirements 3.7, 4.6**

### Property 9: GCD Blocking
*For any* ability with TriggersGCD=true, the ability SHALL NOT be usable during GlobalCooldown state.
**Validates: Requirements 4.5**

### Property 10: Locked State Blocking
*For any* ability, while Combat_State_Machine is in Locked state, the ability SHALL NOT be usable.
**Validates: Requirements 4.7**

### Property 11: Spell Queue Buffer Window
*For any* ability input during Casting or GCD state with time remaining less than BufferWindow (400ms), the ability SHALL be stored as NextAbility and automatically executed when state ends.
**Validates: Requirements 5.2, 5.3**

### Property 12: Spell Queue Replacement
*For any* new ability queued while another is already queued, the new ability SHALL replace the previous queued ability.
**Validates: Requirements 5.4**

### Property 13: Server Validation Completeness
*For any* ability cast request, the server SHALL validate all of: resource availability, target alive status, target in range, and cooldown passed. If any fails, ability SHALL NOT execute.
**Validates: Requirements 6.3**

### Property 14: Resource Consumption on Success
*For any* successful ability validation, the server SHALL consume the ability's resource cost before starting the cast.
**Validates: Requirements 6.5**

### Property 15: Damage Formula Consistency
*For any* damage calculation, the formula (BaseDamage * StatMultiplier) - Mitigation SHALL be applied, and result SHALL be non-negative.
**Validates: Requirements 6.6**

### Property 16: Health Death Threshold
*For any* entity, when CurrentHealth reaches 0 or below, IsDead SHALL be set to true.
**Validates: Requirements 8.3**

### Property 17: Cooldown Accuracy
*For any* ability with Cooldown > 0, after execution the ability SHALL return IsAbilityReady=false until exactly Cooldown seconds have passed.
**Validates: Requirements 3.4 (implicit)**

## Error Handling

### Client-Side Errors
- **Out of Range**: Display "Target is out of range" message
- **No Target**: Display "No target selected" for offensive abilities
- **On Cooldown**: Display "Ability not ready" with remaining time
- **Insufficient Resource**: Display "Not enough [Mana/Colera/etc]"
- **Cannot Move**: Display "Cannot cast while moving" for stationary abilities

### Server-Side Validation Failures
- Return specific error code via ClientRpc
- Client displays appropriate error message
- No resource consumption on failure
- No cooldown triggered on failure

### Network Errors
- Timeout on cast request: Cancel local cast bar
- Desync detection: Request state refresh from server

## Testing Strategy

### Unit Tests
- AbilityDefinitionSO validation (required fields, valid ranges)
- Cooldown timer accuracy
- Spell queue buffer window timing
- Combat state transitions

### Property-Based Tests
- Tab targeting priority calculation with random enemy positions
- Line of sight validation with random obstacle configurations
- Spell queue timing with random input timings
- Health synchronization with random damage values

### Integration Tests
- Full cast flow: input → server validation → execution → damage
- Auto-switch on enemy death
- Movement interruption during stationary cast
- Off-GCD ability during GCD state

### Manual Testing
- Visual feedback (cast bars, projectiles, floating text)
- Network latency simulation
- Multi-client synchronization
