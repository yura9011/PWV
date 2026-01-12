# Requirements Document - Phase 3: Combat System and Abilities

## Introduction

This phase implements a professional "Tab-Targeting" combat cycle for the EtherDomes micro-MMORPG. The system includes intelligent target selection, ability queuing (Spell Queue/Input Buffering), class-specific mechanics (stationary Mage vs mobile Archer), and server-authoritative combat synchronized via Netcode for GameObjects.

## Glossary

- **Targeting_System**: Component responsible for selecting and managing combat targets
- **Tab_Algorithm**: Logic that selects the most relevant enemy based on camera view cone
- **Spell_Queue**: Input buffering system that queues abilities during GCD or casting
- **GCD**: Global Cooldown - shared cooldown between most abilities (1.5s)
- **Off_GCD**: Abilities that bypass the Global Cooldown
- **Combat_State_Machine**: State machine managing player combat states (Idle, Casting, GCD, Locked)
- **AbilityDefinitionSO**: ScriptableObject defining ability properties
- **LoS**: Line of Sight - raycast check for obstacles between caster and target
- **Floating_Combat_Text**: Visual damage/healing numbers that appear above targets

## Requirements

### Requirement 1: Target Selection System

**User Story:** As a player, I want to select enemies intelligently using Tab, so that I can quickly target the most relevant enemy in my field of view.

#### Acceptance Criteria

1. WHEN a player presses Tab, THE Targeting_System SHALL find all enemies within 40m radius using Physics.OverlapSphere
2. WHEN filtering targets, THE Targeting_System SHALL discard enemies outside the camera frustum
3. WHEN prioritizing targets, THE Targeting_System SHALL calculate dot product between Camera.forward and direction to enemy, prioritizing values closer to 1.0
4. WHEN selecting a target, THE Targeting_System SHALL verify Line of Sight using raycast
5. WHEN a valid target is selected, THE Targeting_System SHALL display the Target Frame UI showing enemy name and health
6. WHEN Tab is pressed with an existing target, THE Targeting_System SHALL cycle to the next valid target

### Requirement 2: Auto-Switch Target

**User Story:** As a player, I want my target to automatically switch when my current target dies, so that I can continue combat without manual re-targeting.

#### Acceptance Criteria

1. WHEN the current target dies, THE Targeting_System SHALL search for enemies within 10m of the corpse
2. WHEN multiple enemies are found, THE Targeting_System SHALL prioritize enemies with Threat > 0 (attacking the player)
3. IF no threatening enemies exist, THE Targeting_System SHALL select the closest enemy
4. IF no enemies are found within range, THE Targeting_System SHALL clear the current target

### Requirement 3: Ability Data Architecture

**User Story:** As a developer, I want abilities defined as ScriptableObjects, so that I can easily configure and balance abilities without code changes.

#### Acceptance Criteria

1. THE AbilityDefinitionSO SHALL contain identity fields: ID, Name, Icon
2. THE AbilityDefinitionSO SHALL contain cost/timing fields: CastTime, Cooldown, Range, ManaCost
3. THE AbilityDefinitionSO SHALL contain behavior flags: RequiresStationary, TriggersGCD, IsOffensive
4. THE AbilityDefinitionSO SHALL contain visual fields: ProjectilePrefab, ProjectileSpeed
5. WHEN CastTime equals 0, THE ability SHALL be instant cast
6. WHEN RequiresStationary is true, THE ability SHALL be interrupted if player moves during cast
7. WHEN TriggersGCD is false, THE ability SHALL be usable during Global Cooldown

### Requirement 4: Combat State Machine

**User Story:** As a player, I want clear combat states, so that I understand when I can use abilities and when I'm locked out.

#### Acceptance Criteria

1. THE Combat_State_Machine SHALL support states: Idle, Casting, GlobalCooldown, Locked
2. WHILE in Idle state, THE player SHALL be able to initiate any available ability
3. WHILE in Casting state, THE system SHALL display a cast bar with progress
4. WHILE in Casting state with RequiresStationary ability, IF player moves THEN THE cast SHALL be interrupted
5. WHILE in GlobalCooldown state, THE player SHALL NOT be able to use abilities with TriggersGCD=true
6. WHILE in GlobalCooldown state, THE player SHALL be able to use Off_GCD abilities
7. WHILE in Locked state (stunned/silenced), THE player SHALL NOT be able to use any abilities

### Requirement 5: Spell Queue (Input Buffering)

**User Story:** As a player, I want my ability inputs buffered, so that I can queue my next ability for smooth combat flow.

#### Acceptance Criteria

1. THE Spell_Queue SHALL have a configurable buffer window (default 400ms)
2. WHEN player presses an ability key during Casting or GCD with less than 400ms remaining, THE Spell_Queue SHALL store the ability as NextAbility
3. WHEN the current state ends and NextAbility is set, THE system SHALL automatically execute the queued ability
4. WHEN a new ability is queued, THE Spell_Queue SHALL replace any previously queued ability
5. WHEN the buffer window expires without state change, THE Spell_Queue SHALL clear the queued ability

### Requirement 6: Server-Authoritative Combat

**User Story:** As a player, I want combat to be fair and cheat-resistant, so that damage calculations are validated by the server.

#### Acceptance Criteria

1. WHEN player initiates an ability, THE client SHALL perform local validation (cooldown, mana) for responsiveness
2. WHEN ability is initiated, THE client SHALL send RequestCastServerRPC with AbilityID and TargetNetworkObjectID
3. WHEN server receives cast request, THE server SHALL validate: mana, target alive, target in range, cooldown passed
4. IF server validation fails, THE server SHALL send error ClientRPC with reason
5. WHEN server validation succeeds, THE server SHALL consume mana and start cast timer
6. WHEN cast completes, THE server SHALL calculate damage: (BaseDmg * Stats) - Mitigation
7. WHEN damage is calculated, THE server SHALL apply damage via Target.ReceiveDamage()
8. WHEN health changes, THE server SHALL update NetworkVariable<Health>

### Requirement 7: Visual Feedback and Projectiles

**User Story:** As a player, I want to see visual feedback for abilities, so that combat feels impactful and responsive.

#### Acceptance Criteria

1. WHEN cast begins, THE server SHALL broadcast BroadcastVisualsClientRPC to all clients
2. WHEN clients receive visual broadcast, THE clients SHALL play attack animation and show cast bar
3. WHEN ability has ProjectilePrefab, THE server SHALL spawn visual projectile that homes toward target
4. WHEN projectile reaches target, THE system SHALL display impact effect
5. WHEN damage is dealt, THE system SHALL display Floating_Combat_Text with damage number
6. WHEN target dies, THE system SHALL play death animation and trigger OnEnemyDeath event

### Requirement 8: Health System Synchronization

**User Story:** As a player, I want to see accurate health values for all entities, so that I can make informed combat decisions.

#### Acceptance Criteria

1. THE Health_System SHALL use NetworkVariable for current and max health
2. WHEN health changes on server, THE NetworkVariable SHALL automatically sync to all clients
3. WHEN entity health reaches 0, THE server SHALL mark entity as dead
4. WHEN entity dies, THE server SHALL broadcast death event to all clients
5. THE Target Frame UI SHALL update in real-time when target health changes

### Requirement 9: Class-Specific Combat Behavior

**User Story:** As a player, I want my class to have distinct combat feel, so that each class plays differently.

#### Acceptance Criteria

1. WHEN playing a caster class (MaestroElemental, Clerigo, MedicoBrujo), THE abilities with RequiresStationary=true SHALL interrupt on movement
2. WHEN playing Arquero, THE abilities SHALL allow movement during cast (RequiresStationary=false)
3. WHEN playing melee classes (Berserker, CaballeroRunico), THE abilities SHALL have shorter range requirements
4. WHEN playing tank classes (Cruzado, Protector), THE abilities SHALL include taunt mechanics

### Requirement 10: Test Abilities

**User Story:** As a developer, I want test abilities configured, so that I can validate the combat system.

#### Acceptance Criteria

1. THE system SHALL include "Bola de Fuego" ability: CastTime=2s, RequiresStationary=true, TriggersGCD=true, high damage
2. THE system SHALL include "Disparo Rápido" ability: CastTime=0 (instant), RequiresStationary=false, TriggersGCD=true, medium damage
3. THE system SHALL include "Escudo de Hielo" ability: CastTime=0 (instant), TriggersGCD=false (Off-GCD), self-buff
