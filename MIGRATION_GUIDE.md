# Guía de Migración - The Ether Domes Phase 1

Este documento describe todos los sistemas implementados en este proyecto para facilitar la migración a un nuevo proyecto que combine estos sistemas con el sistema de combate.

## Resumen de Sistemas Implementados

| Sistema | Descripción | Archivos Principales |
|---------|-------------|---------------------|
| Network | Conexión Host/Join con Mirror | `NetworkSessionManager.cs` |
| Movement | Movimiento WoW-style | `WoWMovementController.cs` |
| Camera | Cámara tercera persona | `ThirdPersonCameraController.cs` |
| UI | Menú principal con selección de clase | `MainMenuController.cs` |
| Persistence | Guardado de posición | `PositionPersistenceManager.cs` |
| Visual | Sincronización de color por clase | `PlayerVisualController.cs` |

---

## 1. Dependencias Requeridas

### Packages (manifest.json)
```json
{
  "com.unity.cinemachine": "3.1.2",
  "com.unity.inputsystem": "1.11.2"
}
```

### Assets Externos
- **Mirror** (v96.0.1) - Networking
  - Ubicación: `Assets/Mirror/`
  - Incluye: kcp2k transport

---

## 2. Estructura de Carpetas

```
Assets/_Project/
├── Prefabs/
│   └── Characters/
│       └── NetworkPlayer.prefab
├── Scripts/
│   ├── Camera/
│   │   ├── CinemachinePlayerFollow.cs
│   │   ├── ThirdPersonCameraController.cs
│   │   ├── EtherDomes.Camera.asmdef
│   │   └── Editor/
│   │       ├── CinemachineSetupCreator.cs
│   │       └── EtherDomes.Camera.Editor.asmdef
│   ├── Core/
│   │   ├── PlayerClass.cs
│   │   ├── ClassSelectionData.cs
│   │   ├── ConnectionPayloadMessage.cs
│   │   └── EtherDomes.Core.asmdef
│   ├── Movement/
│   │   ├── WoWMovementController.cs
│   │   └── EtherDomes.Movement.asmdef
│   ├── Network/
│   │   ├── NetworkSessionManager.cs
│   │   ├── ConnectionApprovalAuthenticator.cs
│   │   └── EtherDomes.Network.asmdef
│   ├── Persistence/
│   │   ├── PositionPersistenceManager.cs
│   │   └── EtherDomes.Persistence.asmdef
│   ├── Player/
│   │   ├── PlayerVisualController.cs
│   │   ├── EtherDomes.Player.asmdef
│   │   └── Editor/
│   │       ├── PlayerPrefabCreator.cs
│   │       └── EtherDomes.Player.Editor.asmdef
│   └── UI/
│       ├── MainMenuController.cs
│       ├── ClassSelectorUI.cs
│       ├── EtherDomes.UI.asmdef
│       └── Editor/
│           ├── MainMenuUICreator.cs
│           └── EtherDomes.UI.Editor.asmdef
```

---

## 3. Archivos a Copiar

### 3.1 Core (Obligatorio)

| Archivo | Descripción |
|---------|-------------|
| `Core/PlayerClass.cs` | Enum con las clases disponibles (Guerrero, Mago) |
| `Core/ClassSelectionData.cs` | Almacena la clase seleccionada (static) |
| `Core/ConnectionPayloadMessage.cs` | Mensaje Mirror para enviar datos al conectar |
| `Core/EtherDomes.Core.asmdef` | Assembly definition |

### 3.2 Movement

| Archivo | Descripción |
|---------|-------------|
| `Movement/WoWMovementController.cs` | Controlador de movimiento WoW-style |
| `Movement/EtherDomes.Movement.asmdef` | Assembly definition |

**Características del movimiento:**
- WASD para mover/girar
- Q/E para strafe lateral
- Space para saltar con inercia
- Click derecho + arrastrar = rotar cámara Y personaje
- Click izquierdo + arrastrar = rotar solo cámara
- Sin click = A/D giran el personaje

### 3.3 Camera

| Archivo | Descripción |
|---------|-------------|
| `Camera/ThirdPersonCameraController.cs` | Controlador de cámara tercera persona |
| `Camera/CinemachinePlayerFollow.cs` | Seguimiento automático del jugador local |
| `Camera/EtherDomes.Camera.asmdef` | Assembly definition |
| `Camera/Editor/CinemachineSetupCreator.cs` | Herramienta de editor para setup |
| `Camera/Editor/EtherDomes.Camera.Editor.asmdef` | Assembly definition (Editor) |

**Configuración de cámara:**
- Distancia: 8 unidades
- Altura: 3 unidades
- Offset de mirada: (0, 1.5, 0)
- Sensibilidad: 2.0
- Ángulo vertical: -30° a 60°

### 3.4 Network

| Archivo | Descripción |
|---------|-------------|
| `Network/NetworkSessionManager.cs` | Manager principal de sesiones Mirror |
| `Network/ConnectionApprovalAuthenticator.cs` | Autenticador de conexiones |
| `Network/EtherDomes.Network.asmdef` | Assembly definition |

**Características de red:**
- Host/Client con IP directa
- Puerto por defecto: 7777
- Transporte: KCP (UDP)
- Máximo jugadores: 10
- Payload de conexión con clase y posición

### 3.5 UI

| Archivo | Descripción |
|---------|-------------|
| `UI/MainMenuController.cs` | Controlador del menú principal |
| `UI/ClassSelectorUI.cs` | UI de selección de clase |
| `UI/EtherDomes.UI.asmdef` | Assembly definition |
| `UI/Editor/MainMenuUICreator.cs` | Creador de UI por editor |
| `UI/Editor/EtherDomes.UI.Editor.asmdef` | Assembly definition (Editor) |

### 3.6 Player

| Archivo | Descripción |
|---------|-------------|
| `Player/PlayerVisualController.cs` | Sincroniza color según clase |
| `Player/EtherDomes.Player.asmdef` | Assembly definition |
| `Player/Editor/PlayerPrefabCreator.cs` | Crea prefab del jugador |
| `Player/Editor/EtherDomes.Player.Editor.asmdef` | Assembly definition (Editor) |

### 3.7 Persistence

| Archivo | Descripción |
|---------|-------------|
| `Persistence/PositionPersistenceManager.cs` | Guarda/carga posición con PlayerPrefs |
| `Persistence/EtherDomes.Persistence.asmdef` | Assembly definition |

---

## 4. Assembly Definitions (asmdef)

Las dependencias entre assemblies son:

```
EtherDomes.Core
    └── (sin dependencias)

EtherDomes.Movement
    ├── EtherDomes.Core
    └── Mirror

EtherDomes.Camera
    ├── EtherDomes.Core
    ├── Mirror
    └── Unity.Cinemachine

EtherDomes.Network
    ├── EtherDomes.Core
    ├── EtherDomes.Persistence
    ├── EtherDomes.Player
    └── Mirror

EtherDomes.UI
    ├── EtherDomes.Core
    ├── EtherDomes.Network
    ├── EtherDomes.Persistence
    └── Mirror

EtherDomes.Player
    ├── EtherDomes.Core
    └── Mirror

EtherDomes.Persistence
    └── (sin dependencias)
```

---

## 5. Prefab del Jugador (NetworkPlayer)

### Componentes Requeridos

| Componente | Configuración |
|------------|---------------|
| `NetworkIdentity` | (automático) |
| `CharacterController` | Height: 2, Radius: 0.5, Center: (0, 1, 0) |
| `WoWMovementController` | MoveSpeed: 7, TurnSpeed: 180, JumpForce: 8, Gravity: 20 |
| `PlayerVisualController` | (automático) |
| `Capsule Mesh` | Hijo visual con MeshRenderer |

### Crear Prefab Manualmente

1. Crear GameObject vacío "NetworkPlayer"
2. Agregar `NetworkIdentity`
3. Agregar `CharacterController` con configuración arriba
4. Agregar `WoWMovementController`
5. Agregar `PlayerVisualController`
6. Crear hijo "Visual" con Capsule mesh
7. Guardar como prefab en `Assets/_Project/Prefabs/Characters/`

### Crear Prefab con Editor

```
Menu: EtherDomes > Create Network Player Prefab
```

---

## 6. Configuración de Escena

### Objetos Requeridos en la Escena

| Objeto | Componentes |
|--------|-------------|
| `NetworkManager` | NetworkSessionManager, KcpTransport |
| `Main Camera` | Camera, ThirdPersonCameraController |
| `Ground` | Plane con Collider |
| `Canvas` | Canvas, CanvasScaler, GraphicRaycaster |
| `EventSystem` | EventSystem, StandaloneInputModule |
| `MainMenuController` | MainMenuController |

### Setup Automático

```
Menu: EtherDomes > Setup Complete Scene
```

Este comando:
1. Limpia elementos duplicados
2. Crea plano de terreno
3. Configura cámara con ThirdPersonCameraController
4. Configura NetworkManager con prefab
5. Crea UI del menú principal
6. Guarda la escena

---

## 7. Comandos de Editor Disponibles

| Comando | Descripción |
|---------|-------------|
| `EtherDomes > Setup Complete Scene` | Configura escena completa |
| `EtherDomes > Create Network Player Prefab` | Crea prefab del jugador |
| `EtherDomes > Create Cinemachine Camera` | Crea cámara Cinemachine |
| `EtherDomes > Clean Scene (Remove Duplicate UI)` | Limpia UI duplicada |

---

## 8. Controles del Jugador

| Tecla | Acción |
|-------|--------|
| `W` | Avanzar |
| `S` | Retroceder |
| `A` | Girar izquierda (sin mouse) / Strafe izquierda (con mouse derecho) |
| `D` | Girar derecha (sin mouse) / Strafe derecha (con mouse derecho) |
| `Q` | Strafe izquierda |
| `E` | Strafe derecha |
| `Space` | Saltar |
| `Click Derecho + Arrastrar` | Rotar cámara Y personaje |
| `Click Izquierdo + Arrastrar` | Rotar solo cámara |

---

## 9. Integración con Sistema de Combate

### Pasos para Combinar Proyectos

1. **Copiar carpeta `_Project`** del proyecto de movimiento al nuevo proyecto
2. **Verificar dependencias** en Package Manager:
   - Mirror (v96.0.1+)
   - Cinemachine (3.1.2+)
   - Input System (1.11.2+)
3. **Resolver conflictos de asmdef** si el proyecto de combate tiene sus propios assemblies
4. **Integrar NetworkPlayer**:
   - Agregar componentes de combate al prefab NetworkPlayer
   - O crear nuevo prefab que herede de ambos sistemas

### Puntos de Integración Sugeridos

| Sistema Movimiento | Punto de Integración | Sistema Combate |
|--------------------|---------------------|-----------------|
| `WoWMovementController` | Deshabilitar durante cast | Habilidades |
| `PlayerVisualController` | Efectos visuales | VFX de combate |
| `NetworkSessionManager` | Eventos de conexión | Sincronización de stats |
| `ClassSelectionData` | Clase seleccionada | Stats base por clase |

### Ejemplo: Deshabilitar Movimiento Durante Cast

```csharp
// En tu script de habilidades
public class AbilityCaster : NetworkBehaviour
{
    private WoWMovementController _movement;
    
    void Start()
    {
        _movement = GetComponent<WoWMovementController>();
    }
    
    public void StartCast()
    {
        _movement.enabled = false; // Deshabilitar movimiento
    }
    
    public void EndCast()
    {
        _movement.enabled = true; // Rehabilitar movimiento
    }
}
```

---

## 10. Troubleshooting

### Problema: Botones del menú no responden
**Solución:** Verificar que existe EventSystem en la escena. Ejecutar `EtherDomes > Setup Complete Scene`.

### Problema: Cámara en primera persona
**Solución:** Verificar que ThirdPersonCameraController tiene `_distance = 8` y `_height = 3`.

### Problema: Salto no funciona
**Solución:** El input de salto se captura en `Update()` y se procesa en `FixedUpdate()`. Verificar que `_jumpInput` se mantiene hasta ser procesado.

### Problema: Shader null al crear prefab
**Solución:** El script busca shaders en orden: URP/Lit > Standard > Sprites/Default. Asegurar que al menos uno existe.

### Problema: No se ve el terreno
**Solución:** Verificar que Main Camera tiene tag "MainCamera" y está mirando hacia abajo (Y positivo).

---

## 11. Notas Técnicas

### Mirror vs Unity Netcode
Este proyecto usa **Mirror** (no Unity Netcode for GameObjects). Las diferencias principales:
- `NetworkBehaviour` en lugar de `NetworkBehaviour` (mismo nombre, diferente namespace)
- `[Command]` y `[ClientRpc]` para RPCs
- `isLocalPlayer` para verificar jugador local
- `NetworkServer.active` / `NetworkClient.active` para estado de red

### Cinemachine 3.x
Unity 6 usa Cinemachine 3.x con API diferente:
- Namespace: `Unity.Cinemachine` (no `Cinemachine`)
- `CinemachineCamera` en lugar de `CinemachineVirtualCamera`
- `CinemachineFollow` en lugar de `CinemachineTransposer`

### Input System
El proyecto usa el Input System legacy (`Input.GetKey`, `Input.GetAxis`) para simplicidad. Para migrar al nuevo Input System, reemplazar las llamadas en `WoWMovementController.ReadInput()`.

---

## 12. Checklist de Migración

- [ ] Copiar carpeta `Assets/_Project/` completa
- [ ] Instalar Mirror desde Asset Store o Package Manager
- [ ] Verificar Cinemachine 3.x en Package Manager
- [ ] Ejecutar `EtherDomes > Create Network Player Prefab`
- [ ] Ejecutar `EtherDomes > Setup Complete Scene`
- [ ] Asignar prefab en NetworkManager.playerPrefab
- [ ] Probar con Play > Host
- [ ] Integrar componentes de combate en NetworkPlayer prefab
- [ ] Ajustar asmdef si hay conflictos

---

*Documento generado para The Ether Domes - Phase 1*
*Mirror 96.0.1 | Unity 6 | Cinemachine 3.x*
