# The Ether Domes - Estado del Proyecto

## Resumen
Micro-MMORPG cooperativo para 1-10 jugadores con combate Tab-Target estilo WoW, sistema de clases Trinity, dungeons con bosses y progresión de nivel.

## Stack Tecnológico
- **Motor**: Unity 6.3 LTS
- **Networking**: Unity Netcode for GameObjects (NGO)
- **Arquitectura**: Híbrida Host-Play / Dedicated Server
- **Input**: Unity Input System

---

## Sistemas Implementados

### 1. Networking ✅
- `NetworkSessionManager`: Gestión de sesiones Host/Client/Server
- `ConnectionApprovalHandler`: Validación de conexiones
- `NetworkTestUI`: UI de debug para testing de red
- Puerto por defecto: 7777

### 2. Combat System ✅
- `TargetSystem`: Tab-targeting con ciclo de enemigos
- `AbilitySystem`: Sistema de habilidades con GCD, cooldowns y cast times
- `CombatTestUI`: UI de combate con Target Frame y Action Bar
- `AggroSystem`: Sistema de amenaza (estructura base)
- `CombatSystem`: Coordinador de combate (estructura base)

### 3. Enemy System ✅
- `Enemy`: Entidad enemiga con ITargetable
- `EnemySpawner`: Spawner de enemigos en red
- Daño sincronizado via ServerRpc

### 4. Player System ✅
- `NetworkPlayer`: Jugador networked con stats sincronizados
- `PlayerController`: Movimiento WASD
- `PlayerPhysicsConfig`: Configuración de colisiones

### 5. Progression System (Estructura)
- `ProgressionSystem`: XP y niveles
- `LootSystem`: Sistema de loot
- `EquipmentSystem`: Equipamiento

### 6. Class System (Estructura)
- `ClassSystem`: Gestión de clases
- `ClassAbilityDefinitions`: Definiciones de habilidades por clase

### 7. World System (Estructura)
- `WorldManager`: Gestión de regiones
- `DungeonSystem`: Sistema de dungeons
- `BossSystem`: Sistema de bosses
- `GuildBaseSystem`: Base del gremio

### 8. Persistence (Estructura)
- `CharacterPersistenceService`: Guardado de personajes
- `EncryptionService`: Encriptación de datos
- `WorldPersistenceService`: Persistencia del mundo

---

## Estructura de Carpetas

```
Assets/_Project/
├── Input/
│   ├── EtherDomesInput.inputactions  # Configuración de input
│   └── EtherDomesInput.cs            # Clase generada
├── Prefabs/
│   ├── Player.prefab
│   └── Enemies/BasicEnemy.prefab
├── Scenes/
│   └── TestScene.unity
└── Scripts/
    ├── Combat/
    │   ├── AbilitySystem.cs
    │   ├── TargetSystem.cs
    │   ├── CombatSystem.cs
    │   ├── AggroSystem.cs
    │   └── Interfaces/
    ├── Classes/
    ├── Core/
    │   ├── GameManager.cs
    │   └── GameBootstrap.cs
    ├── Data/
    │   ├── AbilityData.cs
    │   ├── CharacterData.cs
    │   ├── Enums.cs
    │   └── ScriptableObjects/
    ├── Editor/
    │   └── EtherDomesSetup.cs
    ├── Enemy/
    │   ├── Enemy.cs
    │   └── EnemySpawner.cs
    ├── Network/
    │   ├── NetworkSessionManager.cs
    │   └── ConnectionApprovalHandler.cs
    ├── Persistence/
    ├── Player/
    │   ├── NetworkPlayer.cs
    │   └── PlayerController.cs
    ├── Progression/
    ├── UI/
    │   └── Debug/
    │       ├── NetworkTestUI.cs
    │       └── CombatTestUI.cs
    └── World/
```

---

## Controles

| Tecla | Acción |
|-------|--------|
| WASD | Movimiento |
| Tab | Ciclar targets |
| Escape | Limpiar target |
| 1-9 | Usar habilidades |

---

## Cómo Probar

1. Abrir Unity
2. `Tools > EtherDomes > Setup Complete Test Scene`
3. Play
4. Click "Start Host"
5. Tab para seleccionar enemigo
6. 1-4 para usar habilidades

---

## Habilidades de Prueba

| Slot | Nombre | Tipo | Daño | Rango | Cooldown | Cast |
|------|--------|------|------|-------|----------|------|
| 1 | Strike | Instant | 15 | 5m | 0s | - |
| 2 | Heavy Strike | Instant | 35 | 5m | 6s | - |
| 3 | Fireball | Cast | 50 | 30m | 0s | 2s |
| 4 | Execute | Instant | 100 | 5m | 15s | - |

---

## Assembly Definitions

El proyecto usa Assembly Definitions para modularidad:
- `EtherDomes.Core`
- `EtherDomes.Combat`
- `EtherDomes.Player`
- `EtherDomes.Enemy`
- `EtherDomes.Network`
- `EtherDomes.Data`
- `EtherDomes.UI`
- `EtherDomes.Input`
- `EtherDomes.Classes`
- `EtherDomes.Progression`
- `EtherDomes.World`
- `EtherDomes.Persistence`
- `EtherDomes.Editor`

---

## Próximos 10 Pasos de Desarrollo

> **Referencia**: Estos pasos están alineados con los specs en `.kiro/specs/` (requirements.md, design.md, tasks.md)

### Paso 1: Completar CharacterPersistenceService ⭐ PRIORIDAD ALTA
**Objetivo**: Implementar persistencia local encriptada de personajes (Cross-World)
- [ ] Implementar SaveCharacterAsync con encriptación AES
- [ ] Implementar LoadCharacterAsync con decriptación y validación
- [ ] Implementar hash de integridad SHA256
- [ ] Implementar ExportCharacterForNetwork / ImportCharacterFromNetwork
- **Archivos**: `CharacterPersistenceService.cs`, `EncryptionService.cs`
- **Spec Reference**: Requirements 7.1-7.5, Properties 10-13
- **Dependencias**: Ninguna
- **Estimación**: 3-4 horas

### Paso 2: Implementar ConnectionApprovalHandler
**Objetivo**: Validación anti-cheat de datos de personaje al conectar
- [ ] Implementar ValidateConnectionRequest con parsing de payload
- [ ] Validar rangos de stats de equipo
- [ ] Implementar timeout handling
- [ ] Integrar con NetworkSessionManager
- **Archivos**: `ConnectionApprovalHandler.cs`
- **Spec Reference**: Requirements 6.1-6.5, Property 9
- **Dependencias**: Paso 1
- **Estimación**: 2-3 horas

### Paso 3: Sistema de Clases Trinity
**Objetivo**: Implementar las 4 clases base del juego
- [ ] Warrior (Tank/DPS): Alta vida, taunt, defensa
- [ ] Mage (DPS): Alto daño, habilidades de área, cast times
- [ ] Priest (Healer): Curación, buffs, soporte
- [ ] Paladin (Tank/Healer/DPS): Clase híbrida flexible
- **Archivos**: `ClassSystem.cs`, `ClassAbilityDefinitions.cs`
- **Spec Reference**: Requirements 12.1-12.5, 13.1-13.4
- **Dependencias**: Ninguna
- **Estimación**: 4-6 horas

### Paso 4: Habilidades por Clase
**Objetivo**: Crear 4-6 habilidades únicas por clase
- [ ] Warrior: Taunt, Shield Block, Charge, Slam, Execute
- [ ] Mage: Fireball, Frostbolt, Arcane Explosion, Polymorph
- [ ] Priest: Heal, Greater Heal, Renew (HoT), Dispel
- [ ] Paladin: Holy Strike, Lay on Hands, Divine Shield, Consecration
- **Archivos**: `AbilityDataSO.cs`, ScriptableObjects en `ScriptableObjects/Abilities/`
- **Spec Reference**: Requirements 9.1-9.7, 12.2
- **Dependencias**: Paso 3
- **Estimación**: 3-4 horas

### Paso 5: Sistema de Stats y Fórmulas de Combate
**Objetivo**: Stats que afecten el combate real
- [ ] Fórmulas de daño: `Damage = BaseDamage * (1 + Strength/100)`
- [ ] Mitigación: `FinalDamage = Damage * (100 / (100 + Armor))`
- [ ] Stats secundarios: Crit, Haste, Mastery
- [ ] Integrar con sistema de Threat/Aggro
- **Archivos**: `CharacterStats.cs`, `CombatSystem.cs`, `AggroSystem.cs`
- **Spec Reference**: Requirements 10.1-10.6
- **Dependencias**: Paso 3
- **Estimación**: 3-4 horas

### Paso 6: UI de Selección de Personaje
**Objetivo**: Pantalla para crear/seleccionar personaje
- [ ] Menú principal: Nuevo Personaje, Cargar, Opciones
- [ ] Selector de clase con preview
- [ ] Input de nombre de personaje
- [ ] Integrar con CharacterPersistenceService
- **Archivos**: `MainMenuUI.cs`, nuevo `CharacterSelectUI.cs`
- **Spec Reference**: Requirements 7.1, 12.1
- **Dependencias**: Pasos 1, 3
- **Estimación**: 4-5 horas

### Paso 7: IA de Enemigos con Aggro
**Objetivo**: Enemigos que ataquen usando sistema de amenaza
- [ ] Estado Idle: Patrulla o espera
- [ ] Estado Aggro: Detecta jugador en rango
- [ ] Estado Combat: Ataca al jugador con mayor Threat
- [ ] Estado Return: Vuelve a spawn si pierde aggro (5s sin combate)
- **Archivos**: Nuevo `EnemyAI.cs`, `AggroSystem.cs`
- **Spec Reference**: Requirements 10.1-10.6
- **Dependencias**: Paso 5
- **Estimación**: 5-6 horas

### Paso 8: Sistema de Loot y Progresión
**Objetivo**: Enemigos dropean items, jugadores ganan XP
- [ ] Tabla de loot por enemigo (ScriptableObject)
- [ ] Drop rates: Common 70%, Rare 25%, Epic 5%
- [ ] Sistema de XP y niveles (1-60)
- [ ] UI de loot (need/greed para grupos)
- **Archivos**: `LootSystem.cs`, `ProgressionSystem.cs`, `ItemDataSO.cs`
- **Spec Reference**: Requirements 14.1-14.6, 15.1-15.6
- **Dependencias**: Paso 7
- **Estimación**: 5-6 horas

### Paso 9: Sistema de Inventario y Equipamiento
**Objetivo**: Gestionar items y equipar gear
- [ ] Inventario con slots (20-30 slots)
- [ ] Slots de equipamiento: Arma, Armadura, Accesorios
- [ ] Item_Level requirements basados en Level del jugador
- [ ] Stats de items afectan stats del personaje
- [ ] UI de inventario drag & drop
- **Archivos**: `EquipmentSystem.cs`, nuevo `InventoryUI.cs`
- **Spec Reference**: Requirements 15.4-15.6
- **Dependencias**: Paso 8
- **Estimación**: 6-8 horas

### Paso 10: Primera Dungeon con Boss
**Objetivo**: Crear una dungeon pequeña jugable
- [ ] Escena de dungeon con 3 salas
- [ ] 5-8 enemigos normales
- [ ] 1 Boss con 2 mecánicas (AoE, fases al 50% HP)
- [ ] Instanciación por grupo
- [ ] Loot del boss con drop table
- **Archivos**: `DungeonSystem.cs`, `BossSystem.cs`, nueva escena
- **Spec Reference**: Requirements 17.1-17.6, 18.1-18.5
- **Dependencias**: Pasos 7, 8
- **Estimación**: 8-10 horas

---

## Resumen de Estimaciones

| Paso | Descripción | Horas Est. | Spec Ref |
|------|-------------|------------|----------|
| 1 | Persistencia Personajes | 3-4h | Req 7 |
| 2 | Connection Approval | 2-3h | Req 6 |
| 3 | Sistema de Clases | 4-6h | Req 12-13 |
| 4 | Habilidades por Clase | 3-4h | Req 9, 12 |
| 5 | Stats y Combate | 3-4h | Req 10 |
| 6 | UI Selección Personaje | 4-5h | Req 7, 12 |
| 7 | IA Enemigos | 5-6h | Req 10 |
| 8 | Loot y Progresión | 5-6h | Req 14-15 |
| 9 | Inventario/Equipamiento | 6-8h | Req 15 |
| 10 | Primera Dungeon | 8-10h | Req 17-18 |
| **TOTAL** | | **44-56h** | |

---

## Orden de Dependencias

```
Paso 1 (Persistencia)
    └── Paso 2 (Connection Approval)
            └── Paso 6 (UI Personaje)

Paso 3 (Clases)
    ├── Paso 4 (Habilidades)
    └── Paso 5 (Stats/Combate)
            └── Paso 7 (IA Enemigos)
                    └── Paso 8 (Loot/Progresión)
                            └── Paso 9 (Inventario)

Paso 10 (Dungeon) ← Requiere: 7, 8
```

---

## Referencia a Specs

Los specs completos están en:
- **Requirements**: `.kiro/specs/requirements.md` (20 requirements detallados)
- **Design**: `.kiro/specs/design.md` (arquitectura, interfaces, properties)
- **Tasks**: `.kiro/specs/tasks.md` (plan de implementación detallado)

---

## Próximos Pasos Opcionales (Post-MVP)

### Prioridad Media
- [ ] Efectos visuales de habilidades (VFX)
- [ ] Sistema de buffs/debuffs
- [ ] Más dungeons (Large: 5 bosses)
- [ ] Sistema de party/grupo
- [ ] Regiones adicionales (Bosque, Nieve, Pantano, Ciudadela)

### Prioridad Baja
- [ ] Sistema de Guild Base completo (Req 19-20)
- [ ] Matchmaking automático
- [ ] Dedicated server build
- [ ] Leaderboards
- [ ] Sistema de muerte y resurrección (Req 11)

---

## Notas Importantes

1. **Layers**: Crear manualmente en `Edit > Project Settings > Tags and Layers`:
   - Layer 8: Player
   - Layer 9: Enemy

2. **Networking**: El daño se procesa en el servidor via `ServerRpc`

3. **Input System**: Usa el nuevo Input System de Unity, no el legacy

4. **Namespaces**: En `EtherDomes.UI.Debug` usar `UnityEngine.Debug.Log` (no solo `Debug.Log`)

---

## Contacto
Documento generado: Enero 2026
