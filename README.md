# The Ether Domes

Micro-MMORPG cooperativo para 1-10 jugadores con combate Tab-Target.

## Inicio Rápido para Desarrolladores

Antes de comenzar, leer en este orden:

1. [docs/PROYECTO_ESTADO.md](docs/PROYECTO_ESTADO.md) - Estado actual del proyecto
2. [docs/TODO.md](docs/TODO.md) - Tareas pendientes
3. [docs/GUIA_DESARROLLO.md](docs/GUIA_DESARROLLO.md) - Convenciones y flujo de trabajo

Si usas Kiro IDE, los archivos en `.kiro/steering/` se cargan automáticamente.
Para otros IDEs o agentes, leer `.kiro/steering/00-onboarding.md`.

## Stack Tecnológico

- Unity 6.3 LTS
- Netcode for GameObjects
- Unity Relay

## Documentación

| Documento | Descripción |
|-----------|-------------|
| [Estado del Proyecto](docs/PROYECTO_ESTADO.md) | Fase actual y sistemas completados |
| [TODO](docs/TODO.md) | Tareas pendientes |
| [Plan MVP](docs/PLAN_10_FEATURES.md) | Roadmap de 10 features |
| [Guía de Desarrollo](docs/GUIA_DESARROLLO.md) | Onboarding |
| [Arquitectura](docs/ARQUITECTURA.md) | Estructura del código |

## Estructura del Repositorio

```
Assets/_Project/    Código del juego
docs/               Documentación
logs/               Logs de trabajo por sistema
.kiro/              Configuración y especificaciones
```

## Ejecución

1. Abrir con Unity 6.3 LTS
2. Cargar escena: `Assets/_Project/Scenes/Dungeons/Mazmorra1_1.unity`
3. Play para probar en modo offline

## Controles

| Tecla | Acción |
|-------|--------|
| WASD | Movimiento |
| Tab | Ciclar targets |
| 1-2 | Habilidades |
| Click derecho | Rotar cámara |
| Rueda | Zoom |
