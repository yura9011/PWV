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

### Testing Offline Implementado (2026-01-12)
- **TestPlayer**: Jugador de prueba con controles WoW completos
- **TestEnemy**: Enemigos con IA básica, detección de paredes, aggro por línea de visión
- **Escena Mazmorra1_1**: Dungeon de prueba con 13 enemigos
- **Controles universales**: Funcionando correctamente

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

### 5. Combat Testing System ✅ COMPLETADO (2026-01-12)
- TestPlayer: Jugador offline con controles completos
- TestEnemy: Enemigos con IA, colisión de paredes, línea de visión
- Ataques básico (1) y pesado (2) funcionando
- Tab targeting y click targeting funcionando

---

## Estructura de Carpetas Clave

```
Assets/_Project/Scripts/
├── Testing/
│   ├── TestPlayer.cs          # Jugador offline para testing
│   └── TestEnemy.cs           # Enemigo offline para testing
├── Camera/
│   └── ThirdPersonCameraController.cs  # Cámara universal
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
└── Dungeons/
    └── Mazmorra1_1.unity      # Dungeon de testing
```

---

## Cómo Probar el Sistema de Combate

### Testing Offline (Recomendado)
1. Abrir escena `Assets/_Project/Scenes/Dungeons/Mazmorra1_1.unity`
2. Play
3. Usar controles WoW para moverse
4. Tab o Click para seleccionar enemigos
5. 1 para ataque básico, 2 para ataque pesado

### Testing Online
1. Crear partida desde menú principal
2. Unirse con código Relay
3. Probar combate en MainGame

---

## Próximos Pasos

### Fase 3 - Pendiente
- [ ] Floating Combat Text en escena de test
- [ ] Target Frame UI mejorado
- [ ] Efectos visuales de ataques
- [ ] Sonidos de combate
- [ ] Integrar sistema de combate con NetworkPlayer

### Fase 4 - IA y Amenaza
- [ ] Sistema de amenaza (Threat)
- [ ] IA mejorada con NavMesh
- [ ] Roles de la Trinidad (Tank/DPS/Healer)

---

Documento actualizado: 12 Enero 2026
