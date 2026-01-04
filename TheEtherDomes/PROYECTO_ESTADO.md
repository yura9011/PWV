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

## Próximos Pasos Sugeridos

### Prioridad Alta
- [ ] Implementar clases (Warrior, Mage, Healer)
- [ ] Sistema de stats completo
- [ ] UI de selección de personaje
- [ ] Persistencia de datos

### Prioridad Media
- [ ] Sistema de dungeons funcional
- [ ] IA de enemigos
- [ ] Sistema de loot
- [ ] Efectos visuales de habilidades

### Prioridad Baja
- [ ] Sistema de gremio
- [ ] Matchmaking
- [ ] Dedicated server build

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
