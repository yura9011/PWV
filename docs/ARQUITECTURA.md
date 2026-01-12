# Arquitectura del Proyecto

## Estructura de Carpetas del Código

```
Assets/_Project/
├── Scripts/
│   ├── Camera/              # Sistema de cámara tercera persona
│   ├── Classes/             # Sistema de clases y habilidades
│   │   ├── Abilities/       # Definiciones de habilidades por clase
│   │   └── Interfaces/
│   ├── Combat/              # Sistemas de combate
│   │   ├── Abilities/       # AbilityDataSO y lógica
│   │   ├── Interfaces/
│   │   ├── Targeting/       # Sistema de selección de objetivos
│   │   └── Visuals/         # Efectos visuales de combate
│   ├── Core/                # GameManager, Bootstrap, datos globales
│   ├── Data/                # Modelos de datos (CharacterData, ItemData, etc.)
│   │   └── ScriptableObjects/
│   ├── Editor/              # Herramientas del Editor Unity
│   │   └── SceneCreators/
│   ├── Enemy/               # IA de enemigos y bosses
│   │   └── Interfaces/
│   ├── Movement/            # Controlador de movimiento WoW
│   ├── Network/             # Networking (NGO, Relay, Sessions)
│   │   └── Interfaces/
│   ├── Persistence/         # Guardado/carga de datos
│   │   └── Interfaces/
│   ├── Player/              # PlayerController, NetworkPlayer
│   ├── Progression/         # Loot, Equipment, Levels
│   │   └── Interfaces/
│   ├── Testing/             # TestPlayer, TestEnemy para pruebas offline
│   ├── UI/                  # Interfaces de usuario
│   │   ├── CharacterSelect/
│   │   ├── Combat/
│   │   ├── Debug/
│   │   ├── Inventory/
│   │   ├── Loot/
│   │   └── MainMenu/
│   └── World/               # Dungeons, Regiones, Bosses
│       └── Interfaces/
├── Prefabs/
│   ├── Characters/          # NetworkPlayer
│   ├── Enemies/             # BasicEnemy
│   ├── Player/              # Player prefab
│   └── UI/                  # FCTText, etc.
├── Scenes/
│   ├── Dungeons/            # Mazmorras de prueba
│   └── Regions/             # Regiones del mundo
├── ScriptableObjects/
│   └── Classes/             # Definiciones de las 8 clases
├── Input/                   # Input Actions (EtherDomesInput)
└── Materials/               # Materiales del juego
```

## Sistemas Principales

### GameManager
Punto central que contiene referencias a todos los sistemas:
- TargetSystem
- AbilitySystem
- AggroSystem
- CombatSystem
- ClassSystem
- ProgressionSystem
- LootSystem
- EquipmentSystem
- WorldManager
- DungeonSystem
- BossSystem

### Flujo de Datos

```
Input → PlayerController → NetworkTransform → Sincronización
         ↓
    AbilitySystem → CombatSystem → Enemy.TakeDamage
         ↓              ↓
    TargetSystem    AggroSystem
```

### Networking

```
Host (Servidor + Cliente)
  ↓
NetworkSessionManager
  ↓
ConnectionApprovalManager → Valida CharacterData
  ↓
RelayManager → Código de sala para conexión remota
```

## Assembly Definitions

Cada módulo tiene su propio `.asmdef` para compilación modular:
- `EtherDomes.Core`
- `EtherDomes.Combat`
- `EtherDomes.Network`
- `EtherDomes.Player`
- `EtherDomes.Enemy`
- `EtherDomes.UI`
- `EtherDomes.World`
- `EtherDomes.Persistence`
- `EtherDomes.Progression`
- `EtherDomes.Data`
- `EtherDomes.Testing`
- `EtherDomes.Editor`
