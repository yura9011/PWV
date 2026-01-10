# Plan de Desarrollo - 10 Features para MVP

## Resumen Ejecutivo

Este documento detalla el análisis y plan de implementación para las 10 features necesarias para alcanzar el MVP de The Ether Domes. Cada feature incluye:
- Estado actual del código
- Análisis de lo que falta
- Tareas específicas
- Estimación de tiempo
- Dependencias

---

## Feature 1: Completar CharacterPersistenceService

### Estado Actual: 90% ✅

**Código Existente:**
- `CharacterPersistenceService.cs` - Save/Load async completo
- `EncryptionService.cs` - AES-256 encryption + SHA256 hash
- Export/Import para network transfer
- Manejo de errores y logging

**Análisis de Gaps:**
1. La validación de integridad solo verifica estructura, no el hash real
2. No hay tests de property-based testing
3. El hash se computa pero no se verifica al cargar

### Plan de Tareas

```
[ ] 1.1 Mejorar validación de hash de integridad
    - Modificar ValidateCharacterIntegrity() para verificar hash
    - Almacenar hash separado del JSON encriptado
    - Archivos: CharacterPersistenceService.cs
    - Tiempo: 1h

[ ] 1.2 Agregar método de verificación de hash
    - Implementar VerifyIntegrityHash(CharacterData, byte[] storedHash)
    - Usar EncryptionService.VerifyHash()
    - Archivos: CharacterPersistenceService.cs
    - Tiempo: 30min

[ ] 1.3 Tests unitarios básicos
    - Test save/load round-trip
    - Test corrupted data rejection
    - Test export/import round-trip
    - Archivos: PersistencePropertyTests.cs
    - Tiempo: 1h
```

**Tiempo Total Estimado: 2.5 horas**

**Dependencias:** Ninguna

---

## Feature 2: Integrar ConnectionApprovalHandler con NGO

### Estado Actual: 95% ✅

**Código Existente:**
- `ConnectionApprovalHandler.cs` - Validación completa
- Validación de estructura, stats, equipamiento
- Integración con PersistenceService para decrypt

**Análisis de Gaps:**
1. No está conectado al evento de aprobación de NetworkManager
2. Falta el hook en NetworkSessionManager

### Plan de Tareas

```
[ ] 2.1 Agregar hook de Connection Approval en NetworkSessionManager
    - Suscribirse a NetworkManager.ConnectionApprovalCallback
    - Llamar a ConnectionApprovalHandler.ValidateConnectionRequest()
    - Archivos: NetworkSessionManager.cs
    - Tiempo: 1h

[ ] 2.2 Implementar payload de conexión en cliente
    - Crear método para serializar CharacterData como payload
    - Enviar en NetworkManager.StartClient()
    - Archivos: NetworkSessionManager.cs
    - Tiempo: 1h

[ ] 2.3 Test de integración
    - Verificar que cliente con datos válidos conecta
    - Verificar que cliente con datos inválidos es rechazado
    - Archivos: Manual testing
    - Tiempo: 30min
```

**Tiempo Total Estimado: 2.5 horas**

**Dependencias:** Feature 1 (para export de CharacterData)

---

## Feature 3: Sistema de Clases Trinity - Completar Integración

### Estado Actual: 95% ✅

**Código Existente:**
- `ClassSystem.cs` - 4 clases, specs, efectividad
- `ClassAbilityDefinitions.cs` - 40+ habilidades hardcoded
- Enums completos en `Enums.cs`

**Análisis de Gaps:**
1. Las habilidades están hardcoded, no en ScriptableObjects
2. ClassSystem no carga habilidades automáticamente
3. Falta integración con AbilitySystem existente

### Plan de Tareas

```
[ ] 3.1 Crear ScriptableObjects de habilidades base
    - Crear 4-5 AbilityDataSO por clase (16-20 total)
    - Ubicación: ScriptableObjects/Abilities/[Clase]/
    - Archivos: Nuevos .asset files
    - Tiempo: 2h

[ ] 3.2 Crear ClassAbilitiesConfigSO
    - ScriptableObject que mapea clase+spec -> lista de habilidades
    - Permite configurar desde el editor
    - Archivos: Nuevo ClassAbilitiesConfigSO.cs
    - Tiempo: 1h

[ ] 3.3 Modificar ClassSystem para cargar desde SO
    - Inyectar ClassAbilitiesConfigSO
    - Cargar habilidades en Initialize()
    - Archivos: ClassSystem.cs
    - Tiempo: 1h

[ ] 3.4 Integrar con AbilitySystem
    - Cuando jugador selecciona clase, cargar habilidades en action bar
    - Archivos: AbilitySystem.cs, ClassSystem.cs
    - Tiempo: 1h
```

**Tiempo Total Estimado: 5 horas**

**Dependencias:** Ninguna

---

## Feature 4: Habilidades por Clase - ScriptableObjects

### Estado Actual: 90% ✅

**Código Existente:**
- `ClassAbilityDefinitions.cs` - Todas las habilidades definidas
- `AbilityDataSO.cs` - ScriptableObject base existe
- `AbilityData.cs` - Modelo de datos completo

**Análisis de Gaps:**
1. No hay .asset files creados
2. AbilitySystem usa habilidades de prueba hardcoded

### Plan de Tareas

```
[ ] 4.1 Crear AbilityDataSO para Warrior (8 habilidades)
    - Protection: Taunt, Shield Block, Shield Slam, Devastate
    - Arms: Mortal Strike, Overpower, Execute, Bladestorm
    - Shared: Charge, Heroic Strike
    - Tiempo: 1h

[ ] 4.2 Crear AbilityDataSO para Mage (8 habilidades)
    - Fire: Fireball, Pyroblast, Fire Blast, Combustion
    - Frost: Frostbolt, Ice Lance, Blizzard, Icy Veins
    - Shared: Blink, Arcane Intellect
    - Tiempo: 1h

[ ] 4.3 Crear AbilityDataSO para Priest (8 habilidades)
    - Holy: Heal, Flash Heal, Prayer of Healing, Resurrection
    - Shadow: SW:Pain, Mind Blast, Mind Flay, Shadowform
    - Shared: PW:Shield, Smite
    - Tiempo: 1h

[ ] 4.4 Crear AbilityDataSO para Paladin (10 habilidades)
    - Protection: Righteous Defense, Avenger's Shield, Consecration, Ardent Defender
    - Holy: Holy Light, Flash of Light, Beacon of Light, Aura Mastery
    - Retribution: Crusader Strike, Templar's Verdict, Divine Storm
    - Shared: Lay on Hands, Divine Shield
    - Tiempo: 1h

[ ] 4.5 Modificar AbilitySystem para cargar desde SO
    - Reemplazar habilidades de prueba
    - Cargar desde ClassSystem
    - Archivos: AbilitySystem.cs
    - Tiempo: 1h
```

**Tiempo Total Estimado: 5 horas**

**Dependencias:** Feature 3

---

## Feature 5: Sistema de Stats y Fórmulas de Combate

### Estado Actual: 85% ✅

**Código Existente:**
- `CharacterStats.cs` - Todos los stats definidos
- `CombatSystem.cs` - ApplyDamage/ApplyHealing básico
- `ProgressionSystem.cs` - Stats base por nivel y clase

**Análisis de Gaps:**
1. ApplyDamage no usa fórmulas con stats
2. No hay mitigación por Armor
3. No hay cálculo de Crit/Haste

### Plan de Tareas

```
[ ] 5.1 Crear DamageCalculator utility class
    - CalculatePhysicalDamage(baseDamage, attackPower, strength)
    - CalculateSpellDamage(baseDamage, spellPower, intellect)
    - Fórmula: Damage = BaseDamage * (1 + Stat/100)
    - Archivos: Nuevo DamageCalculator.cs en Combat/
    - Tiempo: 1h

[ ] 5.2 Crear ArmorMitigation utility
    - CalculateMitigation(damage, targetArmor)
    - Fórmula: FinalDamage = Damage * (100 / (100 + Armor))
    - Archivos: DamageCalculator.cs
    - Tiempo: 30min

[ ] 5.3 Agregar CriticalHit calculation
    - CalculateCrit(baseCritChance, critRating)
    - CritDamage = Damage * 1.5
    - Archivos: DamageCalculator.cs
    - Tiempo: 30min

[ ] 5.4 Integrar en CombatSystem.ApplyDamage
    - Obtener stats del atacante y defensor
    - Aplicar fórmulas antes de reducir HP
    - Archivos: CombatSystem.cs
    - Tiempo: 1h

[ ] 5.5 Integrar en AbilitySystem.ExecuteAbility
    - Usar DamageCalculator para calcular daño de habilidades
    - Pasar stats del jugador
    - Archivos: AbilitySystem.cs
    - Tiempo: 1h

[ ] 5.6 Tests de fórmulas
    - Verificar que daño escala con stats
    - Verificar mitigación de armor
    - Archivos: CombatPropertyTests.cs
    - Tiempo: 1h
```

**Tiempo Total Estimado: 5 horas**

**Dependencias:** Ninguna (puede hacerse en paralelo)

---

## Feature 6: UI de Selección de Personaje

### Estado Actual: 30% ⚠️

**Código Existente:**
- `MainMenuUI.cs` - Estructura básica con botones
- `CharacterData.cs` - Modelo completo
- `CharacterPersistenceService.cs` - Save/Load funcional

**Análisis de Gaps:**
1. No existe CharacterSelectUI
2. No hay selector de clase visual
3. No hay input de nombre
4. No hay lista de personajes guardados

### Plan de Tareas

```
[ ] 6.1 Crear CharacterSelectUI.cs (OnGUI temporal)
    - Lista de personajes guardados
    - Botón "Nuevo Personaje"
    - Botón "Cargar"
    - Botón "Eliminar"
    - Archivos: Nuevo CharacterSelectUI.cs en UI/
    - Tiempo: 2h

[ ] 6.2 Crear CharacterCreationUI.cs
    - Input de nombre (GUI.TextField)
    - Selector de clase (4 botones con iconos)
    - Selector de especialización
    - Preview de stats base
    - Botón "Crear"
    - Archivos: Nuevo CharacterCreationUI.cs
    - Tiempo: 2h

[ ] 6.3 Integrar con CharacterPersistenceService
    - Cargar lista de IDs guardados
    - Mostrar nombre y nivel de cada personaje
    - Crear nuevo CharacterData al confirmar
    - Archivos: CharacterSelectUI.cs
    - Tiempo: 1h

[ ] 6.4 Flujo de navegación
    - MainMenu -> CharacterSelect -> CharacterCreation
    - CharacterSelect -> Game (con personaje cargado)
    - Archivos: MainMenuUI.cs, GameManager.cs
    - Tiempo: 1h

[ ] 6.5 Pasar CharacterData al NetworkPlayer
    - Al conectar, aplicar datos del personaje
    - Sincronizar clase y stats
    - Archivos: NetworkPlayer.cs, NetworkSessionManager.cs
    - Tiempo: 1h
```

**Tiempo Total Estimado: 7 horas**

**Dependencias:** Feature 1 (Persistence), Feature 3 (Classes)

---

## Feature 7: IA de Enemigos con Aggro

### Estado Actual: 40% ⚠️

**Código Existente:**
- `Enemy.cs` - Entidad básica con health, ITargetable
- `AggroSystem.cs` - Sistema de threat completo
- `EnemySpawner.cs` - Spawning en red

**Análisis de Gaps:**
1. No existe EnemyAI.cs
2. Enemy no se mueve ni ataca
3. No hay detección de jugadores
4. No hay máquina de estados

### Plan de Tareas

```
[ ] 7.1 Crear EnemyAI.cs con máquina de estados
    - Estados: Idle, Patrol, Aggro, Combat, Return, Dead
    - Transiciones basadas en eventos
    - Archivos: Nuevo EnemyAI.cs en Enemy/
    - Tiempo: 2h

[ ] 7.2 Implementar detección de jugadores
    - Sphere trigger para aggro range (15m default)
    - OnTriggerEnter/Exit para detectar jugadores
    - Filtrar por layer "Player"
    - Archivos: EnemyAI.cs
    - Tiempo: 1h

[ ] 7.3 Implementar movimiento hacia target
    - Usar NavMeshAgent o simple MoveTowards
    - Mantener distancia de ataque (2m melee)
    - Archivos: EnemyAI.cs
    - Tiempo: 1h

[ ] 7.4 Implementar ataques automáticos
    - Attack cooldown (2s default)
    - Llamar a CombatSystem.ApplyDamage
    - Generar threat en AggroSystem
    - Archivos: EnemyAI.cs, Enemy.cs
    - Tiempo: 1h

[ ] 7.5 Integrar con AggroSystem
    - Cambiar target cuando aggro cambia
    - Suscribirse a OnAggroChanged
    - Archivos: EnemyAI.cs
    - Tiempo: 1h

[ ] 7.6 Implementar estado Return
    - Si pierde aggro por 5s, volver a spawn
    - Regenerar HP al volver
    - Reset de threat
    - Archivos: EnemyAI.cs
    - Tiempo: 1h

[ ] 7.7 Sincronización en red
    - Estado de IA sincronizado (NetworkVariable)
    - Target sincronizado
    - Solo servidor controla IA
    - Archivos: EnemyAI.cs
    - Tiempo: 1.5h
```

**Tiempo Total Estimado: 8.5 horas**

**Dependencias:** Feature 5 (Combat formulas)

---

## Feature 8: Sistema de Loot - UI

### Estado Actual: 90% ✅

**Código Existente:**
- `LootSystem.cs` - Generación completa con raridades
- `ProgressionSystem.cs` - XP y niveles
- `ItemData.cs` - Modelo de items completo

**Análisis de Gaps:**
1. No hay UI de loot
2. No hay Need/Greed voting
3. No hay notificación de items obtenidos

### Plan de Tareas

```
[ ] 8.1 Crear LootWindowUI.cs (OnGUI)
    - Mostrar items dropeados
    - Color por raridad (blanco, verde, morado)
    - Botón "Tomar" por item
    - Botón "Tomar Todo"
    - Archivos: Nuevo LootWindowUI.cs en UI/
    - Tiempo: 2h

[ ] 8.2 Crear LootNotificationUI.cs
    - Popup temporal cuando recibes item
    - "[Nombre] ha obtenido [Item]"
    - Fade out después de 3s
    - Archivos: Nuevo LootNotificationUI.cs
    - Tiempo: 1h

[ ] 8.3 Integrar con Enemy death
    - Al morir enemigo, mostrar LootWindow
    - Generar loot usando LootSystem
    - Archivos: Enemy.cs, LootWindowUI.cs
    - Tiempo: 1h

[ ] 8.4 Agregar items al inventario
    - Crear InventoryService básico
    - Método AddItem(playerId, ItemData)
    - Archivos: Nuevo InventoryService.cs
    - Tiempo: 1h

[ ] 8.5 Sincronización de loot en red
    - Loot generado en servidor
    - Distribución sincronizada
    - Archivos: LootSystem.cs
    - Tiempo: 1h
```

**Tiempo Total Estimado: 6 horas**

**Dependencias:** Feature 7 (Enemy death)

---

## Feature 9: Sistema de Inventario - UI

### Estado Actual: 85% ✅

**Código Existente:**
- `EquipmentSystem.cs` - Equip/Unequip completo
- `ItemData.cs` - Modelo con stats
- 10 slots de equipamiento definidos

**Análisis de Gaps:**
1. No hay InventoryUI
2. No hay sistema de inventario (solo equipment)
3. No hay drag & drop

### Plan de Tareas

```
[ ] 9.1 Crear InventoryService.cs
    - Lista de items (30 slots)
    - AddItem, RemoveItem, GetItems
    - Persistencia con CharacterData
    - Archivos: Nuevo InventoryService.cs en Progression/
    - Tiempo: 1.5h

[ ] 9.2 Crear InventoryUI.cs (OnGUI)
    - Grid de 30 slots (6x5)
    - Mostrar icono/nombre de item
    - Tooltip con stats al hover
    - Archivos: Nuevo InventoryUI.cs en UI/
    - Tiempo: 2h

[ ] 9.3 Crear EquipmentUI.cs (OnGUI)
    - 10 slots de equipamiento
    - Mostrar item equipado en cada slot
    - Stats totales del equipo
    - Archivos: Nuevo EquipmentUI.cs en UI/
    - Tiempo: 1.5h

[ ] 9.4 Implementar equip desde inventario
    - Click en item -> menú contextual
    - Opción "Equipar" si es equipo
    - Mover item actual a inventario
    - Archivos: InventoryUI.cs, EquipmentSystem.cs
    - Tiempo: 1h

[ ] 9.5 Toggle de UI con tecla (I)
    - Abrir/cerrar inventario con I
    - Agregar al Input System
    - Archivos: InventoryUI.cs, EtherDomesInput.inputactions
    - Tiempo: 30min

[ ] 9.6 Integrar con CharacterData para persistencia
    - Guardar inventario en CharacterData
    - Cargar al iniciar
    - Archivos: CharacterData.cs, InventoryService.cs
    - Tiempo: 1h
```

**Tiempo Total Estimado: 7.5 horas**

**Dependencias:** Feature 8 (Loot para obtener items)

---

## Feature 10: Primera Dungeon Jugable

### Estado Actual: 60% ⚠️

**Código Existente:**
- `DungeonSystem.cs` - Instancias, grupos, dificultad
- `BossSystem.cs` - Fases, transiciones, loot
- `BossDataSO.cs` - ScriptableObject para datos de boss

**Análisis de Gaps:**
1. No hay escena de dungeon
2. No hay prefabs de enemigos configurados
3. No hay mecánicas específicas de boss
4. No hay entrada/salida de dungeon

### Plan de Tareas

```
[ ] 10.1 Crear escena "Dungeon_Crypt_Small"
    - 3 salas conectadas
    - Sala 1: 3 enemigos normales
    - Sala 2: 4 enemigos normales
    - Sala 3: Boss room
    - Archivos: Nueva escena en Scenes/Dungeons/
    - Tiempo: 2h

[ ] 10.2 Crear prefab de enemigo "Skeleton"
    - Mesh: Capsule (placeholder)
    - Enemy.cs + EnemyAI.cs
    - Stats: 500 HP, 20 damage
    - Archivos: Nuevo prefab en Prefabs/Enemies/
    - Tiempo: 1h

[ ] 10.3 Crear prefab de boss "Crypt Lord"
    - Mesh: Capsule grande (placeholder)
    - Enemy.cs + BossAI.cs (nuevo)
    - Stats: 10000 HP, 50 damage
    - Archivos: Nuevo prefab en Prefabs/Bosses/
    - Tiempo: 1h

[ ] 10.4 Crear BossAI.cs con mecánicas
    - Mecánica 1: AoE cada 15s (círculo rojo en suelo)
    - Mecánica 2: Enrage al 25% HP (+50% damage)
    - Fases: Normal -> Enraged
    - Archivos: Nuevo BossAI.cs en Enemy/
    - Tiempo: 2h

[ ] 10.5 Crear DungeonPortal.cs
    - Trigger para entrar a dungeon
    - Crear instancia via DungeonSystem
    - Teleportar grupo a dungeon
    - Archivos: Nuevo DungeonPortal.cs en World/
    - Tiempo: 1.5h

[ ] 10.6 Crear ExitPortal.cs
    - Aparece al derrotar boss
    - Teleporta de vuelta al mundo
    - Archivos: Nuevo ExitPortal.cs en World/
    - Tiempo: 1h

[ ] 10.7 Crear BossDataSO para Crypt Lord
    - Configurar stats, loot table, mecánicas
    - Archivos: Nuevo .asset en ScriptableObjects/Bosses/
    - Tiempo: 30min

[ ] 10.8 Integrar loot de boss
    - Al morir boss, generar loot
    - Mostrar LootWindow
    - Archivos: BossAI.cs, BossSystem.cs
    - Tiempo: 1h

[ ] 10.9 Test completo de dungeon
    - Entrar con grupo
    - Matar enemigos
    - Derrotar boss
    - Obtener loot
    - Salir
    - Tiempo: 1h
```

**Tiempo Total Estimado: 11 horas**

**Dependencias:** Features 7, 8 (Enemy AI, Loot)

---

## Resumen y Análisis Final

### Tabla de Tiempos Totales

| # | Feature | Horas Est. | Estado | Prioridad |
|---|---------|------------|--------|-----------|
| 1 | Persistencia (Hash Validation) | 2.5h | 90% | Alta |
| 2 | Connection Approval (NGO Hook) | 2.5h | 95% | Alta |
| 3 | Clases Trinity (ScriptableObjects) | 5h | 95% | Alta |
| 4 | Habilidades por Clase (.asset) | 5h | 90% | Alta |
| 5 | Stats y Fórmulas de Combate | 5h | 85% | Alta |
| 6 | UI Selección de Personaje | 7h | 30% | Media |
| 7 | IA de Enemigos con Aggro | 8.5h | 40% | Alta |
| 8 | Sistema de Loot - UI | 6h | 90% | Media |
| 9 | Sistema de Inventario - UI | 7.5h | 85% | Media |
| 10 | Primera Dungeon Jugable | 11h | 60% | Alta |
| **TOTAL** | | **60h** | | |

---

### Orden de Ejecución Recomendado

```
FASE 1 - Fundamentos (Paralelo) [~12.5h]
├── Feature 1: Persistencia (2.5h)
├── Feature 3: Clases Trinity (5h)
└── Feature 5: Stats/Combate (5h)

FASE 2 - Integración Red [~2.5h]
└── Feature 2: Connection Approval (2.5h) ← Requiere F1

FASE 3 - Contenido de Clases [~5h]
└── Feature 4: Habilidades (5h) ← Requiere F3

FASE 4 - IA y Combate [~8.5h]
└── Feature 7: Enemy AI (8.5h) ← Requiere F5

FASE 5 - UI y Loot (Paralelo) [~13h]
├── Feature 6: UI Personaje (7h) ← Requiere F1, F3
└── Feature 8: Loot UI (6h) ← Requiere F7

FASE 6 - Inventario [~7.5h]
└── Feature 9: Inventario UI (7.5h) ← Requiere F8

FASE 7 - Dungeon Final [~11h]
└── Feature 10: Primera Dungeon (11h) ← Requiere F7, F8
```

---

### Análisis de Ruta Crítica

La **ruta crítica** (camino más largo) determina el tiempo mínimo total:

```
F5 (5h) → F7 (8.5h) → F8 (6h) → F9 (7.5h) → F10 (11h)
         Stats      Enemy AI    Loot UI   Inventory   Dungeon
         
Tiempo Ruta Crítica: 38 horas
```

**Implicación**: Aunque el total es 60h, con trabajo paralelo el MVP puede completarse en ~38-40h de trabajo efectivo.

---

### Oportunidades de Trabajo Paralelo

| Fase | Features en Paralelo | Ahorro Potencial |
|------|---------------------|------------------|
| 1 | F1 + F3 + F5 | ~7.5h (de 12.5h a 5h) |
| 5 | F6 + F8 | ~6h (de 13h a 7h) |

**Trabajo Paralelo Máximo**: 2 desarrolladores pueden reducir el tiempo a ~30-35h.

---

### Dependencias Visuales

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│   [F1 Persistence]──────┬──────────────────────────────────┐│
│         │               │                                  ││
│         ▼               ▼                                  ││
│   [F2 Connection]   [F6 UI Personaje]                      ││
│                                                            ││
│   [F3 Clases]───────┬───────────────────────────────────┐  ││
│         │           │                                   │  ││
│         ▼           ▼                                   │  ││
│   [F4 Habilidades]  [F6 UI Personaje]                   │  ││
│                                                         │  ││
│   [F5 Stats/Combat]                                     │  ││
│         │                                               │  ││
│         ▼                                               │  ││
│   [F7 Enemy AI]─────────────────────────────────────────┤  ││
│         │                                               │  ││
│         ▼                                               │  ││
│   [F8 Loot UI]──────────────────────────────────────────┤  ││
│         │                                               │  ││
│         ▼                                               │  ││
│   [F9 Inventario]                                       │  ││
│         │                                               │  ││
│         ▼                                               ▼  ▼│
│   [F10 DUNGEON] ◄───────────────────────────────────────────│
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

### Recomendación de Inicio

**Comenzar con estas 3 features en paralelo:**

1. **Feature 1** (Persistencia) - Base para todo el sistema de personajes
2. **Feature 3** (Clases) - Base para habilidades y combate
3. **Feature 5** (Stats/Combat) - Base para IA de enemigos

Estas 3 features no tienen dependencias entre sí y desbloquean el resto del desarrollo.

---

### Archivos Nuevos a Crear

| Feature | Archivos Nuevos |
|---------|-----------------|
| 3 | `ClassAbilitiesConfigSO.cs`, `.asset` files |
| 4 | 34+ `AbilityDataSO.asset` files |
| 5 | `DamageCalculator.cs` |
| 6 | `CharacterSelectUI.cs`, `CharacterCreationUI.cs` |
| 7 | `EnemyAI.cs` |
| 8 | `LootWindowUI.cs`, `LootNotificationUI.cs`, `InventoryService.cs` |
| 9 | `InventoryUI.cs`, `EquipmentUI.cs` |
| 10 | `BossAI.cs`, `DungeonPortal.cs`, `ExitPortal.cs`, escena, prefabs |

---

### Spec References

Cada feature está alineada con los requirements en `.kiro/specs/requirements.md`:

| Feature | Requirements |
|---------|--------------|
| 1 | Req 7.1-7.5 (Persistencia) |
| 2 | Req 6.1-6.5 (Connection Approval) |
| 3 | Req 12.1-12.5 (Classes) |
| 4 | Req 9.1-9.7 (Abilities) |
| 5 | Req 10.1-10.6 (Combat/Aggro) |
| 6 | Req 7.1, 12.1 (Persistence + Classes) |
| 7 | Req 10.1-10.6 (Aggro/Threat) |
| 8 | Req 15.1-15.3 (Loot) |
| 9 | Req 15.4-15.6 (Equipment) |
| 10 | Req 17.1-17.6, 18.1-18.5 (Dungeon/Boss) |

---

## Conclusión

El plan está listo para ejecución. Las 10 features cubren todos los sistemas necesarios para un MVP jugable de The Ether Domes. El código existente está en buen estado (promedio 75% completado), lo que reduce significativamente el trabajo restante.

**Próximo paso sugerido**: Comenzar con Feature 1, 3 y 5 en paralelo.
