---
inclusion: always
---

# Sistema de Logs de Trabajo

## Regla para Agentes

Al completar trabajo significativo en cualquier sistema del proyecto, **crear una entrada de log** en `logs/[sistema]/`.

### Cuándo Crear Log
- Implementar nueva funcionalidad
- Corregir bugs importantes
- Refactorizar código existente
- Cambios que afecten múltiples archivos
- Decisiones técnicas importantes

### Cuándo NO Crear Log
- Cambios menores de formato
- Actualizar documentación existente
- Correcciones de typos

### Formato del Archivo
```
logs/[sistema]/YYYY-MM-DD-[descripcion-breve].md
```

### Sistemas Disponibles
| Carpeta | Contenido |
|---------|-----------|
| `combat/` | Habilidades, targeting, daño, aggro |
| `player/` | Movimiento, controles, cámara |
| `enemy/` | IA, spawning, comportamiento |
| `network/` | Conexiones, sincronización, Relay |
| `ui/` | Menús, HUD, interfaces |
| `persistence/` | Guardado, carga, encriptación |
| `world/` | Dungeons, regiones, escenas |
| `general/` | Cambios multi-sistema o estructura |

### Contenido Mínimo del Log
```markdown
# [Título]

**Fecha:** YYYY-MM-DD  
**Autor:** Kiro  
**Sistema:** [sistema]

## Resumen
[Qué se hizo]

## Archivos Modificados
- `ruta/archivo.cs` - [cambio]

## Notas
[Info importante para otros devs]
```
