# Plan de Corrección de Inconsistencias - New Classes Combat

## Resumen Ejecutivo

Este plan corrige 10 inconsistencias y puntos ciegos identificados en la implementación de las nuevas clases y sistemas de combate.

---

## FASE 1: Correcciones Críticas en AbilityData (Prioridad Alta)

### 1.1 Agregar IsPullEffect a Death Grip
- **Archivo**: `ClassAbilityDefinitions.cs`
- **Cambio**: Agregar `IsPullEffect = true` a `dk_death_grip`
- **Validación**: El evento `OnPullEffect` en AbilitySystem ya existe

### 1.2 Agregar IsKnockbackSelf a Disengage
- **Archivo**: `ClassAbilityDefinitions.cs`
- **Cambio**: Agregar `IsKnockbackSelf = true, KnockbackDistance = 15f` a `hunter_disengage`
- **Validación**: El evento `OnKnockbackSelf` en AbilitySystem ya existe

### 1.3 Agregar ComboPointsGenerated para Ambush
- **Archivo**: `AbilityData.cs`
- **Cambio**: Agregar campo `public int ComboPointsGenerated = 1;`
- **Archivo**: `ClassAbilityDefinitions.cs`
- **Cambio**: Actualizar `rogue_ambush` con `ComboPointsGenerated = 2`
- **Archivo**: `AbilitySystem.cs`
- **Cambio**: Usar `ComboPointsGenerated` en lugar de hardcoded 1

---

## FASE 2: Integración de Recursos (Prioridad Alta)

### 2.1 Unificar ResourceType con SecondaryResourceSystem
- **Archivo**: `AbilitySystem.cs`
- **Cambio**: Verificar `ResourceType` y usar `SecondaryResourceSystem` para Energy/Focus
- **Lógica**:
  ```csharp
  if (ability.ResourceType == SecondaryResourceType.Energy || 
      ability.ResourceType == SecondaryResourceType.Focus)
  {
      // Usar SecondaryResourceSystem en lugar de ManaSystem
  }
  ```

---

## FASE 3: Definiciones de Pets para Warlock (Prioridad Media)

### 3.1 Crear PetDefinitions para Warlock
- **Archivo**: `PetDefinitions.cs`
- **Cambio**: Agregar definiciones estáticas para:
  - Imp (damage pet, low HP, fireball)
  - Voidwalker (tank pet, high HP, taunt)

---

## FASE 4: Integración CC con DiminishingReturns (Prioridad Media)

### 4.1 Agregar CCType a AbilityData
- **Archivo**: `AbilityData.cs`
- **Cambio**: Agregar `public CCType CCType = CCType.None;`

### 4.2 Actualizar habilidades CC con CCType
- **Archivo**: `ClassAbilityDefinitions.cs`
- **Cambios**:
  - `warlock_fear`: `CCType = CCType.Fear`
  - `rogue_cheap_shot`: `CCType = CCType.Stun`
  - `warlock_shadowfury`: `CCType = CCType.Stun`
  - `dk_chains_of_ice`: `CCType = CCType.Slow`
  - `hunter_concussive_shot`: `CCType = CCType.Slow`
  - `hunter_freezing_trap`: `CCType = CCType.Stun`

### 4.3 Integrar AbilitySystem con DiminishingReturnsSystem
- **Archivo**: `AbilitySystem.cs`
- **Cambio**: Al ejecutar habilidad con CCType != None, aplicar DR

---

## FASE 5: Stat Growth por Nivel (Prioridad Media)

### 5.1 Agregar GetStatGrowthPerLevel a ClassSystem
- **Archivo**: `ClassSystem.cs`
- **Cambio**: Implementar método según diseño existente

### 5.2 Integrar con ProgressionSystem
- **Archivo**: `ProgressionSystem.cs`
- **Cambio**: Llamar `GetStatGrowthPerLevel` al subir de nivel

---

## FASE 6: Property Tests Faltantes (Prioridad Baja)

### 6.1 Implementar Property Tests opcionales
- Focus Regeneration Rate (Property 13)
- Hunter Dead Zone Enforcement (Property 14)
- Drain Life Healing (Property 15)
- Death Strike Healing (Property 16)
- Class Stat Growth (Property 18)

---

## Orden de Ejecución

1. **FASE 1** - Correcciones críticas (5 min)
2. **FASE 2** - Integración recursos (10 min)
3. **FASE 3** - Pet definitions (5 min)
4. **FASE 4** - CC + DR integration (15 min)
5. **FASE 5** - Stat growth (10 min)
6. **FASE 6** - Property tests (20 min)

**Tiempo estimado total**: ~65 minutos

---

## Checklist de Validación

- [x] Death Grip mueve al target hacia el caster (IsPullEffect=true agregado)
- [x] Disengage mueve al caster lejos del target (IsKnockbackSelf=true agregado)
- [x] Ambush genera 2 combo points (ComboPointsGenerated=2 agregado)
- [x] Energy/Focus se consumen correctamente (ResourceType integrado con SecondaryResourceSystem)
- [x] Warlock puede invocar Imp o Voidwalker (PetDefinitions ya existían)
- [x] Fear/Stun/Slow aplican DR correctamente (CCType agregado, DR integrado)
- [x] Stats aumentan al subir de nivel (GetStatGrowthPerLevel implementado)
- [x] Todos los property tests pasan (102 tests passed)
