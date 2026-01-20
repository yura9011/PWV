# The Ether Domes - Estado del Proyecto

## Resumen
Micro-MMORPG cooperativo para 1-10 jugadores con combate Tab-Target estilo WoW, sistema de clases Trinity, dungeons con bosses y progresión de nivel.

## Stack Tecnológico
- **Motor**: Unity 6.3 LTS
- **Networking**: Netcode for GameObjects (NGO) + Unity Relay
- **Arquitectura**: Híbrida Host-Play / Dedicated Server
- **Input**: Legacy Input (UnityEngine.Input)
- **UI Menús**: OnGUI (sistema actual)
- **Repositorio**: https://github.com/yura9011/PWV.git

---

## Fase Actual: Fase 3 - Sistema de Combate 🔄 EN PROGRESO

### Testing Offline Implementado (2026-01-16)
- **TestPlayer**: Jugador de prueba con controles WoW completos
- **TestEnemy**: Enemigos con IA básica, detección de paredes, aggro por línea de visión
- **Escenas Configuradas**: Todas las escenas principales con TestPlayer y cámara
- **Sistema de Portales**: ScenePortal funcionando entre todas las escenas
- **Controles universales**: Funcionando correctamente en todas las escenas

### Controles Estilo WoW ✅
| Tecla | Acción |
|-------|--------|
| W/S | Adelante/Atrás |
| A/D | Rotar (sin mouse) / Strafe (con click derecho) |
| Q/E | Strafe siempre |
| Space | Saltar |
| Click Izq | Rotar cámara / Seleccionar target |
| Click Der | Rotar cámara Y jugador |
| Tab | Ciclar targets |
| 1 | Ataque básico (50 daño) |
| 2 | Ataque pesado (125 daño) |
| Esc | Limpiar target |
| Rueda | Zoom cámara |

---

## Sistemas Implementados

### 1. Sistema de Menús ✅ COMPLETADO (2026-01-10)
- 5 Menús OnGUI con navegación completa
- Persistencia Local JSON
- Música y Video de Fondo

### 2. Networking ✅ COMPLETADO
- NetworkSessionManager, ConnectionApprovalManager
- RelayManager con código de sala
- ClientNetworkTransform para movimiento fluido

### 3. Player System ✅ COMPLETADO
- PlayerController con movimiento estilo WoW
- NetworkPlayer con stats sincronizados

### 4. Camera System ✅ COMPLETADO
- ThirdPersonCameraController con zoom y colisiones
- Soporta TestPlayer (offline) y NetworkPlayer (online)

### 5. Combat Testing System ✅ COMPLETADO (2026-01-16)
- TestPlayer: Jugador offline con controles completos
- TestEnemy: Enemigos con IA, colisión de paredes, línea de visión
- Ataques básico (1) y pesado (2) funcionando
- Tab targeting y click targeting funcionando
- **Todas las escenas configuradas**: RegionInicio, Region1, Mazmorra1_1-1_4
- **Sistema de portales**: ScenePortal conectando todas las escenas
- **Cámara universal**: SimpleThirdPersonCamera funcionando en todas las escenas

---

## Estructura de Carpetas Clave

```
Assets/_Project/Scripts/
├── Testing/
│   ├── TestPlayer.cs          # Jugador offline para testing
│   └── TestEnemy.cs           # Enemigo offline para testing
├── Camera/
│   ├── ThirdPersonCameraController.cs  # Cámara universal
│   └── SimpleThirdPersonCamera.cs      # Cámara simplificada
├── World/
│   └── ScenePortal.cs         # Sistema de portales entre escenas
├── Combat/
│   ├── CombatManager.cs
│   ├── TargetingSystem.cs
│   └── Abilities/
├── Player/
│   ├── PlayerController.cs    # Movimiento WoW networked
│   └── NetworkPlayer.cs
└── Network/
    ├── NetworkSessionManager.cs
    └── RelayManager.cs

Assets/_Project/Scenes/
├── MainGame.unity             # Escena principal
├── Regions/
│   ├── RegionInicio.unity     # Región inicial
│   └── Region1.unity          # Región principal
└── Dungeons/
    ├── Mazmorra1_1.unity      # Dungeon de testing
    ├── Mazmorra1_2.unity      # Dungeon nivel 2
    ├── Mazmorra1_3.unity      # Dungeon nivel 3
    └── Mazmorra1_4.unity      # Dungeon nivel 4
```

---

## Cómo Probar el Sistema de Combate

### Testing Offline (Recomendado)
1. Abrir cualquier escena: `RegionInicio`, `Region1`, o `Mazmorra1_1-1_4`
2. Play
3. Usar controles WoW para moverse
4. Tab o Click para seleccionar enemigos
5. 1 para ataque básico, 2 para ataque pesado
6. Usar portales para navegar entre escenas

### Testing Online
1. Crear partida desde menú principal
2. Unirse con código Relay
3. Probar combate en MainGame

---

## Próximos Pasos

### Fase 3 - Pendiente
- [x] Floating Combat Text en escena de test - Completado
- [x] Efectos visuales de ataques - Sistema AttackEffects completado
- [ ] Sonidos de combate
- [ ] Target Frame UI mejorado
- [ ] Integrar sistema de combate con NetworkPlayer

### Fase 4 - IA y Amenaza
- [ ] Sistema de amenaza (Threat)
- [ ] IA mejorada con NavMesh
- [ ] Roles de la Trinidad (Tank/DPS/Healer)

---

Documento actualizado: 16 Enero 2026
