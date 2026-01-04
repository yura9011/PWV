# Implementation Plan: The Ether Domes

## Overview

Plan de implementación incremental para "The Ether Domes". El desarrollo está organizado en fases, comenzando con la creación del proyecto Unity y la infraestructura base, seguido por los sistemas core en orden de dependencia.

**Fases:**
1. Setup del Proyecto (Tareas 1-2)
2. Sistema de Red (Tareas 3-5)
3. Sistema de Combate (Tareas 6-10)
4. Sistema de Clases (Tareas 11-12)
5. Sistema de Progresión (Tareas 13-15)
6. Sistema de Mundo (Tareas 16-19)
7. Integración Final (Tareas 20-21)

## Tasks

### Phase 1: Project Setup

- [x] 1. Crear proyecto Unity y estructura de carpetas
  - Crear nuevo proyecto Unity 2022.3 LTS en `TheEtherDomes/`
  - Instalar paquetes: Netcode for GameObjects, Input System, TextMeshPro
  - Crear estructura de carpetas según design.md (Assets/_Project/Scripts/*, etc.)
  - Configurar assembly definitions (.asmdef) para cada módulo
  - _Requirements: Setup técnico base_

- [x] 2. Configurar datos base y enums
  - [x] 2.1 Crear enums y estructuras de datos
    - Crear `Data/Enums.cs` con CharacterClass, Specialization, ItemRarity, etc.
    - Crear `Data/CharacterData.cs`, `Data/CharacterStats.cs`
    - Crear `Data/AbilityData.cs`, `Data/ItemData.cs`
    - _Requirements: Estructuras de datos compartidas_
  - [x] 2.2 Crear ScriptableObjects base
    - Crear `AbilityDataSO.cs` para definir habilidades en el editor
    - Crear `ItemDataSO.cs` para definir items
    - Crear `RegionDataSO.cs` para definir regiones
    - _Requirements: Configuración de contenido_

### Phase 2: Network Foundation

- [x] 3. Implementar NetworkSessionManager
  - [x] 3.1 Crear interfaces de red
    - Crear `Network/Interfaces/INetworkSessionManager.cs`
    - Crear `Network/Interfaces/IConnectionApprovalHandler.cs`
    - _Requirements: 1.1, 1.2, 1.3_
  - [x] 3.2 Implementar NetworkSessionManager
    - Implementar wrapper sobre Unity NetworkManager
    - Implementar StartAsHost(), StartAsClient(), StartAsDedicatedServer()
    - Implementar eventos OnPlayerConnected, OnPlayerDisconnected
    - Implementar límite de 10 jugadores
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6_
  - [ ]* 3.3 Write property test for Session State Consistency
    - **Property 1: Session State Consistency**
    - **Validates: Requirements 1.1, 1.3**
  - [ ]* 3.4 Write property test for Player Count Enforcement
    - **Property 2: Player Count Enforcement**
    - **Validates: Requirements 1.6**

- [x] 4. Implementar CharacterPersistenceService
  - [x] 4.1 Crear servicio de encriptación
    - Implementar `Persistence/EncryptionService.cs` con AES
    - Implementar hash de integridad SHA256
    - _Requirements: 7.2_
  - [x] 4.2 Implementar persistencia de personaje
    - Crear `Persistence/Interfaces/ICharacterPersistenceService.cs`
    - Implementar SaveCharacterAsync, LoadCharacterAsync
    - Implementar ExportCharacterForNetwork, ImportCharacterFromNetwork
    - _Requirements: 7.1, 7.3, 7.4, 7.5_
  - [ ]* 4.3 Write property test for Character Persistence Round-Trip
    - **Property 4: Character Persistence Round-Trip**
    - **Validates: Requirements 7.1, 7.3**
  - [ ]* 4.4 Write property test for Encrypted Storage
    - **Property 5: Encrypted Storage Non-Plaintext**
    - **Validates: Requirements 7.2**

- [x] 5. Implementar ConnectionApprovalHandler
  - [x] 5.1 Implementar validación de conexión
    - Crear `Network/ConnectionApprovalHandler.cs`
    - Implementar ValidateConnectionRequest con validación de stats
    - Integrar con CharacterPersistenceService
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_

- [x] 6. Checkpoint - Verificar networking base
  - Ensure all tests pass, ask the user if questions arise.
  - Verificar: Host puede iniciar sesión
  - Verificar: Client puede conectar con personaje válido
  - Verificar: Client rechazado con datos inválidos


### Phase 3: Combat System

- [x] 7. Implementar Target System
  - [x] 7.1 Crear interfaces de targeting
    - Crear `Combat/Interfaces/ITargetSystem.cs`
    - Crear `Combat/Interfaces/ITargetable.cs`
    - _Requirements: 8.1, 8.2_
  - [x] 7.2 Implementar TargetSystem
    - Implementar CycleTarget() con Tab
    - Implementar SelectTarget() con click
    - Implementar ClearTarget() con Escape
    - Implementar detección de rango (40m)
    - Implementar limpieza automática al morir target
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6_
  - [ ]* 7.3 Write property test for Tab Target Cycling
    - **Property 6: Tab Target Cycling**
    - **Validates: Requirements 8.1**
  - [ ]* 7.4 Write property test for Target Death Clears Selection
    - **Property 7: Target Death Clears Selection**
    - **Validates: Requirements 8.4**
  - [ ]* 7.5 Write property test for Target Range Tracking
    - **Property 8: Target Range Tracking**
    - **Validates: Requirements 8.5**

- [x] 8. Implementar Ability System
  - [x] 8.1 Crear interfaces de habilidades
    - Crear `Combat/Interfaces/IAbilitySystem.cs`
    - _Requirements: 9.1_
  - [x] 8.2 Implementar AbilitySystem
    - Implementar TryExecuteAbility con hotkeys 1-9
    - Implementar Global Cooldown (1.5s)
    - Implementar Cast Time con barra de casteo
    - Implementar interrupción por movimiento
    - Implementar validación de target para habilidades targeted
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6, 9.7_
  - [ ]* 8.3 Write property test for GCD Enforcement
    - **Property 9: GCD Enforcement**
    - **Validates: Requirements 9.2, 9.3**
  - [ ]* 8.4 Write property test for Cast Interruption
    - **Property 10: Cast Interruption on Movement**
    - **Validates: Requirements 9.4, 9.5**
  - [ ]* 8.5 Write property test for Targeted Ability Requires Target
    - **Property 11: Targeted Ability Requires Target**
    - **Validates: Requirements 9.7**

- [x] 9. Implementar Aggro System
  - [x] 9.1 Crear interfaces de aggro
    - Crear `Combat/Interfaces/IAggroSystem.cs`
    - _Requirements: 10.1_
  - [x] 9.2 Implementar AggroSystem
    - Implementar AddThreat (daño = threat)
    - Implementar Taunt (highest + 10%)
    - Implementar AddHealingThreat (50% dividido)
    - Implementar GetHighestThreatPlayer
    - Implementar threshold switching (10% melee, 30% ranged)
    - Implementar ResetThreat al salir de combate
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 10.6_
  - [ ]* 9.3 Write property test for Threat Calculation
    - **Property 12: Threat Calculation Correctness**
    - **Validates: Requirements 10.1**
  - [ ]* 9.4 Write property test for Taunt
    - **Property 13: Taunt Sets Highest Threat**
    - **Validates: Requirements 10.2**
  - [ ]* 9.5 Write property test for Healing Threat
    - **Property 14: Healing Threat Distribution**
    - **Validates: Requirements 10.3**
  - [ ]* 9.6 Write property test for Highest Threat Target
    - **Property 15: Enemy Targets Highest Threat**
    - **Validates: Requirements 10.4**
  - [ ]* 9.7 Write property test for Threat Reset
    - **Property 16: Threat Reset on Combat End**
    - **Validates: Requirements 10.6**

- [x] 10. Implementar Combat System (Death/Resurrection)
  - [x] 10.1 Crear interfaces de combate
    - Crear `Combat/Interfaces/ICombatSystem.cs`
    - _Requirements: 11.1_
  - [x] 10.2 Implementar CombatSystem
    - Implementar ApplyDamage, ApplyHealing
    - Implementar estado Dead al llegar a 0 HP
    - Implementar ventana de resurrección (60s)
    - Implementar ReleaseSpirit (respawn 50% HP)
    - Implementar detección de Wipe
    - Asegurar que muerte NO pierde equipo/XP
    - _Requirements: 11.1, 11.2, 11.3, 11.4, 11.5, 11.6_
  - [ ]* 10.3 Write property test for Death State
    - **Property 17: Death State Transition**
    - **Validates: Requirements 11.1**
  - [ ]* 10.4 Write property test for Death Preserves Inventory
    - **Property 18: Death Preserves Inventory**
    - **Validates: Requirements 11.6**

- [x] 11. Checkpoint - Verificar sistema de combate
  - Ensure all tests pass, ask the user if questions arise.
  - Verificar: Tab cycling funciona
  - Verificar: Habilidades respetan GCD
  - Verificar: Aggro se calcula correctamente
  - Verificar: Muerte y resurrección funcionan


### Phase 4: Class System

- [x] 12. Implementar Class System
  - [x] 12.1 Crear interfaces de clases
    - Crear `Classes/Interfaces/IClassSystem.cs`
    - _Requirements: 12.1_
  - [x] 12.2 Implementar ClassSystem
    - Implementar GetClass, GetSpecialization
    - Implementar SetSpecialization (solo fuera de combate)
    - Implementar GetClassAbilities por clase/spec
    - Implementar GetSpecEffectiveness (1.0 main, 0.7 off-spec)
    - _Requirements: 12.1, 12.2, 12.3, 12.4, 12.5_
  - [ ]* 12.3 Write property test for Class Ability Assignment
    - **Property 19: Class Ability Assignment**
    - **Validates: Requirements 12.2**
  - [ ]* 12.4 Write property test for Spec Switching
    - **Property 20: Spec Switching Out of Combat**
    - **Validates: Requirements 12.3**
  - [ ]* 12.5 Write property test for Off-Spec Effectiveness
    - **Property 21: Off-Spec Effectiveness**
    - **Validates: Requirements 13.2**

- [x] 13. Crear habilidades base por clase
  - [x] 13.1 Definir habilidades de Warrior
    - Crear ScriptableObjects para Protection (Tank) y Arms (DPS)
    - Incluir Taunt, Shield Block, Mortal Strike, etc.
    - _Requirements: 12.2_
  - [x] 13.2 Definir habilidades de Mage
    - Crear ScriptableObjects para Fire y Frost specs
    - Incluir Fireball, Frostbolt, Blink, etc.
    - _Requirements: 12.2_
  - [x] 13.3 Definir habilidades de Priest
    - Crear ScriptableObjects para Holy (Healer) y Shadow (DPS)
    - Incluir Heal, Resurrect, Shadow Word: Pain, etc.
    - _Requirements: 12.2_
  - [x] 13.4 Definir habilidades de Paladin
    - Crear ScriptableObjects para Protection, Holy, Retribution
    - Incluir Lay on Hands, Divine Shield, Crusader Strike, etc.
    - _Requirements: 12.2, 12.5_

### Phase 5: Progression System

- [x] 14. Implementar Progression System
  - [x] 14.1 Crear interfaces de progresión
    - Crear `Progression/Interfaces/IProgressionSystem.cs`
    - _Requirements: 14.1_
  - [x] 14.2 Implementar ProgressionSystem
    - Implementar AddExperience con fórmula basada en nivel
    - Implementar level up (1-60)
    - Implementar aumento de stats por nivel
    - Implementar reducción de XP para enemigos -10 niveles
    - Implementar desbloqueo de habilidades en niveles 10,20,30,40,50
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5, 14.6_
  - [ ]* 14.3 Write property test for Experience Calculation
    - **Property 23: Experience Calculation**
    - **Validates: Requirements 14.1, 14.6**
  - [ ]* 14.4 Write property test for Level Progression
    - **Property 24: Level Progression Bounds**
    - **Validates: Requirements 14.2, 14.3**
  - [ ]* 14.5 Write property test for Level Up Stats
    - **Property 25: Level Up Stats Increase**
    - **Validates: Requirements 14.4**
  - [ ]* 14.6 Write property test for Ability Unlock
    - **Property 26: Ability Unlock at Milestones**
    - **Validates: Requirements 14.5**

- [x] 15. Implementar Loot System
  - [x] 15.1 Crear interfaces de loot
    - Crear `Progression/Interfaces/ILootSystem.cs`
    - Crear `Progression/Interfaces/IEquipmentSystem.cs`
    - _Requirements: 15.1_
  - [x] 15.2 Implementar LootSystem
    - Implementar GenerateLoot con loot tables
    - Implementar Drop Rates (Common 70%, Rare 25%, Epic 5%)
    - Implementar DistributeLoot (RoundRobin, NeedGreed)
    - _Requirements: 15.1, 15.2, 15.3_
  - [x] 15.3 Implementar EquipmentSystem
    - Implementar CanEquip con validación de nivel
    - Implementar Equip/Unequip
    - Implementar GetEquipmentStats
    - _Requirements: 15.4, 15.5, 15.6_
  - [ ]* 15.4 Write property test for Loot Drop Rate
    - **Property 27: Loot Drop Rate Distribution**
    - **Validates: Requirements 15.2**
  - [ ]* 15.5 Write property test for Equipment Stats
    - **Property 28: Equipment Stats Application**
    - **Validates: Requirements 15.4**
  - [ ]* 15.6 Write property test for Equipment Level Requirement
    - **Property 29: Equipment Level Requirement**
    - **Validates: Requirements 15.5**
  - [ ]* 15.7 Write property test for Equipment Serialization
    - **Property 30: Equipment Serialization Round-Trip**
    - **Validates: Requirements 15.6**

- [x] 16. Checkpoint - Verificar progresión
  - Ensure all tests pass, ask the user if questions arise.
  - Verificar: XP se gana correctamente
  - Verificar: Level up aumenta stats
  - Verificar: Loot se genera con probabilidades correctas
  - Verificar: Equipo se puede equipar/desequipar


### Phase 6: World System

- [x] 17. Implementar World Manager
  - [x] 17.1 Crear interfaces de mundo
    - Crear `World/Interfaces/IWorldManager.cs`
    - _Requirements: 16.1_
  - [x] 17.2 Implementar WorldManager
    - Implementar LoadRegion, UnloadRegion
    - Implementar GetCurrentRegion
    - Definir 5 regiones con rangos de nivel
    - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5_
  - [x] 17.3 Crear RegionDataSO para cada región
    - Roca (1-15), Bosque (15-30), Nieve (30-40), Pantano (40-50), Ciudadela (50-60)
    - Definir spawn points y dungeons por región
    - _Requirements: 16.1, 16.5_
  - [ ]* 17.4 Write property test for Region Enemy Scaling
    - **Property 31: Region Enemy Level Scaling**
    - **Validates: Requirements 16.5**

- [x] 18. Implementar Dungeon System
  - [x] 18.1 Crear interfaces de mazmorras
    - Crear `World/Interfaces/IDungeonSystem.cs`
    - _Requirements: 17.1_
  - [x] 18.2 Implementar DungeonSystem
    - Implementar CreateInstance para grupos
    - Implementar EnterInstance, LeaveInstance
    - Implementar DestroyInstance (5 min delay)
    - Implementar escalado de dificultad por grupo (2-10)
    - Implementar tracking de jefes derrotados
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5, 17.6_
  - [ ]* 18.3 Write property test for Dungeon Instance Isolation
    - **Property 32: Dungeon Instance Isolation**
    - **Validates: Requirements 17.2**
  - [ ]* 18.4 Write property test for Boss Progress Tracking
    - **Property 33: Boss Progress Tracking**
    - **Validates: Requirements 17.5, 17.6**
  - [ ]* 18.5 Write property test for Group Size Scaling
    - **Property 22: Group Size Scaling**
    - **Validates: Requirements 13.4, 17.4**

- [x] 19. Implementar Boss System
  - [x] 19.1 Crear interfaces de jefes
    - Crear `World/Interfaces/IBossSystem.cs`
    - _Requirements: 18.1_
  - [x] 19.2 Implementar BossSystem
    - Implementar StartEncounter, EndEncounter
    - Implementar fases (100-75%, 75-50%, 50-25%, 25-0%)
    - Implementar rotación de habilidades
    - Integrar con LootSystem al derrotar
    - _Requirements: 18.1, 18.2, 18.3, 18.4, 18.5_
  - [ ]* 19.3 Write property test for Boss Phase Transitions
    - **Property 34: Boss Phase Transitions**
    - **Validates: Requirements 18.3**

- [x] 20. Implementar Guild Base System
  - [x] 20.1 Crear interfaces de guild base
    - Crear `World/Interfaces/IGuildBaseSystem.cs`
    - _Requirements: 19.1_
  - [x] 20.2 Implementar GuildBaseSystem
    - Implementar Enter, Leave
    - Implementar PlaceFurniture, RemoveFurniture
    - Implementar UnlockTrophy al derrotar jefes
    - _Requirements: 19.1, 19.2, 19.3, 19.4, 19.5, 19.6_

- [x] 21. Implementar World Persistence
  - [x] 21.1 Crear interfaces de persistencia de mundo
    - Crear `Persistence/Interfaces/IWorldPersistenceService.cs`
    - _Requirements: 20.1_
  - [x] 21.2 Implementar WorldPersistenceService
    - Implementar SaveWorldStateAsync, LoadWorldStateAsync
    - Implementar auto-save para servidor dedicado (5 min)
    - Guardar progreso de dungeons y guild base
    - _Requirements: 20.1, 20.2, 20.3, 20.4, 20.5_
  - [ ]* 21.3 Write property test for World State Persistence
    - **Property 35: World State Persistence Round-Trip**
    - **Validates: Requirements 20.1, 20.2**
  - [ ]* 21.4 Write property test for World Pause
    - **Property 36: World Pause When Offline**
    - **Validates: Requirements 20.3**

- [x] 22. Checkpoint - Verificar sistema de mundo
  - Ensure all tests pass, ask the user if questions arise.
  - Verificar: Regiones cargan correctamente
  - Verificar: Mazmorras se instancian por grupo
  - Verificar: Jefes tienen fases
  - Verificar: Guild base persiste

### Phase 7: Integration

- [x] 23. Implementar Player Controller y Prefab
  - [x] 23.1 Implementar PlayerController
    - Crear `Player/PlayerController.cs`
    - Implementar movimiento 8 direcciones con IsOwner check
    - Implementar movimiento relativo a cámara
    - _Requirements: 3.1, 3.2, 3.3, 3.4, 3.5_
  - [x] 23.2 Crear Player Prefab
    - Crear prefab con NetworkObject, NetworkTransform
    - Añadir PlayerController, Collider, Rigidbody
    - Configurar layer "Player" para no-colisión entre jugadores
    - _Requirements: 2.1, 2.2, 5.1, 5.2, 5.3_
  - [ ]* 23.3 Write property test for Ownership Input
    - **Property 3: Ownership-Based Input Processing**
    - **Validates: Requirements 3.1, 3.2**

- [x] 24. Crear UI de combate
  - [x] 24.1 Implementar Action Bar
    - Crear UI con 9 slots de habilidades
    - Mostrar cooldowns y GCD
    - Conectar con AbilitySystem
    - _Requirements: 9.1, 9.6_
  - [x] 24.2 Implementar Target Frame
    - Mostrar nombre, vida, indicador de target
    - Mostrar "Out of Range" cuando aplique
    - _Requirements: 8.3, 8.5_
  - [x] 24.3 Implementar Cast Bar
    - Mostrar progreso de casteo
    - _Requirements: 9.4_

- [x] 25. Crear escenas de prueba
  - [x] 25.1 Crear MainMenu scene
    - UI para Host/Client/Server
    - Selección de personaje
    - _Requirements: 1.1, 1.2, 1.3_
  - [x] 25.2 Crear GuildBase scene
    - Escena instanciada para guild base
    - _Requirements: 19.1_
  - [x] 25.3 Crear Roca region scene (starter)
    - Terreno básico con enemigos nivel 1-15
    - Entrada a mazmorra de prueba
    - _Requirements: 16.1, 16.5_
  - [x] 25.4 Crear TestDungeon scene
    - Mazmorra pequeña (3 jefes) para testing
    - _Requirements: 17.1_

- [x] 26. Checkpoint final - Integración completa
  - Ensure all tests pass, ask the user if questions arise.
  - ✅ Verificar: Dos jugadores pueden conectar (NetworkSessionManager + ConnectionApprovalHandler)
  - ✅ Verificar: Combate Tab-Target funciona (TargetSystem + AbilitySystem + CombatSystem)
  - ✅ Verificar: Mazmorras se pueden completar (DungeonSystem + BossSystem)
  - ✅ Verificar: Loot se distribuye (LootSystem con RoundRobin/NeedGreed)
  - ✅ Verificar: Progresión funciona (ProgressionSystem + EquipmentSystem)
  - ✅ Verificar: Persistencia funciona (CharacterPersistenceService + WorldPersistenceService)

## Notes

- Las tareas marcadas con `*` son property tests opcionales pero recomendados
- Cada fase tiene un checkpoint para validación incremental
- El proyecto usa Unity 2022.3 LTS con Netcode for GameObjects
- FsCheck se usa para property-based testing
- La estructura de carpetas sigue el patrón `_Project/` para separar código del proyecto de assets de terceros
