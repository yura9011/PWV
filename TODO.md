# The Ether Domes - Tareas Pendientes

## Prioridad Alta (Próxima Sesión)

### ✅ Enemy AI Movement (Completado 2026-01-09)
- [x] Ejecutado `EtherDomes > Fix Enemy Components` via MCP
- [x] Enemigos ya tienen EnemyAI configurado
- [x] TargetSystem agregado a la escena MainGame

### ✅ Tab Targeting System (Completado 2026-01-09)
- [x] `TargetSystem.cs` implementado con Tab cycling
- [x] `Enemy.cs` implementa ITargetable e ITargetIndicator
- [x] Input Actions configurados (Tab=CycleTarget, Escape=ClearTarget)
- [x] Prefab BasicEnemy tiene componente Enemy
- [x] Indicador visual configurado en prefab BasicEnemy
- [x] Probado y funcionando en partida

### ✅ Floating Combat Text (Completado 2026-01-09)
- [x] `FloatingCombatText.cs` ya existía
- [x] Creado prefab `FCTText.prefab` con TextMeshPro
- [x] Creado `CombatEvents.cs` para desacoplar Enemy de UI
- [x] Enemy emite evento al recibir daño via ClientRpc
- [x] FloatingCombatText escucha eventos y muestra números
- [ ] **PENDIENTE**: Probar en partida atacando enemigos

---

## Prioridad Media

### UI Improvements
- [ ] Add Health/Mana bars above player heads
- [ ] Add Target Frame improvements

### ✅ MCP Integration (Completado 2026-01-09)
- [x] Paquete MCP for Unity instalado via git URL
- [x] Servidor HTTP corriendo en localhost:8080
- [x] 18 tools disponibles para Kiro
- [x] Documentación en `.kiro/steering/unity-mcp.md`

---

## Prioridad Baja (No urgente)

### Migración Input System
- [ ] Migrar de Input Manager (legacy) al nuevo Input System package
- Razón: Unity marcó Input Manager como deprecado
- El código actual usa `Input.GetAxis`, `Input.GetKey`, etc.
- Archivos afectados: `PlayerController.cs` y otros que usen input
- Documentación: https://docs.unity3d.com/Packages/com.unity.inputsystem@latest

### Lobby Warnings
- [ ] Clean up Unity Lobby warnings (optional feature not configured)

---

## ✅ Completado (2026-01-08)

### Sesión Tarde/Noche
- [x] Background wallpaper en menú principal (Coralwallpaper.png)
- [x] Wallpaper se oculta al entrar al juego
- [x] Relay code visible en UI durante partida (GameSessionUI)
- [x] Cliente puede mover su personaje (ClientNetworkTransform)
- [x] Zoom de cámara con rueda del ratón (min: primera persona, max: 16m)
- [x] Colisión de cámara con suelo y obstáculos
- [x] Menú de pausa con ESC (botón SALIR)

### Sesión Mañana/Mediodía
- [x] Controles estilo WoW implementados:
  - W/S: Adelante/Atrás
  - A/D: Rotar (sin mouse) / Strafe (con click derecho)
  - Q/E: Strafe siempre
  - Click izquierdo: Solo rota cámara
  - Click derecho: Rota cámara Y jugador
- [x] Camera system fix (solo ThirdPersonCameraController controla cámara)
- [x] Main Menu "zombie" UI fix
- [x] Player spawning and movement verified
- [x] Relay multiplayer tested and working
- [x] NetworkUISetupCreator.cs - Editor tool para crear UI de red
- [x] Eliminado menú viejo "Setup Complete Scene"

---

## Archivos Clave

- `MainMenuController.cs` - Controlador del menú principal + wallpaper
- `PlayerController.cs` - Movimiento del jugador estilo WoW
- `ThirdPersonCameraController.cs` - Cámara con zoom y colisiones
- `ClientNetworkTransform.cs` - Autoridad de cliente para movimiento
- `NetworkUISetupCreator.cs` - Editor tool para UI de red
- `GameSessionUI.cs` - Muestra código Relay en partida
- `PauseMenuController.cs` - Menú de pausa con ESC

---

## Notas

- El proyecto compila correctamente en Unity 6.3 LTS
- Netcode for GameObjects versión 1.8.1
- MCP for Unity package installed from git URL
- Estructura de carpetas: `Assets/_Project/Scripts/` organizado por módulos
- Wallpaper ubicado en: `Assets/Resources/Wallpapers/Coralwallpaper.png`
