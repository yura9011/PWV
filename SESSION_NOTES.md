# Session Notes - The Ether Domes

## 2026-01-08 (Noche) - Background Wallpaper & Polish

### ✅ Completado

1. **Background Wallpaper en Menú Principal**
   - Agregado `SetupBackgroundWallpaper()` en `MainMenuController.cs`
   - Carga sprite desde `Resources/Wallpapers/Coralwallpaper`
   - Se posiciona detrás de todos los paneles (sibling index 0)
   - Se oculta automáticamente al entrar al juego

2. **Configuración de Textura**
   - Actualizado `Coralwallpaper.png.meta` para importar como Sprite
   - `textureType: 8` (Sprite), `spriteMode: 1` (Single)

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `MainMenuController.cs` | Agregado `SetupBackgroundWallpaper()`, ocultar background en `OnClassAcceptClicked()` |
| `Coralwallpaper.png.meta` | Cambiado a tipo Sprite |

---

## 2026-01-08 (Tarde) - Client Movement & Camera Polish

### ✅ Completado

1. **Cliente Puede Mover su Personaje**
   - Creado `ClientNetworkTransform.cs` que retorna `false` en `OnIsServerAuthoritative()`
   - Actualizado `NetworkUISetupCreator.cs` para usar `ClientNetworkTransform` en el prefab

2. **Zoom de Cámara con Rueda del Ratón**
   - Min: 0 (primera persona)
   - Max: 16 (doble de distancia default)
   - Agregado en `ThirdPersonCameraController.cs`

3. **Colisión de Cámara**
   - `ApplyGroundCollision()` - Evita que la cámara atraviese el suelo
   - `ApplyObstacleCollision()` - Evita clipping con objetos

4. **Menú de Pausa (ESC)**
   - Creado `PauseMenuController.cs`
   - Botón "SALIR" desconecta y cierra el juego
   - Toggle con ESC

5. **Relay Code en UI**
   - Creado `GameSessionUI.cs` - Muestra código en esquina inferior izquierda
   - Agregado `JoinCode` property en `RelayManager.cs`

### Archivos Creados/Modificados

| Archivo | Cambio |
|---------|--------|
| `ClientNetworkTransform.cs` | NUEVO - Autoridad de cliente |
| `ThirdPersonCameraController.cs` | Zoom + colisiones |
| `PauseMenuController.cs` | NUEVO - Menú pausa |
| `GameSessionUI.cs` | NUEVO - UI código relay |
| `RelayManager.cs` | Agregado JoinCode property |
| `NetworkUISetupCreator.cs` | Usa ClientNetworkTransform |

---

## 2026-01-08 (Mediodía) - WoW-Style Controls

### ✅ Completado

1. **Controles Estilo WoW**
   - W/S: Adelante/Atrás
   - A/D: Rotar jugador (sin mouse) / Strafe (con click derecho)
   - Q/E: Strafe siempre
   - Click izquierdo: Solo rota cámara
   - Click derecho: Rota cámara Y jugador
   - W+A/W+D con click derecho: Movimiento diagonal

2. **Eliminado Menú Viejo**
   - Removido `SetupCompleteScene` de `CinemachineSetupCreator.cs`
   - Solo usar `Tools > EtherDomes > Setup Network and UI`

### Archivos Modificados

| Archivo | Cambio |
|---------|--------|
| `PlayerController.cs` | Controles WoW completos |
| `CinemachineSetupCreator.cs` | Eliminado SetupCompleteScene |

---

## 2026-01-08 (Mañana) - Camera & Movement Fix

### ✅ Completado

1. **Camera System Conflict FIXED**
   - Root cause: `PlayerController` y `ThirdPersonCameraController` ambos controlaban cámara
   - Solución: Solo `ThirdPersonCameraController` maneja cámara
   - `PlayerController` lee yaw de `Camera.main` para dirección de movimiento

2. **Main Menu "Zombie" UI FIXED**
   - Canvas se oculta correctamente al iniciar partida

3. **Relay Multiplayer Working**
   - Host genera código
   - Cliente se une con código
   - Ambos se ven y pueden moverse

---

## Estado del Proyecto

### Fase 1 (Core de Red y Locomoción) - ✅ COMPLETADA
- Relay multiplayer funcionando
- Movimiento de jugador sincronizado (cliente tiene autoridad)
- Cámara tercera persona con zoom y colisiones
- Controles estilo WoW
- Menú principal con wallpaper
- Menú de pausa funcional
- UI de código Relay en partida

### Próximo Objetivo: Fase 4 (IA y Amenaza)
- Enemy AI movement pendiente de testing
- Sistema de aggro por implementar
- Tab targeting system

---

## How to Test

### Solo Test
1. Open Unity
2. `Tools > EtherDomes > Setup Network and UI`
3. Play
4. Click "INICIAR" > "CREAR" > "INICIAR SERVIDOR"
5. Seleccionar clase y jugar

### Multiplayer Test
1. Build: File > Build Settings > Build
2. Run dos instancias
3. Host: "Usar Relay" ON → CREAR → INICIAR SERVIDOR → Copiar código
4. Client: "Usar Relay" ON → UNIRSE → Pegar código → ACEPTAR

---

## Archivos Clave del Proyecto

```
Assets/_Project/Scripts/
├── Camera/
│   └── ThirdPersonCameraController.cs  # Cámara con zoom y colisiones
├── Editor/
│   └── NetworkUISetupCreator.cs        # Setup de escena
├── Network/
│   ├── ClientNetworkTransform.cs       # Autoridad de cliente
│   ├── RelayManager.cs                 # Gestión de Relay
│   └── NetworkSessionManager.cs        # Sesiones de red
├── Player/
│   └── PlayerController.cs             # Movimiento estilo WoW
└── UI/
    ├── MainMenu/
    │   └── MainMenuController.cs       # Menú principal + wallpaper
    ├── GameSessionUI.cs                # UI código Relay
    └── PauseMenuController.cs          # Menú pausa

Assets/Resources/Wallpapers/
└── Coralwallpaper.png                  # Fondo del menú
```
