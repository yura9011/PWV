# The Ether Domes - Tareas Pendientes

## 🔄 EN PROGRESO - Fase 3: Sistema de Combate (2026-01-12)

### Sistema de Testing Offline ✅ COMPLETADO
- [x] `TestPlayer.cs` - Jugador de prueba con controles WoW completos
- [x] `TestEnemy.cs` - Enemigos de prueba con IA básica y colisión de paredes
- [x] Escena `Mazmorra1_1.unity` configurada para testing
- [x] 13 enemigos de prueba (Skeletons, Ghouls, Wraiths, Revenants, Crypt_Lord)
- [x] ThirdPersonCameraController integrado con TestPlayer
- [x] Controles universales estilo WoW funcionando

### Controles Implementados ✅
- W/S: Adelante/Atrás
- A/D: Rotar (sin mouse) / Strafe (con click derecho)
- Q/E: Strafe siempre
- Space: Saltar
- Click izquierdo: Rotar cámara / Seleccionar target
- Click derecho: Rotar cámara Y jugador
- Tab: Ciclar targets
- 1: Ataque básico (50 daño)
- 2: Ataque pesado (125 daño)
- Esc: Limpiar target
- Rueda del ratón: Zoom

### Pendiente de Refinamiento
- [ ] Mejorar feedback visual de ataques
- [ ] Agregar efectos de sonido
- [ ] Floating Combat Text en escena de test
- [ ] Target Frame UI mejorado

---

## ✅ Completado (2026-01-10) - Sistema de Menús Nuevo + Música

### Nuevo Sistema de Menús OnGUI
- [x] Tarea 1.1: Renombrar menú del Editor a "Crear Escena Menu Principal"
- [x] Tarea 1.2: Crear modelos de datos para persistencia
- [x] Tarea 1.3: Crear `MenuNavigator.cs` - Controlador central de navegación

### Menús Implementados
- [x] Menu 1-5: Sistema completo de menús
- [x] Popups: Crear mundo, borrar, crear personaje, etc.
- [x] Integración con NetworkSessionManager

---

## ✅ Completado (2026-01-09) - Tab Targeting y FCT

### Tab Targeting System
- [x] `TargetSystem.cs` implementado con Tab cycling
- [x] `Enemy.cs` implementa ITargetable e ITargetIndicator
- [x] Input Actions configurados

### Floating Combat Text
- [x] `FloatingCombatText.cs` existente
- [x] Prefab `FCTText.prefab` creado
- [x] `CombatEvents.cs` para desacoplar Enemy de UI

---

## Prioridad Media

### UI Improvements
- [ ] Add Health/Mana bars above player heads
- [ ] Add Target Frame improvements

---

## Archivos Clave - Fase 3

- `Assets/_Project/Scripts/Testing/TestPlayer.cs` - Jugador de prueba offline
- `Assets/_Project/Scripts/Testing/TestEnemy.cs` - Enemigo de prueba offline
- `Assets/_Project/Scripts/Camera/ThirdPersonCameraController.cs` - Cámara universal
- `Assets/_Project/Scenes/Dungeons/Mazmorra1_1.unity` - Escena de testing

---

## Notas

- El proyecto compila correctamente en Unity 6.3 LTS
- Netcode for GameObjects versión 1.8.1
- MCP for Unity package installed
- Estructura de carpetas: `Assets/_Project/Scripts/` organizado por módulos
