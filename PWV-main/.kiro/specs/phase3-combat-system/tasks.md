# Implementation Plan: Phase 3 - Combat System and Abilities

## Overview

This plan implements the Tab-Targeting combat system in sequential order, ensuring each component builds on the previous. The implementation prioritizes core functionality first, then adds network synchronization, and finally visual feedback.

## Tasks

- [-] 1. Create Core Data Structures
  - [x] 1.1 Create AbilityDefinitionSO ScriptableObject
    - Define all fields: ID, Name, Icon, CastTime, Cooldown, Range, ResourceCost, ResourceType
    - Add behavior flags: RequiresStationary, TriggersGCD, IsOffensive, IsSelfCast
    - Add damage/healing fields and visual references
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

  - [x] 1.2 Create CombatConfigSO ScriptableObject
    - Define targeting config: MaxTargetRange, AutoSwitchRange, TabConeAngle
    - Define timing config: GlobalCooldownDuration, SpellQueueWindow
    - Define layer masks: EnemyLayer, ObstacleLayer
    - _Requirements: 1.1, 2.1, 5.1_

  - [x] 1.3 Create test ability assets
    - Create "Bola de Fuego": CastTime=2s, RequiresStationary=true, TriggersGCD=true
    - Create "Disparo Rápido": CastTime=0, RequiresStationary=false, TriggersGCD=true
    - Create "Escudo de Hielo": CastTime=0, TriggersGCD=false, IsSelfCast=true
    - _Requirements: 10.1, 10.2, 10.3_

- [-] 2. Implement Combat State Machine
  - [x] 2.1 Create CombatStateMachine component
    - Implement CombatState enum: Idle, Casting, GlobalCooldown, Locked
    - Implement state transitions and time tracking
    - Add events: OnStateChanged, OnCastProgress, OnCastInterrupted, OnCastCompleted
    - _Requirements: 4.1, 4.2_

  - [ ]* 2.2 Write property test for state transitions
    - **Property 6: Instant Cast Execution**
    - **Property 9: GCD Blocking**
    - **Property 10: Locked State Blocking**
    - **Validates: Requirements 3.5, 4.5, 4.7**

  - [x] 2.3 Implement cast interruption on movement
    - Connect to player movement input
    - Check RequiresStationary flag
    - Fire OnCastInterrupted when movement detected during stationary cast
    - _Requirements: 3.6, 4.4_

  - [ ]* 2.4 Write property test for stationary cast interruption
    - **Property 7: Stationary Cast Interruption**
    - **Validates: Requirements 3.6, 4.4**

- [-] 3. Implement Spell Queue (Input Buffering)
  - [x] 3.1 Create SpellQueue component
    - Implement buffer window timing (400ms default)
    - Store queued ability and slot
    - Implement TryQueueAbility, ConsumeQueuedAbility, ClearQueue
    - _Requirements: 5.1, 5.2, 5.4_

  - [x] 3.2 Integrate SpellQueue with CombatStateMachine
    - Check buffer window on ability input during Casting/GCD
    - Auto-execute queued ability on state transition
    - _Requirements: 5.3_

  - [ ]* 3.3 Write property test for spell queue
    - **Property 11: Spell Queue Buffer Window**
    - **Property 12: Spell Queue Replacement**
    - **Validates: Requirements 5.2, 5.3, 5.4**

- [x] 4. Checkpoint - Core Combat Logic
  - Ensure all tests pass, ask the user if questions arise.

- [-] 5. Implement Targeting System
  - [x] 5.1 Create TargetingSystem component
    - Implement ITargetable interface
    - Implement GetValidTargets with Physics.OverlapSphere
    - Add frustum culling check
    - _Requirements: 1.1, 1.2_

  - [x] 5.2 Implement Tab targeting algorithm
    - Calculate dot product priority for each target
    - Implement Line of Sight raycast check
    - Implement target cycling on repeated Tab
    - _Requirements: 1.3, 1.4, 1.6_

  - [ ]* 5.3 Write property tests for targeting
    - **Property 1: Tab Target Range Filter**
    - **Property 2: Tab Target Priority by Dot Product**
    - **Property 3: Line of Sight Validation**
    - **Property 4: Tab Cycling**
    - **Validates: Requirements 1.1, 1.3, 1.4, 1.6**

  - [x] 5.4 Implement auto-switch on target death
    - Subscribe to target death event
    - Search enemies within AutoSwitchRange
    - Prioritize by threat, then distance
    - _Requirements: 2.1, 2.2, 2.3, 2.4_

  - [ ]* 5.5 Write property test for auto-switch
    - **Property 5: Auto-Switch Priority**
    - **Validates: Requirements 2.1, 2.2, 2.3**

- [x] 6. Implement Health System
  - [x] 6.1 Create HealthSystem NetworkBehaviour
    - Add NetworkVariables: CurrentHealth, MaxHealth, IsDead
    - Implement TakeDamage and Heal server methods
    - Add events: OnHealthChanged, OnDamageTaken, OnDeath
    - _Requirements: 8.1, 8.2, 8.3_

  - [ ]* 6.2 Write property test for health system
    - **Property 16: Health Death Threshold**
    - **Validates: Requirements 8.3**

  - [x] 6.3 Create IDamageable interface
    - Define TakeDamage method signature
    - Implement on HealthSystem
    - _Requirements: 6.7_

- [x] 7. Checkpoint - Targeting and Health
  - Ensure all tests pass, ask the user if questions arise.

- [x] 8. Implement Ability Executor (Network)
  - [x] 8.1 Create AbilityExecutor NetworkBehaviour
    - Manage ability slots and cooldowns
    - Implement local validation (client-side prediction)
    - Add events: OnCooldownStarted, OnAbilityExecuted, OnAbilityError
    - _Requirements: 6.1_

  - [x] 8.2 Implement server validation
    - Validate resource availability
    - Validate target alive and in range
    - Validate cooldown passed
    - _Requirements: 6.3_

  - [ ]* 8.3 Write property test for server validation
    - **Property 13: Server Validation Completeness**
    - **Property 14: Resource Consumption on Success**
    - **Validates: Requirements 6.3, 6.5**

  - [x] 8.4 Implement RequestCastServerRpc
    - Receive ability slot and target reference
    - Run server validation
    - Consume resources and start cast
    - _Requirements: 6.2, 6.5_

  - [x] 8.5 Implement damage calculation
    - Apply formula: (BaseDamage * StatMultiplier) - Mitigation
    - Call target.TakeDamage()
    - _Requirements: 6.6, 6.7_

  - [ ]* 8.6 Write property test for damage calculation
    - **Property 15: Damage Formula Consistency**
    - **Validates: Requirements 6.6**

  - [x] 8.7 Implement cooldown tracking
    - Start cooldown after ability execution
    - Track remaining time per slot
    - _Requirements: 3.4 (implicit)_

  - [ ]* 8.8 Write property test for cooldown accuracy
    - **Property 17: Cooldown Accuracy**
    - **Validates: Requirements 3.4**

- [x] 9. Implement Off-GCD Logic
  - [x] 9.1 Add Off-GCD ability support
    - Check TriggersGCD flag in CanUseAbility
    - Allow Off-GCD abilities during GCD state
    - _Requirements: 3.7, 4.6_

  - [ ]* 9.2 Write property test for Off-GCD
    - **Property 8: Off-GCD During GCD**
    - **Validates: Requirements 3.7, 4.6**

- [x] 10. Checkpoint - Network Combat
  - Ensure all tests pass, ask the user if questions arise.

- [x] 11. Implement Visual Feedback
  - [x] 11.1 Create Target Frame UI
    - Display target name and health bar
    - Update on target change and health change
    - _Requirements: 1.5, 8.5_

  - [x] 11.2 Create Cast Bar UI
    - Display during Casting state
    - Show progress and ability name
    - _Requirements: 4.3_

  - [x] 11.3 Implement BroadcastVisualsClientRpc
    - Broadcast cast start to all clients
    - Play animations and sounds
    - _Requirements: 7.1, 7.2_

  - [x] 11.4 Create ProjectileController
    - Implement homing projectile movement
    - Spawn impact effect on reach target
    - _Requirements: 7.3, 7.4_

  - [x] 11.5 Create FloatingCombatText
    - Spawn damage/heal numbers above targets
    - Animate float up and fade
    - _Requirements: 7.5_

- [x] 12. Final Integration
  - [x] 12.1 Connect all systems
    - Wire TargetingSystem to AbilityExecutor
    - Wire CombatStateMachine to SpellQueue
    - Wire HealthSystem death to TargetingSystem auto-switch
    - _Requirements: All_

  - [x] 12.2 Create CombatManager facade
    - Provide unified API for combat operations
    - Handle initialization and cleanup
    - _Requirements: All_

- [x] 13. Final Checkpoint
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional property-based tests
- Each checkpoint ensures incremental validation
- Network testing requires multiple clients or mock setup
- Visual tasks (11.x) can be tested manually
