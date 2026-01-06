# Implementation Plan: New Classes and Combat Systems

## Overview

Plan de implementación para agregar 4 nuevas clases (Rogue, Hunter, Warlock, Death Knight) y los sistemas de soporte: BuffSystem, PetSystem, StealthSystem, y extensiones al AbilitySystem para habilidades canalizadas.

## Tasks

- [x] 1. Sistema de Buffs/Debuffs
  - [x] 1.1 Crear IBuffSystem interface
    - Definir métodos ApplyBuff, ApplyDebuff, RemoveBuff, RemoveDebuff
    - Definir eventos OnBuffApplied, OnBuffExpired, OnDoTTick, OnHoTTick
    - Definir constantes MaxBuffsPerEntity=20, MinDuration=1, MaxDuration=300
    - Archivos: `Scripts/Combat/Interfaces/IBuffSystem.cs`
    - _Requirements: 1.1, 1.7_

  - [x] 1.2 Crear BuffData y BuffInstance classes
    - BuffData: BuffId, DisplayName, Duration, EffectType, TickInterval, TickDamage/Healing
    - BuffInstance: Data, RemainingDuration, NextTickTime, SourceId, AppliedTime
    - Archivos: `Scripts/Data/BuffData.cs`
    - _Requirements: 1.2, 1.4, 1.5_

  - [x] 1.3 Implementar BuffSystem
    - Diccionario de buffs/debuffs activos por entityId
    - Update loop para decrementar duraciones y procesar ticks
    - Lógica de reemplazo cuando se excede límite de 20
    - Integración con CombatSystem para aplicar daño/heal de DoT/HoT
    - Archivos: `Scripts/Combat/BuffSystem.cs`
    - _Requirements: 1.2, 1.3, 1.4, 1.5, 1.6, 1.7, 1.8_

  - [x] 1.4 Property test: Buff Duration Bounds
    - **Property 1: Buff Duration Bounds**
    - Verificar que duración se clampea entre 1 y 300 segundos
    - **Validates: Requirements 1.1**

  - [x] 1.5 Property test: Buff/Debuff Limit
    - **Property 2: Buff/Debuff Limit Enforcement**
    - Verificar que nunca hay más de 20 buffs o 20 debuffs
    - **Validates: Requirements 1.7, 1.8**

  - [x] 1.6 Property test: DoT/HoT Ticks
    - **Property 3: DoT/HoT Tick Consistency**
    - Verificar número de ticks y daño/heal por tick
    - **Validates: Requirements 1.4, 1.5, 1.6**

---

- [x] 2. Habilidades Canalizadas
  - [x] 2.1 Extender AbilityData para channeled
    - Agregar IsChanneled, ChannelDuration, TickInterval, TotalTicks
    - Archivos: `Scripts/Data/AbilityData.cs`
    - _Requirements: 2.1, 2.3_

  - [x] 2.2 Implementar lógica de channel en AbilitySystem
    - Estado de channeling separado de casting
    - Aplicar efectos en cada tick durante el channel
    - Interrupción por movimiento (threshold 0.1m)
    - Evento OnChannelCompleted
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 2.2, 2.4, 2.5, 2.7_

  - [x] 2.3 Actualizar CastBar para channels
    - Mostrar barra que se vacía (depleting) en lugar de llenarse
    - Archivos: `Scripts/UI/CastBar.cs`
    - _Requirements: 2.6_

  - [x] 2.4 Property test: Channel Tick Count
    - **Property 4: Channeled Ability Tick Count**
    - Verificar número de ticks = ceil(duration / interval)
    - **Validates: Requirements 2.1, 2.2, 2.3**

  - [x] 2.5 Property test: Channel Interruption
    - **Property 5: Channel Interruption on Movement**
    - Verificar que movimiento interrumpe channel
    - **Validates: Requirements 2.4, 2.5**

---

- [x] 3. Checkpoint - Buff y Channel Systems
  - Verificar que buffs/debuffs se aplican y expiran correctamente
  - Verificar que DoTs/HoTs hacen ticks de daño/heal
  - Verificar que channels funcionan con ticks
  - Preguntar al usuario si hay dudas

---

- [x] 4. Sistema de Mascotas Pasivas
  - [x] 4.1 Crear IPetSystem interface
    - Definir métodos SummonPet, DismissPet, CommandAttack, CommandFollow
    - Definir eventos OnPetSummoned, OnPetDied, OnPetDamageDealt
    - Definir constantes FollowDistance=3, TeleportDistance=40, ResummonCooldown=10
    - Archivos: `Scripts/Combat/Interfaces/IPetSystem.cs`
    - _Requirements: 3.1, 3.5_

  - [x] 4.2 Crear PetData y PetInstance classes
    - PetData: PetId, DisplayName, PetType, BaseHealth, BaseDamage, AttackSpeed
    - PetInstance: Data, OwnerId, PetEntityId, CurrentHealth, CurrentTargetId, State
    - Archivos: `Scripts/Data/PetData.cs`
    - _Requirements: 3.4_

  - [x] 4.3 Implementar PetSystem
    - Spawn de pet entity con NavMeshAgent
    - Estado Following: seguir al owner a 3m de distancia
    - Estado Attacking: atacar target del owner automáticamente
    - Teleport si distancia > 40m
    - Cooldown de resummon de 10s después de muerte
    - Archivos: `Scripts/Combat/PetSystem.cs`
    - _Requirements: 3.1, 3.2, 3.3, 3.5, 3.6, 3.7_

  - [x] 4.4 Crear Pet prefab base
    - Capsule mesh placeholder
    - NavMeshAgent configurado
    - PetBehaviour component
    - Archivos: `Prefabs/Pets/BasePet.prefab`
    - _Requirements: 3.6_

  - [x] 4.5 Property test: Pet Follow Distance
    - **Property 6: Pet Follow Distance**
    - Verificar que pet mantiene ~3m de distancia
    - **Validates: Requirements 3.3**

  - [x] 4.6 Property test: Pet Resummon Cooldown
    - **Property 7: Pet Resummon Cooldown**
    - Verificar que no se puede resummonear antes de 10s
    - **Validates: Requirements 3.5**

---

- [x] 5. Sistema de Sigilo
  - [x] 5.1 Crear IStealthSystem interface
    - Definir métodos TryEnterStealth, BreakStealth, IsInStealth
    - Definir eventos OnStealthEntered, OnStealthBroken
    - Definir constantes MovementSpeedMultiplier=0.7, StealthCooldown=2
    - Archivos: `Scripts/Combat/Interfaces/IStealthSystem.cs`
    - _Requirements: 4.1, 4.4, 4.6_

  - [x] 5.2 Implementar StealthSystem
    - Estado de stealth por playerId
    - Break automático al atacar o recibir daño
    - Cooldown de 2s después de break
    - Modificador de velocidad 70%
    - Archivos: `Scripts/Combat/StealthSystem.cs`
    - _Requirements: 4.1, 4.2, 4.3, 4.4, 4.6_

  - [x] 5.3 Integrar con AbilitySystem
    - Verificar RequiresStealth antes de ejecutar habilidad
    - Llamar BreakStealth cuando BreaksStealth=true
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 4.5_

  - [x] 5.4 Implementar visual de stealth
    - Reducir opacity del renderer a 30% para local player
    - Ocultar completamente para enemigos (0% opacity)
    - Archivos: `Scripts/Combat/StealthVisual.cs`
    - _Requirements: 4.7_

  - [x] 5.5 Property test: Stealth Break Conditions
    - **Property 8: Stealth Break Conditions**
    - Verificar que daño y ataque rompen stealth
    - **Validates: Requirements 4.2, 4.3**

  - [x] 5.6 Property test: Stealth Movement Speed
    - **Property 9: Stealth Movement Speed**
    - Verificar velocidad = 70% de base
    - **Validates: Requirements 4.4**

  - [x] 5.7 Property test: Stealth Cooldown
    - **Property 10: Stealth Cooldown Enforcement**
    - Verificar cooldown de 2s después de break
    - **Validates: Requirements 4.6**

---

- [x] 6. Checkpoint - Pet y Stealth Systems
  - Verificar que pets siguen y atacan correctamente
  - Verificar que stealth funciona con break conditions
  - Verificar integración con AbilitySystem
  - Preguntar al usuario si hay dudas

---

- [x] 7. Recursos Secundarios Extendidos
  - [x] 7.1 Agregar Energy y Focus a SecondaryResourceSystem
    - Energy: 100 max, regenera 10/s
    - Focus: 100 max, regenera 5/s
    - Archivos: `Scripts/Combat/SecondaryResourceSystem.cs`
    - _Requirements: 5.2, 6.2_

  - [x] 7.2 Implementar Combo Points
    - 5 max, no regenera
    - Generado por habilidades generator
    - Consumido por habilidades finisher
    - Archivos: `Scripts/Combat/SecondaryResourceSystem.cs`
    - _Requirements: 5.3, 5.4, 5.5_

  - [x] 7.3 Integrar con AbilitySystem
    - Verificar GeneratesComboPoint y ConsumesComboPoints
    - Escalar daño por combo points consumidos
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 5.4, 5.5_

  - [x] 7.4 Property test: Energy Regeneration
    - **Property 11: Energy Regeneration Rate**
    - Verificar 10/s de regeneración
    - **Validates: Requirements 5.2**

  - [x] 7.5 Property test: Combo Points
    - **Property 12: Combo Point Generation and Consumption**
    - Verificar generación y consumo con scaling
    - **Validates: Requirements 5.3, 5.4, 5.5**

  - [x]* 7.6 Property test: Focus Regeneration
    - **Property 13: Focus Regeneration Rate**
    - Verificar 5/s de regeneración
    - **Validates: Requirements 6.2**

---

- [x] 8. Clase Rogue
  - [x] 8.1 Agregar Rogue a CharacterClass enum
    - Agregar Rogue con specs Assassination y Combat
    - Archivos: `Scripts/Data/Enums.cs`
    - _Requirements: 5.1_

  - [x] 8.2 Definir base stats de Rogue
    - High Agility (15), Medium Stamina (10), Low Strength/Intellect
    - Growth: +2 Agility, +1 Stamina per level
    - Archivos: `Scripts/Classes/ClassSystem.cs`
    - _Requirements: 12.2_

  - [x] 8.3 Crear habilidades de Rogue Assassination
    - Mutilate (generator, 50 energy)
    - Envenom (finisher, 35 energy)
    - Rupture (finisher DoT, 25 energy)
    - Vendetta (buff, 0 energy, 2min CD)
    - Archivos: `ScriptableObjects/Abilities/Rogue/Assassination/`
    - _Requirements: 5.6_

  - [x] 8.4 Crear habilidades de Rogue Combat
    - Sinister Strike (generator, 45 energy)
    - Eviscerate (finisher, 35 energy)
    - Blade Flurry (buff, 0 energy, 2min CD)
    - Adrenaline Rush (buff, 0 energy, 3min CD)
    - Archivos: `ScriptableObjects/Abilities/Rogue/Combat/`
    - _Requirements: 5.7_

  - [x] 8.5 Crear habilidades compartidas de Rogue
    - Stealth (toggle, 0 energy)
    - Cheap Shot (stun from stealth, 40 energy)
    - Ambush (damage from stealth, 60 energy)
    - Kick (interrupt, 15 energy)
    - Archivos: `ScriptableObjects/Abilities/Rogue/Shared/`
    - _Requirements: 5.8_

---

- [x] 9. Clase Hunter
  - [x] 9.1 Agregar Hunter a CharacterClass enum
    - Agregar Hunter con specs BeastMastery y Marksmanship
    - Archivos: `Scripts/Data/Enums.cs`
    - _Requirements: 6.1_

  - [x] 9.2 Definir base stats de Hunter
    - High Agility (14), Medium Stamina (11), Low Strength/Intellect
    - Growth: +2 Agility, +1 Stamina per level
    - Archivos: `Scripts/Classes/ClassSystem.cs`
    - _Requirements: 12.3_

  - [x] 9.3 Crear habilidades de Hunter Beast Mastery
    - Kill Command (pet attack, 30 focus)
    - Bestial Wrath (buff, 0 focus, 2min CD)
    - Cobra Shot (damage, 35 focus)
    - Multi-Shot (AoE, 40 focus)
    - Archivos: `ScriptableObjects/Abilities/Hunter/BeastMastery/`
    - _Requirements: 6.4_

  - [x] 9.4 Crear habilidades de Hunter Marksmanship
    - Aimed Shot (cast time, 50 focus)
    - Rapid Fire (channel, 0 focus, 2min CD)
    - Arcane Shot (instant, 40 focus)
    - Volley (AoE channel, 60 focus)
    - Archivos: `ScriptableObjects/Abilities/Hunter/Marksmanship/`
    - _Requirements: 6.5_

  - [x] 9.5 Crear habilidades compartidas de Hunter
    - Concussive Shot (slow, 20 focus)
    - Freezing Trap (CC, 0 focus, 30s CD)
    - Disengage (knockback self, 0 focus, 20s CD)
    - Counter Shot (interrupt, 40 focus)
    - Archivos: `ScriptableObjects/Abilities/Hunter/Shared/`
    - _Requirements: 6.6_

  - [x] 9.6 Implementar dead zone de Hunter
    - MinRange = 8m para habilidades ranged
    - Error "Too close" si target dentro de MinRange
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 6.8_

  - [x]* 9.7 Property test: Hunter Dead Zone
    - **Property 14: Hunter Dead Zone Enforcement**
    - Verificar que habilidades fallan dentro de 8m
    - **Validates: Requirements 6.8**

---

- [x] 10. Checkpoint - Rogue y Hunter
  - Verificar que Rogue funciona con energy y combo points
  - Verificar que Hunter funciona con focus y pet
  - Verificar dead zone de Hunter
  - Preguntar al usuario si hay dudas



---

- [x] 11. Clase Warlock
  - [x] 11.1 Agregar Warlock a CharacterClass enum
    - Agregar Warlock con specs Affliction y Destruction
    - Archivos: `Scripts/Data/Enums.cs`
    - _Requirements: 7.1_

  - [x] 11.2 Definir base stats de Warlock
    - High Intellect (15), Medium Stamina (9), Low Strength/Agility
    - Growth: +2 Intellect, +1 Stamina per level
    - Archivos: `Scripts/Classes/ClassSystem.cs`
    - _Requirements: 12.4_

  - [x] 11.3 Crear habilidades de Warlock Affliction
    - Corruption (DoT instant, 20 mana)
    - Agony (DoT instant, 25 mana)
    - Drain Life (channel heal, 30 mana)
    - Haunt (damage + debuff, 40 mana)
    - Archivos: `ScriptableObjects/Abilities/Warlock/Affliction/`
    - _Requirements: 7.4_

  - [x] 11.4 Crear habilidades de Warlock Destruction
    - Chaos Bolt (cast time, 50 mana)
    - Incinerate (cast time, 35 mana)
    - Immolate (DoT, 25 mana)
    - Rain of Fire (AoE channel, 60 mana)
    - Archivos: `ScriptableObjects/Abilities/Warlock/Destruction/`
    - _Requirements: 7.5_

  - [x] 11.5 Crear habilidades compartidas de Warlock
    - Fear (CC, 30 mana)
    - Shadowfury (AoE stun, 40 mana, 1min CD)
    - Summon Demon (summon pet, 100 mana)
    - Archivos: `ScriptableObjects/Abilities/Warlock/Shared/`
    - _Requirements: 7.6_

  - [x] 11.6 Implementar Drain Life healing
    - Heal = 50% of damage dealt
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 7.7_

  - [x] 11.7 Crear pets de Warlock
    - Voidwalker (tank): high HP, taunt ability
    - Imp (damage): low HP, fireball ability
    - Archivos: `ScriptableObjects/Pets/Warlock/`
    - _Requirements: 7.8_

  - [x]* 11.8 Property test: Drain Life Healing
    - **Property 15: Drain Life Healing**
    - Verificar heal = 50% of damage
    - **Validates: Requirements 7.7**

---

- [x] 12. Clase Death Knight
  - [x] 12.1 Agregar DeathKnight a CharacterClass enum
    - Agregar DeathKnight con specs Blood y Frost
    - Archivos: `Scripts/Data/Enums.cs`
    - _Requirements: 8.1_

  - [x] 12.2 Definir base stats de Death Knight
    - High Strength (14), High Stamina (14), Medium Intellect (8)
    - Growth: +2 Strength, +2 Stamina per level
    - Archivos: `Scripts/Classes/ClassSystem.cs`
    - _Requirements: 12.5_

  - [x] 12.3 Crear habilidades de Death Knight Blood
    - Death Strike (heal, 45 mana)
    - Heart Strike (damage, 30 mana)
    - Blood Boil (AoE, 35 mana)
    - Vampiric Blood (buff, 0 mana, 2min CD)
    - Archivos: `ScriptableObjects/Abilities/DeathKnight/Blood/`
    - _Requirements: 8.5_

  - [x] 12.4 Crear habilidades de Death Knight Frost
    - Obliterate (damage, 45 mana)
    - Frost Strike (damage, 25 mana)
    - Howling Blast (AoE, 40 mana)
    - Pillar of Frost (buff, 0 mana, 1min CD)
    - Archivos: `ScriptableObjects/Abilities/DeathKnight/Frost/`
    - _Requirements: 8.6_

  - [x] 12.5 Crear habilidades compartidas de Death Knight
    - Death Grip (pull, 30 mana)
    - Chains of Ice (slow, 25 mana)
    - Mind Freeze (interrupt, 20 mana)
    - Archivos: `ScriptableObjects/Abilities/DeathKnight/Shared/`
    - _Requirements: 8.7_

  - [x] 12.6 Implementar Death Strike healing
    - Track damage taken in last 5 seconds
    - Heal = 25% of tracked damage (min 10% max HP)
    - Archivos: `Scripts/Combat/CombatSystem.cs`
    - _Requirements: 8.8_

  - [x]* 12.7 Property test: Death Strike Healing
    - **Property 16: Death Strike Healing**
    - Verificar heal = 25% of recent damage
    - **Validates: Requirements 8.8**

---

- [x] 13. Checkpoint - Warlock y Death Knight
  - Verificar que Warlock funciona con DoTs y pet
  - Verificar que Death Knight funciona con Death Strike
  - Verificar Drain Life healing
  - Preguntar al usuario si hay dudas

---

- [x] 14. Sistema de Efectos CC
  - [x] 14.1 Implementar efectos de CC en BuffSystem
    - Slow: reduce movement speed
    - Stun: prevent all actions
    - Fear: random movement
    - Root: prevent movement only
    - Archivos: `Scripts/Combat/BuffSystem.cs`
    - _Requirements: 11.1, 11.2, 11.3_

  - [x] 14.2 Implementar Death Grip (pull)
    - Move target to caster position
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 11.4_

  - [x] 14.3 Implementar Disengage (knockback self)
    - Move caster away from target
    - Archivos: `Scripts/Combat/AbilitySystem.cs`
    - _Requirements: 11.5_

  - [x] 14.4 Implementar Diminishing Returns
    - Track CC applications per target per type
    - 100% -> 50% -> 25% -> immune (15s)
    - Reset after 15s without CC of that type
    - Archivos: `Scripts/Combat/DiminishingReturnsSystem.cs`
    - _Requirements: 11.6, 11.7_

  - [x] 14.5 Property test: Diminishing Returns
    - **Property 17: Diminishing Returns on CC**
    - Verificar reducción de duración y inmunidad
    - **Validates: Requirements 11.6, 11.7**

---

- [x] 15. UI de Buffs/Debuffs
  - [x] 15.1 Crear BuffUI component
    - Grid de iconos para buffs (arriba de health bar)
    - Grid de iconos para debuffs (abajo de health bar)
    - Countdown de duración en cada icono
    - Archivos: `Scripts/UI/BuffUI.cs`
    - _Requirements: 1.9, 1.10_

  - [x] 15.2 Integrar buffs/debuffs en TargetFrame
    - Mostrar buffs/debuffs del target seleccionado
    - Highlight debuffs aplicados por el jugador local
    - Archivos: `Scripts/UI/TargetFrame.cs`
    - _Requirements: 9.1, 9.2, 9.3, 9.4, 9.5, 9.6_

  - [x] 15.3 Crear PetUI component
    - Health bar de pet cerca del player frame
    - Archivos: `Scripts/UI/PetUI.cs`
    - _Requirements: 3.8_

  - [x] 15.4 Actualizar ResourceUI para nuevos recursos
    - Energy bar para Rogue
    - Focus bar para Hunter
    - Combo points como pips discretos
    - Archivos: `Scripts/UI/ResourceUI.cs`
    - _Requirements: 10.6, 10.7_

---

- [x] 16. Integración con ClassSystem
  - [x] 16.1 Actualizar ClassSystem con nuevas clases
    - Agregar Rogue, Hunter, Warlock, DeathKnight
    - Asignar recursos primarios y secundarios por clase
    - Archivos: `Scripts/Classes/ClassSystem.cs`
    - _Requirements: 10.1, 10.2, 10.3_

  - [x] 16.2 Implementar stat growth por nivel
    - Aplicar growth rates al subir de nivel
    - Archivos: `Scripts/Progression/ProgressionSystem.cs`
    - _Requirements: 12.6, 12.7_

  - [x]* 16.3 Property test: Class Stat Growth
    - **Property 18: Class Stat Growth**
    - Verificar que stats aumentan según growth rates
    - **Validates: Requirements 12.6, 12.7**

---

- [x] 17. Checkpoint Final - New Classes Complete
  - Ejecutar todos los property tests (18 total)
  - Verificar que las 4 clases funcionan correctamente
  - Verificar integración de todos los sistemas
  - Preguntar al usuario si hay dudas

---

## Notes

- Tareas marcadas con `*` son property tests opcionales
- Total de property tests: 18
- Cada checkpoint es un punto de validación con el usuario
- Los property tests usan FsCheck para C#/.NET
- Mínimo 100 iteraciones por property test
- Tag format: `// Feature: new-classes-combat, Property N: [title]`

### Property Tests Summary

| # | Property | Requirement |
|---|----------|-------------|
| 1 | Buff Duration Bounds | 1.1 |
| 2 | Buff/Debuff Limit Enforcement | 1.7, 1.8 |
| 3 | DoT/HoT Tick Consistency | 1.4, 1.5, 1.6 |
| 4 | Channeled Ability Tick Count | 2.1, 2.2, 2.3 |
| 5 | Channel Interruption on Movement | 2.4, 2.5 |
| 6 | Pet Follow Distance | 3.3 |
| 7 | Pet Resummon Cooldown | 3.5 |
| 8 | Stealth Break Conditions | 4.2, 4.3 |
| 9 | Stealth Movement Speed | 4.4 |
| 10 | Stealth Cooldown Enforcement | 4.6 |
| 11 | Energy Regeneration Rate | 5.2 |
| 12 | Combo Point Generation/Consumption | 5.3, 5.4, 5.5 |
| 13 | Focus Regeneration Rate | 6.2 |
| 14 | Hunter Dead Zone Enforcement | 6.8 |
| 15 | Drain Life Healing | 7.7 |
| 16 | Death Strike Healing | 8.8 |
| 17 | Diminishing Returns on CC | 11.6, 11.7 |
| 18 | Class Stat Growth | 12.6, 12.7 |

### Archivos Nuevos a Crear

| Archivo | Descripción |
|---------|-------------|
| `Scripts/Combat/BuffSystem.cs` | Sistema de buffs/debuffs |
| `Scripts/Combat/Interfaces/IBuffSystem.cs` | Interface de BuffSystem |
| `Scripts/Combat/PetSystem.cs` | Sistema de mascotas |
| `Scripts/Combat/Interfaces/IPetSystem.cs` | Interface de PetSystem |
| `Scripts/Combat/StealthSystem.cs` | Sistema de sigilo |
| `Scripts/Combat/Interfaces/IStealthSystem.cs` | Interface de StealthSystem |
| `Scripts/Combat/StealthVisual.cs` | Visual de sigilo |
| `Scripts/Combat/DiminishingReturnsSystem.cs` | Sistema de DR |
| `Scripts/Data/BuffData.cs` | Datos de buff/debuff |
| `Scripts/Data/PetData.cs` | Datos de mascota |
| `Scripts/UI/BuffUI.cs` | UI de buffs/debuffs |
| `Scripts/UI/PetUI.cs` | UI de mascota |
| `Prefabs/Pets/BasePet.prefab` | Prefab base de pet |

### Archivos a Modificar

| Archivo | Cambios |
|---------|---------|
| `Scripts/Data/Enums.cs` | +4 clases, +ResourceType |
| `Scripts/Data/AbilityData.cs` | +channeled, +stealth, +combo |
| `Scripts/Combat/AbilitySystem.cs` | +channel, +stealth, +combo, +minRange |
| `Scripts/Combat/SecondaryResourceSystem.cs` | +Energy, +Focus, +ComboPoints |
| `Scripts/Classes/ClassSystem.cs` | +4 clases con stats |
| `Scripts/UI/CastBar.cs` | +channel display |
| `Scripts/UI/TargetFrame.cs` | +buff/debuff display |
| `Scripts/UI/ResourceUI.cs` | +Energy, +Focus, +ComboPoints |

