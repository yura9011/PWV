# Implementation Plan: MVP 10 Features

## Overview

Plan de implementación para las 10 features del MVP de The Ether Domes. Las tareas están organizadas por prioridad y dependencias, con property tests integrados.

## Tasks

- [x] 0. Extensión de CharacterData (Prerequisito)
  - [x] 0.1 Agregar campos de migración y mana
    - Agregar `public int DataVersion = 1;`
    - Agregar `public float CurrentMana;` y `public float MaxMana;`
    - Archivos: `Scripts/Data/CharacterData.cs`
    - _Requirements: 11.1, 11.6_

  - [x] 0.2 Agregar campos de recursos secundarios
    - Agregar `public float SecondaryResource;`
    - Agregar `public SecondaryResourceType ResourceType;`
    - Archivos: `Scripts/Data/CharacterData.cs`, `Scripts/Data/Enums.cs`
    - _Requirements: 11.2_

  - [x] 0.3 Agregar campos de loot y lockout
    - Agregar `public Dictionary<ItemRarity, int> LootAttempts;`
    - Agregar `public List<string> LockedBossIds;`
    - Agregar `public DateTime LockoutResetTime;`
    - Archivos: `Scripts/Data/CharacterData.cs`
    - _Requirements: 11.3, 11.4_

  - [x] 0.4 Agregar inventario separado
    - Agregar `public List<ItemData> Inventory;`
    - Inicializar con capacidad 30 en constructor
    - Archivos: `Scripts/Data/CharacterData.cs`
    - _Requirements: 11.5_

  - [x] 0.5 Actualizar constructor con defaults
    - Inicializar DataVersion = 1
    - Inicializar LootAttempts = new Dictionary
    - Inicializar LockedBossIds = new List
    - Inicializar Inventory = new List con capacidad 30
    - Archivos: `Scripts/Data/CharacterData.cs`
    - _Requirements: 11.7_

---

- [x] 1. Sistema de Session Locking
  - [x] 1.1 Crear ISessionLockManager interface
    - Definir métodos TryAcquireLock, ReleaseLock, IsLocked, CleanupStaleLocks
    - Definir eventos OnLockAcquired, OnLockReleased, OnLockDenied
    - Archivos: `Scripts/Persistence/Interfaces/ISessionLockManager.cs`
    - _Requirements: 1.1, 1.2_

  - [x] 1.2 Implementar SessionLockManager
    - Crear lock files en Application.persistentDataPath/Locks/
    - Implementar detección de locks stale (>5 minutos)
    - Manejar crashes con timestamp en lock file
    - Archivos: `Scripts/Persistence/SessionLockManager.cs`
    - _Requirements: 1.1, 1.2, 1.4_

  - [x] 1.3 Integrar con CharacterPersistenceService
    - Llamar TryAcquireLock antes de LoadCharacterAsync
    - Llamar ReleaseLock en Disconnect/OnApplicationQuit
    - Retornar error "Character in use" si lock falla
    - Archivos: `Scripts/Persistence/CharacterPersistenceService.cs`
    - _Requirements: 1.1, 1.2_

  - [x] 1.4 Property test: Session Lock Mutual Exclusion
    - **Property 1: Session Lock Mutual Exclusion**
    - Verificar que segundo TryAcquireLock falla mientras lock activo
    - **Validates: Requirements 1.1, 1.2**

---

- [x] 2. Sistema de Data Migration
  - [x] 2.1 Agregar DataVersion a CharacterData
    - Agregar campo `public int DataVersion;`
    - Inicializar en 1 para nuevos personajes
    - Archivos: `Scripts/Data/CharacterData.cs`
    - _Requirements: 1.5_

  - [x] 2.2 Crear IDataMigrationService interface
    - Definir CurrentVersion, Migrate, NeedsMigration
    - Definir RegisterMigration para cadena de migraciones
    - Archivos: `Scripts/Persistence/Interfaces/IDataMigrationService.cs`
    - _Requirements: 1.6_

  - [x] 2.3 Implementar DataMigrationService
    - Mantener diccionario de migradores por versión
    - Ejecutar cadena de migraciones secuencialmente
    - Crear backup antes de migrar
    - Archivos: `Scripts/Persistence/DataMigrationService.cs`
    - _Requirements: 1.6, 1.7, 1.8_

  - [x] 2.4 Integrar con CharacterPersistenceService
    - Llamar NeedsMigration después de deserializar
    - Ejecutar Migrate si es necesario
    - Actualizar DataVersion al guardar
    - Archivos: `Scripts/Persistence/CharacterPersistenceService.cs`
    - _Requirements: 1.5, 1.6_

  - [x] 2.5 Property test: Migration Idempotence
    - **Property 4: Migration Idempotence**
    - Verificar que migrar datos actuales no cambia nada
    - **Validates: Requirements 1.6**

---

- [x] 3. Checkpoint - Persistencia Completa
  - [x] Verificar que session locking funciona
  - [x] Verificar que migration preserva datos
  - [x] Ejecutar tests de persistencia existentes
  - [x] Preguntar al usuario si hay dudas

---

- [x] 3.5 Sistema de Mana
  - [x] 3.5.1 Crear IManaSystem interface
    - GetCurrentMana, GetMaxMana, TrySpendMana, RestoreMana
    - Eventos OnManaChanged, OnManaEmpty
    - Archivos: `Scripts/Combat/Interfaces/IManaSystem.cs`
    - _Requirements: 12.1, 12.2_

  - [x] 3.5.2 Implementar ManaSystem
    - Trackear mana por playerId
    - Regeneración: 2% fuera de combate, 0.5% en combate
    - Archivos: `Scripts/Combat/ManaSystem.cs`
    - _Requirements: 12.1, 12.2, 12.4, 12.5_

  - [x] 3.5.3 Integrar con AbilitySystem
    - Verificar mana antes de ejecutar habilidad
    - Deducir ManaCost al ejecutar
    - Mostrar error "Not enough mana" si insuficiente
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 12.2, 12.3_

  - [x] 3.5.4 Property test: Mana Cost Enforcement
    - **Property 31: Mana Cost Enforcement**
    - Verificar que habilidad no ejecuta si mana insuficiente
    - **Validates: Requirements 12.3**

---

- [x] 3.6 Sistema de Party
  - [x] 3.6.1 Crear IPartySystem interface
    - IsInParty, GetPartyMembers, GetPartyLeader
    - TryInvitePlayer, AcceptInvite, LeaveParty
    - Archivos: `Scripts/Network/Interfaces/IPartySystem.cs`
    - _Requirements: 14.1, 14.2_

  - [x] 3.6.2 Implementar PartySystem
    - Diccionario de parties por leaderId
    - Máximo 10 jugadores por party
    - Promoción automática de líder al desconectar
    - Archivos: `Scripts/Network/PartySystem.cs`
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 14.5_

  - [x] 3.6.3 Integrar con NetworkSessionManager
    - Suscribirse a OnPlayerDisconnected para manejar líder
    - Archivos: `Scripts/Network/NetworkSessionManager.cs`
    - _Requirements: 14.5_

  - [x] 3.6.4 Property test: Party Size Limits
    - **Property 33: Party Size Limits**
    - Verificar que party size está entre 2 y 10
    - **Validates: Requirements 14.1**

---

- [x] 3.7 Enemy Damage Tracking
  - [x] 3.7.1 Agregar tracking de daño a Enemy.cs
    - Agregar `private Dictionary<ulong, float> _damageByPlayer;`
    - Método RecordDamage(ulong playerId, float damage)
    - Método GetHighestDamageDealer()
    - Archivos: `Scripts/Enemy/Enemy.cs`
    - _Requirements: 13.1, 13.2, 13.3_

  - [x] 3.7.2 Integrar con ApplyDamageInternal
    - Llamar RecordDamage al aplicar daño
    - Limpiar tracking al resetear enemigo
    - Archivos: `Scripts/Enemy/Enemy.cs`
    - _Requirements: 13.2, 13.4_

  - [x] 3.7.3 Property test: Highest Damage Dealer Selection
    - **Property 38: Highest Damage Dealer Selection**
    - Verificar que GetHighestDamageDealer retorna jugador correcto
    - **Validates: Requirements 13.3**

---

- [x] 3.8 Latency Monitor
  - [x] 3.8.1 Crear ILatencyMonitor interface
    - CurrentLatency, IsHighLatency, IsPaused
    - Eventos OnLatencyStateChanged, OnActionsPaused
    - Archivos: `Scripts/Network/Interfaces/ILatencyMonitor.cs`
    - _Requirements: 21.1, 21.2_

  - [x] 3.8.2 Implementar LatencyMonitor
    - Warning threshold: 200ms
    - Pause threshold: 500ms
    - Resume threshold: 400ms
    - Archivos: `Scripts/Network/LatencyMonitor.cs`
    - _Requirements: 21.2, 21.3, 21.4_

  - [x] 3.8.3 Integrar con NetworkSessionManager
    - Monitorear RTT continuamente
    - Pausar acciones cuando latencia > 500ms
    - Archivos: `Scripts/Network/NetworkSessionManager.cs`
    - _Requirements: 21.1, 21.3, 21.4_

  - [x] 3.8.4 Property test: Latency State Transitions
    - **Property 35: Latency State Transitions**
    - Verificar transiciones de estado correctas
    - **Validates: Requirements 21.3, 21.4**

---

- [x] 4. Enemy AI Controller
  - [x] 4.1 Crear IEnemyAI interface
    - Definir estados: Idle, Patrol, Aggro, Combat, Returning, Dead
    - Definir eventos OnStateChanged, OnTargetChanged
    - Archivos: `Scripts/Enemy/Interfaces/IEnemyAI.cs`
    - _Requirements: 3.1, 3.2, 3.3_

  - [x] 4.2 Crear EnemyAIConfig ScriptableObject
    - AggroRange (15m), LeashDistance (40m), AttackRange (2m)
    - AttackCooldown (2s), AlertRadius (15m)
    - Archivos: `Scripts/Data/ScriptableObjects/EnemyAIConfigSO.cs`
    - _Requirements: 3.2, 3.3_

  - [x] 4.3 Implementar EnemyAI máquina de estados
    - Estado Idle: esperar detección de jugador
    - Estado Aggro: moverse hacia target
    - Estado Combat: atacar, cambiar target por threat
    - Estado Returning: volver a spawn sin regenerar HP
    - Archivos: `Scripts/Enemy/EnemyAI.cs`
    - _Requirements: 3.1, 3.2, 3.3, 3.4_

  - [x] 4.4 Implementar detección y alerta de enemigos cercanos
    - OnTriggerEnter para detectar jugadores en AggroRange
    - AlertNearbyEnemies: buscar enemigos en AlertRadius y llamar EnterCombat
    - Archivos: `Scripts/Enemy/EnemyAI.cs`
    - _Requirements: 3.2_

  - [x] 4.5 Implementar selección de target por daño total
    - Trackear daño total por jugador en Enemy
    - Al morir target actual, seleccionar jugador con más daño
    - Archivos: `Scripts/Enemy/EnemyAI.cs`, `Scripts/Enemy/Enemy.cs`
    - _Requirements: 3.1_

  - [x] 4.6 Implementar leashing
    - Calcular distancia a SpawnPosition cada frame
    - Si > LeashDistance, transicionar a Returning
    - Preservar HP al volver (no regenerar)
    - Archivos: `Scripts/Enemy/EnemyAI.cs`
    - _Requirements: 3.3, 3.4_

  - [x] 4.7 Property test: Leash Distance Enforcement
    - **Property 10: Leash Distance Enforcement**
    - Verificar que enemigo a >40m entra en estado Returning
    - **Validates: Requirements 3.3**

  - [x] 4.8 Property test: HP Preservation on Return
    - **Property 11: HP Preservation on Return**
    - Verificar que HP no cambia al volver a spawn
    - **Validates: Requirements 3.4**

---

- [x] 5. Sistema de Clases con Habilidades Diferenciadas
  - [x] 5.1 Modificar ClassSystem para reemplazar habilidades completamente
    - Al cambiar spec, limpiar lista de habilidades y cargar nuevas
    - No usar multiplicadores de efectividad
    - Archivos: `Scripts/Classes/ClassSystem.cs`
    - _Requirements: 2.1_

  - [x] 5.2 Crear 4-5 habilidades únicas por spec
    - Warrior Protection: Taunt, Shield Block, Shield Slam, Devastate
    - Warrior Arms: Mortal Strike, Overpower, Execute, Bladestorm
    - Mage Fire: Fireball, Pyroblast, Fire Blast, Combustion
    - Mage Frost: Frostbolt, Ice Lance, Blizzard, Cone of Cold
    - Priest Holy: Heal, Flash Heal, Prayer of Healing, Guardian Spirit
    - Priest Shadow: Shadow Word Pain, Mind Blast, Mind Flay, Vampiric Embrace
    - Paladin Protection: Righteous Defense, Avenger Shield, Consecration, Ardent Defender
    - Paladin Holy: Holy Light, Flash of Light, Beacon of Light, Lay on Hands
    - Paladin Retribution: Crusader Strike, Templar Verdict, Divine Storm, Avenging Wrath
    - Archivos: `ScriptableObjects/Abilities/[Class]/[Spec]/`
    - _Requirements: 2.2_

  - [x] 5.3 Property test: Spec Change Replaces Abilities
    - **Property 6: Spec Change Replaces Abilities**
    - Verificar que no hay habilidades compartidas entre specs
    - **Validates: Requirements 2.1**

---

- [x] 6. Checkpoint - Combat Core
  - Verificar que Enemy AI funciona con aggro
  - Verificar que cambio de spec reemplaza habilidades
  - Probar combate básico con nuevo AI
  - Preguntar al usuario si hay dudas

---

- [x] 6.5 UI de Selección de Personaje
  - [x] 6.5.1 Crear ICharacterSelectUI interface
    - Show, Hide, RefreshCharacterList, ShowCreationPanel
    - Eventos OnCharacterSelected, OnPlayClicked, OnDeleteClicked
    - Archivos: `Scripts/UI/Interfaces/ICharacterSelectUI.cs`
    - _Requirements: 17.1, 17.2_

  - [x] 6.5.2 Implementar CharacterSelectUI
    - Lista de personajes con nombre, clase, nivel
    - Botones New Character, Play, Delete
    - Panel de creación con selección de clase
    - Archivos: `Scripts/UI/CharacterSelect/CharacterSelectUI.cs`
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5_

  - [x] 6.5.3 Integrar con CharacterPersistenceService
    - Cargar lista de personajes guardados
    - Crear/eliminar personajes
    - Archivos: `Scripts/UI/CharacterSelect/CharacterSelectUI.cs`
    - _Requirements: 17.6_

---

- [x] 6.6 UI de Inventario
  - [x] 6.6.1 Crear IInventoryUI interface
    - Show, Hide, Toggle, RefreshSlots, SetSlot
    - Eventos OnSlotClicked, OnSlotRightClicked
    - Archivos: `Scripts/UI/Interfaces/IInventoryUI.cs`
    - _Requirements: 16.1, 16.2_

  - [x] 6.6.2 Implementar InventoryUI
    - Grid de 30 slots
    - Tooltip al hover con stats
    - Menú contextual: Equip, Salvage
    - Toggle con tecla "I"
    - Archivos: `Scripts/UI/Inventory/InventoryUI.cs`
    - _Requirements: 16.1, 16.2, 16.3, 16.4, 16.5, 16.6_

  - [x] 6.6.3 Property test: Inventory Slot Count
    - **Property 40: Inventory Slot Count**
    - Verificar que SlotCount = 30
    - **Validates: Requirements 16.1**

---

- [x] 7. Sistema de Soft Caps
  - [x] 7.1 Crear ISoftCapSystem interface
    - ApplyDiminishingReturns, GetEffectiveValue, GetSoftCapInfo
    - Archivos: `Scripts/Progression/Interfaces/ISoftCapSystem.cs`
    - _Requirements: 8.2, 8.3_

  - [x] 7.2 Implementar SoftCapSystem
    - FirstThreshold: 30%, FirstPenalty: 50% DR
    - SecondThreshold: 50%, SecondPenalty: 75% DR
    - HardCap: 75% máximo
    - Archivos: `Scripts/Progression/SoftCapSystem.cs`
    - _Requirements: 8.2, 8.3_

  - [x] 7.3 Integrar con CharacterStats
    - Aplicar soft caps al calcular stats efectivos
    - Afectar: CritChance, Haste, Mastery
    - Archivos: `Scripts/Data/CharacterStats.cs`
    - _Requirements: 8.2_

  - [x] 7.4 Property test: Diminishing Returns Formula
    - **Property 21: Diminishing Returns Formula**
    - Verificar que valor > 30% resulta en valor efectivo menor
    - **Validates: Requirements 8.2, 8.3**

---

- [x] 8. Sistema de Durabilidad
  - [x] 8.1 Agregar durabilidad a ItemData
    - Campos: MaxDurability, CurrentDurability
    - Archivos: `Scripts/Data/ItemData.cs`
    - _Requirements: 8.5_

  - [x] 8.2 Crear IDurabilitySystem interface
    - DegradeDurability, RepairItem, GetStatPenalty
    - BrokenItemStatPenalty = 0.5
    - Archivos: `Scripts/Progression/Interfaces/IDurabilitySystem.cs`
    - _Requirements: 8.5, 8.6_

  - [x] 8.3 Implementar DurabilitySystem
    - Degradar durabilidad en combate (por hit recibido)
    - Aplicar 50% penalty a stats cuando durability = 0
    - Archivos: `Scripts/Progression/DurabilitySystem.cs`
    - _Requirements: 8.5, 8.6_

  - [x] 8.4 Integrar con EquipmentSystem
    - Modificar GetEquipmentStats para aplicar penalty
    - Archivos: `Scripts/Progression/EquipmentSystem.cs`
    - _Requirements: 8.6_

  - [x] 8.5 Property test: Broken Item Stat Penalty
    - **Property 24: Broken Item Stat Penalty**
    - Verificar que item con durability=0 tiene 50% stats
    - **Validates: Requirements 8.6**

---

- [x] 9. Sistema de Loot Need/Greed
  - [x] 9.1 Crear ILootDistributionSystem interface
    - StartNeedGreedRoll, SubmitRoll, FinalizeRolls
    - GetBadLuckProtectionBonus, RecordLootAttempt
    - Archivos: `Scripts/Progression/Interfaces/ILootDistributionSystem.cs`
    - _Requirements: 4.1, 4.4_

  - [x] 9.2 Implementar LootDistributionSystem
    - Need > Greed > Pass en prioridad
    - Roll 1-100 para desempate
    - Timeout de 30s para auto-pass
    - Archivos: `Scripts/Progression/LootDistributionSystem.cs`
    - _Requirements: 4.1, 4.2, 4.3_

  - [x] 9.3 Implementar Bad Luck Protection
    - Trackear intentos fallidos por raridad
    - Activar después de 10 intentos
    - +5% por intento adicional
    - Archivos: `Scripts/Progression/LootDistributionSystem.cs`
    - _Requirements: 4.4, 4.5_

  - [x] 9.4 Property test: Bad Luck Protection Scaling
    - **Property 13: Bad Luck Protection Scaling**
    - Verificar que bonus = (N-10) * 0.05 para N > 10
    - **Validates: Requirements 4.5**

---

- [x] 10. Sistema de Salvage
  - [x] 10.1 Crear ISalvageSystem interface
    - Salvage, PreviewSalvage
    - Archivos: `Scripts/Progression/Interfaces/ISalvageSystem.cs`
    - _Requirements: 4.7_

  - [x] 10.2 Implementar SalvageSystem
    - Materiales basados en raridad del item
    - Common: 1 material, Rare: 2-3, Epic: 3-5
    - Archivos: `Scripts/Progression/SalvageSystem.cs`
    - _Requirements: 4.7, 4.8_

  - [x] 10.3 Property test: Salvage Produces Materials
    - **Property 14: Salvage Produces Materials**
    - Verificar que salvage de item Rare+ produce materiales
    - **Validates: Requirements 4.7, 4.8**

---

- [x] 11. Checkpoint - Progression Systems
  - Verificar soft caps funcionan correctamente
  - Verificar durabilidad degrada y penaliza
  - Verificar loot Need/Greed distribuye correctamente
  - Verificar salvage produce materiales
  - Preguntar al usuario si hay dudas

---

- [x] 11.5 UI de Loot Window
  - [x] 11.5.1 Crear ILootWindowUI interface
    - Show, Hide, UpdateRollStatus, ShowWinner
    - Evento OnRollSubmitted
    - Archivos: `Scripts/UI/Interfaces/ILootWindowUI.cs`
    - _Requirements: 15.1, 15.2_

  - [x] 11.5.2 Implementar LootWindowUI
    - Mostrar items con nombre, icono, color de rareza
    - Botones Need, Greed, Pass por item
    - Countdown de 30 segundos
    - Auto-pass al expirar
    - Archivos: `Scripts/UI/Loot/LootWindowUI.cs`
    - _Requirements: 15.1, 15.2, 15.3, 15.4, 15.5, 15.6_

  - [x] 11.5.3 Integrar con LootDistributionSystem
    - Suscribirse a eventos de loot
    - Enviar rolls al sistema
    - Archivos: `Scripts/UI/Loot/LootWindowUI.cs`
    - _Requirements: 15.2_

  - [x] 11.5.4 Property test: Loot Window Timeout
    - **Property 39: Loot Window Timeout**
    - Verificar auto-pass después de timeout
    - **Validates: Requirements 15.4**

---

- [x] 11.6 UI de Reparación
  - [x] 11.6.1 Crear IRepairUI interface
    - Show, Hide, RefreshEquipment, UpdatePlayerGold
    - Eventos OnRepairClicked, OnRepairAllClicked
    - Archivos: `Scripts/UI/Interfaces/IRepairUI.cs`
    - _Requirements: 18.1, 18.2_

  - [x] 11.6.2 Implementar RepairUI
    - Mostrar equipo con durabilidad actual
    - Costo de reparación por item
    - Botones Repair y Repair All
    - Deshabilitar si no hay oro suficiente
    - Archivos: `Scripts/UI/Repair/RepairUI.cs`
    - _Requirements: 18.1, 18.2, 18.3, 18.4, 18.5_

  - [x] 11.6.3 Integrar con DurabilitySystem
    - Obtener durabilidad actual
    - Ejecutar reparación
    - Archivos: `Scripts/UI/Repair/RepairUI.cs`
    - _Requirements: 18.4_

---

- [x] 12. Sistema de Dificultades de Dungeon
  - [x] 12.1 Crear IDungeonDifficultySystem interface
    - SetDifficulty, GetModifiers, GetExclusiveLoot, GetAdditionalMechanics
    - Archivos: `Scripts/World/Interfaces/IDungeonDifficultySystem.cs`
    - _Requirements: 5.1, 5.2, 5.3_

  - [x] 12.2 Implementar DungeonDifficultySystem
    - Normal: 1.0x HP/DMG, 0 mecánicas extra
    - Heroic: 1.5x HP, 1.3x DMG, 1 mecánica extra
    - Mythic: 2.0x HP, 1.6x DMG, 2 mecánicas extra, loot exclusivo
    - Archivos: `Scripts/World/DungeonDifficultySystem.cs`
    - _Requirements: 5.1, 5.2, 5.3_

  - [x] 12.3 Integrar con DungeonSystem
    - Aplicar modificadores al crear instancia
    - Cargar mecánicas adicionales según dificultad
    - Archivos: `Scripts/World/DungeonSystem.cs`
    - _Requirements: 5.2_

---

- [x] 13. Sistema de Wipe Tracking
  - [x] 13.1 Crear IWipeTracker interface
    - GetWipeCount, RecordWipe, ShouldExpelGroup
    - MaxWipesBeforeExpulsion = 3
    - Archivos: `Scripts/World/Interfaces/IWipeTracker.cs`
    - _Requirements: 5.4, 5.5_

  - [x] 13.2 Implementar WipeTracker
    - Contar wipes por instancia
    - Respawn en entrada de dungeon
    - Expulsar y resetear después de 3 wipes
    - Archivos: `Scripts/World/WipeTracker.cs`
    - _Requirements: 5.4, 5.5_

  - [x] 13.3 Integrar con DungeonSystem
    - Suscribirse a OnWipe de CombatSystem
    - Llamar RecordWipe y verificar expulsión
    - Archivos: `Scripts/World/DungeonSystem.cs`
    - _Requirements: 5.4, 5.5_

  - [x] 13.4 Property test: Three Wipe Expulsion
    - **Property 17: Three Wipe Expulsion**
    - Verificar que ShouldExpelGroup = true después de 3 wipes
    - **Validates: Requirements 5.5**

---

- [x] 14. Sistema de Weekly Lockout
  - [x] 14.1 Crear IWeeklyLockoutSystem interface
    - IsLockedOut, RecordKill, GetResetTime, GetLockedBosses
    - Archivos: `Scripts/World/Interfaces/IWeeklyLockoutSystem.cs`
    - _Requirements: 5.7_

  - [x] 14.2 Implementar WeeklyLockoutSystem
    - Guardar kills en CharacterData.LockedBossIds
    - Reset cada lunes 00:00 UTC
    - Archivos: `Scripts/World/WeeklyLockoutSystem.cs`
    - _Requirements: 5.7, 5.8_

  - [x] 14.3 Integrar con BossSystem
    - Verificar lockout antes de generar loot
    - Permitir participación pero sin loot si locked
    - Archivos: `Scripts/World/BossSystem.cs`
    - _Requirements: 5.7, 5.8_

  - [x] 14.4 Property test: Weekly Lockout Enforcement
    - **Property 18: Weekly Lockout Enforcement**
    - Verificar que IsLockedOut = true después de kill
    - **Validates: Requirements 5.7**

---

- [x] 15. Checkpoint - Dungeon Systems
  - Verificar dificultades escalan correctamente
  - Verificar wipe tracking y expulsión
  - Verificar weekly lockout bloquea loot
  - Preguntar al usuario si hay dudas

---

- [x] 16. Floating Combat Text
  - [x] 16.1 Crear IFloatingCombatText interface
    - ShowDamage, ShowHealing, ShowMiss, ShowStatus
    - Archivos: `Scripts/UI/Interfaces/IFloatingCombatText.cs`
    - _Requirements: 6.3_

  - [x] 16.2 Implementar FloatingCombatText
    - Colores: blanco (normal), amarillo (crit), verde (heal), rojo (daño recibido)
    - Animación: flotar hacia arriba, fade out
    - Escala mayor para crits
    - Archivos: `Scripts/UI/FloatingCombatText.cs`
    - _Requirements: 6.3, 6.4_

  - [x] 16.3 Integrar con CombatSystem
    - Suscribirse a OnDamageDealt, OnHealingDone
    - Crear FCT en posición del target
    - Archivos: `Scripts/Combat/CombatSystem.cs`
    - _Requirements: 6.3_

  - [x] 16.4 Property test: FCT on Damage
    - **Property 19: FCT on Damage**
    - Verificar que daño > 0 genera FCT
    - **Validates: Requirements 6.3**

---

- [x] 17. Sistema de Indicadores de Aggro
  - [x] 17.1 Crear IAggroIndicator interface
    - ShowAggroIcon, UpdatePartyFrameColor, DrawAggroLine
    - Archivos: `Scripts/UI/Interfaces/IAggroIndicator.cs`
    - _Requirements: 6.2_

  - [x] 17.2 Implementar AggroIndicator
    - Icono sobre jugador con aggro
    - Color en party frame según nivel de threat
    - Línea visual entre enemigo y target
    - Archivos: `Scripts/UI/AggroIndicator.cs`
    - _Requirements: 6.2_

  - [x] 17.3 Integrar con AggroSystem
    - Suscribirse a OnAggroChanged
    - Actualizar indicadores cuando cambia aggro
    - Archivos: `Scripts/Combat/AggroSystem.cs`
    - _Requirements: 6.2_

---

- [x] 18. Sistema de Procs
  - [x] 18.1 Crear IProcSystem interface
    - RegisterProc, CheckProcs
    - Archivos: `Scripts/Combat/Interfaces/IProcSystem.cs`
    - _Requirements: 9.1_

  - [x] 18.2 Implementar ProcSystem
    - Verificar probabilidad en cada trigger
    - Aplicar efecto si proc activa
    - Internal cooldown para evitar spam
    - Archivos: `Scripts/Combat/ProcSystem.cs`
    - _Requirements: 9.1, 9.2_

  - [x] 18.3 Integrar con AbilitySystem
    - Llamar CheckProcs después de ejecutar habilidad
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 9.1_

  - [x] 18.4 Property test: Proc Probability
    - **Property 25: Proc Probability**
    - Verificar que tasa de procs aproxima probabilidad configurada
    - **Validates: Requirements 9.1**

---

- [x] 19. Sistema de Recursos Secundarios
  - [x] 19.1 Crear ISecondaryResourceSystem interface
    - RegisterResource, AddResource, SpendResource, GetResourceType
    - Archivos: `Scripts/Combat/Interfaces/ISecondaryResourceSystem.cs`
    - _Requirements: 9.3_

  - [x] 19.2 Implementar SecondaryResourceSystem
    - Rage para Warrior: genera al dar/recibir daño, decae fuera de combate
    - Holy Power para Paladin: genera con habilidades específicas, no decae
    - Archivos: `Scripts/Combat/SecondaryResourceSystem.cs`
    - _Requirements: 9.3_

  - [x] 19.3 Integrar con ClassSystem
    - Asignar recurso correcto al seleccionar clase
    - Archivos: `Scripts/Classes/ClassSystem.cs`
    - _Requirements: 9.3_

  - [x] 19.4 Property test: Class Resource Assignment
    - **Property 26: Class Resource Assignment**
    - Verificar Warrior=Rage, Paladin=HolyPower
    - **Validates: Requirements 9.3**

---

- [x] 20. Sistema de Friendly Fire
  - [x] 20.1 Crear IFriendlyFireSystem interface
    - ApplyAoEDamage, ApplyAoEHealing, GetAffectedTargets
    - Archivos: `Scripts/Combat/Interfaces/IFriendlyFireSystem.cs`
    - _Requirements: 9.4, 9.5_

  - [x] 20.2 Implementar FriendlyFireSystem
    - AoE damage afecta aliados si affectAllies=true
    - AoE heal afecta enemigos si affectEnemies=true
    - Archivos: `Scripts/Combat/FriendlyFireSystem.cs`
    - _Requirements: 9.4, 9.5_

  - [x] 20.3 Integrar con AbilitySystem
    - Usar FriendlyFireSystem para habilidades AoE
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 9.4, 9.5_

  - [x] 20.4 Property test: AoE Affects All Targets
    - **Property 27: AoE Affects All Targets**
    - Verificar que AoE con friendly fire afecta aliados y enemigos
    - **Validates: Requirements 9.4, 9.5**

---

- [x] 21. Sistema de Interrupts
  - [x] 21.1 Agregar habilidad Interrupt a cada clase
    - Warrior: Pummel
    - Mage: Counterspell
    - Priest: Silence
    - Paladin: Rebuke
    - Archivos: `ScriptableObjects/Abilities/[Class]/`
    - _Requirements: 9.6_

  - [x] 21.2 Implementar lógica de interrupt
    - Cancelar cast del enemigo
    - Aplicar lockout de 4 segundos
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 9.6, 9.7_

  - [x] 21.3 Property test: Interrupt Lockout Duration
    - **Property 29: Interrupt Lockout Duration**
    - Verificar que enemigo no puede castear por 4s después de interrupt
    - **Validates: Requirements 9.7**

---

- [x] 22. Checkpoint - Combat Polish
  - Verificar FCT aparece correctamente
  - Verificar indicadores de aggro funcionan
  - Verificar procs activan con probabilidad correcta
  - Verificar recursos secundarios funcionan por clase
  - Verificar friendly fire afecta targets correctos
  - Verificar interrupts cancelan y aplican lockout
  - Preguntar al usuario si hay dudas

---

- [x] 22.5 Setup de NavMesh para Enemy AI
  - [x] 22.5.1 Configurar NavMesh settings
    - Agent Radius: 0.5, Agent Height: 2.0
    - Step Height: 0.4, Max Slope: 45
    - Archivos: `ProjectSettings/NavMeshAreas.asset`
    - _Requirements: 19.4_

  - [x] 22.5.2 Agregar NavMeshAgent a Enemy prefab
    - Configurar speed, acceleration, stopping distance
    - Archivos: `Prefabs/Enemies/Enemy.prefab`
    - _Requirements: 19.1_

  - [x] 22.5.3 Crear herramienta de bake de NavMesh
    - MenuItem para rebakear todas las escenas
    - Archivos: `Scripts/Editor/NavMeshBakeTool.cs`
    - _Requirements: 19.5_

  - [x] 22.5.4 Bakear NavMesh en escenas existentes
    - TestScene.unity
    - Cualquier escena de región existente
    - Archivos: `Scenes/`
    - _Requirements: 19.2_

---

- [x] 22.6 Setup de FsCheck para Property Tests
  - [x] 22.6.1 Agregar FsCheck a manifest.json
    - Agregar referencia NuGet o OpenUPM
    - Nota: Unity no soporta NuGet directamente, usar .dll
    - Archivos: `Packages/manifest.json` o `Assets/Plugins/`
    - _Requirements: 22.1_

  - [x] 22.6.2 Configurar assembly de tests
    - Agregar referencia a FsCheck en test assembly
    - Archivos: `Assets/Tests/EditMode/EtherDomes.Tests.EditMode.asmdef`
    - _Requirements: 22.2_

  - [x] 22.6.3 Crear generadores custom para tipos de juego
    - CharacterDataGenerator
    - ItemDataGenerator
    - AbilityDataGenerator
    - Archivos: `Assets/Tests/EditMode/Generators/`
    - _Requirements: 22.3_

  - [x] 22.6.4 Crear base class para property tests
    - Configurar 100 iteraciones mínimo
    - Helper methods para assertions
    - Archivos: `Assets/Tests/EditMode/PropertyTests/PropertyTestBase.cs`
    - _Requirements: 22.4_

---

- [x] 23. Primera Dungeon - Proof of Concept
  - [x] 23.1 Crear escena Dungeon_Crypt_Small
    - 3 salas conectadas
    - Sala 1-2: 3-4 enemigos normales cada una
    - Sala 3: Boss room
    - Archivos: `Scenes/Dungeons/Dungeon_Crypt_Small.unity`
    - _Requirements: 10.1_

  - [x] 23.2 Crear prefab de enemigo Skeleton
    - Mesh placeholder (Capsule)
    - Enemy.cs + EnemyAI.cs
    - Stats: 500 HP, 20 damage
    - Archivos: `Prefabs/Enemies/Skeleton.prefab`
    - _Requirements: 10.2_

  - [x] 23.3 Crear BossDataSO para Crypt Lord
    - BaseHealth: 10000, BaseDamage: 50
    - 2 mecánicas con tells visuales
    - Loot tables por dificultad
    - Archivos: `ScriptableObjects/Bosses/CryptLord.asset`
    - _Requirements: 10.3, 10.4, 10.5_

  - [x] 23.4 Implementar mecánicas del boss
    - Mecánica 1: Ground AoE cada 15s (círculo rojo, 2s tell)
    - Mecánica 2: Cleave frontal cada 10s (cono, 1.5s tell)
    - Archivos: `Scripts/Enemy/BossAI.cs`
    - _Requirements: 10.3_

  - [x] 23.5 Implementar scaling por grupo
    - HP = BaseHealth * (1 + (N-1) * 0.2)
    - Archivos: `Scripts/World/DungeonSystem.cs`
    - _Requirements: 10.6, 10.7_

  - [x] 23.6 Property test: HP Scaling Per Player
    - **Property 30: HP Scaling Per Player**
    - Verificar fórmula de scaling de HP
    - **Validates: Requirements 10.7**

---

- [x] 24. Checkpoint Final - MVP Complete
  - Ejecutar dungeon completa con grupo
  - Verificar todas las mecánicas funcionan
  - Verificar loot se distribuye correctamente
  - Verificar weekly lockout funciona
  - Ejecutar todos los property tests
  - Preguntar al usuario si hay dudas

---

## Notes

- Todos los property tests son requeridos (cobertura completa)
- Total de property tests: 25 (15 originales + 10 de puntos ciegos)
- Cada checkpoint es un punto de validación con el usuario
- Los property tests usan FsCheck para C#/.NET
- Mínimo 100 iteraciones por property test
- Tag format: `// Feature: mvp-10-features, Property N: [title]`

### Property Tests Summary

| # | Property | Requirement |
|---|----------|-------------|
| 1 | Session Lock Mutual Exclusion | 1.1, 1.2 |
| 4 | Migration Idempotence | 1.6 |
| 6 | Spec Change Replaces Abilities | 2.1 |
| 10 | Leash Distance Enforcement | 3.3 |
| 11 | HP Preservation on Return | 3.4 |
| 13 | Bad Luck Protection Scaling | 4.5 |
| 14 | Salvage Produces Materials | 4.7, 4.8 |
| 17 | Three Wipe Expulsion | 5.5 |
| 18 | Weekly Lockout Enforcement | 5.7 |
| 19 | FCT on Damage | 6.3 |
| 21 | Diminishing Returns Formula | 8.2, 8.3 |
| 24 | Broken Item Stat Penalty | 8.6 |
| 25 | Proc Probability | 9.1 |
| 26 | Class Resource Assignment | 9.3 |
| 27 | AoE Affects All Targets | 9.4, 9.5 |
| 29 | Interrupt Lockout Duration | 9.7 |
| 30 | HP Scaling Per Player | 10.7 |
| 31 | Mana Cost Enforcement | 12.3 |
| 33 | Party Size Limits | 14.1 |
| 35 | Latency State Transitions | 21.3, 21.4 |
| 38 | Highest Damage Dealer Selection | 13.3 |
| 39 | Loot Window Timeout | 15.4 |
| 40 | Inventory Slot Count | 16.1 |

### Archivos a Modificar (Código Existente)

| Archivo | Cambios |
|---------|---------|
| `CharacterData.cs` | +8 campos nuevos |
| `Enemy.cs` | +damage tracking dictionary |
| `AbilitySystem.cs` | +mana integration |
| `NetworkSessionManager.cs` | +latency monitor, +party system |
| `manifest.json` | +FsCheck (via Plugins/) |
