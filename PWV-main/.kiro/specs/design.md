# Design Document: The Ether Domes

## Overview

Este documento define la arquitectura técnica completa para "The Ether Domes", un Micro-MMORPG cooperativo para 1-10 jugadores. El diseño implementa:

- **Networking**: Modelo híbrido Host-Play/Servidor Dedicado con Unity Netcode for GameObjects (NGO)
- **Combate**: Sistema Tab-Target con GCD, Threat/Aggro, y habilidades con cast time
- **Clases**: 4 clases con sistema Trinity flexible (Tank/Healer/DPS)
- **Progresión**: Niveles 1-60 con loot de jefes
- **Mundo**: 5 regiones instanciadas con mazmorras de 3/5 jefes
- **Persistencia**: Personajes Cross-World encriptados, mundo pausado cuando Host offline

## Architecture

```mermaid
graph TB
    subgraph "Presentation Layer"
        UI[UI Manager]
        HUD[Combat HUD]
        ActionBar[Action Bar]
    end
    
    subgraph "Input Layer"
        InputSys[Input System]
        TargetInput[Target Input]
        AbilityInput[Ability Input]
    end
    
    subgraph "Game Systems"
        Combat[Combat System]
        Target[Target System]
        Ability[Ability System]
        Aggro[Aggro System]
        Class[Class System]
        Progression[Progression System]
        Loot[Loot System]
    end
    
    subgraph "World Systems"
        World[World Manager]
        Region[Region System]
        Dungeon[Dungeon System]
        Boss[Boss System]
        GuildBase[Guild Base System]
    end
    
    subgraph "Network Layer"
        NetSession[Network Session Manager]
        ConnApproval[Connection Approval]
        NetTransform[NetworkTransform]
        NetObject[NetworkObject]
    end
    
    subgraph "Data Layer"
        CharPersist[Character Persistence]
        WorldPersist[World Persistence]
        Crypto[Encryption Service]
    end
    
    UI --> Combat
    HUD --> Target
    ActionBar --> Ability
    InputSys --> TargetInput
    InputSys --> AbilityInput
    TargetInput --> Target
    AbilityInput --> Ability
    Combat --> Aggro
    Combat --> Class
    Ability --> Target
    Ability --> Combat
    Progression --> Loot
    World --> Region
    World --> Dungeon
    Dungeon --> Boss
    Boss --> Loot
    NetSession --> ConnApproval
    ConnApproval --> CharPersist
    CharPersist --> Crypto
    World --> WorldPersist


### Connection Flow

```mermaid
sequenceDiagram
    participant C as Client
    participant NM as NetworkManager
    participant CA as Connection Approval
    participant PS as Persistence System
    
    C->>NM: StartClient(IP)
    NM->>CA: OnConnectionApproval(payload)
    CA->>PS: LoadCharacterData()
    PS-->>CA: CharacterData (encrypted)
    CA->>CA: Decrypt & Validate
    alt Valid Data
        CA-->>NM: Approve Connection
        NM->>NM: SpawnPlayerPrefab()
        NM-->>C: Connection Success
    else Invalid Data
        CA-->>NM: Reject Connection
        NM-->>C: Error: Invalid Character
    end
```

### Combat Flow

```mermaid
sequenceDiagram
    participant P as Player
    participant TS as Target System
    participant AS as Ability System
    participant CS as Combat System
    participant AG as Aggro System
    participant E as Enemy
    
    P->>TS: Press Tab
    TS-->>P: Target Enemy
    P->>AS: Press Ability Key (1)
    AS->>AS: Check GCD
    AS->>TS: Get Current Target
    AS->>CS: Execute Ability
    CS->>E: Apply Damage
    CS->>AG: Add Threat (damage)
    AG->>E: Update Target (highest threat)


## Components and Interfaces

### Network Components

#### INetworkSessionManager
```csharp
public interface INetworkSessionManager
{
    void StartAsHost(ushort port = 7777);
    void StartAsClient(string ipAddress, ushort port = 7777);
    void StartAsDedicatedServer(ushort port = 7777);
    void Disconnect();
    
    event Action<ulong> OnPlayerConnected;
    event Action<ulong> OnPlayerDisconnected;
    event Action<string> OnConnectionFailed;
    
    bool IsHost { get; }
    bool IsClient { get; }
    bool IsServer { get; }
    int ConnectedPlayerCount { get; }
    int MaxPlayers { get; } // Returns 10
}
```

#### IConnectionApprovalHandler
```csharp
public interface IConnectionApprovalHandler
{
    ConnectionApprovalResult ValidateConnectionRequest(byte[] payload);
    TimeSpan ValidationTimeout { get; set; }
}

public struct ConnectionApprovalResult
{
    public bool Approved;
    public string RejectionReason;
    public ApprovalErrorCode ErrorCode;
}

public enum ApprovalErrorCode
{
    None = 0,
    InvalidDataFormat = 1,
    CorruptedData = 2,
    StatsOutOfRange = 3,
    ValidationTimeout = 4,
    EncryptionFailure = 5,
    ServerFull = 6
}
```

### Combat Components

#### ITargetSystem
```csharp
public interface ITargetSystem
{
    void CycleTarget();           // Tab key
    void SelectTarget(ITargetable target);  // Click
    void ClearTarget();           // Escape key
    
    ITargetable CurrentTarget { get; }
    bool HasTarget { get; }
    bool IsTargetInRange { get; }
    float TargetDistance { get; }
    
    event Action<ITargetable> OnTargetChanged;
    event Action OnTargetLost;
    
    float MaxTargetRange { get; } // 40 meters
}

public interface ITargetable
{
    uint NetworkId { get; }
    string DisplayName { get; }
    Vector3 Position { get; }
    bool IsAlive { get; }
    TargetType Type { get; } // Enemy, Friendly, Neutral
}
```

#### IAbilitySystem
```csharp
public interface IAbilitySystem
{
    bool TryExecuteAbility(int slotIndex);
    void InterruptCast();
    
    bool IsOnGCD { get; }
    float GCDRemaining { get; }
    bool IsCasting { get; }
    float CastProgress { get; }
    AbilityData CurrentCastAbility { get; }
    
    AbilityData[] GetAbilities();
    float GetCooldownRemaining(int slotIndex);
    
    event Action OnGCDStarted;
    event Action OnGCDEnded;
    event Action<AbilityData> OnCastStarted;
    event Action<AbilityData> OnCastCompleted;
    event Action<AbilityData> OnCastInterrupted;
    event Action<string> OnAbilityError;
    
    float GlobalCooldownDuration { get; } // 1.5 seconds
}
```

#### IAggroSystem
```csharp
public interface IAggroSystem
{
    void AddThreat(ulong playerId, ulong enemyId, float amount);
    void Taunt(ulong playerId, ulong enemyId);
    void AddHealingThreat(ulong healerId, float healAmount, ulong[] engagedEnemies);
    void ResetThreat(ulong enemyId);
    
    ulong GetHighestThreatPlayer(ulong enemyId);
    float GetThreat(ulong playerId, ulong enemyId);
    Dictionary<ulong, float> GetThreatTable(ulong enemyId);
    
    float MeleeThreatThreshold { get; }  // 1.1 (110%)
    float RangedThreatThreshold { get; } // 1.3 (130%)
    float HealingThreatMultiplier { get; } // 0.5 (50%)
}
```

#### ICombatSystem
```csharp
public interface ICombatSystem
{
    void ApplyDamage(ulong targetId, float damage, DamageType type);
    void ApplyHealing(ulong targetId, float healing);
    void Kill(ulong targetId);
    void Resurrect(ulong targetId, float healthPercent);
    void ReleaseSpirit(ulong playerId);
    
    bool IsInCombat(ulong entityId);
    bool IsDead(ulong entityId);
    float GetHealth(ulong entityId);
    float GetMaxHealth(ulong entityId);
    
    event Action<ulong> OnEntityDied;
    event Action<ulong> OnEntityResurrected;
    event Action OnWipe;
    
    float ResurrectionWindowSeconds { get; } // 60
    float RespawnHealthPercent { get; } // 0.5
}
```

### Class Components

#### IClassSystem
```csharp
public interface IClassSystem
{
    CharacterClass GetClass(ulong playerId);
    Specialization GetSpecialization(ulong playerId);
    void SetSpecialization(ulong playerId, Specialization spec);
    
    AbilityData[] GetClassAbilities(CharacterClass charClass, Specialization spec);
    bool CanSwitchSpec(ulong playerId); // Only out of combat
    float GetSpecEffectiveness(ulong playerId); // 1.0 main, 0.7 off-spec
    
    event Action<ulong, Specialization> OnSpecializationChanged;
}

public enum CharacterClass
{
    Warrior,  // Tank, DPS
    Mage,     // DPS
    Priest,   // Healer
    Paladin   // Tank, Healer, DPS
}

public enum Specialization
{
    // Warrior
    Protection,    // Tank
    Arms,          // DPS
    
    // Mage
    Fire,          // DPS
    Frost,         // DPS
    
    // Priest
    Holy,          // Healer
    Shadow,        // DPS
    
    // Paladin
    ProtectionPaladin, // Tank
    HolyPaladin,       // Healer
    Retribution        // DPS
}
```

### Progression Components

#### IProgressionSystem
```csharp
public interface IProgressionSystem
{
    void AddExperience(ulong playerId, int amount);
    int GetLevel(ulong playerId);
    int GetExperience(ulong playerId);
    int GetExperienceToNextLevel(ulong playerId);
    
    int CalculateExperienceReward(int playerLevel, int enemyLevel);
    CharacterStats GetBaseStats(int level, CharacterClass charClass);
    
    event Action<ulong, int> OnLevelUp;
    event Action<ulong, AbilityData> OnAbilityUnlocked;
    
    int MaxLevel { get; } // 60
    int[] AbilityUnlockLevels { get; } // 10, 20, 30, 40, 50
}
```

#### ILootSystem
```csharp
public interface ILootSystem
{
    LootDrop[] GenerateLoot(string bossId, int groupSize);
    void DistributeLoot(LootDrop[] loot, ulong[] playerIds, LootDistributionMode mode);
    
    event Action<ulong, ItemData> OnItemAwarded;
}

public enum LootDistributionMode
{
    RoundRobin,
    NeedGreed,
    MasterLooter
}

public enum ItemRarity
{
    Common,   // 70%
    Rare,     // 25%
    Epic      // 5%
}
```

#### IEquipmentSystem
```csharp
public interface IEquipmentSystem
{
    bool CanEquip(ulong playerId, ItemData item);
    void Equip(ulong playerId, ItemData item, EquipmentSlot slot);
    void Unequip(ulong playerId, EquipmentSlot slot);
    
    ItemData GetEquippedItem(ulong playerId, EquipmentSlot slot);
    CharacterStats GetEquipmentStats(ulong playerId);
    
    event Action<ulong, ItemData, EquipmentSlot> OnItemEquipped;
}
```


### World Components

#### IWorldManager
```csharp
public interface IWorldManager
{
    void LoadRegion(RegionId region);
    void UnloadRegion(RegionId region);
    RegionId GetCurrentRegion(ulong playerId);
    
    RegionData GetRegionData(RegionId region);
    bool CanEnterRegion(ulong playerId, RegionId region);
    
    event Action<RegionId> OnRegionLoading;
    event Action<RegionId> OnRegionLoaded;
}

public enum RegionId
{
    Roca,      // Levels 1-15
    Bosque,    // Levels 15-30
    Nieve,     // Levels 30-40
    Pantano,   // Levels 40-50
    Ciudadela  // Levels 50-60
}

public class RegionData
{
    public RegionId Id;
    public string DisplayName;
    public int MinLevel;
    public int MaxLevel;
    public Vector3[] SpawnPoints;
    public string[] DungeonIds;
}
```

#### IDungeonSystem
```csharp
public interface IDungeonSystem
{
    string CreateInstance(string dungeonId, ulong[] groupMembers);
    void EnterInstance(string instanceId, ulong playerId);
    void LeaveInstance(ulong playerId);
    void DestroyInstance(string instanceId);
    
    DungeonInstanceData GetInstanceData(string instanceId);
    bool IsBossDefeated(string instanceId, int bossIndex);
    void MarkBossDefeated(string instanceId, int bossIndex);
    bool IsCompleted(string instanceId);
    
    event Action<string, int> OnBossDefeated;
    event Action<string> OnDungeonCompleted;
    event Action<string> OnWipe;
    
    float InstanceDestroyDelayMinutes { get; } // 5
}

public class DungeonInstanceData
{
    public string InstanceId;
    public string DungeonId;
    public DungeonSize Size;
    public int BossCount;
    public bool[] BossesDefeated;
    public ulong[] GroupMembers;
    public int GroupSize;
    public float DifficultyMultiplier;
}

public enum DungeonSize
{
    Small, // 3 bosses
    Large  // 5 bosses
}
```

#### IBossSystem
```csharp
public interface IBossSystem
{
    void StartEncounter(string instanceId, int bossIndex);
    void EndEncounter(string instanceId, int bossIndex, bool victory);
    
    BossPhase GetCurrentPhase(string instanceId, int bossIndex);
    float GetHealthPercent(string instanceId, int bossIndex);
    
    event Action<string, int> OnEncounterStarted;
    event Action<string, int, BossPhase> OnPhaseTransition;
    event Action<string, int> OnEncounterEnded;
}

public enum BossPhase
{
    Phase1,  // 100% - 75%
    Phase2,  // 75% - 50%
    Phase3,  // 50% - 25%
    Phase4   // 25% - 0%
}
```

#### IGuildBaseSystem
```csharp
public interface IGuildBaseSystem
{
    void Enter(ulong playerId);
    void Leave(ulong playerId);
    
    void PlaceFurniture(FurnitureData furniture, Vector3 position, Quaternion rotation);
    void RemoveFurniture(string furnitureInstanceId);
    void UnlockTrophy(string bossId);
    
    GuildBaseState GetState();
    bool IsTrophyUnlocked(string bossId);
    
    event Action<string> OnTrophyUnlocked;
}
```

### Persistence Components

#### ICharacterPersistenceService
```csharp
public interface ICharacterPersistenceService
{
    Task<bool> SaveCharacterAsync(CharacterData data);
    Task<CharacterData> LoadCharacterAsync(string characterId);
    bool ValidateCharacterIntegrity(CharacterData data);
    
    byte[] ExportCharacterForNetwork(CharacterData data);
    CharacterData ImportCharacterFromNetwork(byte[] payload);
}
```

#### IWorldPersistenceService
```csharp
public interface IWorldPersistenceService
{
    Task SaveWorldStateAsync(WorldState state);
    Task<WorldState> LoadWorldStateAsync();
    
    void EnableAutoSave(TimeSpan interval); // For dedicated server
    void DisableAutoSave();
}

public class WorldState
{
    public Dictionary<string, bool[]> DungeonProgress; // DungeonId -> BossesDefeated
    public GuildBaseState GuildBase;
    public DateTime LastSaveTime;
}
```

## Data Models

### Character Data
```csharp
[Serializable]
public class CharacterData
{
    public string CharacterId;
    public string CharacterName;
    public CharacterClass Class;
    public Specialization CurrentSpec;
    public int Level;
    public int Experience;
    public CharacterStats BaseStats;
    public EquipmentData Equipment;
    public AbilityData[] UnlockedAbilities;
    public DateTime LastSaveTime;
    public byte[] IntegrityHash;
}

[Serializable]
public class CharacterStats
{
    public int Health;
    public int MaxHealth;
    public int Mana;
    public int MaxMana;
    public int Strength;
    public int Intellect;
    public int Stamina;
    public int AttackPower;
    public int SpellPower;
    public int Armor;
}

[Serializable]
public class EquipmentData
{
    public Dictionary<EquipmentSlot, ItemData> EquippedItems;
    public int TotalItemLevel;
}

[Serializable]
public class ItemData
{
    public string ItemId;
    public string ItemName;
    public int ItemLevel;
    public ItemRarity Rarity;
    public EquipmentSlot Slot;
    public Dictionary<string, int> Stats;
    public int RequiredLevel;
    public CharacterClass[] AllowedClasses;
}

public enum EquipmentSlot
{
    Head, Shoulders, Chest, Hands, Legs, Feet,
    MainHand, OffHand, Trinket1, Trinket2
}
```

### Ability Data
```csharp
[Serializable]
public class AbilityData
{
    public string AbilityId;
    public string AbilityName;
    public string Description;
    public Sprite Icon;
    public float CastTime;        // 0 for instant
    public float Cooldown;
    public float ManaCost;
    public float Range;
    public bool RequiresTarget;
    public bool AffectedByGCD;
    public AbilityType Type;
    public CharacterClass RequiredClass;
    public Specialization? RequiredSpec;
    public int UnlockLevel;
}

public enum AbilityType
{
    Damage,
    Healing,
    Buff,
    Debuff,
    Taunt,
    Utility
}
```


## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Network Properties

**Property 1: Session State Consistency**
*For any* session start operation (Host, Client, or DedicatedServer), the resulting state flags (IsHost, IsClient, IsServer) SHALL be mutually consistent:
- Host: IsHost=true, IsClient=true, IsServer=true
- Client: IsHost=false, IsClient=true, IsServer=false
- DedicatedServer: IsHost=false, IsClient=false, IsServer=true
**Validates: Requirements 1.1, 1.3**

**Property 2: Player Count Enforcement**
*For any* session with N connected players where N >= MaxPlayers (10), subsequent connection attempts SHALL be rejected.
**Validates: Requirements 1.6**

**Property 3: Ownership-Based Input Processing**
*For any* PlayerController instance and any movement input, if IsOwner is false, the input SHALL be ignored and CurrentVelocity SHALL remain unchanged.
**Validates: Requirements 3.1, 3.2**

**Property 4: Character Persistence Round-Trip**
*For any* valid CharacterData, calling SaveCharacterAsync followed by LoadCharacterAsync with the same characterId SHALL return an equivalent CharacterData object.
**Validates: Requirements 7.1, 7.3**

**Property 5: Encrypted Storage Non-Plaintext**
*For any* saved CharacterData, the raw bytes written to storage SHALL NOT contain the CharacterName as a plaintext substring.
**Validates: Requirements 7.2**

### Combat Properties

**Property 6: Tab Target Cycling**
*For any* list of N visible enemies within 40 meters, pressing Tab N times SHALL cycle through all enemies exactly once before returning to the first.
**Validates: Requirements 8.1**

**Property 7: Target Death Clears Selection**
*For any* selected target, when that target's IsAlive becomes false, CurrentTarget SHALL become null.
**Validates: Requirements 8.4**

**Property 8: Target Range Tracking**
*For any* selected target at distance D, IsTargetInRange SHALL equal (D <= 40).
**Validates: Requirements 8.5**

**Property 9: GCD Enforcement**
*For any* ability execution that triggers GCD, all subsequent ability attempts within 1.5 seconds SHALL fail if AffectedByGCD is true.
**Validates: Requirements 9.2, 9.3**

**Property 10: Cast Interruption on Movement**
*For any* ability with CastTime > 0, if player movement occurs during cast, the cast SHALL be interrupted and OnCastInterrupted SHALL fire.
**Validates: Requirements 9.4, 9.5**

**Property 11: Targeted Ability Requires Target**
*For any* ability with RequiresTarget=true, if CurrentTarget is null, TryExecuteAbility SHALL return false and fire OnAbilityError.
**Validates: Requirements 9.7**

**Property 12: Threat Calculation Correctness**
*For any* damage dealt D to enemy E by player P, GetThreat(P, E) SHALL increase by exactly D.
**Validates: Requirements 10.1**

**Property 13: Taunt Sets Highest Threat**
*For any* taunt by player P on enemy E, GetThreat(P, E) SHALL equal GetHighestThreat(E) * 1.1.
**Validates: Requirements 10.2**

**Property 14: Healing Threat Distribution**
*For any* healing H by healer P with N engaged enemies, each enemy SHALL receive Threat increase of (H * 0.5) / N.
**Validates: Requirements 10.3**

**Property 15: Enemy Targets Highest Threat**
*For any* enemy E with threat table, GetHighestThreatPlayer(E) SHALL return the player with maximum threat value.
**Validates: Requirements 10.4**

**Property 16: Threat Reset on Combat End**
*For any* enemy E, when combat ends (5 seconds no combat), all threat values for E SHALL be 0.
**Validates: Requirements 10.6**

**Property 17: Death State Transition**
*For any* entity with Health reaching 0, IsDead SHALL return true and all actions SHALL be disabled.
**Validates: Requirements 11.1**

**Property 18: Death Preserves Inventory**
*For any* player death, Equipment and Experience SHALL remain unchanged after death and respawn.
**Validates: Requirements 11.6**

### Class Properties

**Property 19: Class Ability Assignment**
*For any* CharacterClass C and Specialization S, GetClassAbilities(C, S) SHALL return a non-empty set of abilities appropriate for that combination.
**Validates: Requirements 12.2**

**Property 20: Spec Switching Out of Combat**
*For any* player P, CanSwitchSpec(P) SHALL return true if and only if IsInCombat(P) is false.
**Validates: Requirements 12.3**

**Property 21: Off-Spec Effectiveness**
*For any* Hybrid_Class in Off_Spec, GetSpecEffectiveness SHALL return 0.7.
**Validates: Requirements 13.2**

**Property 22: Group Size Scaling**
*For any* dungeon with group size N (2-10), DifficultyMultiplier SHALL scale proportionally to N.
**Validates: Requirements 13.4, 17.4**

### Progression Properties

**Property 23: Experience Calculation**
*For any* enemy at level E killed by player at level P:
- If E >= P-10: XP = BaseXP * (E/P)
- If E < P-10: XP = BaseXP * 0.1 (reduced)
**Validates: Requirements 14.1, 14.6**

**Property 24: Level Progression Bounds**
*For any* player, Level SHALL be in range [1, 60] and SHALL increase by exactly 1 when Experience threshold is reached.
**Validates: Requirements 14.2, 14.3**

**Property 25: Level Up Stats Increase**
*For any* level up from L to L+1, all base stats SHALL increase by a positive amount.
**Validates: Requirements 14.4**

**Property 26: Ability Unlock at Milestones**
*For any* player reaching level 10, 20, 30, 40, or 50, OnAbilityUnlocked SHALL fire with a new ability.
**Validates: Requirements 14.5**

**Property 27: Loot Drop Rate Distribution**
*For any* large sample of loot drops, the distribution SHALL approximate: Common 70%, Rare 25%, Epic 5%.
**Validates: Requirements 15.2**

**Property 28: Equipment Stats Application**
*For any* item equipped, GetEquipmentStats SHALL include all stats from that item.
**Validates: Requirements 15.4**

**Property 29: Equipment Level Requirement**
*For any* item with RequiredLevel R and player at level L, CanEquip SHALL return (L >= R).
**Validates: Requirements 15.5**

**Property 30: Equipment Serialization Round-Trip**
*For any* valid EquipmentData, serializing then deserializing SHALL produce an equivalent object.
**Validates: Requirements 15.6**

### World Properties

**Property 31: Region Enemy Level Scaling**
*For any* region R, all spawned enemies SHALL have levels within [R.MinLevel, R.MaxLevel].
**Validates: Requirements 16.5**

**Property 32: Dungeon Instance Isolation**
*For any* two dungeon instances I1 and I2, events in I1 SHALL NOT affect I2.
**Validates: Requirements 17.2**

**Property 33: Boss Progress Tracking**
*For any* dungeon instance, when all BossesDefeated[i] are true, IsCompleted SHALL return true.
**Validates: Requirements 17.5, 17.6**

**Property 34: Boss Phase Transitions**
*For any* boss at health thresholds (75%, 50%, 25%), OnPhaseTransition SHALL fire with the correct phase.
**Validates: Requirements 18.3**

**Property 35: World State Persistence Round-Trip**
*For any* WorldState, SaveWorldStateAsync followed by LoadWorldStateAsync SHALL return an equivalent state.
**Validates: Requirements 20.1, 20.2**

**Property 36: World Pause When Offline**
*For any* Host-based session, when Host disconnects, no game time SHALL progress until reconnection.
**Validates: Requirements 20.3**


## Error Handling

### Network Errors

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| Connection timeout | Fire OnConnectionFailed with timeout message, allow retry |
| Host unreachable | Fire OnConnectionFailed with "Host not found" message |
| Host disconnects mid-session | Notify all clients, save world state, clean up |
| Max players reached | Reject with ServerFull error code |
| Invalid character data | Reject with appropriate ApprovalErrorCode |

### Combat Errors

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| No target selected | Display "No target" message, ability not executed |
| Target out of range | Display "Out of range" message, ability not executed |
| On cooldown | Display remaining cooldown, ability not executed |
| Insufficient mana | Display "Not enough mana" message |
| Cast interrupted | Fire OnCastInterrupted, refund partial mana |

### Persistence Errors

| Error Scenario | Handling Strategy |
|----------------|-------------------|
| Corrupted save file | Notify player, offer to create new character |
| Encryption failure | Log error, retry with backup key |
| Disk full | Notify player, suggest cleanup |
| Auto-save failure | Retry in 30 seconds, log warning |

## Testing Strategy

### Unit Tests

Unit tests verify specific examples and edge cases:

1. **Network Tests**
   - StartAsHost sets correct state flags
   - Connection rejection with invalid data
   - Player count enforcement at limit

2. **Combat Tests**
   - Tab cycling through enemy list
   - GCD blocking ability execution
   - Cast interruption on movement
   - Threat calculation accuracy

3. **Class Tests**
   - Ability assignment per class/spec
   - Spec switching restrictions
   - Off-spec effectiveness multiplier

4. **Progression Tests**
   - XP calculation formulas
   - Level up stat increases
   - Ability unlocks at milestones

5. **World Tests**
   - Region level bounds
   - Dungeon instance creation/destruction
   - Boss phase transitions

### Property-Based Tests

Property-based tests use **FsCheck** (C#/.NET) to generate random inputs and verify universal properties.

**Configuration:**
- Minimum 100 iterations per test
- Each test references its design document property
- Tag format: `// Feature: ether-domes, Property N: [title]`

**Key Properties to Test:**
- P4: Character Persistence Round-Trip
- P9: GCD Enforcement
- P12-16: Threat System Correctness
- P23-24: Experience and Level Progression
- P27: Loot Drop Rate Distribution
- P30: Equipment Serialization Round-Trip
- P35: World State Persistence Round-Trip

### Integration Tests

1. **Full Connection Flow** - Host + Client connection with character validation
2. **Combat Scenario** - Target, ability, damage, threat, death cycle
3. **Dungeon Run** - Enter, fight bosses, loot, complete
4. **Cross-World Character** - Save on World A, load on World B

## Project Structure

```
TheEtherDomes/
├── .kiro/
│   └── specs/
│       ├── requirements.md
│       ├── design.md
│       └── tasks.md
├── Assets/
│   ├── _Project/
│   │   ├── Scripts/
│   │   │   ├── Network/
│   │   │   │   ├── NetworkSessionManager.cs
│   │   │   │   ├── ConnectionApprovalHandler.cs
│   │   │   │   └── Interfaces/
│   │   │   ├── Combat/
│   │   │   │   ├── TargetSystem.cs
│   │   │   │   ├── AbilitySystem.cs
│   │   │   │   ├── AggroSystem.cs
│   │   │   │   ├── CombatSystem.cs
│   │   │   │   └── Interfaces/
│   │   │   ├── Classes/
│   │   │   │   ├── ClassSystem.cs
│   │   │   │   └── Abilities/
│   │   │   ├── Progression/
│   │   │   │   ├── ProgressionSystem.cs
│   │   │   │   ├── LootSystem.cs
│   │   │   │   ├── EquipmentSystem.cs
│   │   │   │   └── Interfaces/
│   │   │   ├── World/
│   │   │   │   ├── WorldManager.cs
│   │   │   │   ├── DungeonSystem.cs
│   │   │   │   ├── BossSystem.cs
│   │   │   │   ├── GuildBaseSystem.cs
│   │   │   │   └── Interfaces/
│   │   │   ├── Persistence/
│   │   │   │   ├── CharacterPersistenceService.cs
│   │   │   │   ├── WorldPersistenceService.cs
│   │   │   │   ├── EncryptionService.cs
│   │   │   │   └── Interfaces/
│   │   │   ├── Player/
│   │   │   │   ├── PlayerController.cs
│   │   │   │   └── PlayerInput.cs
│   │   │   ├── UI/
│   │   │   │   ├── CombatHUD.cs
│   │   │   │   ├── ActionBar.cs
│   │   │   │   └── TargetFrame.cs
│   │   │   └── Data/
│   │   │       ├── CharacterData.cs
│   │   │       ├── AbilityData.cs
│   │   │       ├── ItemData.cs
│   │   │       └── Enums.cs
│   │   ├── ScriptableObjects/
│   │   │   ├── Abilities/
│   │   │   ├── Items/
│   │   │   ├── Bosses/
│   │   │   └── Regions/
│   │   ├── Prefabs/
│   │   │   ├── Player/
│   │   │   ├── Enemies/
│   │   │   ├── Bosses/
│   │   │   └── UI/
│   │   └── Scenes/
│   │       ├── MainMenu.unity
│   │       ├── GuildBase.unity
│   │       ├── Regions/
│   │       └── Dungeons/
│   ├── Plugins/
│   │   └── FsCheck/ (for property testing)
│   └── Tests/
│       ├── EditMode/
│       └── PlayMode/
├── Packages/
│   └── manifest.json
└── ProjectSettings/
```
