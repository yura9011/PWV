---
inclusion: always
---

# The Ether Domes - Convenciones del Proyecto

## Equipo
Este proyecto es desarrollado por 2 personas. La documentación clara y estructura consistente son críticas.

## Estructura de Carpetas

```
/                           # Raíz del proyecto Unity
├── .kiro/                  # Configuración de Kiro
│   ├── settings/           # MCP y configuraciones
│   ├── specs/              # Especificaciones por feature
│   ├── steering/           # Reglas de contexto para Kiro
│   ├── doc-coauthoring/    # Skill: Co-autoría de documentos
│   ├── docx/               # Skill: Creación de documentos Word
│   └── skill-creator/      # Skill: Creación de nuevos skills
├── Assets/_Project/        # Código del juego
│   ├── Scripts/            # C# organizado por módulo
│   ├── Prefabs/            # Prefabs del juego
│   ├── Scenes/             # Escenas Unity
│   └── ScriptableObjects/  # Datos configurables
├── docs/                   # Documentación del proyecto (crear)
└── *.md                    # Documentos de estado en raíz
```

## Convenciones de Código C#

### Namespaces
```csharp
namespace EtherDomes.Combat { }
namespace EtherDomes.Player { }
namespace EtherDomes.Network { }
```

### Naming
- Clases: `PascalCase` (ej. `TargetSystem`)
- Métodos: `PascalCase` (ej. `ApplyDamage`)
- Variables privadas: `_camelCase` (ej. `_currentTarget`)
- Variables públicas/propiedades: `PascalCase`
- Interfaces: `IPascalCase` (ej. `ITargetable`)

### Organización de Scripts
Cada módulo tiene su carpeta con:
- `[Module]System.cs` - Sistema principal
- `Interfaces/` - Interfaces del módulo
- Archivos auxiliares relacionados

## Documentación

### Archivos en Raíz
| Archivo | Propósito |
|---------|-----------|
| `PROYECTO_ESTADO.md` | Estado actual, sistemas completados |
| `TODO.md` | Tareas pendientes inmediatas |
| `PLAN_10_FEATURES.md` | Roadmap de features para MVP |
| `GUIA_DESARROLLO.md` | Onboarding para desarrolladores |
| `SESSION_NOTES.md` | Notas de sesiones de trabajo |

### Specs (.kiro/specs/)
Cada feature mayor tiene su carpeta con:
- `requirements.md` - Requisitos y criterios de aceptación
- `design.md` - Diseño técnico e interfaces
- `tasks.md` - Tareas de implementación

## Flujo de Trabajo

### Antes de Implementar
1. Revisar `PROYECTO_ESTADO.md` para contexto
2. Consultar spec relevante en `.kiro/specs/`
3. Verificar `TODO.md` para prioridades

### Durante Desarrollo
1. Seguir convenciones de código
2. Usar MCP de Unity para operaciones en Editor
3. Actualizar `TODO.md` al completar tareas

### Al Finalizar Sesión
1. Actualizar `SESSION_NOTES.md` con cambios
2. Actualizar `PROYECTO_ESTADO.md` si hay avances mayores
3. Commit con mensaje descriptivo

## Skills Disponibles

### doc-coauthoring
Para crear documentación estructurada (specs, propuestas, decisiones técnicas).
Usar cuando se necesite documentar una nueva feature o decisión.

### docx
Para crear documentos Word profesionales.
Útil para documentación externa o reportes.

### skill-creator
Para crear nuevos skills personalizados.
Usar si se necesita automatizar un flujo de trabajo repetitivo.
