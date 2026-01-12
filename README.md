# The Ether Domes

Micro-MMORPG cooperativo para 1-10 jugadores con combate Tab-Target estilo WoW.

## 🚀 Para Agentes/Desarrolladores - Leer Primero

**Antes de hacer cualquier cosa, leer en este orden:**

1. **[docs/PROYECTO_ESTADO.md](docs/PROYECTO_ESTADO.md)** - Estado actual del proyecto
2. **[docs/TODO.md](docs/TODO.md)** - Tareas pendientes
3. **[docs/GUIA_DESARROLLO.md](docs/GUIA_DESARROLLO.md)** - Convenciones y flujo de trabajo

**Si usas Kiro IDE:** Los archivos en `.kiro/steering/` se cargan automáticamente con reglas adicionales.

**Si usas otro IDE/agente:** Lee también `.kiro/steering/00-onboarding.md` para reglas completas.

---

## Stack
- Unity 6.3 LTS
- Netcode for GameObjects + Unity Relay
- MCP for Unity (integración con Kiro)

## Documentación

| Documento | Descripción |
|-----------|-------------|
| [Estado del Proyecto](docs/PROYECTO_ESTADO.md) | Fase actual y sistemas completados |
| [TODO](docs/TODO.md) | Tareas pendientes |
| [Plan MVP](docs/PLAN_10_FEATURES.md) | Roadmap de 10 features (~60h) |
| [Guía de Desarrollo](docs/GUIA_DESARROLLO.md) | Onboarding para devs |
| [Arquitectura](docs/ARQUITECTURA.md) | Estructura del código |
| [Notas de Sesión](docs/SESSION_NOTES.md) | Historial de trabajo |

## Estructura

```
/
├── Assets/_Project/    # Código del juego
├── docs/               # Documentación
├── logs/               # Logs de trabajo por sistema
└── .kiro/              # Configuración de Kiro y specs
```

## Quick Start

1. Abrir con Unity 6.3 LTS
2. Escena de prueba: `Assets/_Project/Scenes/Dungeons/Mazmorra1_1.unity`
3. Play para probar controles y combate offline

## Controles (Estilo WoW)

| Tecla | Acción |
|-------|--------|
| WASD | Movimiento |
| Tab | Ciclar targets |
| 1-2 | Ataques |
| Click Der | Rotar cámara + jugador |
| Rueda | Zoom |
