---
inclusion: always
---

# Onboarding - The Ether Domes

## Sobre el Proyecto

**The Ether Domes** es un Micro-MMORPG cooperativo (1-10 jugadores) con combate Tab-Target estilo WoW, desarrollado en Unity 6.3 LTS con Netcode for GameObjects.

**Equipo:** 2 desarrolladores + agentes de IA (Kiro)

---

## Primera Acción: Leer Contexto

Antes de cualquier trabajo, leer estos archivos en orden:

1. **`docs/PROYECTO_ESTADO.md`** - Estado actual, fase, sistemas completados
2. **`docs/TODO.md`** - Tareas pendientes y prioridades
3. **`docs/ARQUITECTURA.md`** - Estructura del código

Para tareas específicas, consultar también:
- **`docs/PLAN_10_FEATURES.md`** - Roadmap del MVP
- **`.kiro/specs/[feature]/`** - Specs técnicas detalladas

---

## Reglas de Comportamiento

### Al Recibir una Tarea

1. **Verificar contexto** - ¿Está relacionada con algo en `docs/TODO.md`?
2. **Buscar spec** - ¿Existe en `.kiro/specs/`?
3. **Revisar logs** - ¿Hay trabajo previo en `logs/[sistema]/`?

### Al Escribir Código

1. **Seguir convenciones** - Ver `project-conventions.md`
2. **Namespace correcto** - `EtherDomes.[Modulo]`
3. **Ubicación correcta** - `Assets/_Project/Scripts/[Modulo]/`

### Al Modificar Unity

1. **Usar MCP** - Ver `mcp-servers.md` para herramientas disponibles
2. **Verificar Unity abierto** - MCP requiere Editor activo

### Al Buscar Información

1. **Documentación Unity** - Usar `unity-docs` MCP para búsqueda semántica
2. **Código existente** - Usar herramientas de archivo o grep

### Al Completar Trabajo

1. **Crear log** - En `logs/[sistema]/YYYY-MM-DD-descripcion.md`
2. **Actualizar TODO** - Marcar tareas completadas en `docs/TODO.md`
3. **Notificar cambios importantes** - Actualizar `docs/PROYECTO_ESTADO.md` si aplica

---

## Estructura del Repositorio

```
/
├── README.md                 # Entrada al proyecto
├── Assets/_Project/          # Código del juego
│   ├── Scripts/              # C# por módulo
│   ├── Prefabs/              # Prefabs
│   ├── Scenes/               # Escenas Unity
│   └── ScriptableObjects/    # Datos configurables
├── docs/                     # Toda la documentación
├── logs/                     # Logs de trabajo por sistema
└── .kiro/
    ├── settings/             # Configuración MCP
    ├── specs/                # Especificaciones técnicas
    └── steering/             # Reglas para agentes (este archivo)
```

---

## Sistemas del Juego

| Sistema | Carpeta | Estado |
|---------|---------|--------|
| Combat | `Scripts/Combat/` | 🔄 En progreso |
| Player | `Scripts/Player/` | ✅ Completado |
| Camera | `Scripts/Camera/` | ✅ Completado |
| Network | `Scripts/Network/` | ✅ Completado |
| Enemy | `Scripts/Enemy/` | 🔄 En progreso |
| UI | `Scripts/UI/` | 🔄 En progreso |
| Persistence | `Scripts/Persistence/` | 90% |
| World | `Scripts/World/` | ✅ Completado |
| Progression | `Scripts/Progression/` | 85% |

---

## Escena de Prueba

Para probar el juego:
1. Abrir cualquier escena: `RegionInicio`, `Region1`, o `Mazmorra1_1-1_4`
2. Play en Unity
3. Controles: WASD, Tab (target), 1-2 (ataques), Click derecho (cámara)
4. Usar portales para navegar entre escenas

---

## Preguntas Frecuentes

**¿Dónde está el código principal?**
`Assets/_Project/Scripts/`

**¿Cómo sé qué hacer?**
Leer `docs/TODO.md` y `docs/PROYECTO_ESTADO.md`

**¿Cómo documento mi trabajo?**
Crear log en `logs/[sistema]/`

**¿Cómo interactúo con Unity?**
Usar MCP `unityMCP` (ver `mcp-servers.md`)
