# The Ether Domes - Estado del Proyecto

## Resumen
Micro-MMORPG cooperativo para 1-10 jugadores con combate Tab-Target estilo WoW, sistema de clases Trinity, dungeons con bosses y progresión de nivel.

## Stack Tecnológico
- **Motor**: Unity 6.3 LTS
- **Networking**: Unity Netcode for GameObjects (NGO) 1.8.1
- **Relay**: Unity Relay para conexión sin puertos
- **Arquitectura**: Híbrida Host-Play / Dedicated Server
- **Input**: Legacy Input (UnityEngine.Input) - Pendiente migración a Input System

---

## Sistemas Implementados

### 1. Networking ✅ COMPLETADO
- `NetworkSessionManager`: Gestión de sesiones Host/Client/Server
- `ConnectionApprovalManager`: Validación de conexiones
- `RelayManager`: Conexión via Unity Relay con código de sala
- `ClientNetworkTransform`: Autoridad de cliente para movimiento fluido
- Puerto por defecto: 7777

### 2. Player System ✅ COMPLETADO
- `PlayerController`: Movimiento estilo WoW completo
  - W/S: Adelante/Atrás
  - A/D: Rotar (sin mouse) / Strafe (con click derecho)
  - Q/E: Strafe siempre
  - Click izquierdo: Solo rota cámara
  - Click derecho: Rota cámara Y jugador
- `NetworkPlayer`: Jugador networked con stats sincronizados
- Selección de clase: Guerrero (rojo) / Mago (azul)

### 3. Camera System ✅ COMPLETADO
- `ThirdPersonCameraController`: Cámara tercera persona
  - Zoom con rueda del ratón (0-16m)
  - Colisión con suelo y obstáculos
  - Rotación independiente del jugador
  - Click derecho para rotar jugador con cámara

### 4. UI System ✅ COMPLETADO
- `MainMenuController`: Menú principal multi-panel
  - Flujo: Main → Mode → Create/Join → Class → Game
  - Background wallpaper (Coralwallpaper.png)
  - Se oculta al entrar al juego
- `PauseMenuController`: Menú de pausa con ESC
  - Botón SALIR para desconectar y cerrar
- `GameSessionUI`: Muestra código Relay en partida

### 5. Combat System (Estructura Base)
- `TargetSystem`: Tab-targeting con ciclo de enemigos
- `AbilitySystem`: Sistema de habilidades con GCD, cooldowns y cast times
- `CombatTestUI`: UI de combate con Target Frame y Action Bar
- `AggroSystem`: Sistema de amenaza (estructura base)
- **Status**: Pendiente de testing e integración

### 6. Enemy System (Estructura Base)
- `Enemy`: Entidad enemiga con ITargetable
- `EnemySpawner`: Spawner de enemigos en red
- `EnemyAI`: Máquina de estados (Idle, Patrol, Aggro, Combat, Return)
- **Status**: Pendiente de testing

### 7. Progression System (Estructura)
- `ProgressionSystem`: XP y niveles
- `LootSystem`: Sistema de loot
- `EquipmentSystem`: Equipamiento

### 8. Class System (Estructura)
- `ClassSystem`: Gestión de clases
- `ClassAbilityDefinitions`: Definiciones de habilidades por clase

---

## Estructura de Carpetas

```
Assets/_Project/
├── Prefabs/
│   └── Characters/
│       └── NetworkPlayer.prefab      # Prefab del jugador con ClientNetworkTransform
├── Scenes/
│   └── MainGame.unity                # Escena principal
└── Scripts/
    ├── Camera/
    │   └── ThirdPersonCameraController.cs
    ├── Combat/
    │   ├── AbilitySystem.cs
    │   ├── TargetSystem.cs
    │   └── AggroSystem.cs
    ├── Core/
    │   └── ClassSelectionData.cs
    ├── Editor/
    │   ├── NetworkUISetupCreator.cs  # Setup principal de escena
    │   └── CinemachineSetupCreator.cs
    ├── Enemy/
    │   ├── Enemy.cs
    │   ├── EnemyAI.cs
    │   └── EnemySpawner.cs
    ├── Network/
    │   ├── NetworkSessionManager.cs
    │   ├── ConnectionApprovalManager.cs
    │   ├── RelayManager.cs
    │   ├── LobbyManager.cs
    │   └── ClientNetworkTransform.cs
    ├── Player/
    │   └── PlayerController.cs
    └── UI/
        ├── MainMenu/
        │   └── MainMenuController.cs
        ├── GameSessionUI.cs
        └── PauseMenuController.cs

Assets/Resources/
└── Wallpapers/
    └── Coralwallpaper.png            # Fondo del menú principal
```

---

## Controles Actuales

| Tecla | Acción |
|-------|--------|
| W/S | Adelante/Atrás |
| A/D | Rotar (sin mouse) / Strafe (con click derecho) |
| Q/E | Strafe siempre |
| Click Izq | Rotar solo cámara |
| Click Der | Rotar cámara Y jugador |
| Rueda | Zoom cámara |
| ESC | Menú pausa |
| Tab | Ciclar targets (pendiente) |

---

## Cómo Probar

### Setup Inicial
1. Abrir Unity
2. `Tools > EtherDomes > Setup Network and UI`
3. Guardar escena

### Solo Test
1. Play
2. INICIAR → CREAR → INICIAR SERVIDOR
3. Seleccionar clase → ACEPTAR
4. Probar movimiento y cámara

### Multiplayer Test
1. Build: File > Build Settings > Build
2. Ejecutar build + Unity Editor
3. Host: Usar Relay ON → CREAR → INICIAR SERVIDOR → Copiar código de consola
4. Client: Usar Relay ON → UNIRSE → Pegar código → ACEPTAR
5. Ambos seleccionan clase y juegan

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
- `EtherDomes.Camera`
- `EtherDomes.Classes`
- `EtherDomes.Progression`
- `EtherDomes.World`
- `EtherDomes.Persistence`
- `EtherDomes.Editor`

---

## Próximos Pasos de Desarrollo

### Inmediato (Fase 4 - IA y Amenaza)
1. [ ] Probar Enemy AI Movement
2. [ ] Implementar Tab targeting funcional
3. [ ] Sistema de aggro básico

### Corto Plazo
4. [ ] Sistema de combate básico
5. [ ] Feedback visual de selección
6. [ ] Barras de vida sobre jugadores/enemigos

### Mediano Plazo
7. [ ] Sistema de habilidades por clase
8. [ ] Primera dungeon con boss
9. [ ] Sistema de loot

---

## Notas Importantes

1. **Namespace Conflicts**: 
   - En `EtherDomes.UI` usar `UnityEngine.Debug` (alias `UnityDebug`)
   - En scripts de Input usar `UnityEngine.Input` (alias `UnityInput`)

2. **Networking**: 
   - Cliente tiene autoridad sobre su posición (`ClientNetworkTransform`)
   - Daño se procesa en servidor via `ServerRpc`

3. **Setup de Escena**:
   - Usar SOLO `Tools > EtherDomes > Setup Network and UI`
   - No usar el menú viejo de Cinemachine

4. **Wallpaper**:
   - Ubicación: `Assets/Resources/Wallpapers/Coralwallpaper.png`
   - Debe estar configurado como Sprite (textureType: 8)

---

## Contacto
Documento actualizado: 8 Enero 2026
